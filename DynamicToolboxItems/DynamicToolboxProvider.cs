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

namespace MonoDevelop.Samples.DynamicToolboxItems
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

            if (consumer is ViewContent vc)
            {
                var editor = EditorWorkarounds.GetTextEditor(vc);
                if (editor == null)
                {
                    yield break;
                }

                // here you'd typically return a whole set of nodes.
                // you'd typically cache them too.

                // The XAML previewer regressed support for normal text nodes, they are displayed
                // but do not work. See devdiv#723997.
                // Work around this for now using the custom node type.
                if (EditorWorkarounds.IsXamlPreviewer(vc))
                {
                    yield return new Xamarin.Designer.Forms.FormsTextToolboxNode("My Custom XAML Node")
                    {
                        Category = "My Custom Category",
                        Name = "My Custom XAML Node",
                        Icon = ImageService.GetIcon(Stock.TextFileIcon, Gtk.IconSize.Menu)
                    };
                }
                else
                {
                    yield return new MyToolboxNode();
                }
            }
        }
    }

    // Toolbox nodes must subclass ItemToolboxNode. Nodes to be consumed by in
    // the editor must implement ITextToolboxNode. There is a TextToolboxNode
	// but it doesn't let us customize everything.
    public class MyToolboxNode : ItemToolboxNode, ITextToolboxNode
    {
        public MyToolboxNode()
        {
            // prior to 7.5, the IDE crashes unless we set an icon with an explicit size
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
            var editor = EditorWorkarounds.GetActiveTextEditor(document);
            editor.InsertAtCaret("This is the inserted text");
        }

        public bool IsCompatibleWith(Document document)
        {
            var editor = EditorWorkarounds.GetActiveTextEditor(document);
            return editor.MimeType == "application/xaml";
        }
    }

    // for the standard source editor, ActiveView.GetContent<TextEditor> returns nothing
    // but for the XAML previewer, document.TextEditor is not wired up
    // try to paper over this inconsistency
    static class EditorWorkarounds
    {
        /// <summary>
        /// Gets the TextEditor for the active ViewContent.
        /// </summary>
        public static TextEditor GetActiveTextEditor(Document document)
        {
            return document.ActiveView.GetContent<TextEditor>() ?? document.Editor;
        }

        /// <summary>
        /// Gets the TextEditor for the ViewContent.
        /// </summary>
        public static TextEditor GetTextEditor(ViewContent vc)
        {
            // We need to check the active view we call GetContent, or we
            // might get a chained value from another, non-visible view, which
            // is usually desired but not in this case.
            // 
            //if it's a text editor it should implement ITextFile
            //however, the XAML previewer currently does not, so special case it
            if (vc is ITextFile || IsXamlPreviewer(vc))
            {
                return GetContentWithWorkarounds<TextEditor>(vc);
            }
            return null;
        }

        public static bool IsXamlPreviewer (ViewContent vc)
        {
            return vc.GetType().FullName == "Xamarin.Designer.Forms.XamarinStudioXamlPreviewerView";
        }

        // work around issues on GetContent on SourceEditorView
        static T GetContentWithWorkarounds<T>(ViewContent vc) where T : class
        {
            // https://github.com/mono/monodevelop/issues/3559
            // GetContent<TextEditor> on a SourceEditorView returns null, need
            // to go up to the document via the window
            var content = vc?.WorkbenchWindow?.Document?.GetContent<T>();
            if (content != null)
            {
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
                if (editorImplType.IsAssignableFrom(vc.GetType()))
                {
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
}
