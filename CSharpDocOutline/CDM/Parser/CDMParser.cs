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
        private IBlacklistRule[] m_blacklist = new IBlacklistRule[] {
            new BRSkipLinesInsideFunctions(),
            new BRIgnoreKeyword("using"),
        };

        private ICEParser[] m_elementParser = new ICEParser[] 
        { 
            new GenericKeywordCEParser("namespace", CEKind.Namespace, canHaveMember:false, hasType:false),
            new GenericKeywordCEParser("class", CEKind.Class, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("struct", CEKind.Struct, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("interface", CEKind.Interface, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("enum", CEKind.Enum, canHaveMember:false, hasType:true),
            new CEFunctionParser(),
            new CEFieldParser(),
        };

        private CodeDocumentModel m_cdm;

        private ICodeDocumentElement m_currentParent;
        public ICodeDocumentElement CurrentParent { get { return m_currentParent; } }

        private ICodeDocumentElement m_lastParsedElement;
        public ICodeDocumentElement LastParsedElement { get { return m_lastParsedElement; } }

        public void Init()
        {
            m_cdm = null;
            m_currentParent = null;
            m_lastParsedElement = null;
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

            // Skip comment lines
            line = line.Trim(new Char[] { ' ', '\t' });
            if (line.StartsWith("//"))
                return;
            // TODO: correctly find multi-line comments "/* ... */"

            int indexOpenBracket = -1;
            while ((indexOpenBracket = line.IndexOf("{")) >= 0)
            {
                // Get string string until bracket found.
                // This string includes one or more statements but no {
                string preBracket = line.Substring(0, indexOpenBracket);
                line = line.Remove(0, indexOpenBracket + 1);

                // Update parent if closing bracket is found.
                if (preBracket.IndexOf("}") > 0 && m_currentParent != null)
                    m_currentParent = m_currentParent.Parent;

                ParseSemicolonSeperatedElements(line, lineNumber);

                // The last parsed element is related to the found bracket.
                // Use this as new parent element.
                m_currentParent = m_lastParsedElement;
            }

            // string line does not contain any more opening brackets.

            // TODO: check for multiple closing brackets
            // Update parent if closing bracket is found.
            if (line.IndexOf("}") >= 0 && m_currentParent != null)
                m_currentParent = m_currentParent.Parent;

            // Check for simicolons even if there are no opening brackets
            ParseSemicolonSeperatedElements(line, lineNumber);
        }

        private void ParseElement(string statement, int lineNumber)
        {
            ICodeDocumentElement element = null;

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
                if (parser.CheckPreCondition(statement))
                {
                    element = parser.Parse(statement, lineNumber);
                    break;
                }
            }

            // This statement does not contain any outline element
            if (element == null)
                return;

            AddToCDM(element);
            m_lastParsedElement = element;
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

        private void ParseSemicolonSeperatedElements(string statements, int lineNumber)
        {
            int indexOfSemicolon = -1;
            while ((indexOfSemicolon = statements.IndexOf(";")) >= 0)
            {
                // Make sure to keep the semicolon on the statement if there is any
                string statement = statements.Substring(0, indexOfSemicolon + 1);
                statements = statements.Remove(0, statement.Length);
                ParseElement(statement, lineNumber);
            }

            // Parse the rest behind the las semiconlons
            ParseElement(statements, lineNumber);
        }
    }
}
