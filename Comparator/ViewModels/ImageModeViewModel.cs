using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

using Comparator.Models;
using Comparator.Services;

using Core.Models;
using Core.Services;

using DiffPlex.DiffBuilder.Model;

namespace Comparator.ViewModels;

[RegionMemberLifetime(KeepAlive = true)]
public class ImageModeViewModel : BindableBase
{
    private LabelDpi _dpi;
    private LabelSize _size;
    private SideBySideDiffModel _diff = null!;

    private string _leftFilename;

    /// <summary>
    /// Nombre del archivo del lado izquierdo.
    /// </summary>
    public string LeftFilename
    {
        get => _leftFilename;
        set => SetProperty(ref _leftFilename, value);
    }

    private Visibility _leftLoading;

    public Visibility LeftLoading
    {
        get => _leftLoading;
        set => SetProperty(ref _leftLoading, value);
    }

    private Visibility _leftVisibility;

    public Visibility LeftVisibility
    {
        get => _leftVisibility;
        set => SetProperty(ref _leftVisibility, value);
    }

    private string _rightFilename;

    /// <summary>
    /// Nombre del archivo del lado derecho.
    /// </summary>
    public string RightFilename
    {
        get => _rightFilename;
        set => SetProperty(ref _rightFilename, value);
    }

    private Visibility _rightLoading;

    public Visibility RightLoading
    {
        get => _rightLoading;
        set => SetProperty(ref _rightLoading, value);
    }

    private Visibility _rightVisibility;

    public Visibility RightVisibility
    {
        get => _rightVisibility;
        set => SetProperty(ref _rightVisibility, value);
    }

    private Visibility _diffLoading;

    public Visibility DiffLoading
    {
        get => _diffLoading;
        set => SetProperty(ref _diffLoading, value);
    }

    private Visibility _diffVisibility;

    public Visibility DiffVisibility
    {
        get => _diffVisibility;
        set => SetProperty(ref _diffVisibility, value);
    }

    private BitmapSource _leftImage = null!;

    /// <summary>
    /// Imagen a comparar del lado izquierdo, considerada como la 'imagen vieja'.
    /// </summary>
    public BitmapSource LeftImage
    {
        get => _leftImage;
        set => SetProperty(ref _leftImage, value);
    }

    private BitmapSource _rightImage = null!;

    /// <summary>
    /// Imagen a comparar del lado derecho, considerada como la 'imagen nueva'.
    /// </summary>
    public BitmapSource RightImage
    {
        get => _rightImage;
        set => SetProperty(ref _rightImage, value);
    }

    private BitmapSource _centerImage = null!;

    /// <summary>
    /// Imagen del centro, solamente incluye las lineas de diferencia entre ambos lados.
    /// </summary>
    public BitmapSource CenterImage
    {
        get => _centerImage;
        set => SetProperty(ref _centerImage, value);
    }

    public ImageModeViewModel(ICommandService commandService, LabelDpi dpi, LabelSize size, string leftName, string rightName)
    {
        _dpi = dpi;
        _size = size;
        _leftFilename = leftName;
        _rightFilename = rightName;

        ResetLoading();

        commandService.GenerateDiff.RegisterCommand(new AsyncDelegateCommand<SideBySideDiffModel>(LoadImages));
        commandService.Refresh.RegisterCommand(new AsyncDelegateCommand(async () => await LoadImages(_diff)));
        commandService.ChangeFiles.RegisterCommand(new DelegateCommand<SelectionResult>(result =>
        {
            _dpi = result.Dpi;
            _size = result.Size;
            ResetLoading();
        }));
    }

    private void ResetLoading()
    {
        LeftImage = null!;
        RightImage = null!;
        CenterImage = null!;

        LeftLoading = Visibility.Visible;
        LeftVisibility = Visibility.Collapsed;
        RightLoading = Visibility.Visible;
        RightVisibility = Visibility.Collapsed;
        DiffLoading = Visibility.Visible;
        DiffVisibility = Visibility.Collapsed;
    }

    private async Task LoadImages(SideBySideDiffModel diff)
    {
        ResetLoading();
        _diff = diff;

        await Task.Run(() =>
        {
            var leftImg = GenerateImage(string.Join(Environment.NewLine, diff.OldText.Lines.Select(l => l.Text)));
            if (leftImg is not null)
            {
                LeftImage = leftImg;
                LeftLoading = Visibility.Collapsed;
                LeftVisibility = Visibility.Visible;
            }

            var rightImg = GenerateImage(string.Join(Environment.NewLine, diff.NewText.Lines.Select(l => l.Text)));
            if (rightImg is not null)
            {
                RightImage = rightImg;
                RightLoading = Visibility.Collapsed;
                RightVisibility = Visibility.Visible;
            }

            var centerText = diff.NewText.Lines
                .Aggregate(new StringBuilder(), (p, n) => n.Type == ChangeType.Unchanged ? p : p.AppendLine(n.Text))
                .ToString();

            if (!centerText.StartsWith("^XA"))
            {
                centerText = "^XA ^CI28" + centerText;
            }

            if (!centerText.EndsWith("^XZ"))
            {
                centerText += "^XZ";
            }

            var centerImg = GenerateImage(centerText);
            if (centerImg is not null)
            {
                CenterImage = centerImg;
            }
            DiffLoading = Visibility.Collapsed;
            DiffVisibility = Visibility.Visible;
        });
    }

    private BitmapFrame? GenerateImage(string content)
    {
        var dpiValue = _dpi.Value;
        var sizeValue = _size.Value;
        var preview = PreviewServiceProvider
            .ProvideService(content)
            .ParseMetadata()
            .LoadVariables();

        using var task = Task.Run(() => preview.Build(dpiValue, sizeValue));
        task.Wait();

        var labels = task.Result;
        if (labels is not null)
        {
            foreach (var label in labels)
            {
                if (label is not null)
                {
                    using MemoryStream stream = new(label);
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
        }

        return null;
    }
}