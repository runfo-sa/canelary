using System.Diagnostics;

namespace Cohere.ViewModels
{
    public class MenuViewModel(IDialogService dialogService) : BindableBase
    {
        public DelegateCommand HelpCommand => new(() => Process.Start(new ProcessStartInfo(".\\Manual\\index.html") { UseShellExecute = true }));

        public DelegateCommand AboutCommand => new(() => dialogService.ShowDialog("About"));

        public DelegateCommand CreateRuleCommand => new(() => dialogService.ShowDialog("CreateRuleDialog"));

        public DelegateCommand AlterRuleCommand => new(() => dialogService.ShowDialog("AlterRuleDialog"));
    }
}
