using System.IO;
using System.Windows;

using Cohere.Models;
using Cohere.Services;

using Core.Database;
using Core.Database.IdeDbModels;
using Core.FileTree;
using Core.Services.BackendModel;

namespace Cohere.ViewModels
{
    public class CohereViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private int _productsCount = 0;
        private IEnumerable<Product>? _products;
        private IFile? _currentLabel;
        private RuleLabel? _ruleLabel;

        private int _errorCount = 0;

        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        private Visibility _warningVisibility;

        public Visibility WarningVisibility
        {
            get => _warningVisibility;
            set => SetProperty(ref _warningVisibility, value);
        }

        public DelegateCommand GenerateSampleCommand { get; private set; }
        public ICommandService CommandService { get; }

        public CohereViewModel(IContainerRegistry containerRegistry, IContainerProvider container, IDialogService dialogService)
        {
            _dialogService = dialogService;

            containerRegistry.RegisterScoped<ICommandService, CommandsService>();
            CommandService = container.Resolve<ICommandService>();
            CommandService.CreateRuleCommand.RegisterCommand(new DelegateCommand(() => dialogService.Show("CreateRuleDialog")));
            CommandService.OpenItemCommand.RegisterCommand(new DelegateCommand<object?>(e =>
            {
                if (e is IFile file)
                {
                    _currentLabel = file;
                    using var context = new IdeDbContext();
                    var labelName = Path.GetFileNameWithoutExtension(_currentLabel.Name);
                    _ruleLabel = context.RuleLabel.FirstOrDefault(r => r.LabelName == labelName);
                    WarningVisibility = (_ruleLabel != null) ? Visibility.Collapsed : Visibility.Visible;
                }
            }));
            CommandService.RefreshErrorCount.RegisterCommand(new DelegateCommand<ErrorCounter>(e =>
            {
                ErrorCount = e.ErrorCount;
                _productsCount = e.ProductsCount.Count();
                _products = e.ProductsCount;
                GenerateSampleCommand?.RaiseCanExecuteChanged();
            }));

            GenerateSampleCommand = new DelegateCommand(OpenSampleDialog, () => _productsCount > 0 && _currentLabel != null && _ruleLabel != null);
            WarningVisibility = Visibility.Collapsed;
        }

        private void OpenSampleDialog()
        {
            var param = new DialogParameters
            {
                { "File", _currentLabel! },
                { "Products", _products! }
            };
            _dialogService.Show("GenerateSample", param, _ => { });
        }
    }
}