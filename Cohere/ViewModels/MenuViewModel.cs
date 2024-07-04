using Cohere.Models;
using Cohere.Services;
using System.Diagnostics;

namespace Cohere.ViewModels
{
    public class MenuViewModel(IDialogService dialogService, ICommandService commandService) : BindableBase
    {
        public DelegateCommand HelpCommand => new(() => Process.Start(new ProcessStartInfo(".\\Manual\\index.html") { UseShellExecute = true }));

        public DelegateCommand AboutCommand => new(() => dialogService.ShowDialog("About"));

        public DelegateCommand CreateRuleCommand => new(() => dialogService.ShowDialog("CreateRuleDialog", result =>
        {
            if (result.Result != ButtonResult.OK)
            {
                return;
            }

            var rc = (CreateRuleResult)result.Parameters["Result"];
            commandService.CreateRuleCommand.Execute(rc);
        }));
    }
}
