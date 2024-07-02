namespace Comparator.Services
{
    public interface ICommandService
    {
        CompositeCommand GenerateDiff { get; }
        CompositeCommand ChangeFiles { get; }
        CompositeCommand Refresh { get; }
    }
}
