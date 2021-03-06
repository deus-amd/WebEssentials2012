﻿using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace MadsKristensen.EditorExtensions
{
    [Export(typeof(IDropHandlerProvider))]
    [DropFormat("CF_VSSTGPROJECTITEMS")]
    [Name("BundleDropDropHandler")]
    [ContentType("XML")]
    [Order(Before = "DefaultFileDropHandler")]
    internal class BundleDropHandlerProvider : IDropHandlerProvider
    {
        public IDropHandler GetAssociatedDropHandler(IWpfTextView view)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new BundleDropHandler(view));
        }
    }

    internal class BundleDropHandler : IDropHandler
    {
        private readonly IWpfTextView _view;
        private readonly List<string> _allowedExtensions = new List<string> { ".css", ".less", ".js", ".coffee" };
        private string _draggedFilename;
        private readonly string _format = Environment.NewLine + "\t<file>/{0}</file>";

        public BundleDropHandler(IWpfTextView view)
        {
            this._view = view;
        }

        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo)
        {
            string reference = FileHelpers.RelativePath(ProjectHelpers.GetRootFolder(), _draggedFilename);

            if (reference.StartsWith("http://localhost:"))
            {
                int index = reference.IndexOf('/', 20);
                if (index > -1)
                    reference = reference.Substring(index + 1).ToLowerInvariant();
            }

            _view.TextBuffer.Insert(dragDropInfo.VirtualBufferPosition.Position.Position, string.Format(_format, reference));

            return DragDropPointerEffects.Copy;
        }

        public void HandleDragCanceled()
        {

        }

        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public bool IsDropEnabled(DragDropInfo dragDropInfo)
        {
            _draggedFilename = FontDropHandler.GetImageFilename(dragDropInfo);

            if (!string.IsNullOrEmpty(_draggedFilename))
            {
                string fileExtension = Path.GetExtension(_draggedFilename).ToLowerInvariant();
                if (_allowedExtensions.Contains(fileExtension))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
