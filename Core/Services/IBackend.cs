using Core.Database.IdeDbModels;
using Core.Services.BackendModel;

namespace Core.Services
{
    public interface IBackend
    {
        public Dictionary<string, string> GetValues();

        public Dictionary<Product, Dictionary<string, string?>> GetValues(List<Product> products, List<RuleAttributes> attributes);

        public List<string> GetAttributes();

        public List<Product> GetProducts(string label);
    }
}
