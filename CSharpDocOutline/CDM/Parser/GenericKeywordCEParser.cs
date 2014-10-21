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
            Keyword = keyword;
            Kind = kind;
            CanHaveMember = canHaveMember;
        }

        public bool CheckPreCondition(string statement)
        {
            return (statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase)) > 0;
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
                accessModifier = accessModifier.Split(new Char[] { ' ' })[0];

                // Get string for type/name of class
                int typeEndIndex = statement.IndexOf(" ", indexOfClass + 6);
                if (typeEndIndex < 0)
                    typeEndIndex = statement.Length;
                string name = statement.Substring(indexOfClass + 6, typeEndIndex - (indexOfClass + 6));

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
