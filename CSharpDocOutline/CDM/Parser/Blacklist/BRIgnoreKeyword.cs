using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class BRIgnoreKeyword : IBlacklistRule
    {
        private string m_keyword;
        private string m_startkeyword;

        public BRIgnoreKeyword(string keyword)
        {
            m_keyword = " " + keyword.Trim() + " "; // keyword inside statement
            m_startkeyword = keyword.Trim() + " "; // keyword at statement beginning
        }

        public bool CheckCondition(CDMParser parser, string statement)
        {
            // Trigger rule if keyword appears in the statement
            return statement.IndexOf(m_keyword, StringComparison.CurrentCultureIgnoreCase) >= 0
                || statement.IndexOf(m_startkeyword, StringComparison.CurrentCultureIgnoreCase) == 0;
        }
    }
}
