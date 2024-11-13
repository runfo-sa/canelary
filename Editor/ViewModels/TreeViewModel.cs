using Core.FileTree;
using Editor.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Editor.ViewModels
{
    public class TreeViewModel : BindableBase
    {
        public ObservableCollection<object> Tree { get; set; }
        public DelegateCommand ClickSelectedCommand { get; private set; }
        public DelegateCommand<KeyEventArgs> PressSelectedCommand { get; private set; }
        public DelegateCommand<object?> ChangedItemCommand { get; private set; }

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
            _commandService.ReloadTree.RegisterCommand(new DelegateCommand(() =>
            {
                _tree.ClearCache();
                var tree = _tree.InitTree();

                Tree.Clear();
                foreach (var item in tree)
                {
                    Tree.Add(item);
                }
            }));
        }

        private void PressSelected(KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                _commandService.OpenItemCommand.Execute(_currentItem);
            }
        }
    }
}