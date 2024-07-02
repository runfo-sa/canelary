using Comparator.Models;
using Comparator.Services;
using Comparator.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Comparator.Views
{
    public partial class Comparator : UserControl
    {
        private IRegion _region = null!;
        private TextMode _textView = null!;
        private ImageMode _imageView = null!;
        private readonly ComparatorViewModel _viewModel;

        private bool _shouldClose = false;
        public bool ShouldClose
        {
            get => _shouldClose;
            private set => _shouldClose = value;
        }

        public Comparator(IContainerProvider container, IDialogService dialogService)
        {
            InitializeComponent();

            dialogService.ShowDialog("SelectLabelsDialog", result =>
            {
                if (result.Result != ButtonResult.OK)
                {
                    ShouldClose = true;
                    return;
                }

                var sr = (SelectionResult)result.Parameters["SelectionResult"];
                var commandService = container.Resolve<ICommandService>();

                var regionManager = new RegionManager();
                RegionManager.SetRegionManager(this, regionManager);
                _region = regionManager.Regions["ContentRegion"];

                var textVM = new TextModeViewModel(commandService, sr.LeftFile, sr.RightFile);
                _textView = new TextMode(commandService)
                {
                    DataContext = textVM
                };

                var imageVM = new ImageModeViewModel(commandService, sr.Dpi, sr.Size);
                _imageView = new ImageMode()
                {
                    DataContext = imageVM
                };

                _region.Add(_textView);
                _region.Add(_imageView);

                _region.Activate(_textView);
            });

            _viewModel = (ComparatorViewModel)DataContext;
            Loaded += ShouldCloseWindow;
        }

        private void SwitchToImageMode(Object sender, RoutedEventArgs e)
        {
            _region.Activate(_imageView);
            _viewModel.TextMode = false;
            _viewModel.ImageMode = true;
        }

        private void SwitchToTextMode(Object sender, RoutedEventArgs e)
        {
            _region.Activate(_textView);
            _viewModel.TextMode = true;
            _viewModel.ImageMode = false;
        }

        private void ShouldCloseWindow(Object sender, RoutedEventArgs e)
        {
            if (ShouldClose)
            {
                Window.GetWindow(this).Close();
            }
        }
    }
}
