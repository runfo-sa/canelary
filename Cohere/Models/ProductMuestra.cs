using Core.Services.BackendModel;

namespace Cohere.Models
{
    public class ProductoMuestra : Product
    {
        private bool _printable;
        public bool Printable
        {
            get => _printable;
            set => SetProperty(ref _printable, value);
        }

        public ProductoMuestra(Product product, bool printable = false)
        {
            Code = product.Code;
            Name = product.Name;
            Senasa = product.Senasa;
            Printable = printable;
        }
    }
}
