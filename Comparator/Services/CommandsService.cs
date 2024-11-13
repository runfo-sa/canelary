namespace Comparator.Services
{
    public class CommandsService : ICommandService
    {
        private readonly CompositeCommand _generateDiff = new();
        public CompositeCommand GenerateDiff => _generateDiff;

        private readonly CompositeCommand _changeFiles = new();
        public CompositeCommand ChangeFiles => _changeFiles;

        private readonly CompositeCommand _refresh = new();
        public CompositeCommand Refresh => _refresh;
    }
}