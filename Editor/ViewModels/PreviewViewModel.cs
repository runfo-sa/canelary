using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Core.Models;
using Core.Services;

using Editor.Services;

namespace Editor.ViewModels;

public class PreviewViewModel : BindableBase
{
    private List<byte[]>? _labelsRawData;

    public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);
    public ListCollectionView SizeList { get; set; } = new(LabelSize.GetList(File.ReadAllText("SizeList.xml")));

    private int _currentLabel = 0;

    public int CurrentLabel
    {
        get => _currentLabel;
        set
        {
            SetProperty(ref _currentLabel, value);
            PreviousLabel.RaiseCanExecuteChanged();
            NextLabel.RaiseCanExecuteChanged();
        }
    }

    private int _totalLabels = 0;

    public int TotalLabel
    {
        get => _totalLabels;
        set
        {
            SetProperty(ref _totalLabels, value);
            PreviousLabel.RaiseCanExecuteChanged();
            NextLabel.RaiseCanExecuteChanged();
        }
    }

    private BitmapSource _previewImage = null!;

    public BitmapSource PreviewImage
    {
        get => _previewImage;
        set
        {
            SetProperty(ref _previewImage, value);
            RotateRightCommand.RaiseCanExecuteChanged();
            RotateLeftCommand.RaiseCanExecuteChanged();
        }
    }

    private double _previewAngle = 0.0;

    public double PreviewAngle
    {
        get => _previewAngle;
        set => SetProperty(ref _previewAngle, value);
    }

    public DelegateCommand RotateRightCommand { get; private set; }
    public DelegateCommand RotateLeftCommand { get; private set; }
    public DelegateCommand DownSizeCommand { get; private set; }
    public DelegateCommand UpSizeCommand { get; private set; }
    public DelegateCommand PreviousLabel { get; private set; }
    public DelegateCommand NextLabel { get; private set; }

    public IEditorPreviewMediator Mediator { get; }

    public PreviewViewModel(IEditorPreviewMediator mediator)
    {
        Mediator = mediator;
        Mediator.GeneratePreview.RegisterCommand(new DelegateCommand<string>(GeneratePreview));
        Mediator.SendData.RegisterCommand(new DelegateCommand(SendData));

        RotateRightCommand = new(() =>
        {
            PreviewImage = new TransformedBitmap(PreviewImage, new RotateTransform(90.0));
            var angle = PreviewAngle + 90.0;
            PreviewAngle = angle >= 360.0 ? 0.0 : angle;
        }, () => PreviewImage != null);

        RotateLeftCommand = new(() =>
        {
            PreviewImage = new TransformedBitmap(PreviewImage, new RotateTransform(-90.0));
            var angle = PreviewAngle - 90.0;
            PreviewAngle = angle < 0.0 ? 270.0 : angle;
        }, () => PreviewImage != null);

        UpSizeCommand = new(() =>
        {
            SizeList.MoveCurrentToPrevious();
            if (SizeList.IsCurrentBeforeFirst)
            {
                SizeList.MoveCurrentToLast();
            }
            SizeList.Refresh();
        });

        DownSizeCommand = new(() =>
        {
            SizeList.MoveCurrentToNext();
            if (SizeList.IsCurrentAfterLast)
            {
                SizeList.MoveCurrentToFirst();
            }
            SizeList.Refresh();
        });

        PreviousLabel = new(() =>
        {
            if (_labelsRawData != null)
            {
                using MemoryStream stream = new(_labelsRawData[--CurrentLabel]);
                PreviewImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }, () => _labelsRawData?.Count > 0 && CurrentLabel > 0);

        NextLabel = new(() =>
        {
            if (_labelsRawData != null)
            {
                using MemoryStream stream = new(_labelsRawData[++CurrentLabel]);
                PreviewImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }, () => _labelsRawData?.Count > 0 && CurrentLabel < _labelsRawData.Count - 1);
    }

    public async void GeneratePreview(string content)
    {
        _labelsRawData = null;

        var preview = PreviewServiceProvider
            .ProvideService(content)
            .ParseMetadata()
            .LoadVariables();

        var labels = await preview.Build(((LabelDpi)DpiList.CurrentItem).Value, ((LabelSize)SizeList.CurrentItem).Value);
        if (labels is not null)
        {
            _labelsRawData = labels
                .Where(b => b != null)
                .Select(b => b!)
                .ToList();
            CurrentLabel = 0;
            using MemoryStream stream = new(_labelsRawData[CurrentLabel]);
            PreviewImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        TotalLabel = _labelsRawData?.Count - 1 ?? 0;
        Mediator.SendErrors.Execute(preview.Error);
    }

    private void SendData()
    {
        Mediator.GenerateLinter.Execute($"{((LabelDpi)DpiList.CurrentItem).Value};{((LabelSize)SizeList.CurrentItem).Value}");
    }
}