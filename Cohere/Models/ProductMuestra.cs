using Core.Services.BackendModel;

namespace Cohere.Models;

public class ProductoMuestra : Product
{
    private bool _printable;

    public bool Printable
    {
        get => _printable;
        set => SetProperty(ref _printable, value);
    }

    private bool _enable;

    public bool Enable
    {
        get => _enable;
        set => SetProperty(ref _enable, value);
    }

    public ProductoMuestra(Product product, bool printable = false)
    {
        Id = product.Id;
        Code = product.Code;
        Name = product.Name;
        Senasa = product.Senasa;
        Error = product.Error;
        Printable = printable;

        Enable = Error switch
        {
            Core.Models.ProductError.Incomplete or Core.Models.ProductError.Incoherent => false,
            _ => true,
        };
    }
}