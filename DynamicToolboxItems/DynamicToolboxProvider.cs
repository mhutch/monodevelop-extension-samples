// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MonoDevelop.DesignerSupport.Toolbox;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects.Text;

namespace DynamicToolboxItems
{
    //NOTE: this is registered in the addin manifest
    public class MyDynamicToolboxProvider : IToolboxDynamicProvider
    {
        #pragma warning disable 67
        public event EventHandler ItemsChanged;
        #pragma warning restore 67

        // dynamically fetch or generate items for a specific file
        // this is called every time the active document changes
        public IEnumerable<ItemToolboxNode> GetDynamicItems(IToolboxConsumer consumer)
        {
            // if it's a text editor view, it should be a subclass of ViewContent
            // and implement ITextFile
            if (consumer is ViewContent vc && vc is ITextFile)
            {
                var editor = GetContentWithWorkarounds<TextEditor>(vc);

                // the toolbox crashes in XAML files, see devdiv#548996
                if (editor == null || editor.MimeType == "application/xaml+xml")
                {
                    yield break;
                }

                // here you'd typically return a whole set of nodes.
                // you'd typically cache them too.
                yield return new MyToolboxNode();
            }
        }


        // work around issues on GetContent on SourceEditorView
        T GetContentWithWorkarounds<T> (ViewContent vc) where T : class
        {
            // https://github.com/mono/monodevelop/issues/3559
            // GetContent<TextEditor> on a SourceEditorView returns null, need
            // to go up to the document via the window
            var content = vc?.WorkbenchWindow?.Document?.GetContent<T>();
            if (content != null) {
                return content;
            }

            // WorkbenchWindow on the SourceViewContent in the XAML editor is null (devdiv#548308)
            // it's re-hosted inside the previewer's splitter view, which doesn't
            // wire up the child correctly
            if (vc.WorkbenchWindow == null)
            {
                // if it's SourceEditorView we can get the Document from the
                // EditorExtension property
                var editorImplType = typeof(TextEditor).Assembly.GetType("MonoDevelop.Ide.Editor.ITextEditorImpl");
                if (editorImplType.IsAssignableFrom(vc.GetType())) {
                    var prop = editorImplType.GetProperty("EditorExtension");
                    if (prop != null)
                    {
                        var ext = prop.GetValue(vc) as TextEditorExtension;
                        return ext?.DocumentContext?.GetContent<T>();
                    }
                }
            }
            return null;
        }
    }

    // Toolbox nodes must subclass ItemToolboxNode. Nodes to be consumed by in
    // the editor must implement ITextToolboxNode. There is a TextToolboxNode
	// but it doesn't let us customize everything.
    public class MyToolboxNode : ItemToolboxNode, ITextToolboxNode
    {
        public MyToolboxNode()
        {
            // the IDE crashes unless we set an icon with an explicit size
            // https://github.com/mono/monodevelop/issues/3590
            Icon = ImageService.GetIcon(Stock.TextFileIcon, Gtk.IconSize.Menu);

            // you won't get an error if these are empty/null
            // see https://github.com/mono/monodevelop/issues/3604
            Category = "My Dynamic Section";
            Description = "Toolbox description for tooltip";
            Name = "My Dynamic Item";
        }

        public string GetDragPreview(Document document)
        {
            return "This is the preview shown when dragging";
        }

        public void InsertAtCaret(Document document)
        {
            // you could actually do more complex things here, like adding
            // references to the project, or performing refactorings
            document.Editor.InsertAtCaret("This is the inserted text");
        }

        public bool IsCompatibleWith(Document document)
        {
            return document.Editor.MimeType == "application/xaml";
        }
    }
}
