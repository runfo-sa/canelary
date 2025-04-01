using System.Diagnostics;

using Editor.Services;

namespace Editor.ViewModels;

public class MenuViewModel(ICommandService commandService, IDialogService dialogService) : BindableBase
{
    public ICommandService CommandService => commandService;

    public DelegateCommand OpenSettingsCommand => new(() => dialogService.Show("Settings"));
    public DelegateCommand HelpCommand => new(() => Process.Start(new ProcessStartInfo(".\\Manual\\index.html") { UseShellExecute = true }));
    public DelegateCommand AboutCommand => new(() => dialogService.Show("About"));
}