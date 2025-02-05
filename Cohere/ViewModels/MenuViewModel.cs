using System.Diagnostics;

using Cohere.Services;

using Core;
using Core.Database;

using Microsoft.IdentityModel.Tokens;

namespace Cohere.ViewModels
{
    public class MenuViewModel(IDialogService dialogService, ICommandService commandService) : BindableBase
    {
        public ICommandService CommandService => commandService;
        public DelegateCommand HelpCommand => new(() => Process.Start(new ProcessStartInfo(Globals.DOCS_URL) { UseShellExecute = true }));

        public DelegateCommand AboutCommand => new(() => dialogService.Show("About"));

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