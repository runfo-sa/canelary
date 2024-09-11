using Cohere.Models;
using Cohere.Services;
using Core.FileTree;

namespace Cohere.ViewModels
{
    public class CohereViewModel : BindableBase
    {
        private readonly ICommandService _commandService;
        private readonly IDialogService _dialogService;
        private int _productsCount = 0;
        private LabelFile? _currentLabel;

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
            _commandService = container.Resolve<ICommandService>();
            _commandService.OpenItemCommand.RegisterCommand(new DelegateCommand<object?>(e =>
            {
                if (e is not null and LabelFile file)
                {
                    _currentLabel = file;
                }
            }));
            _commandService.RefreshErrorCount.RegisterCommand(new DelegateCommand<ErrorCounter>(e =>
            {
                ErrorCount = e.ErrorCount;
                _productsCount = e.ProductsCount;
                GenerateSampleCommand?.RaiseCanExecuteChanged();
            }));

            GenerateSampleCommand = new DelegateCommand(OpenSampleDialog, () => _productsCount > 0 && ErrorCount == 0);
        }

        private void OpenSampleDialog()
        {
            var param = new DialogParameters
            {
                { "File", _currentLabel }
            };
            _dialogService.Show("GenerateSample", param, _ => { });
        }
    }
}
