using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class CENamespaceParser : BaseKeywordCEParser
    {
        protected override string Keyword { get { return "namespace"; } }

        protected override ICodeDocumentElement GetInstanceOfElement()
        {
            return new GenericCodeElement(CEKind.Namespace, false);
        }
    }
}
