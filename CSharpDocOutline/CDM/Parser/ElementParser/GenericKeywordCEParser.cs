using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Code element parser for all keyword identified code elements.
	/// </summary>
    public class GenericKeywordCEParser : ICEParser
    {
		/// <summary>
		/// Possible options how to handle the datatype for a created CodeDocumentElement.
		/// </summary>
        public enum HandleType
        {
            NoType,
            TypeEqualName,
            ParseType
        }

		/// <summary>
		/// The keyword which identifies the wanted statement with spaces.
		/// Spaces are added to improve precision.
		/// </summary>
        string Keyword { get; set; }
		/// <summary>
		/// The keyword which identifies the wanted statement with only following space.
		/// This is used to find the keyword at the beginning of the statement.
		/// </summary>
        string StartKeyword { get; set; }

        CEKind Kind { get; set; }
        HandleType HowHandleType { get; set; }
		// Class, structs and interfaces can have member
		bool CanHaveMember { get; set; }
		bool HasParameter { get; set; }
		// The DefaultAccessModifier will be used if no access modifier is set in a statement.
		CEAccessModifier DefaultAccessModifier { get; set; }

        public GenericKeywordCEParser(string keyword, CEKind kind, CEAccessModifier defaultAccessModifier, HandleType howHandleType, bool canHaveMember, bool hasParameter)
        {
            // C# keywords require a space behind the keyword. 
            // Add this to the requirement reduce miss-interpretation rate.
            Keyword = " " + keyword.Trim() + " ";
            StartKeyword = keyword.Trim() + " ";

            Kind = kind;
			DefaultAccessModifier = defaultAccessModifier;
            CanHaveMember = canHaveMember;
            HowHandleType = howHandleType;
			HasParameter = hasParameter;
        }

        public bool CheckPreCondition(string statement, CDMParser parser)
        {
            // Search for keyword surrounded by spaces or at the beginning of the statements
            return (statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase)) >= 0
                || (statement.IndexOf(StartKeyword, StringComparison.CurrentCultureIgnoreCase)) == 0;
        }

        public ICodeDocumentElement TryParse(string statement, int lineNumber, CEKind parentKind)
        {
            int indexOfKeyword = statement.IndexOf(Keyword, StringComparison.CurrentCultureIgnoreCase);
            if (indexOfKeyword < 0 && statement.IndexOf(StartKeyword, StringComparison.CurrentCultureIgnoreCase) == 0)
                indexOfKeyword = 0;

			return this.TryParse(statement, lineNumber, indexOfKeyword, parentKind);
        }

		public ICodeDocumentElement TryParse(string statement, int lineNumber, int indexOfKeyword, CEKind parentKind)
        {
            if (indexOfKeyword < 0)
                return null;

            try
            {
				statement.Trim();

                // Get string for access modifier
                string accessModifier = statement.Substring(0, indexOfKeyword).Trim();
				var split = ParserUtilities.GetWords(accessModifier);
                accessModifier = (split.Length > 0) ? split[0] : "";

				string definitionString = (string) statement.Clone();

				// If there is a '(' ignore everthing behind this char
				// for the defintion parsing
				int bracketStart = -1;
				if (HasParameter)
				{
					if ((bracketStart = statement.IndexOf('(')) >= 0)
						definitionString = definitionString.Substring(0, bracketStart);
				}

                // Get string for type/name of element
				int definitionsStartIndex = indexOfKeyword + Keyword.Length - 1;
				definitionString = definitionString.Substring(definitionsStartIndex);


				// If there is a '=' ignore everthing behind this char 
				// (the default value doesn't matter for the outlining)
				int index = -1;
				if ((index = definitionString.IndexOf('=')) >= 0)
					definitionString = definitionString.Substring(0, index);

				string[] definitions = ParserUtilities.GetWords(definitionString);

                var cde = new GenericCodeElement();
				cde.Kind = Kind;
				cde.CanHaveMember = CanHaveMember;
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

				// If a opening bracked was found, find the closing one
				// and parse the parameters inside.
				if (bracketStart > 0)
				{
					int bracketEnd = statement.IndexOf(')');
					string parameterString = statement.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
					ParseParameters(parameterString, ref cde);
				}

                cde.LineNumber = lineNumber;

				// Use the code element kind of the parent to figure out the default access modifier.
				// This is necessary a the default access modifier depents on the parent element.
				switch (parentKind)
				{
					case CEKind.Class:
					case CEKind.Struct:
						// Subclasses or substructs are private by default
						cde.AccessModifier = CEAccessModifierHelper.Parse(accessModifier, CEAccessModifier.Private);
						break;
					case CEKind.Interface:
						// Interface Member do not define access modifier
						cde.AccessModifier = CEAccessModifier.None;
						break;
					default:
						cde.AccessModifier = CEAccessModifierHelper.Parse(accessModifier, DefaultAccessModifier);
						break;
				}

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

		/// <summary>
		/// Parse the parameters type and name from a given parameter string.
		/// </summary>
		public void ParseParameters(string paramString, ref GenericCodeElement cde)
		{
			string[] parameters = paramString.Split(new Char[]{','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var param in parameters)
			{
				// Each function parameter must have the form: [type] [name]
				string[] paramDefinitions = ParserUtilities.GetWords(param);
				string paramType = paramDefinitions[0];
				string paramName = paramDefinitions[1];
				cde.Parameters.Add(new CEParameter(paramType, paramName));
			}
		}
    }
}
