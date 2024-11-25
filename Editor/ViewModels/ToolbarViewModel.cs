using Editor.Services;
using System.Drawing.Printing;
using System.Windows.Data;

namespace Editor.ViewModels
{
    public class ToolbarViewModel(ICommandService commandService) : BindableBase
    {
        public ICommandService CommandService => commandService;
        public static ListCollectionView Printers { get; set; } = new(GetPrinters());

        private static List<string> GetPrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
        }
    }
}