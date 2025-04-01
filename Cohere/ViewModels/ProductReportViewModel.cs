using System.Collections.ObjectModel;

using Cohere.Services;

using Core.Models;
using Core.Services.BackendModel;

namespace Cohere.ViewModels;

public class ProductReportViewModel : BindableBase
{
    public ObservableCollection<ProductReport> SelectedValues { get; set; } = [];

    public ProductReportViewModel(ICommandService commandService)
    {
        commandService.LoadProductCommand.RegisterCommand(new DelegateCommand<object?>(ProcessProduct));
    }

    private void ProcessProduct(object? item)
    {
        if (item is Product prod)
        {
            SelectedValues.Clear();
            foreach (var value in prod.Attributes)
            {
                SelectedValues.Add(value);
            }
        }
    }
}