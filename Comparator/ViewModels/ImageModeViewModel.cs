using Comparator.Models;
using Comparator.Services;
using Core.Models;
using Core.Services;
using DiffPlex.DiffBuilder.Model;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Comparator.ViewModels
{
    [RegionMemberLifetime(KeepAlive = true)]
    public class ImageModeViewModel : BindableBase
    {
        private LabelDpi _dpi;
        private LabelSize _size;
        private SideBySideDiffModel _diff = null!;

        private string _leftFilename = string.Empty;
        /// <summary>
        /// Nombre del archivo del lado izquierdo.
        /// </summary>
        public string LeftFilename
        {
            get => _leftFilename;
            set => SetProperty(ref _leftFilename, value);
        }

        private string _rightFilename = string.Empty;
        /// <summary>
        /// Nombre del archivo del lado derecho.
        /// </summary>
        public string RightFilename
        {
            get => _rightFilename;
            set => SetProperty(ref _rightFilename, value);
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

        public ImageModeViewModel(ICommandService commandService, LabelDpi dpi, LabelSize size)
        {
            _dpi = dpi;
            _size = size;
            commandService.GenerateDiff.RegisterCommand(new DelegateCommand<SideBySideDiffModel>(LoadImages));
            commandService.Refresh.RegisterCommand(new DelegateCommand(() => LoadImages(_diff)));
            commandService.ChangeFiles.RegisterCommand(new DelegateCommand<SelectionResult>(result =>
            {
                _dpi = result.Dpi;
                _size = result.Size;
            }));
        }

        private void LoadImages(SideBySideDiffModel diff)
        {
            _diff = diff;

            var leftImg = GenerateImage(string.Join(Environment.NewLine, diff.OldText.Lines.Select(l => l.Text)));
            if (leftImg is not null)
            {
                LeftImage = leftImg;
            }

            var rightImg = GenerateImage(string.Join(Environment.NewLine, diff.NewText.Lines.Select(l => l.Text)));
            if (rightImg is not null)
            {
                RightImage = rightImg;
            }

            var centerText = diff.NewText.Lines
                .Aggregate(new StringBuilder(), (p, n) =>
                {
                    if (n.Type == ChangeType.Unchanged) { return p; }
                    return p.AppendLine(n.Text);
                })
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
                        {
                            using MemoryStream stream = new(label);
                            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        }
                    }
                }
            }

            return null;
        }
    }
}
