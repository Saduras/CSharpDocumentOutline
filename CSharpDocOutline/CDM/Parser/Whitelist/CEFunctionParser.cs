using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class CEFunctionParser : ICEParser
    {
        public bool CheckPreCondition(string statement)
        {
            return statement.IndexOf("(") > 0;
        }

        public ICodeDocumentElement Parse(string statement, int lineNumber)
        {
            try
            {
                int openBracketIndex = statement.IndexOf("(");

                string definitionString = statement.Substring(0, openBracketIndex);
                string paramString = statement.Substring(openBracketIndex + 1, statement.Length - openBracketIndex - 2);

                CEFunction ceFunction = new CEFunction();
                ceFunction.LineNumber = lineNumber;

                ParseDefinitons(definitionString, ref ceFunction);
                ParseParameters(paramString, ref ceFunction);

                return ceFunction;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is ArgumentOutOfRangeException
                    || e is IndexOutOfRangeException)
                {
                    Debug.WriteLine("Failed to parse function from statement: " + statement + "\n Exception: " + e.Message);
                    return null;
                }

                throw;
            }
        }

        private void ParseDefinitons(string definitionString , ref CEFunction ceFunction)
        {
            string[] definitons = definitionString.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // The last word before the parameters is always the function name.
            string name = definitons[definitons.Length - 1];
            // The one before the name is the type.
            string type = definitons[definitons.Length - 2];
            // Now check for optional definitions. 
            CEAccessModifier accessModifier = CEAccessModifier.Internal;
            if (definitons.Length > 2)
                accessModifier = CEAccessModifierHelper.Parse(definitons[0], CEAccessModifier.Internal);

            ceFunction.ElementName = name;
            ceFunction.ElementType = type;
            ceFunction.AccessModifier = accessModifier;
        }

        private void ParseParameters(string paramString, ref CEFunction ceFunction)
        {
            string[] parameters = paramString.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in parameters)
            {
                // Each function parameter must have the form: [type] [name]
                string[] paramDefinitions = param.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string paramType = paramDefinitions[0];
                string paramName = paramDefinitions[1];
                ceFunction.Parameters.Add(new CEFunction.Parameter(paramType, paramName));
            }
        }
    }
}
