using Core.Models;
using Core.Services;
using Editor.Services;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.ViewModels
{
    public class PreviewViewModel : BindableBase
    {
        public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);
        public ListCollectionView SizeList { get; set; } = new(LabelSize.GetList(File.ReadAllText("SizeList.xml")));

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

        public DelegateCommand DownSizeCommand => new(() =>
        {
            SizeList.MoveCurrentToNext();
            if (SizeList.IsCurrentAfterLast)
            {
                SizeList.MoveCurrentToFirst();
            }
            SizeList.Refresh();
        });

        public DelegateCommand UpSizeCommand => new(() =>
        {
            SizeList.MoveCurrentToPrevious();
            if (SizeList.IsCurrentBeforeFirst)
            {
                SizeList.MoveCurrentToLast();
            }
            SizeList.Refresh();
        });

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
        }

        public async void GeneratePreview(string content)
        {
            var preview = PreviewServiceProvider
                .ProvideService(content)
                .FillTestVariables()
                .LoadFonts();

            var bytes = await preview.Build(((LabelDpi)DpiList.CurrentItem).Value, ((LabelSize)SizeList.CurrentItem).Value);
            if (bytes is not null)
            {
                using MemoryStream stream = new(bytes);
                PreviewImage = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            Mediator.SendErrors.Execute(preview.Error);
        }

        private void SendData()
        {
            Mediator.GenerateLinter.Execute($"{((LabelDpi)DpiList.CurrentItem).Value};{((LabelSize)SizeList.CurrentItem).Value}");
        }
    }
}
