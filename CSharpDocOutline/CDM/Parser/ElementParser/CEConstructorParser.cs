﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Element parser for constructors. Use the parent class name as keyword.
	/// </summary>
	public class CEConstructorParser : ICEParser
	{
		public bool CheckPreCondition(string statement, CDMParser parser)
		{
			var parent = parser.CurrentParent;
			return parent != null
				&& parent.Kind == CEKind.Class
				&& statement.IndexOf(parent.ElementName, StringComparison.CurrentCultureIgnoreCase) > 0;
		}

		public ICodeDocumentElement TryParse(string statement, int lineNumber, CEKind parentKind)
		{
			try
			{
				int openBracketIndex = statement.IndexOf("(");

				string definitionString = statement.Substring(0, openBracketIndex);
				string paramString = statement.Substring(openBracketIndex + 1, statement.Length - openBracketIndex - 2);

				GenericCodeElement ceMethod = new GenericCodeElement();
				ceMethod.LineNumber = lineNumber;
				ceMethod.Kind = CEKind.Method;

				ParseDefinitons(definitionString, ref ceMethod);
				ParseParameters(paramString, ref ceMethod);

				return ceMethod;
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

		private void ParseDefinitons(string definitionString, ref GenericCodeElement ceFunction)
		{
			string[] definitons = ParserUtilities.GetWords(definitionString);
			// The last word before the parameters is always the function name.
			string name = definitons[definitons.Length - 1];
			// Now check for optional definitions. 
			CEAccessModifier accessModifier = CEAccessModifier.Internal;
			if (definitons.Length > 1)
				accessModifier = CEAccessModifierHelper.Parse(definitons[0], CEAccessModifier.Internal);

			ceFunction.ElementName = name;
			ceFunction.ElementType = name; // for constructor type and name are equal
			ceFunction.AccessModifier = accessModifier;
		}

		private void ParseParameters(string paramString, ref GenericCodeElement ceFunction)
		{
			string[] parameters = paramString.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var param in parameters)
			{
				// Each function parameter must have the form: [type] [name]
				string[] paramDefinitions = ParserUtilities.GetWords(param);
				string paramType = paramDefinitions[0];
				string paramName = paramDefinitions[1];
				ceFunction.Parameters.Add(new CEParameter(paramType, paramName));
			}
		}
	}
}
