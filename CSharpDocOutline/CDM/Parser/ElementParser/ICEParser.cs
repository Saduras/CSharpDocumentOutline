using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Interface for code element parser. A code element parser is used to verify that a statement
	/// represents a certain code element and to parse this statement into the code element datatype.
	/// </summary>
    public interface ICEParser
    {
		/// <summary>
		/// If this condition is true, this parser is able to parse this statement.
		/// </summary>
        bool CheckPreCondition(string statement, CDMParser parser);
		/// <summary>
		/// Take the statement and try to create a CodeDocumentElement from it.
		/// </summary>
        ICodeDocumentElement TryParse(string statement, int lineNumber, CEKind parentKind);
    }
}
