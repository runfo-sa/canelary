using Core.Database.IdeDbModels;
using Core.Services.BackendModel;
using System.Text;

namespace Core.Services
{
    public interface IBackend
    {
        public List<KeyValuePair<string, string?>> GetValues(int id);

        public List<KeyValuePair<Product, List<KeyValuePair<string, string?>>>> GetValues(List<Product> products, List<RuleAttributes> attributes);

        public List<string> GetAttributes();

        public List<Product> GetProducts(string label);

        public string GetTranslation(int languageId, string description);

        public string ParseVariable(string key, ref List<KeyValuePair<string, string?>> dictionary);

        public string LoadVariables(string content, int id, ref StringBuilder error);
    }
}
