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
        // Type Type { get; }
        string ElementName { get; set; }

        string ToString(int depth);
    }
}
