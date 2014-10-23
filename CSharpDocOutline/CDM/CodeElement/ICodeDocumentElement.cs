using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public interface ICodeDocumentElement
    {
        ICodeDocumentElement Parent { get; set; }
        List<ICodeDocumentElement> Children { get; }

        int LineNumber { get; set; }
		bool CanHaveMember { get; }

        CEAccessModifier AccessModifier { get; set; }
		CEKind Kind { get; }
        string ElementType { get; set; }
        string ElementName { get; set; }
		List<CEParameter> Parameters { get; }

        string ToString(int depth);
    }

	public class CEParameter
	{
		public string Type { get; set; }
		public string Name { get; set; }

		public CEParameter(string type, string name)
		{
			Type = type;
			Name = name;
		}
	}
}
