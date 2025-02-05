using System.Drawing.Printing;
using System.Windows.Data;

using Editor.Services;

namespace Editor.ViewModels;

public class ToolbarViewModel(ICommandService commandService) : BindableBase
{
    public ICommandService CommandService => commandService;
    public static ListCollectionView Printers { get; set; } = new(GetPrinters());

    private static List<string> GetPrinters()
    {
        return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
    }

    private string _printer = new PrinterSettings().PrinterName;

    public string Printer
    {
        get => _printer;
        set => SetProperty(ref _printer, value);
    }
}