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
        public enum HandleType
        {
            NoType,
            TypeEqualName,
            ParseType
        }

        string Keyword { get; set; }
        string StartKeyword { get; set; }
        CEKind Kind { get; set; }
        bool CanHaveMember { get; set; }
        HandleType HowHandleType { get; set; }

        public GenericKeywordCEParser(string keyword, CEKind kind, HandleType howHandleType, bool canHaveMember)
        {
            // C# keywords require a space behind the keyword. 
            // Add this to the requirement reduce miss-interpretation rate.
            Keyword = " " + keyword.Trim() + " ";
            StartKeyword = keyword.Trim() + " ";
            Kind = kind;
            CanHaveMember = canHaveMember;
            HowHandleType = howHandleType;
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
				if (Keyword == "event")
				{
					var a =1;
				}

                // Get string for access modifier
                string accessModifier = statement.Substring(0, indexOfKeyword).Trim();
                var split = accessModifier.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                accessModifier = (split.Length > 0) ? split[0] : "";

                // Get string for type/name of element
				int definitionsStartIndex = indexOfKeyword + Keyword.Length;
				string definitionString = statement.Substring(definitionsStartIndex);

				// If there is a '=' ignore everthing behind this char 
				// (the default value doesn't matter for the outlining)
				int index = -1;
				if ((index = statement.IndexOf('=')) >= 0)
					definitionString = statement.Substring(0, index);

				string[] definitions = definitionString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				

                var cde = new GenericCodeElement(Kind, CanHaveMember);
                
                
                switch (HowHandleType)
                {
                    // e.g. namespace does not have any type, so leave the type empty
                    case HandleType.NoType:
                        cde.ElementName = definitions[0];
                        cde.ElementType = "";
                        break;
                    // For enum, class, struct and interface the type is equal their name
                    case HandleType.TypeEqualName:
                        cde.ElementName = definitions[0];
                        cde.ElementType = definitions[0];
                        break;
                    // If the type is different from the name, the type comes first
                    case HandleType.ParseType:
                        cde.ElementName = definitions[1];
                        cde.ElementType = definitions[0];
                        break;
                }

                cde.LineNumber = lineNumber;
                cde.AccessModifier = CEAccessModifierHelper.Parse(accessModifier, CEAccessModifier.Internal);
                // TODO: change default to private if this is a subclass

                return cde;
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException || e is ArgumentNullException)
                {
                    Debug.WriteLine("Parsing statement as " +  Keyword.Trim() + " failed. Statement: " + statement);
                    return null;
                }

                throw;
            }
        }
    }
}
