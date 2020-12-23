using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MonoDevelop.Samples.MainMenuCommand
{
    class ShowDateCommandHandler : CommandHandler
    {
        protected override void Run()
        {
            MessageService.ShowMessage($"The date is {DateTime.Now}");
        }

        protected override void Update(CommandInfo info)
        {
            info.Enabled = true;
        }
    }
}