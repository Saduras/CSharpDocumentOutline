using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public interface IBlacklistRule
    {
        bool CheckCondition(CDMParser parser, string statement);
    }
}
