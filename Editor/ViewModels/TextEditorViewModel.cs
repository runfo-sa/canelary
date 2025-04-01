using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Core.FileTree;
using Core.Helpers;
using Core.Models;
using Core.Services;

using Editor.Models;
using Editor.Services;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;

using TabItem = Editor.Models.TabItem;

namespace Editor.ViewModels;

public class TextEditorViewModel : BindableBase
{
    public ObservableCollection<TabItem> TabsList { get; set; } = [];

    private int _currentTabIndex;

    public int CurrentTabIndex
    {
        get => _currentTabIndex;
        set
        {
            SetProperty(ref _currentTabIndex, value);
            _previewCommand.RaiseCanExecuteChanged();
            _printCommand.RaiseCanExecuteChanged();
            _resizeCommand.RaiseCanExecuteChanged();
            _closeAllCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _enableErrorMsg = true;
    private Visibility _errorWindow = Visibility.Hidden;

    public Visibility ShowErrorWindow
    {
        get => _errorWindow;
        set
        {
            if (_enableErrorMsg)
            {
                SetProperty(ref _errorWindow, value);
            }
        }
    }

    private string? _errorsMessage = null;

    public string? ErrorsMessage
    {
        get => _errorsMessage;
        set => SetProperty(ref _errorsMessage, value);
    }

    private bool _previewOnSave = false;

    public bool PreviewOnSave
    {
        get => _previewOnSave;
        set => SetProperty(ref _previewOnSave, value);
    }

    private bool _enableLinting = true;

    public bool EnableLinting
    {
        get => _enableLinting;
        set
        {
            SetProperty(ref _enableLinting, value);
            _refreshLinter.Execute();
        }
    }

    public ICommandService CommandService { get; }
    public IEditorPreviewMediator Mediator { get; }

    public DelegateCommand CloseErrorWindowCommand { get; }

    public DelegateCommand<MouseButtonEventArgs> CloseMiddleClickCommand { get; private set; }

    private readonly DelegateCommand _saveCommand;
    private readonly DelegateCommand _previewCommand;
    private readonly DelegateCommand _printCommand;
    private readonly DelegateCommand _refreshLinter;
    private readonly DelegateCommand _resizeCommand;
    private readonly DelegateCommand _closeAllCommand;
    private readonly IDialogService _dialogService;

    public TextEditorViewModel(ICommandService commandService, IEditorPreviewMediator mediator, IDialogService dialogService)
    {
        Mediator = mediator;
        _dialogService = dialogService;
        CommandService = commandService;
        CloseMiddleClickCommand = new(CloseMiddleClick);

        CommandService.OpenItemCommand.RegisterCommand(new DelegateCommand<object?>(OpenCurrentItem));
        CommandService.NewCommand.RegisterCommand(new DelegateCommand(() => AddTab($"new {NextNewItem()}", "^XA\r\n\r\n^XZ")));
        CommandService.CloseCommand.RegisterCommand(new DelegateCommand<TabItem>(CloseItem));
        CommandService.OpenCommand.RegisterCommand(new DelegateCommand(OpenFile));
        CommandService.SaveAsCommand.RegisterCommand(new DelegateCommand(SaveAsFile));
        CommandService.SaveAllCommand.RegisterCommand(new DelegateCommand(SaveAllFile));
        CommandService.SwitchPosCommand.RegisterCommand(new DelegateCommand(() => PreviewOnSave = !PreviewOnSave));
        CommandService.SwitchLinterCommand.RegisterCommand(new DelegateCommand(() => EnableLinting = !EnableLinting));
        CommandService.ShowErrorsCommand.RegisterCommand(new DelegateCommand(() => _enableErrorMsg = !_enableErrorMsg));

        _previewCommand = new DelegateCommand(SendToPreview, () => 0 <= CurrentTabIndex && CurrentTabIndex < TabsList.Count);
        CommandService.PreviewCommand.RegisterCommand(_previewCommand);

        _saveCommand = new DelegateCommand(SaveFile, () => TabsList.Count > 0 && TabsList[CurrentTabIndex].HasUnsavedChanges);
        CommandService.SaveCommand.RegisterCommand(_saveCommand);

        _printCommand = new DelegateCommand(Print, () => 0 <= CurrentTabIndex && CurrentTabIndex < TabsList.Count);
        CommandService.PrintCommand.RegisterCommand(_printCommand);

        _resizeCommand = new DelegateCommand(ResizeFile, () => 0 <= CurrentTabIndex && CurrentTabIndex < TabsList.Count);
        CommandService.ResizeCommand.RegisterCommand(_resizeCommand);

        _closeAllCommand = new DelegateCommand(CloseAll, () => TabsList.Count > 0);
        CommandService.CloseAllCommand.RegisterCommand(_closeAllCommand);

        Mediator.SendErrors.RegisterCommand(new DelegateCommand<string>(ShowErrors));
        Mediator.GenerateLinter.RegisterCommand(new DelegateCommand<string>(async e => await UpdateLinter(e)));

        CloseErrorWindowCommand = new DelegateCommand(() =>
        {
            ShowErrorWindow = Visibility.Hidden;
            ErrorsMessage = string.Empty;
        });

        _refreshLinter = new DelegateCommand(() =>
        {
            if (EnableLinting)
            {
                Mediator.SendData.Execute(null);
            }
            else
            {
                foreach (var item in TabsList)
                {
                    item.ClearLinting();
                }
            }
        });
    }

    private void AddTab(string header, string content, string? path = null)
    {
        var item = new TabItem(header, content, path);
        item.WasModified += UpdateItemSaveState;
        TabsList.Add(item);
        CurrentTabIndex = TabsList.Count - 1;

        if (EnableLinting)
        {
            Mediator.SendData.Execute(null);
        }
    }

    private int NextNewItem()
    {
        var lastNew = TabsList.LastOrDefault(item => item.Header.StartsWith("new ") && item.Path is null);
        if (lastNew is not null && int.TryParse(lastNew.Header[4..], out int idx))
        {
            return idx + 1;
        }

        return 1;
    }

    private void OpenFile()
    {
        var filters = new StringBuilder();

        foreach (var ext in SettingsService.Instance.Extension)
        {
            filters.Append($"ZPL File (*.{ext})|*.{ext}|");
        }
        filters.Append("Todos los archivos (*.*)|*.*");

        OpenFileDialog dialog = new()
        {
            Filter = filters.ToString()
        };

        if (dialog.ShowDialog() == true)
        {
            var content = File.ReadAllText(dialog.FileName);
            AddTab(dialog.SafeFileName, content, dialog.FileName);
        }
    }

    private void OpenCurrentItem(object? item)
    {
        if (item is IFile file)
        {
            var tabExists = TabsList.FirstOrDefault(item => item.Header == file.Name);
            if (tabExists is not null)
            {
                CurrentTabIndex = TabsList.IndexOf(tabExists);
            }
            else
            {
                var content = file.Read();
                AddTab(file.Name, content, file.Path);
            }
        }
    }

    private void UpdateItemSaveState(TabItem item)
    {
        _saveCommand.RaiseCanExecuteChanged();
    }

    private void SaveFile()
    {
        if (PreviewOnSave)
        {
            CommandService.PreviewCommand.Execute(null);
        }

        if (TabsList[CurrentTabIndex].SaveItem())
        {
            CommandService.ReloadTree.Execute(null);
        }
    }

    private void SaveAsFile()
    {
        if (0 <= CurrentTabIndex && CurrentTabIndex < TabsList.Count)
        {
            var item = TabsList[CurrentTabIndex];
            item.Path = null;
            if (item.SaveItem())
            {
                CommandService.ReloadTree.Execute(null);
            }
        }
    }

    private void SaveAllFile()
    {
        foreach (var item in TabsList)
        {
            if (item.SaveItem())
            {
                CommandService.ReloadTree.Execute(null);
            }
        }
    }

    private void CloseMiddleClick(MouseButtonEventArgs args)
    {
        if (args.MiddleButton == MouseButtonState.Pressed)
        {
            CommandService.CloseCommand.Execute(((FrameworkElement)args.Source).DataContext);
        }
    }

    private void CloseItem(TabItem item)
    {
        if (item.HasUnsavedChanges)
        {
            switch (MessageBox.Show($"Guardar archivo '{item.Header}'?", "Guardar", MessageBoxButton.YesNoCancel))
            {
                case MessageBoxResult.Yes:
                    if (item.SaveItem())
                    {
                        TabsList.Remove(item);
                    }
                    break;

                case MessageBoxResult.No:
                    TabsList.Remove(item);
                    break;

                case MessageBoxResult.Cancel:
                    break;
            }
        }
        else
        {
            TabsList.Remove(item);
        }
    }

    private void CloseAll()
    {
        for (int i = TabsList.Count - 1; i >= 0; i--)
        {
            CloseItem(TabsList[i]);
        }
    }

    private void SendToPreview()
    {
        CloseErrorWindowCommand.Execute();
        Mediator.GeneratePreview.Execute(TabsList[CurrentTabIndex].Content.Text);
    }

    private void ShowErrors(string errors)
    {
        if (!errors.IsNullOrEmpty())
        {
            ErrorsMessage = errors;
            ShowErrorWindow = Visibility.Visible;
        }
    }

    private void Print()
    {
        var printer = (string)ToolbarViewModel.Printers.CurrentItem;
        var item = TabsList[CurrentTabIndex];
        var content = PreviewServiceProvider
            .ProvideService(item.Content.Text)
            .ParseMetadata()
            .LoadVariables()
            .Content;

        if (printer is not null)
        {
            PrinterHelper.SendStringToPrinter(printer, content, item.Header);
        }
    }

    private async Task UpdateLinter(string data)
    {
        var tab = TabsList[CurrentTabIndex];
        var content = TabsList[CurrentTabIndex].Content.Text;

        var values = data.Split(';');
        var lintings = await PreviewServiceProvider
            .ProvideService(content)
            .Linting(content, values[0], values[1]);

        if (lintings is not null)
        {
            tab.LintingData = lintings.Select(LintingInfo.Parse).ToList();
        }
    }

    private void ResizeFile()
    {
        var label = TabsList[CurrentTabIndex];

        var param = new DialogParameters
        {
            { "LabelName", label.Header }
        };

        _dialogService.Show("ResizeLabelDialog", param, rc =>
        {
            if (rc.Result == ButtonResult.OK)
            {
                var fromDpi = (LabelDpi?)rc.Parameters["FromDpi"];
                var toDpi = (LabelDpi?)rc.Parameters["ToDpi"];
                var factor = (float)(Convert.ToDouble(toDpi?.Value) / Convert.ToDouble(fromDpi?.Value));
                var content = ResizeZPL.Resize(label.Content.Text, factor);

                AddTab(
                    label.Header.Replace(
                        $".{SettingsService.Instance.Extension[0]}",
                        $"_{toDpi?.Display}.{SettingsService.Instance.Extension[0]}",
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    content
                );
                TabsList[^1].SetAsUnsaved();
            }
        });
    }
}