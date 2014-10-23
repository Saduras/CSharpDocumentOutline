using DavidSpeck.CSharpDocOutline.CDM;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DavidSpeck.CSharpDocOutline
{
	public class CETreeViewItem : TreeViewItem
	{
		public ICodeDocumentElement CDElement { get; private set; }
		private DocOutlineView m_docOutlineView;

		public CETreeViewItem(DocOutlineView docOutlineView, ICodeDocumentElement element)
			: base()
		{
			m_docOutlineView = docOutlineView;
			CDElement = element;
		}
	}
}
