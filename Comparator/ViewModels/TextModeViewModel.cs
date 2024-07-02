using Comparator.Models;
using Comparator.Services;
using Core.FileTree;
using Core.Models;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using ICSharpCode.AvalonEdit.Document;
using System.IO;

namespace Comparator.ViewModels
{
    [RegionMemberLifetime(KeepAlive = true)]
    public class TextModeViewModel : BindableBase
    {
        private TextDocument _leftText = null!;
        /// <summary>
        /// Codigo a comparar del lado izquierdo, considerado como el 'codigo viejo'.
        /// </summary>
        public TextDocument LeftText
        {
            get => _leftText;
            set => SetProperty(ref _leftText, value);
        }

        private TextDocument _rightText = null!;
        /// <summary>
        /// Codigo a comparar del lado derecho, considerado como el 'codigo nuevo'.
        /// </summary>
        public TextDocument RightText
        {
            get => _rightText;
            set => SetProperty(ref _rightText, value);
        }

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

        public DelegateCommand CalculateDiffCommand { get; private set; }

        private SideBySideDiffModel? _diff = null;

        public TextModeViewModel(ICommandService commandService, LabelFile leftFile, LabelFile rightFile)
        {
            LeftText = new TextDocument(File.ReadAllText(leftFile.Path));
            RightText = new TextDocument(File.ReadAllText(rightFile.Path));
            LeftFilename = leftFile.Name;
            RightFilename = rightFile.Name;

            CalculateDiffCommand = new(async () =>
            {
                if (_diff != null)
                {
                    return;
                }

                var leftText = LeftText.Text;
                var rightText = RightText.Text;

                _diff = await Task.Run(() =>
                {
                    return SideBySideDiffBuilder.Diff(leftText, rightText);
                });

                commandService.GenerateDiff.Execute(_diff);
            });

            commandService.ChangeFiles.RegisterCommand(new DelegateCommand<SelectionResult>(ChangeFiles));
            commandService.Refresh.RegisterCommand(new DelegateCommand(()
                => ChangeFiles(new SelectionResult(leftFile, rightFile, new LabelDpi(), new LabelSize()))));
        }

        private void ChangeFiles(SelectionResult result)
        {
            _diff = null;
            LeftText = new TextDocument(File.ReadAllText(result.LeftFile.Path));
            RightText = new TextDocument(File.ReadAllText(result.RightFile.Path));
            LeftFilename = result.LeftFile.Name;
            RightFilename = result.RightFile.Name;
            CalculateDiffCommand.Execute();
        }
    }
}
