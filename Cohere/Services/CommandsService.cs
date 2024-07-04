namespace Cohere.Services
{
    public class CommandsService : ICommandService
    {
        private readonly CompositeCommand _openItem = new();
        public CompositeCommand OpenItemCommand => _openItem;

        private readonly CompositeCommand _loadProduct = new();
        public CompositeCommand LoadProductCommand => _loadProduct;

        private readonly CompositeCommand _errorCount = new();
        public CompositeCommand RefreshErrorCount => _errorCount;

        private readonly CompositeCommand _createRule = new();
        public CompositeCommand CreateRuleCommand => _createRule;
    }
}
