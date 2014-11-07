using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Parser for propertiers
	/// </summary>
	public class CEPropertyParser : ICEParser
	{
		public bool CheckPreCondition(string statement, CDMParser parser)
		{
			// Properties don't have a secure sign in the same line
			// The '{' might be in the next line
			// For now assume everything that wasn't parsed already might be a property
			// A Property has atleast two words (type and name)
			return ParserUtilities.GetWords(statement).Length > 0 && parser.CurrentParent != null && parser.CurrentParent.CanHaveMember;
		}

		public ICodeDocumentElement TryParse(string statement, int lineNumber, CEKind parentKind)
		{
			try
			{
				// Type and Name of a property are either in front of a "{" or the last two words
				string definitionString = "";
				int index = -1;
				if ((index = statement.IndexOf('{')) >= 0) 
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

				GenericCodeElement property = new GenericCodeElement();
				property.Kind = CEKind.Property;
				property.CanHaveMember = false;
				property.LineNumber = lineNumber;
				property.ElementName = name;
				property.ElementType = type;
				property.AccessModifier = accessModifier;

				return property;
			}
			catch (Exception e)
			{
				if (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException || e is ArgumentException)
				{
					Debug.WriteLine("Could not parse statement as property. Line: " + lineNumber + " exception: " + e.Message);
					return null;
				}

				throw;
			}
		}
	}
}
