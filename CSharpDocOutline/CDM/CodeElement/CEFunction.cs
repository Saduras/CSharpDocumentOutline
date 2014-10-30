using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    public class CEFunction : ICodeDocumentElement
    {
        public ICodeDocumentElement Parent { get; set; }
        public List<ICodeDocumentElement> Children { get; private set; }

        public int LineNumber { get; set; }
        public bool CanHaveMember { get { return false; } }

        public CEAccessModifier AccessModifier { get; set; }
        public CEKind Kind { get { return CEKind.Function; } }
        public string ElementName { get; set; }
        public string ElementType { get; set; }
        public List<CEParameter> Parameters { get; private set; }
		public bool IsConstructor { get { return Parent.Kind == CEKind.Class && Parent.ElementName == this.ElementType; } }

        public CEFunction() 
        {
            Children = new List<ICodeDocumentElement>();
            Parameters = new List<CEParameter>();
        }

        public string ToString(int depth)
        {
            string tabs = "";
            for (int i = 0; i < depth; i++)
            {
                tabs += "\t";
            }

            string result = LineNumber.ToString() + ": " + tabs + AccessModifier + " " + Kind + " " + ElementName;
            result += "(";
            for (int i = 0; i < Parameters.Count; i++)
            {
                result += Parameters[i].Type + " " + Parameters[i].Name;
                if (i < Parameters.Count - 1)
                    result += ", ";
            }
            result += ")\n";
            foreach (var child in Children)
            {
                result += child.ToString(depth + 1);
            }

            return result;
        }

        public override string ToString()
        {
            string result = (AccessModifier != CEAccessModifier.None) ? AccessModifier + " " : "";
            result += Kind + " " + ElementName;
			result += "(";
			for (int i = 0; i < Parameters.Count; i++)
			{
				result += Parameters[i].Type + " " + Parameters[i].Name;
				if (i < Parameters.Count - 1)
					result += ", ";
			}
			result += ")";
			return result;
        }
    }
}
