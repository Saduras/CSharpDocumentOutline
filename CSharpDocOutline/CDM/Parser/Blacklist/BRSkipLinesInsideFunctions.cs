using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class BRSkipLinesInsideFunctions : IBlacklistRule
    {
        public bool CheckCondition(CDMParser parser, string statment)
        {
            return parser.CurrentParent != null && parser.CurrentParent.Kind == CEKind.Function;
        }
    }
}
