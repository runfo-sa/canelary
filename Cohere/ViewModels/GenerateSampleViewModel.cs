using Cohere.Models;
using Core.Database;
using Core.Database.Model;
using Core.FileTree;
using Core.Helpers;
using Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Data;

namespace Cohere.ViewModels
{
    public class GenerateSampleViewModel : BindableBase, IDialogAware
    {
        public string Title => "Generar Muestra";

        private readonly IDialogService _dialogService;
        private LabelFile _labelFile = null!;

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
                item.Muestra = SelectAll;
            }
        }

        private void PrintLabels(IEnumerable<ProductoMuestra> products)
        {
            foreach (var prod in products)
            {
                if (prod.Muestra)
                {
                    var label = PreviewServiceProvider
                        .ProvideService(File.ReadAllText(_labelFile.Path))
                        .FillProduct(prod.Codigo)
                        .FillTestVariables();
                    PrinterHelper.SendStringToPrinter(Printers.CurrentItem.ToString()!, label.Content, $"{_labelFile.Name} - {prod.Nombre}");
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
                GenerateRecall(ProductsList.Where(p => p.Muestra));
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
            if (parameters.TryGetValue("File", out LabelFile? file) && file is not null)
            {
                _labelFile = file;

                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(_labelFile.Name);
                    var param = new SqlParameter("@Etiqueta", labelName);
                    var list = context.Database
                        .SqlQueryRaw<ListarProductos>("ide.ListarProductos @Etiqueta", param)
                        .AsEnumerable()
                        .Select(p => new ProductoMuestra(p.Codigo, p.Nombre, p.Senasa, false));

                    foreach (var item in list)
                    {
                        ProductsList.Add(item);
                    }
                }
            }
        }
    }
}
