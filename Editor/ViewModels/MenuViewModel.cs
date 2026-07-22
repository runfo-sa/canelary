using System.Diagnostics;

using Editor.Services;

namespace Editor.ViewModels;

public class MenuViewModel : BindableBase
{
    public ICommandService CommandService { get; }
    public DelegateCommand OpenSettingsCommand { get; }
    public DelegateCommand HelpCommand { get; }
    public DelegateCommand AboutCommand { get; }

    public MenuViewModel(ICommandService commandService, IDialogService dialogService)
    {
        CommandService = commandService;
        OpenSettingsCommand = new(() => dialogService.Show("Settings"));
        HelpCommand = new(() =>
        {
            using var proc = Process.Start(new ProcessStartInfo(".\\Manual\\index.html") { UseShellExecute = true });
        });
        AboutCommand = new(() => dialogService.Show("About"));
    }
}