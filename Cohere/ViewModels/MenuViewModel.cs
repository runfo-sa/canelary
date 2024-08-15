using Cohere.Services;
using Core.Database;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace Cohere.ViewModels
{
    public class MenuViewModel(IDialogService dialogService, ICommandService commandService) : BindableBase
    {
        public DelegateCommand HelpCommand => new(() => Process.Start(new ProcessStartInfo(".\\Manual\\index.html") { UseShellExecute = true }));

        public DelegateCommand AboutCommand => new(() => dialogService.Show("About"));

        public DelegateCommand CreateRuleCommand => new(() => dialogService.Show("CreateRuleDialog"));

        public DelegateCommand AlterRuleCommand => new(() => dialogService.Show("AlterRuleDialog", r =>
        {
            if (r.Result == ButtonResult.OK)
            {
                commandService.RefreshListCommand.Execute(null);
            }
        }), () =>
        {
            using var context = new IdeDbContext();
            return !context.Rule.IsNullOrEmpty();
        });
    }
}
