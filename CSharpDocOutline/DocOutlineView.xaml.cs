using DavidSpeck.CSharpDocOutline.CDM;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DavidSpeck.CSharpDocOutline
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class DocOutlineView : UserControl
    {

		public DTE2 DTE { get; private set; }
		public Document CurrentDocument { get; private set; }

        public DocOutlineView()
        {
            InitializeComponent();
            
        }

		public void Init(DTE2 dte)
		{
			DTE = dte;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
            //                "Document Outline CSharp");
            Debug.WriteLine("The buttons was clicked");
        }

        public void OutlineDocument(CodeDocumentModel cdm, Document doc)
        {
			CurrentDocument = doc;

			// Remove old tree
			outlineTreeView.Items.Clear();

			var rootItem = new TreeViewItem();
			rootItem.Header = cdm.DocumentName;
			rootItem.IsExpanded = true;
			outlineTreeView.Items.Add(rootItem);

			foreach (var element in cdm.RootElements)
			{
				AddCodeElementToTreeViewRecursively(element, rootItem);
			}
        }

		public void AddCodeElementToTreeViewRecursively(ICodeDocumentElement element, TreeViewItem parent)
		{
			var item = new CETreeViewItem(this, element);
			item.Header = element.ToString();
			parent.Items.Add(item);
			item.IsExpanded = true;

			foreach (var child in element.Children) 
			{
				AddCodeElementToTreeViewRecursively(child, item);
			}
		}

		public void MoveToLineAndOffSetInDocument(int line, int offset)
		{
			CurrentDocument.Activate();

			var selection = (EnvDTE.TextSelection) CurrentDocument.Selection;
			selection.MoveToLineAndOffset(line, offset);
		}
    }
}