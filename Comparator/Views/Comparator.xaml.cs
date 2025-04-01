using System.Windows;
using System.Windows.Controls;

using Comparator.Models;
using Comparator.Services;
using Comparator.ViewModels;

using MaterialDesignThemes.Wpf;

namespace Comparator.Views;

public partial class Comparator : UserControl
{
    private IRegion _region = null!;
    private TextMode _textView = null!;
    private ImageMode _imageView = null!;
    private readonly ComparatorViewModel _viewModel;
    private readonly IContainerProvider _container;
    private bool _first = false;

    public Comparator(IContainerProvider container)
    {
        InitializeComponent();

        _container = container;
        _viewModel = (ComparatorViewModel)DataContext;
        dialog.Loaded += OpenDialog;
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

    private async void OpenDialog(object sender, RoutedEventArgs e)
    {
        if (_first)
        {
            return;
        }

        _first = true;
        await DialogHost.Show(new SelectLabelsDialog(_viewModel.DialogIdentifier), _viewModel.DialogIdentifier, delegate (object sender, DialogClosingEventArgs args)
        {
            if (args.Parameter is SelectionResult result)
            {
                var commandService = _container.Resolve<ICommandService>();

                var regionManager = new RegionManager();
                RegionManager.SetRegionManager(this, regionManager);
                _region = regionManager.Regions["ContentRegion"];

                var textVM = new TextModeViewModel(commandService, result.LeftFile, result.RightFile);
                _textView = new TextMode(commandService)
                {
                    DataContext = textVM
                };

                var imageVM = new ImageModeViewModel(commandService, result.Dpi, result.Size, result.LeftFile.Name, result.RightFile.Name);
                _imageView = new ImageMode()
                {
                    DataContext = imageVM
                };

                _region.Add(_textView);
                _region.Add(_imageView);

                _region.Activate(_textView);
            }
        });
    }
}