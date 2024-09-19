using Comparator.Models;
using Comparator.Services;

namespace Comparator.ViewModels
{
    public delegate void ComparatorDelegate();

    /// <summary>
    /// <para><b>View Model</b> para la ventana de Comparación de etiquetas.</para>
    /// </summary>
    public class ComparatorViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly ICommandService _commandService;

        private bool _textMode = true;
        /// <summary>
        /// Indica si la ventana esta comparando codigo o no.
        /// </summary>
        public bool TextMode
        {
            get => _textMode;
            set => SetProperty(ref _textMode, value);
        }

        private bool _imageMode = false;
        /// <summary>
        /// Indica si la ventana esta comparando imagenes o no.
        /// </summary>
        public bool ImageMode
        {
            get => _imageMode;
            set => SetProperty(ref _imageMode, value);
        }

        public DelegateCommand ChangeFilesCommand { get; private set; }

        public DelegateCommand RefreshCommand { get; private set; }

        public ComparatorViewModel(IContainerRegistry containerRegistry, IDialogService dialogService, IContainerProvider container)
        {
            containerRegistry.RegisterScoped<ICommandService, CommandsService>();
            _commandService = container.Resolve<ICommandService>();
            _dialogService = dialogService;

            ChangeFilesCommand = new(ChangeFiles);
            RefreshCommand = new(() => _commandService.Refresh.Execute(null));
        }

        private void ChangeFiles()
        {
            _dialogService.ShowDialog("SelectLabelsDialog", result =>
            {
                if (result.Result != ButtonResult.OK)
                {
                    return;
                }

                var sr = result.Parameters["SelectionResult"] as SelectionResult;
                _commandService.ChangeFiles.Execute(sr);
            });
        }
    }
}
