using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class GenericCodeElement : ICodeDocumentElement
    {
        public ICodeDocumentElement Parent { get; set; }
        public List<ICodeDocumentElement> Children { get; private set; }

        public int LineNumber { get; set; }
        public bool CanHaveMember { get; private set; }

        public CEAccessModifier AccessModifier { get; set; }
        public CEKind Kind { get; private set; }
        public string ElementType { get; set; }
        public string ElementName { get; set;}

        public GenericCodeElement(CEKind kind, bool canHaveMember)
        {
            Children = new List<ICodeDocumentElement>();
            Kind = kind;
            CanHaveMember = canHaveMember;
        }

        public string ToString(int depth)
        {
            string tabs = "";
            for (int i = 0; i < depth; i++)
            {
                tabs += "\t";
            }

            string result = LineNumber.ToString() + ": " + tabs + AccessModifier + " " + Kind + " " + ElementName + "\n";
            foreach (var child in Children)
            {
                result += child.ToString(depth + 1);
            }

            return result;
        }

        public override string ToString()
        {
           return this.ToString(0);
        }
    }
}
