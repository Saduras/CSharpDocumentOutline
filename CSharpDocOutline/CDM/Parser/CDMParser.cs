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
        private ICEParser[] _elementParser = new ICEParser[] { new CEClassParser() };

        private CodeDocumentModel _cdm;

        private ICodeDocumentElement _currentParent;
        private ICodeDocumentElement _lastParsedElement;
        private uint _openBrackets;

        public void Parse(StreamReader reader, ref CodeDocumentModel cdm)
        {
            _cdm = cdm;

            string line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                ParseLine(line, lineNumber++);
            }
        }

        private void ParseLine(string line, int lineNumber)
        {
            Debug.WriteLine("Parsing line " + lineNumber);
            int indexOpenBracket = -1;
            string[] statements;

            // TODO: For now assume, we have only one element per line - handle multiple elements per line later
            while ((indexOpenBracket = line.IndexOf("{")) > 0)
            {
                // Get string string until bracket found.
                // This string includes one or more statements but no {
                string preBracket = line.Substring(0, indexOpenBracket);
                line = line.Remove(0, indexOpenBracket + 1);

                // Update parent if closing bracket is found.
                if (preBracket.IndexOf("}") > 0 && _currentParent != null)
                    _currentParent = _currentParent.Parent;

                statements = preBracket.Split(new Char[]{';'});
                foreach (var element in statements)
                {
                    ParseElement(element, lineNumber);
                }

                // The last parsed element is related to the found bracket.
                // Use this as new parent element.
                _currentParent = _lastParsedElement;
            }

            // string line does not contain any more opening brackets.

            // Update parent if closing bracket is found.
            if (line.IndexOf("}") > 0 && _currentParent != null)
                _currentParent = _currentParent.Parent;

            statements = line.Split(new Char[] { ';' });
            foreach (var element in statements)
            {
                ParseElement(element, lineNumber);
            }
        }

        private void ParseElement(string statement, int lineNumber)
        {
            // Skip comment lines
            statement.Trim();
            if (statement.StartsWith("//"))
                return;
            // TODO: correlty find multi-line comments "/* ... */"

            ICodeDocumentElement element = null;

            foreach (var parser in _elementParser)
            {
                if (parser.CheckPreCondition(statement))
                {
                    element = parser.Parse(statement, lineNumber);
                    break;
                }
            }
            
            //if (text.IndexOf("struct", StringComparison.CurrentCultureIgnoreCase) > 0)
            //{
            //    //ParseStruct(text, lineNumber);
            //}

            //if (text.IndexOf("interface", StringComparison.CurrentCultureIgnoreCase) > 0)
            //{
            //    //ParseInterface(text, lineNumber);
            //}

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
    }
}
