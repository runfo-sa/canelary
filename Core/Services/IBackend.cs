using Core.Database.IdeDbModels;
using Core.Services.BackendModel;

namespace Core.Services
{
    public interface IBackend
    {
        public List<KeyValuePair<string, string?>> GetValues(int id);

        public List<KeyValuePair<Product, List<KeyValuePair<string, string?>>>> GetValues(List<Product> products, List<RuleAttributes> attributes);

        public List<string> GetAttributes();

        public List<Product> GetProducts(string label);

        public string GetTranslation(int languageId, string description);
    }
}
