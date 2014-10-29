﻿using System;
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
		private IBlacklistRule[] m_blacklist = new IBlacklistRule[] {
            new BRSkipLinesInsideFunctions(),
            new BRIgnoreKeyword("using"),
        };
		#endregion

		#region Element parser
		private ICEParser[] m_elementParser = new ICEParser[]
        { 
            new GenericKeywordCEParser("namespace",     CEKind.Namespace,	CEAccessModifier.None,		GenericKeywordCEParser.HandleType.NoType,           canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("class",         CEKind.Class,		CEAccessModifier.Internal,  GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("struct",        CEKind.Struct,      CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("interface",     CEKind.Interface,   CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:true,		hasParameter:false),
            new GenericKeywordCEParser("enum",          CEKind.Enum,        CEAccessModifier.Internal,	GenericKeywordCEParser.HandleType.TypeEqualName,    canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("event",         CEKind.Event,       CEAccessModifier.Private,	GenericKeywordCEParser.HandleType.ParseType,        canHaveMember:false,	hasParameter:false),
            new GenericKeywordCEParser("delegate",      CEKind.Delegate,    CEAccessModifier.Private,	GenericKeywordCEParser.HandleType.ParseType,        canHaveMember:false,	hasParameter:true),
			new CERegionParser(),
            new CEFieldParser(),
			new CEConstructorParser(),
			new CEMethodParser(),
			new CEPropertyParser(),
        };
		#endregion

		private CodeDocumentModel m_cdm;

		private ICodeDocumentElement m_currentParent;
		public ICodeDocumentElement CurrentParent { get { return m_currentParent; } }

		private ICodeDocumentElement m_lastParsedElement;
		public ICodeDocumentElement LastParsedElement { get { return m_lastParsedElement; } }

		private int m_openBrackets = 0;
		private bool m_multiLineComment = false;

		public void Init()
		{
			m_cdm = null;
			m_currentParent = null;
			m_lastParsedElement = null;

			m_openBrackets = 0;
			m_multiLineComment = false;
		}

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
					element = parser.Parse(statement, lineNumber, parentKind);
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

	}
}
