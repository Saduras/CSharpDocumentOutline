using DavidSpeck.CSharpDocOutline.CDM;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
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
using System.Windows.Interop;
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
		public CodeDocumentModel CDM { get; private set; }
		public Document CurrentDocument { get; private set; }

		CETreeViewItem m_selected = null;

		SortMode m_sortMode = SortMode.LineNumber;

		public DocOutlineView()
		{
			InitializeComponent();

			// Default sort mode selection
			sortByLineBtn.IsChecked = true;

			// Sort button handler
			// Change sort mode for current CDM and generate outline new.
			sortByLineBtn.Click += new RoutedEventHandler((o, e) =>
			{
				m_sortMode = SortMode.LineNumber;
				OutlineDocument(CDM, CurrentDocument);
				// Uncheck other sort buttons
				sortByKindBtn.IsChecked = false;
				sortByNameBtn.IsChecked = false;
			});
			sortByKindBtn.Click += new RoutedEventHandler((o, e) =>
			{
				m_sortMode = SortMode.ElementKind;
				OutlineDocument(CDM, CurrentDocument);
				// Uncheck other sort buttons
				sortByLineBtn.IsChecked = false;
				sortByNameBtn.IsChecked = false;
			});
			sortByNameBtn.Click += new RoutedEventHandler((o, e) =>
			{
				m_sortMode = SortMode.ElementName;
				OutlineDocument(CDM, CurrentDocument);
				// Uncheck other sort buttons
				sortByKindBtn.IsChecked = false;
				sortByLineBtn.IsChecked = false;
			});
		}

		/// <summary>
		/// Create a CETreeView hierarchy for the given CodeDocumentModel.
		/// This calls CDM.Sort() before creating the hierarchy.
		/// </summary>
		/// <param name="cdm"></param>
		/// <param name="doc"></param>
		public void OutlineDocument(CodeDocumentModel cdm, Document doc)
		{
			if (cdm == null)
				return;

			CDM = cdm;
			CurrentDocument = doc;

			// Apply sorting
			CDM.Sort(m_sortMode);

			// Remove old tree
			outlineTreeView.Items.Clear();

			// Build up new tree recursively
			var rootItem = new CETreeViewItem(null);
			rootItem.Header = cdm.DocumentName;
			rootItem.IsExpanded = true;
			rootItem.MouseDoubleClick += new MouseButtonEventHandler(OnRootElementMouseDoubleClick);
			outlineTreeView.Items.Add(rootItem);

			foreach (var element in cdm.RootElements)
			{
				AddCodeElementToTreeViewRecursively(element, rootItem);
			}
		}

		/// <summary>
		/// Generate CETreeViewItems for all CodeDocumentElements in a tree recursivly.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="parent"></param>
		public void AddCodeElementToTreeViewRecursively(ICodeDocumentElement element, CETreeViewItem parent)
		{
			var item = new CETreeViewItem(element);
			item.Header = element.ToString();
			parent.Items.Add(item);
			item.IsExpanded = true;
			item.Style = (Style) this.Resources["CETreeViewItem"];
			item.MouseDoubleClick += new MouseButtonEventHandler(OnMouseDoubleClick);
			item.Selected += new RoutedEventHandler(OnItemSelected);

			foreach (var child in element.Children)
			{
				AddCodeElementToTreeViewRecursively(child, item);
			}
		}

		#region Event handler
		/// <summary>
		/// Mark a CETreeViewItem once it is selected.
		/// This will be used to identify the item which triggered a event bubble.
		/// </summary>
		private void OnItemSelected(object sender, RoutedEventArgs e)
		{
			m_selected = (CETreeViewItem) sender;
			e.Handled = true;
		}

		/// <summary>
		/// Unique double click event handler for the root TreeViewItem.
		/// This is required because of the way TreeViewItem let events bubble up.
		/// There is now way to stop the bubbling and all TreeViewItems will (re-)claim the focus.
		/// So do all focus changes only once in the root element.
		/// Change the focus to the current code document.
		/// </summary>
		public void OnRootElementMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			CurrentDocument.Activate();
		}

		/// <summary>
		/// On double click move the text cursor in the code document window to the position
		/// of the element presented by the clicked CETreeViewItem.
		/// Don't change the focus here because of the way how events bubble in TreeViewItems.
		/// See OnRootElementMouseDoubleClick() for the focus change.
		/// </summary>
		public void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = (CETreeViewItem) sender;

			if (item != m_selected)
				return;

			// Handle only on source TreeViewItem
			//if (!(e.OriginalSource is TextBlock) 
			//    || (string) item.Header != ((TextBlock) e.OriginalSource).Text)
			//    return;

			int line = item.CDElement.LineNumber;
			int offset = 1;

			// Get the document selection and move it to the wanted code element
			var selection = (EnvDTE.TextSelection) CurrentDocument.Selection;
			selection.MoveToLineAndOffset(line, offset);

			// Move the cursor to the first found (, or if it's not found, move to the end of the line
			if (!selection.FindPattern("("))
				selection.EndOfLine();

			string name = item.CDElement.ElementName;

			// Find the name on the line, moving backwards from the current position
			// This is to the put the cursor in the same place as it would be if you chose the element in the navigation bar
			selection.FindPattern(name, (int) vsFindOptions.vsFindOptionsMatchCase | (int) vsFindOptions.vsFindOptionsBackwards);
			selection.CharLeft();

			// Prevent further handling
			e.Handled = true;
		}
		#endregion
	}
}