using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MainMenuCommand
{
    class InsertDateHandler : CommandHandler
    {
        protected override void Run()
        {
            var editor = IdeApp.Workbench.ActiveDocument.Editor;
            editor.InsertAtCaret(DateTime.Now.ToString());
        }

        protected override void Update(CommandInfo info)
        {
            info.Enabled = IdeApp.Workbench.ActiveDocument?.Editor != null;
        }
    }

    public enum DateInserterCommands
    {
        InsertDate,
    }
}