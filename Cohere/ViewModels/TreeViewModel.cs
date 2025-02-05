using System.Collections.ObjectModel;
using System.Windows.Input;

using Cohere.Services;

using Core.FileTree;

namespace Cohere.ViewModels
{
    public class TreeViewModel : BindableBase
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
        private object? _currentItem;

        public TreeViewModel(ICommandService commandService)
        {
            Tree = _tree.InitTree();
            _commandService = commandService;
            PressSelectedCommand = new(PressSelected);
            ChangedItemCommand = new((obj) => _currentItem = obj);
            ClickSelectedCommand = new(() => commandService.OpenItemCommand.Execute(_currentItem));
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
            _tree.ClearCache();
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
}