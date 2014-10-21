using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    // CodeElementClassParser
    public class CEClassParser : BaseKeywordCEParser
    {
        protected override string Keyword { get { return "class"; } }

        protected override ICodeDocumentElement GetInstanceOfElement()
        {
            return new GenericCodeElement(CEKind.Class, true);
        }
    }
}
