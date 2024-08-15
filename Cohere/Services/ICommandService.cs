namespace Cohere.Services
{
    public interface ICommandService
    {
        CompositeCommand OpenItemCommand { get; }
        CompositeCommand LoadProductCommand { get; }
        CompositeCommand RefreshErrorCount { get; }
        CompositeCommand CreateRuleCommand { get; }
        CompositeCommand RefreshListCommand { get; }
    }
}
