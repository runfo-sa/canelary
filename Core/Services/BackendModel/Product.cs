using Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Services.BackendModel
{
    public class Product : BindableBase
    {
        public Int16 Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Senasa { get; set; } = string.Empty;

        [NotMapped]
        public ProductError Error { get; set; } = ProductError.None;

        [NotMapped]
        public List<ProductReport> Attributes { get; set; } = [];
    }
}
