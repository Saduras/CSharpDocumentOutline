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
        string StartKeyword { get; set; }
        CEKind Kind { get; set; }
        bool CanHaveMember { get; set; }
        bool HasType { get; set; }

        public GenericKeywordCEParser(string keyword, CEKind kind, bool canHaveMember, bool hasType)
        {
            // C# keywords require a space behind the keyword. 
            // Add this to the requirement reduce miss-interpretation rate.
            Keyword = " " + keyword + " ";
            StartKeyword = keyword + " ";
            Kind = kind;
            CanHaveMember = canHaveMember;
            HasType = hasType;
        }

        public bool CheckPreCondition(string statement)
        {
            // Search for keyword surrounded by spaces or at the beginning of the statements
            return (statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase)) >= 0
                || (statement.IndexOf(StartKeyword, StringComparison.CurrentCultureIgnoreCase)) == 0;
        }

        public ICodeDocumentElement Parse(string statement, int lineNumber)
        {
            int indexOfKeyword = statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase);
            if (indexOfKeyword < 0 && statement.IndexOf(StartKeyword, StringComparison.CurrentCultureIgnoreCase) == 0)
                indexOfKeyword = 0;

            return this.Parse(statement, lineNumber, indexOfKeyword);
        }

        public ICodeDocumentElement Parse(string statement, int lineNumber, int indexOfKeyword)
        {
            if (indexOfKeyword < 0)
                return null;

            try
            {
                // Get string for access modifier
                string accessModifier = statement.Substring(0, indexOfKeyword).Trim();
                var split = accessModifier.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                accessModifier = (split.Length > 0) ? split[0] : "";

                // Get string for type/name of element
                int nameStartIndex = indexOfKeyword + Keyword.Length;
                int nameEndIndex = statement.IndexOf(" ", nameStartIndex);
                if (nameEndIndex < 0)
                    nameEndIndex = statement.Length;
                string name = statement.Substring(nameStartIndex, nameEndIndex - (nameStartIndex));

                
                var cde = GetInstanceOfElement();
                cde.ElementName = name;
                // This parser is used for namespace, class, struct, interface, enum.
                // Their type is equal their name (class, struct, interface, enum) or they don't have one (namespace)
                if(HasType)
                    cde.ElementType = name;

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
