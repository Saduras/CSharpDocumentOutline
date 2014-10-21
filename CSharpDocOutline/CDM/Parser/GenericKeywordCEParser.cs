using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class GenericKeywordCEParser : ICEParser
    {
        string Keyword { get; set; }
        CEKind Kind { get; set; }
        bool CanHaveMember { get; set; }

        public GenericKeywordCEParser(string keyword, CEKind kind, bool canHaveMember)
        {
            // C# keywords require a space behind the keyword. 
            // Add this to the requirement reduce miss-interpretation rate.
            Keyword = keyword + " ";
            Kind = kind;
            CanHaveMember = canHaveMember;
        }

        public bool CheckPreCondition(string statement)
        {
            return (statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase)) >= 0;
        }

        public ICodeDocumentElement Parse(string text, int lineNumber)
        {
            int indexOfClass = text.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase);
            return this.Parse(text, lineNumber, indexOfClass);
        }

        public ICodeDocumentElement Parse(string statement, int lineNumber, int indexOfClass)
        {
            if (indexOfClass < 0)
                return null;

            try
            {
                // Get string for access modifier
                string accessModifier = statement.Substring(0, indexOfClass).Trim();
                var split = accessModifier.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                accessModifier = (split.Length > 0) ? split[0] : "";

                // Get string for type/name of class
                int nameStartIndex = indexOfClass + Keyword.Length;
                int nameEndIndex = statement.IndexOf(" ", nameStartIndex);
                if (nameEndIndex < 0)
                    nameEndIndex = statement.Length;
                string name = statement.Substring(nameStartIndex, nameEndIndex - (nameStartIndex));

                var cde = GetInstanceOfElement();
                cde.ElementName = name;
                cde.LineNumber = lineNumber;
                cde.AccessModifier = CEAccessModifierHelper.Parse(accessModifier, CEAccessModifier.Internal);
                // TODO: change default to private if this is a subclass

                return cde;
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException || e is ArgumentNullException)
                {
                    Debug.WriteLine("Parsing statement as " +  Keyword + " failed. Statement: " + statement);
                    return null;
                }

                throw;
            }
        }

        protected ICodeDocumentElement GetInstanceOfElement()
        {
            return new GenericCodeElement(Kind, CanHaveMember);
        }
    }
}
