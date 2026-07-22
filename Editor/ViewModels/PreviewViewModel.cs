using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Core.Controls;
using Core.Models;
using Core.Services;

using Editor.Models;
using Editor.Services;

namespace Editor.ViewModels;

public class PreviewViewModel : BindableBase
{
    private TabItem? _activeTab;

    public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);

    private ListCollectionView _sizeList = new(new List<LabelSize>());

    public ListCollectionView SizeList
    {
        get => _sizeList;
        private set => SetProperty(ref _sizeList, value);
    }

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

    public event Action<ZoomState?>? ApplyZoomState;

    public PreviewViewModel(IEditorPreviewMediator mediator)
    {
        Mediator = mediator;
        _ = Task.Run(async () =>
        {
            var xml = await File.ReadAllTextAsync("SizeList.xml");
            var list = LabelSize.GetList(xml);
            Application.Current.Dispatcher.Invoke(() => SizeList = new ListCollectionView(list));
        });
        Mediator.GeneratePreview.RegisterCommand(new DelegateCommand<TabItem>(GeneratePreview));
        Mediator.SendData.RegisterCommand(new DelegateCommand(SendData));
        Mediator.TabSelected.RegisterCommand(new DelegateCommand<TabItem>(OnTabSelected));

        RotateRightCommand = new(() =>
        {
            var rotated = new TransformedBitmap(PreviewImage, new RotateTransform(90.0));
            rotated.Freeze();
            PreviewImage = rotated;
            var angle = PreviewAngle + 90.0;
            PreviewAngle = angle >= 360.0 ? 0.0 : angle;
            if (_activeTab?.Preview is { } preview)
            {
                preview.PreviewAngle = PreviewAngle;
            }
        }, () => PreviewImage != null);

        RotateLeftCommand = new(() =>
        {
            var rotated = new TransformedBitmap(PreviewImage, new RotateTransform(-90.0));
            rotated.Freeze();
            PreviewImage = rotated;
            var angle = PreviewAngle - 90.0;
            PreviewAngle = angle < 0.0 ? 270.0 : angle;
            if (_activeTab?.Preview is { } preview)
            {
                preview.PreviewAngle = PreviewAngle;
            }
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
            if (_activeTab?.Preview is { } preview)
            {
                preview.CurrentLabel = --CurrentLabel;
                DisplayLabel(preview, preview.CurrentLabel);
            }
        }, () => _activeTab?.Preview?.RawLabelsData.Count > 0 && CurrentLabel > 0);

        NextLabel = new(() =>
        {
            if (_activeTab?.Preview is { } preview)
            {
                preview.CurrentLabel = ++CurrentLabel;
                DisplayLabel(preview, preview.CurrentLabel);
            }
        }, () => _activeTab?.Preview?.RawLabelsData.Count > 0 && CurrentLabel < _activeTab.Preview.RawLabelsData.Count - 1);
    }

    public async void GeneratePreview(TabItem tab)
    {
        try
        {
            var content = tab.Content.Text;

            var preview = PreviewServiceProvider
                .ProvideService(content)
                .ParseMetadata()
                .LoadVariables();

            var labels = await preview.Build(((LabelDpi)DpiList.CurrentItem).Value, ((LabelSize)SizeList.CurrentItem).Value);
            if (labels is not null)
            {
                var rawLabelsData = labels
                    .Where(b => b != null)
                    .Select(b => b!)
                    .ToList();

                var state = tab.Preview ??= new PreviewState { RawLabelsData = rawLabelsData };
                state.RawLabelsData = rawLabelsData;
                state.CurrentLabel = 0;
                tab.Preview = state;

                if (_activeTab == tab)
                {
                    CurrentLabel = 0;
                    TotalLabel = rawLabelsData.Count - 1;
                    PreviewAngle = state.PreviewAngle;
                    DisplayLabel(state, state.CurrentLabel);
                    ApplyZoomState?.Invoke(state.Zoom);
                }
            }

            Mediator.SendErrors.Execute(preview.Error);
        }
        catch (Exception ex)
        {
            Mediator.SendErrors.Execute(ex.Message);
        }
    }

    private void OnTabSelected(TabItem? tab)
    {
        _activeTab = tab;

        if (tab?.Preview is { } state)
        {
            CurrentLabel = state.CurrentLabel;
            TotalLabel = state.RawLabelsData.Count - 1;
            PreviewAngle = state.PreviewAngle;
            DisplayLabel(state, state.CurrentLabel);
            ApplyZoomState?.Invoke(state.Zoom);
        }
        else
        {
            PreviewImage = null!;
            CurrentLabel = 0;
            TotalLabel = 0;
            PreviewAngle = 0.0;
            ApplyZoomState?.Invoke(null);
        }
    }

    public void SaveZoomState(ZoomState state)
    {
        if (_activeTab?.Preview is { } preview)
        {
            preview.Zoom = state;
        }
    }

    private void DisplayLabel(PreviewState state, int index)
    {
        using MemoryStream stream = new(state.RawLabelsData[index]);
        BitmapSource image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        if (state.PreviewAngle != 0.0)
        {
            var rotated = new TransformedBitmap(image, new RotateTransform(state.PreviewAngle));
            rotated.Freeze();
            image = rotated;
        }

        PreviewImage = image;
    }

    private void SendData()
    {
        Mediator.GenerateLinter.Execute($"{((LabelDpi)DpiList.CurrentItem).Value};{((LabelSize)SizeList.CurrentItem).Value}");
    }
}
