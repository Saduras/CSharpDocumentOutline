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
		private ICodeDocumentElement m_cdElement;
		private DocOutlineView m_docOutlineView;

		public CETreeViewItem(DocOutlineView docOutlineView, ICodeDocumentElement element)
			: base()
		{
			m_docOutlineView = docOutlineView;
			m_cdElement = element;
		}

		protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			if((e.OriginalSource as TextBlock).Text == this.Header)
				m_docOutlineView.MoveToLineAndOffSetInDocument(m_cdElement.LineNumber, 1);
		}
	}
}
