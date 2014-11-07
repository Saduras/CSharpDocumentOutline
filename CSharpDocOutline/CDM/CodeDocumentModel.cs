using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	/// <summary>
	/// All supported modi to sort all code elements recursivly.
	/// </summary>
	public enum SortMode
	{
		LineNumber,
		ElementName,
		ElementKind,
	}

	/// <summary>
	/// Datastructur to store document properties and code document element structure.
	/// </summary>
    public class CodeDocumentModel
    {
        public string DocumentName { get; set; }
        public string FullPath { get; set; }

		/// <summary>
		/// First level of code elements. Walking through those trees will give you all other code elements of the document.
		/// </summary>
        public List<ICodeDocumentElement> RootElements { get; private set; }

        public CodeDocumentModel()
        {
            RootElements = new List<ICodeDocumentElement>();
        }

		#region Sort Hierarchie
		/// <summary>
		/// Sort the all code elements whith the same parent with the given sort mode.
		/// </summary>
		public void Sort(SortMode mode)
		{
			SortList(RootElements, mode);

			foreach (var element in RootElements)
			{
				RecursivSort(element, mode);
			}
		}

		/// <summary>
		/// Recusively sort children list of code elements.
		/// </summary>
		private void RecursivSort(ICodeDocumentElement element, SortMode mode)
		{
			SortList(element.Children, mode);

			foreach (var children in element.Children)
			{
				RecursivSort(children, mode);
			}
		}

		/// <summary>
		/// Sort a list of ICodeDocumentElement's with the given sort mode.
		/// </summary>
		private void SortList(List<ICodeDocumentElement> list, SortMode mode)
		{
			switch (mode)
			{
				case SortMode.LineNumber:
					list.Sort((x, y) =>
					{
						if (x == null || y == null)
							return 0;

						return x.LineNumber.CompareTo(y.LineNumber);
					});
					break;

				case SortMode.ElementName:
					list.Sort((x, y) =>
					{
						if (x == null || y == null)
							return 0;

						return x.ElementName.CompareTo(y.ElementName);
					});
					break;

				case SortMode.ElementKind:
					list.Sort((x, y) =>
					{
						if (x == null || y == null)
							return 0;

						return x.Kind.CompareTo(y.Kind);
					});
					break;
			}
		}
		#endregion

        public override string ToString()
        {
            string result = "CDM: " + DocumentName + "\n";
            foreach(var element in RootElements)
                result += element.ToString(0);

            return result;
        }
    }
}
