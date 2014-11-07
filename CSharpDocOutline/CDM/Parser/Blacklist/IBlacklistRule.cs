using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Interface for all blacklist rules for the CDMParser.
	/// If the condition for a blacklist rule is true, the current statment will not be parsed but skipped.
	/// </summary>
    public interface IBlacklistRule
    {
        bool CheckCondition(CDMParser parser, string statement);
    }
}
