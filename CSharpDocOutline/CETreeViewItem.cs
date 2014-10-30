using DavidSpeck.CSharpDocOutline.CDM;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DavidSpeck.CSharpDocOutline
{
	public class CETreeViewItem : TreeViewItem
	{
		public ICodeDocumentElement CDElement { get; private set; }

		public CETreeViewItem(ICodeDocumentElement element)
			: base()
		{
			CDElement = element;
		}
	}
}
