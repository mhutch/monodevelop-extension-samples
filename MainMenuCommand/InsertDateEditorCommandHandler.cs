using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace MonoDevelop.Samples.MainMenuCommand
{
    // this class uses the MEF-based VS Editor commanding API, so it could
    // be used as-is on Windows. however, in both cases the args class must be registered
    // and mapped to shell command, and *that* is done differently on Mac and Windows.
    [Name("InsertDateCommandHandler")]
    [ContentType(StandardContentTypeNames.Any)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [Export(typeof(ICommandHandler))]
    class InsertDateEditorCommandHandler : ICommandHandler<InsertDateCommandArgs>
    {
        [Import]
        public IEditorOperationsFactoryService OperationsFactory { get; set; }

        public string DisplayName => "Insert Date Command Handler";

        public bool ExecuteCommand(InsertDateCommandArgs args, CommandExecutionContext executionContext)
        {
            var operations = OperationsFactory.GetEditorOperations(args.TextView);
            operations.InsertText(DateTime.Now.ToString());
            return true;
        }

        public CommandState GetCommandState(InsertDateCommandArgs args) => CommandState.Available;
    }

    class InsertDateCommandArgs : EditorCommandArgs
    {
        public InsertDateCommandArgs(ITextView textView, ITextBuffer subjectBuffer) : base(textView, subjectBuffer)
        {
        }
    }
}