using Cohere.Models;
using Core.Database;
using Core.FileTree;
using Core.Helpers;
using Core.Services;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Data;

namespace Cohere.ViewModels
{
    public class GenerateSampleViewModel : BindableBase, IDialogAware
    {
        public static string Title => "Generar Muestra";

        private readonly IDialogService _dialogService;
        private IFile _labelFile = null!;

        private readonly ListCollectionView _printers = new(PrinterSettings.InstalledPrinters.Cast<string>().ToList());
        public ListCollectionView Printers => _printers;
        public ObservableCollection<ProductoMuestra> ProductsList { get; set; } = [];

        private bool _selectAll;
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                SetProperty(ref _selectAll, value);
                SelectedAll();
            }
        }

        private bool _enableRecall = false;
        public bool EnableRecall
        {
            get => _enableRecall;
            set => SetProperty(ref _enableRecall, value);
        }

        public DelegateCommand CloseDialogCommand { get; private set; }

        public DialogCloseListener RequestClose { get; }

        public GenerateSampleViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            CloseDialogCommand = new(ClosingDialog);
        }

        private void SelectedAll()
        {
            foreach (var item in ProductsList)
            {
                item.Printable = SelectAll;
            }
        }

        private void PrintLabels(IEnumerable<ProductoMuestra> products)
        {
            foreach (var prod in products)
            {
                if (prod.Printable)
                {
                    var label = PreviewServiceProvider
                        .ProvideService(_labelFile.Read())
                        .LoadVariables();
                    PrinterHelper.SendStringToPrinter(Printers.CurrentItem.ToString()!, label.Content, $"{_labelFile.Name} - {prod.Name}");
                }
            }
        }

        private void GenerateRecall(IEnumerable<ProductoMuestra> products)
        {
            var param = new DialogParameters
            {
                { "Products", products }
            };
            _dialogService.ShowDialog("GenerateRecallDialog", param);
        }

        private void ClosingDialog()
        {
            if (EnableRecall)
            {
                GenerateRecall(ProductsList.Where(p => p.Printable));
            }
            PrintLabels(ProductsList);
            RequestClose.Invoke();
        }

        public Boolean CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("File", out IFile? file) && file is not null)
            {
                _labelFile = file;

                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(_labelFile.Name);
                    var list = BackendServiceProvider.Backend
                        .GetProducts(labelName)
                        .Select(p => new ProductoMuestra(p));

                    foreach (var item in list)
                    {
                        ProductsList.Add(item);
                    }
                }
            }
        }
    }
}
