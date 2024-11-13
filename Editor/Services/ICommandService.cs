namespace Editor.Services
{
    public interface ICommandService
    {
        CompositeCommand NewCommand { get; }
        CompositeCommand OpenCommand { get; }
        CompositeCommand OpenItemCommand { get; }
        CompositeCommand SaveCommand { get; }
        CompositeCommand SaveAsCommand { get; }
        CompositeCommand SaveAllCommand { get; }
        CompositeCommand CloseCommand { get; }
        CompositeCommand CloseAllCommand { get; }
        CompositeCommand SwitchPosCommand { get; }
        CompositeCommand SwitchLinterCommand { get; }
        CompositeCommand PreviewCommand { get; }
        CompositeCommand PrintCommand { get; }
        CompositeCommand ResizeCommand { get; }
        CompositeCommand ReloadTree { get; }
        CompositeCommand ShowErrorsCommand { get; }
    }
}