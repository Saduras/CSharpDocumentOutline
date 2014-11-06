using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class CEVariableParser : ICEParser
    {
        public bool CheckPreCondition(string statement, CDMParser parser)
        {
			// To be a variable the statement must end of ;
			// But there are cases were first a '{' appears (list/array with default values) than there is atleast a =
            return !string.IsNullOrEmpty(statement) && (statement.Last() == ';' || statement.IndexOf('=') > 0);
        }

		public ICodeDocumentElement Parse(string statement, int lineNumber, CEKind parentKind)
        {
            try
            {
                // Type and Name of a field are either in front of a "=" or in front of the ";"
                string definitionString = "";
                int index = -1;
                if ((index = statement.IndexOf('=')) > 0) // If there is a '=' at index 0, this is not a valid field...
                    definitionString = statement.Substring(0, index);
                else
                    definitionString = statement.Substring(0, statement.Length - 1);

                // Type and Name are the last two words in the definitions string now
				string[] definitions = ParserUtilities.GetWords(definitionString);
                string name = definitions[definitions.Length - 1];
                string type = definitions[definitions.Length - 2];
                // Try to parse the additional access modifier if there are more than two words
                CEAccessModifier accessModifier = CEAccessModifier.Internal;
                if (definitions.Length > 0)
                    accessModifier = CEAccessModifierHelper.Parse(definitions[0], CEAccessModifier.Private);

                GenericCodeElement field = new GenericCodeElement();
				field.Kind = CEKind.Variable;
				field.CanHaveMember = false;
                field.LineNumber = lineNumber;
                field.ElementName = name;
                field.ElementType = type;
                field.AccessModifier = accessModifier;

                return field;
            } catch(Exception e) {
                if (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException || e is ArgumentException)
                {
                    Debug.WriteLine("Could not parse statement as field. Line: " + lineNumber + " exception: " + e.Message);
                    return null;
                }

                throw;
            }
        }
    }
}
