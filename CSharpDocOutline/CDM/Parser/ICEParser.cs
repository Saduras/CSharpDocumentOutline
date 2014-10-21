using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    interface ICEParser
    {
        bool CheckPreCondition(string statement);
        ICodeDocumentElement Parse(string statement, int lineNumber);
        ICodeDocumentElement Parse(string statement, int lineNumber, int startIndex);
    }
}
