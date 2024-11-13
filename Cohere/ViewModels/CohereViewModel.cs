using Cohere.Models;
using Cohere.Services;
using Core.FileTree;

namespace Cohere.ViewModels
{
    public class CohereViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private int _productsCount = 0;
        private IFile? _currentLabel;

        private int errorCount = 0;

        public int ErrorCount
        {
            get => errorCount;
            set => SetProperty(ref errorCount, value);
        }

        public DelegateCommand GenerateSampleCommand { get; private set; }

        public CohereViewModel(IContainerRegistry containerRegistry, IContainerProvider container, IDialogService dialogService)
        {
            _dialogService = dialogService;

            containerRegistry.RegisterScoped<ICommandService, CommandsService>();
            var commandService = container.Resolve<ICommandService>();
            commandService.OpenItemCommand.RegisterCommand(new DelegateCommand<object?>(e =>
            {
                if (e is IFile file)
                {
                    _currentLabel = file;
                }
            }));
            commandService.RefreshErrorCount.RegisterCommand(new DelegateCommand<ErrorCounter>(e =>
            {
                ErrorCount = e.ErrorCount;
                _productsCount = e.ProductsCount;
                GenerateSampleCommand?.RaiseCanExecuteChanged();
            }));

            GenerateSampleCommand = new DelegateCommand(OpenSampleDialog, () => _productsCount > 0 && ErrorCount == 0 && _currentLabel != null);
        }

        private void OpenSampleDialog()
        {
            var param = new DialogParameters
            {
                { "File", _currentLabel! }
            };
            _dialogService.Show("GenerateSample", param, _ => { });
        }
    }
}