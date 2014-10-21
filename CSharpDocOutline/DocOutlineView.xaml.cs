using DavidSpeck.CSharpDocOutline.CDM;
using EnvDTE;
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

        public DocOutlineView()
        {
            InitializeComponent();
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
            //                "Document Outline CSharp");
            Debug.WriteLine("The buttons was clicked");
        }

        public void OutlineDocument(CodeDocumentModel cdm)
        {
            this.content.Text = cdm.ToString();
        }
    }
}