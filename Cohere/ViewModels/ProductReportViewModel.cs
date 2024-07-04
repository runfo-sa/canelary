using Cohere.Services;
using Core.Database.Model;
using System.Collections.ObjectModel;

namespace Cohere.ViewModels
{
    public class ProductReportViewModel : BindableBase
    {
        public ObservableCollection<Valor> SelectedValues { get; set; } = [];

        public ProductReportViewModel(ICommandService commandService)
        {
            commandService.LoadProductCommand.RegisterCommand(new DelegateCommand<object?>(ProcessProduct));
        }

        private void ProcessProduct(object? item)
        {
            if (item is not null && item is ListarProductos prod)
            {
                SelectedValues.Clear();
                foreach (var valor in prod.Valores)
                {
                    SelectedValues.Add(valor);
                }
            }
        }
    }
}
