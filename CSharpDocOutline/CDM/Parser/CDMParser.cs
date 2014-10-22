using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    class CDMParser
    {
        private ICEParser[] _elementParser = new ICEParser[] 
        { 
            new GenericKeywordCEParser("namespace", CEKind.Namespace, canHaveMember:false, hasType:false),
            new GenericKeywordCEParser("class", CEKind.Class, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("struct", CEKind.Struct, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("interface", CEKind.Interface, canHaveMember:true, hasType:true),
            new GenericKeywordCEParser("enum", CEKind.Enum, canHaveMember:false, hasType:true),
            new CEFunctionParser(),
            new CEFieldParser(),
        };

        private CodeDocumentModel _cdm;

        private ICodeDocumentElement _currentParent;
        private ICodeDocumentElement _lastParsedElement;
        private uint _openBrackets;

        public void Init()
        {
            _cdm = null;
            _currentParent = null;
            _lastParsedElement = null;
        }

        public void Parse(StreamReader reader, ref CodeDocumentModel cdm)
        {
            _cdm = cdm;

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
                if (preBracket.IndexOf("}") > 0 && _currentParent != null)
                    _currentParent = _currentParent.Parent;

                ParseSemicolonSeperatedElements(line, lineNumber);

                // The last parsed element is related to the found bracket.
                // Use this as new parent element.
                _currentParent = _lastParsedElement;
            }

            // string line does not contain any more opening brackets.

            // TODO: check for multiple closing brackets
            // Update parent if closing bracket is found.
            if (line.IndexOf("}") >= 0 && _currentParent != null)
                _currentParent = _currentParent.Parent;

            // Check for simicolons even if there are no opening brackets
            ParseSemicolonSeperatedElements(line, lineNumber);
        }

        private void ParseElement(string statement, int lineNumber)
        {
            ICodeDocumentElement element = null;

            foreach (var parser in _elementParser)
            {
                if (parser.CheckPreCondition(statement))
                {
                    element = parser.Parse(statement, lineNumber);
                    break;
                }
            }

            //if(text.IndexOf("#region", StringComparison.CurrentCultureIgnoreCase) > 0)
            //{
            //    //ParseRegion(text, lineNumber);
            //}

            //// The following elements require certain parents to be valid.
            //// Required parents are: class, interface, struct
            //if (_currentParent != null && !_currentParent.CanHaveMember)
            //{
            //    if (text.IndexOf("event", StringComparison.CurrentCultureIgnoreCase) > 0)
            //    {
            //        //PraseEvent(text, lineNumber);
            //    }

            //    // TODO: Check for fields
            //    // TODO: Check for methods
            //}



            // This statement does not contain any outline element
            if (element == null)
                return;

            AddToCDM(element);
            _lastParsedElement = element;
        }

        private void AddToCDM(ICodeDocumentElement element)
        {
            Debug.WriteLine("Add " + element.Kind + " to CDM");
            if (_currentParent == null)
            {
                _cdm.RootElements.Add(element);
            }
            else
            {
                _currentParent.Children.Add(element);
                element.Parent = _currentParent;
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
