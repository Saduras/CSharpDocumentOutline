﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// Generic implementation of the ICodeDocumentElement interface to represent all possible code elements.
	/// Pure data class.
	/// </summary>
    public class GenericCodeElement : ICodeDocumentElement
    {
        public ICodeDocumentElement Parent { get; set; }
        public List<ICodeDocumentElement> Children { get; private set; }

        public int LineNumber { get; set; }
        public bool CanHaveMember { get; set; }

        public CEAccessModifier AccessModifier { get; set; }
        public CEKind Kind { get; set; }
        public string ElementType { get; set; }
        public string ElementName { get; set;}
		public List<CEParameter> Parameters { get; private set; }

        public GenericCodeElement()
        {
            Children = new List<ICodeDocumentElement>();
			Parameters = new List<CEParameter>();
        }

		/// <summary>
		/// Recursive ToString implementation to creat a string whichs visualies the tree structur.
		/// </summary>
		/// <param name="depth">Current level of depth in the tree, used for indentation.</param>
        public string ToString(int depth)
        {
            string tabs = "";
            for (int i = 0; i < depth; i++)
            {
                tabs += "\t";
            }

            string result = LineNumber.ToString() + ": " + tabs + AccessModifier + " " + Kind + " " + ElementName;
			if (Parameters.Count > 0)
			{
				result += "(";
				for (int i = 0; i < Parameters.Count; i++)
				{
					result += Parameters[i].Type + " " + Parameters[i].Name;
					if (i < Parameters.Count - 1)
						result += ", ";
				}
				result += ")";
			}
			result += "\n";
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
			if (Parameters.Count > 0)
			{
				result += "(";
				for (int i = 0; i < Parameters.Count; i++)
				{
					result += Parameters[i].Type + " " + Parameters[i].Name;
					if (i < Parameters.Count - 1)
						result += ", ";
				}
				result += ")";
			}
			return result;
        }
    }
}
