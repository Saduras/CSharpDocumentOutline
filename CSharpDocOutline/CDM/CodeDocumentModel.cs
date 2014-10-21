using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class CodeDocumentModel
    {
        public string DocumentName { get; set; }
        public string FullPath { get; set; }

        public List<ICodeDocumentElement> RootElements { get; private set; }

        public CodeDocumentModel()
        {
            RootElements = new List<ICodeDocumentElement>();
        }

        public override string ToString()
        {
            string result = "CDM: " + DocumentName + "\n";
            foreach(var element in RootElements)
                result += element.ToString(0);

            return result;
        }
    }
}
