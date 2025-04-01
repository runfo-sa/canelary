namespace Editor.Services;

public class CommandsService : ICommandService
{
    private readonly CompositeCommand _newCommand = new();
    public CompositeCommand NewCommand => _newCommand;

    private readonly CompositeCommand _openCommand = new();
    public CompositeCommand OpenCommand => _openCommand;

    private readonly CompositeCommand _openItem = new();
    public CompositeCommand OpenItemCommand => _openItem;

    private readonly CompositeCommand _saveCommand = new();
    public CompositeCommand SaveCommand => _saveCommand;

    private readonly CompositeCommand _saveAsCommand = new();
    public CompositeCommand SaveAsCommand => _saveAsCommand;

    private readonly CompositeCommand _saveAllCommand = new();
    public CompositeCommand SaveAllCommand => _saveAllCommand;

    private readonly CompositeCommand _closeCommand = new();
    public CompositeCommand CloseCommand => _closeCommand;

    private readonly CompositeCommand _closeAllCommand = new();
    public CompositeCommand CloseAllCommand => _closeAllCommand;

    private readonly CompositeCommand _switchPosCommand = new();
    public CompositeCommand SwitchPosCommand => _switchPosCommand;

    private readonly CompositeCommand _switchLinterCommand = new();
    public CompositeCommand SwitchLinterCommand => _switchLinterCommand;

    private readonly CompositeCommand _previewCommand = new();
    public CompositeCommand PreviewCommand => _previewCommand;

    private readonly CompositeCommand _printCommand = new();
    public CompositeCommand PrintCommand => _printCommand;

    private readonly CompositeCommand _resizeCommand = new();
    public CompositeCommand ResizeCommand => _resizeCommand;

    private readonly CompositeCommand _reloadTree = new();
    public CompositeCommand ReloadTree => _reloadTree;

    private readonly CompositeCommand _showErrors = new();
    public CompositeCommand ShowErrorsCommand => _showErrors;
}