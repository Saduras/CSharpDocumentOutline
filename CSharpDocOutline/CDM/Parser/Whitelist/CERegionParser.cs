using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	public class CERegionParser : ICEParser
	{
		public bool CheckPreCondition(string statement, CDMParser parser)
		{
			return statement.Trim().StartsWith("#region");
		}

		public ICodeDocumentElement Parse(string statement, int lineNumber, CEKind parentKind)
		{
			try
			{
				// Remove "#region" from statement
				statement = statement.Remove(0, 7).Trim();
				// Rest is region name
				string name = statement;

				var region = new GenericCodeElement();
				region.Kind = CEKind.Region;
				region.CanHaveMember = false;
				region.AccessModifier = CEAccessModifier.None;
				region.ElementType = "";

				region.LineNumber = lineNumber;
				region.ElementName = name;

				return region;
			}
			catch (ArgumentOutOfRangeException e)
			{
				Debug.WriteLine("Parsing statement as region failed. Line: " + lineNumber + " Excpetion: " + e.Message);
				return null;
			}
		}
	}
}
