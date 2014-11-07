using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using System.Diagnostics;
using DavidSpeck.CSharpDocOutline.CDM;
using System.IO;

namespace DavidSpeck.CSharpDocOutline
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("bc1bea3f-26f8-4e72-b3c6-087cd5b46332")]
    public class DocumentOutlineWindow : ToolWindowPane
    {
        CDMParser m_parser = new CDMParser();
		DocOutlineView m_docOutline;

        Events m_events;
        DocumentEvents m_docEvents;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public DocumentOutlineWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            m_docOutline = new DocOutlineView();
            base.Content = m_docOutline;
        }

        protected override void Initialize()
        {
            base.Initialize();
            ConnectEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisconnectEvents();
        }

        #region Connect events
        private void ConnectEvents()
        {
            DTE2 dte = GetService(typeof(DTE)) as DTE2;

            // Keeping this references is required to prevent these objects from
            // beeing garbage collected.
            m_events = dte.Events;
            m_docEvents = m_events.DocumentEvents;

            m_events.WindowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(OnWindowActivated);
            m_docEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(OnDocumentSaved);
        }

        

        private void DisconnectEvents()
        {
            DTE2 dte = GetService(typeof(DTE)) as DTE2;
            if (dte != null)
            {
                dte.Events.WindowEvents.WindowActivated -= new _dispWindowEvents_WindowActivatedEventHandler(OnWindowActivated);
                dte.Events.DocumentEvents.DocumentSaved -= new _dispDocumentEvents_DocumentSavedEventHandler(OnDocumentSaved);
            }

			// Free references for garbage collection.
            m_events = null;
            m_docEvents = null;
        }
        #endregion

        #region Event handler
		/// <summary>
		/// If another document got active that currently outlined generate outline for this new document.
		/// </summary>
        private void OnWindowActivated(EnvDTE.Window GotFocus, EnvDTE.Window LostFocus)
        {
            if (GotFocus.Document != null)
            {
                var doc = GotFocus.Document;
				// Cancle re-parsing if the document hasn't changed
				if (m_docOutline.CurrentDocument != null && doc.FullName == m_docOutline.CurrentDocument.FullName)
					return;

                OutlineDocument(doc);
            }
        }

		/// <summary>
		/// Regenerate outline when a document is saved.
		/// </summary>
        private void OnDocumentSaved(Document Document)
        {
            OutlineDocument(Document);
        }
        #endregion

		/// <summary>
		/// Use parser to create a CodeDocumentModel from the given document and hand it to the view
		/// to display the outline.
		/// </summary>
        private void OutlineDocument(Document document)
        {
            var reader = new StreamReader(document.Path + document.Name);
            var cdm = new CodeDocumentModel();
            cdm.DocumentName = document.Name;
            cdm.FullPath = document.Path;
            m_parser.Init();
            m_parser.Parse(reader, ref cdm);

            m_docOutline.OutlineDocument(cdm, document);
        }
    }
}
