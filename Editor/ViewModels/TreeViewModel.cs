using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using Core.FileTree;

using Editor.Services;

namespace Editor.ViewModels;

public class TreeViewModel : BindableBase, IDisposable
{
    public ObservableCollection<object> Tree { get; set; }
    public DelegateCommand ClickSelectedCommand { get; private set; }
    public DelegateCommand<KeyEventArgs> PressSelectedCommand { get; private set; }
    public DelegateCommand<object?> ChangedItemCommand { get; private set; }

    private string? _searchBox;

    public string? SearchBox
    {
        get => _searchBox;
        set
        {
            SetProperty(ref _searchBox, value);
            Find(value);
        }
    }

    private readonly ICommandService _commandService;
    private readonly TreeGenerator _tree = new();
    private readonly DelegateCommand _reloadTreeCommand;
    private object? _currentItem;

    public TreeViewModel(ICommandService commandService)
    {
        Tree = [];
        _ = Task.Run(() =>
        {
            var tree = _tree.InitTree();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in tree) Tree.Add(item);
            });
        });
        _commandService = commandService;
        PressSelectedCommand = new(PressSelected);
        ChangedItemCommand = new((obj) => _currentItem = obj);
        ClickSelectedCommand = new(() => commandService.OpenItemCommand.Execute(_currentItem));
        _reloadTreeCommand = new DelegateCommand(() =>
        {
            _tree.ClearCache();
            var tree = _tree.InitTree();

            Tree.Clear();
            foreach (var item in tree)
            {
                Tree.Add(item);
            }
        });
        _commandService.ReloadTree.RegisterCommand(_reloadTreeCommand);
    }

    public void Dispose()
    {
        _commandService.ReloadTree.UnregisterCommand(_reloadTreeCommand);
    }

    private void PressSelected(KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            _commandService.OpenItemCommand.Execute(_currentItem);
        }
    }

    private void Find(string? search)
    {
        var tree = _tree.InitTree();

        if (search != null && search != "")
        {
            var dirs = new List<VirtualDirectory>();
            foreach (var obj in tree)
            {
                if (obj is VirtualDirectory dir)
                {
                    var filter = dir.Files.Where(f => f.Name.Contains(search!, StringComparison.CurrentCultureIgnoreCase));
                    var new_dir = new VirtualDirectory(dir.Name);
                    foreach (var f in filter)
                    {
                        new_dir.Files.Add(f);
                    }
                    dirs.Add(new_dir);
                }
            }

            Tree.Clear();
            foreach (var d in dirs)
            {
                Tree.Add(d);
            }
        }
        else
        {
            Tree.Clear();
            foreach (var t in tree)
            {
                Tree.Add(t);
            }
        }
    }
}