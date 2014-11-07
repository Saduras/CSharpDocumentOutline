using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	public class CDMParser
	{
		#region Blacklist
		/// <summary>
		/// Blacklist rules are conditions when to not parse a certain element.
		/// </summary>
		private IBlacklistRule[] m_blacklist = new IBlacklistRule[] {
            new BRSkipLinesInsideMethods(),
			new BRSkipLinesInsideProperties(),
            new BRIgnoreKeyword("using"),
        };
		#endregion

		#region Element parser
		/// <summary>
		/// All setuped element parser. They will check in the same exact order. The Order matters in some cases.
		/// </summary>
		private ICEParser[] m_elementParser = new ICEParser[]
        { 
			new CERegionParser(),
            new GenericKeywordCEParser("namespace",     CEKind.Namespace,	CEAccessModifier.None,		GenericKeywordCEParser.HandleType.NoType,           canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("class",         CEKind.Class,		CEAccessModifier.Internal,  GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("struct",        CEKind.Struct,      CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("interface",     CEKind.Interface,   CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("enum",          CEKind.Enum,        CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("event",         CEKind.Event,       CEAccessModifier.Private,	GenericKeywordCEParser.HandleType.ParseType,        canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("delegate",      CEKind.Delegate,    CEAccessModifier.Private,	GenericKeywordCEParser.HandleType.ParseType,        canHaveMember:false,	hasParameter:true),
            new CEVariableParser(),
			new CEConstructorParser(),
			new CEMethodParser(),
			new CEPropertyParser(),
        };
		#endregion

        #region Member
        private CodeDocumentModel m_cdm;

		private ICodeDocumentElement m_currentParent;
		public ICodeDocumentElement CurrentParent { get { return m_currentParent; } }

		private ICodeDocumentElement m_lastParsedElement;
		public ICodeDocumentElement LastParsedElement { get { return m_lastParsedElement; } }

		private int m_openBrackets = 0;
		private bool m_multiLineComment = false;
        #endregion

        public void Init()
		{
			m_cdm = null;
			m_currentParent = null;
			m_lastParsedElement = null;

			m_openBrackets = 0;
			m_multiLineComment = false;
		}

		/// <summary>
		/// Parse the a given document and create a CodeDocumentModel for it.
		/// </summary>
		public void Parse(StreamReader reader, ref CodeDocumentModel cdm)
		{
			m_cdm = cdm;

			string line;
			int lineNumber = 1;
			while ((line = reader.ReadLine()) != null)
			{
				ParseLine(line, lineNumber++);
			}
		}

		/// <summary>
		/// Try parsing a line of code. Find element statements and forward them to the ParseElement() function.
		/// Once an element is found, repead recursivly with the rest of the line.
		/// </summary>
		private void ParseLine(string line, int lineNumber)
		{
			Debug.WriteLine("Parsing line " + lineNumber);

			line = line.Trim(new Char[] { ' ', '\t' });

			// Handle comments
			// Skip comment lines
			if (line.StartsWith("//"))
				return;

			// Find mutli line comments
			if (line.StartsWith("/*"))
				m_multiLineComment = true;

			int indexOfCommentEnd = -1;
			if ((indexOfCommentEnd = line.IndexOf("*/")) >= 0)
			{
				// Crop of comment area and close multi line comment flag
				line = line.Substring(indexOfCommentEnd + 2);
				m_multiLineComment = false;
			}

			if (m_multiLineComment)
				return;

			if (lineNumber == 74)
			{
				var a = 1;
			}

			line = RemoveStrings(line);

			// Now parse the line until the next '{' and ';'
			// Save line length
			int length = line.Length;

			int indexOpenBracket = -1;
			if ((indexOpenBracket = line.IndexOf("{")) >= 0)
			{
				// Get string string until bracket found.
				// This string includes one or more statements but no {
				string preBracket = line.Substring(0, indexOpenBracket);
				line = line.Remove(0, indexOpenBracket + 1);

				CheckForClosingBrackets(preBracket);

				ParseSemicolonSeperatedElements(preBracket, lineNumber);

				// The last parsed element is related to the found bracket.
				// Use this as new parent element.
				if (m_currentParent != m_lastParsedElement)
				{
					m_currentParent = m_lastParsedElement;
					m_openBrackets = 1;
				}
				else
				{
					m_openBrackets++;
				}
			}
			else
			{
				// string line does not contain any more opening brackets.
				string preSemicolon = line;
				int indexOfSemicolon = -1;
				if ((indexOfSemicolon = line.IndexOf(";")) >= 0)
				{
					preSemicolon = line.Substring(0, indexOfSemicolon + 1);
				}

				CheckForClosingBrackets(preSemicolon);

				// Check for simicolons even if there are no opening brackets
				line = ParseSemicolonSeperatedElements(line, lineNumber);
			}
			
			if (line.Length > 0)
			{
				// Look again for delimiter and comment on the rest of the line
				ParseLine(line, lineNumber);
			}
		}

		/// <summary>
		/// Check all blacklist rules for the given statement. Then check for a matching element parser
		/// and hand the statment to the found parser.
		/// </summary>
		private void ParseElement(string statement, int lineNumber)
		{
			ICodeDocumentElement element = null;

			if (statement.Trim().StartsWith("#endregion") && m_currentParent.Kind == CEKind.Region)
			{
				// Close #region
				m_currentParent = m_currentParent.Parent;
			}


			// Check blacklist rules. If a rule applies, don't parse this statement
			foreach (var blackRule in m_blacklist)
			{
				if (blackRule.CheckCondition(this, statement))
				{
					return;
				}
			}

			// Check element parser and use the first one which applies
			foreach (var parser in m_elementParser)
			{
				if (parser.CheckPreCondition(statement, this))
				{
					var parentKind = (CurrentParent != null) ? CurrentParent.Kind : CEKind.Namespace;
					element = parser.TryParse(statement, lineNumber, parentKind);
					break;
				}
			}

			// This statement does not contain any outline element
			if (element == null)
				return;

			AddToCDM(element);
			m_lastParsedElement = element;

			if (element.Kind == CEKind.Region)
			{
				m_currentParent = m_lastParsedElement;
			}
		}

		/// <summary>
		/// Add a parsed CodeDocumentElement to the CodeDocumentModel.
		/// </summary>
		private void AddToCDM(ICodeDocumentElement element)
		{
			Debug.WriteLine("Add " + element.Kind + " to CDM");
			if (m_currentParent == null)
			{
				m_cdm.RootElements.Add(element);
			}
			else
			{
				m_currentParent.Children.Add(element);
				element.Parent = m_currentParent;
			}
		}

		/// <summary>
		/// Find a semicolon delimiter and gives the found statement to the ParseElement() function.
		/// </summary>
		private string ParseSemicolonSeperatedElements(string statements, int lineNumber)
		{
			string statement = statements;

			int indexOfSemicolon = -1;
			if ((indexOfSemicolon = statements.IndexOf(";")) >= 0)
			{
				// Make sure to keep the semicolon on the statement if there is any
				statement = statements.Substring(0, indexOfSemicolon + 1);
				statements = statements.Remove(0, statement.Length);
			}
			else
			{
				// Everything was parsed of this element
				statements = "";
			}
			ParseElement(statement, lineNumber);

			return statements;
		}

		/// <summary>
		/// Find closing cureld brackets and update parenting accordingly.
		/// </summary>
		private void CheckForClosingBrackets(string statement)
		{
			string tmpString = (string) statement.Clone();
			int indexOfClosingBracket = -1;
			while ( (indexOfClosingBracket = tmpString.IndexOf("}")) >= 0 && m_currentParent != null)
			{
				tmpString = tmpString.Substring(indexOfClosingBracket + 1);
				if (m_openBrackets > 1)
				{
					m_openBrackets--;
				}
				else
				{
					m_currentParent = m_currentParent.Parent;
				}
			}
		}

		/// <summary>
		/// Crop out strings - the parsers should ignore everthing inside strings anyway.
		/// Find pairs of double and single quotation marks and remove everthing in between.
		/// </summary>
		private string RemoveStrings(string line)
		{
			int lastFound = -1;
			int indexFound = -1;
			int indexEnd = -1;
			while ((indexFound = line.IndexOf("\"")) > lastFound)
			{
				if ((indexEnd = line.IndexOf("\"", indexFound + 1)) > 0)
				{
					line = line.Remove(indexFound, indexEnd - indexFound + 1);
					lastFound = -1;
				}
				else
				{
					lastFound = indexFound;
				}
			}
			lastFound = -1;
			indexFound = -1;
			indexEnd = -1;
			while ((indexFound = line.IndexOf("'")) > lastFound)
			{
				if ((indexEnd = line.IndexOf("'", indexFound + 1)) > 0)
				{
					line = line.Remove(indexFound, indexEnd - indexFound + 1);
					lastFound = -1;
				}
				else
				{
					lastFound = indexFound;
				}
			}

			return line;
		}
	}
}
