using Comparator.Models;
using Core.FileTree;
using Core.Git;
using Core.Models;
using Core.Services;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace Comparator.Views
{
    public partial class SelectLabelsDialog : UserControl, IDialogAware
    {
        public string Title => "Seleccionar Etiquetas";

        public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);
        public ListCollectionView SizeList { get; set; } = new(LabelSize.GetList(File.ReadAllText("SizeList.xml")));

        public ListCollectionView LeftGitVer { get; set; }
        public ListCollectionView RightGitVer { get; set; }

        public LabelFile LeftLabel => (LabelFile)leftLabel.SelectedItem;
        public LabelFile RightLabel => (LabelFile)rightLabel.SelectedItem;

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        private readonly IEnumerable<LabelFile> _files;
        private readonly SettingsService _settings;

        public SelectLabelsDialog()
        {
            InitializeComponent();
            DataContext = this;

            _settings = SettingsService.Instance;
            _files = Directory
                .GetFiles(_settings.EtiquetasDir, $"*.{_settings.EtiquetasExtension}")
                .Select(f => new LabelFile(f));

            leftLabel.ItemsSource = _files;
            rightLabel.ItemsSource = _files;

            var tags = Git.RunGitCommand(
                "for-each-ref",
                "--format=\"%(refname:short)|%(creatordate:format:%Y/%m/%d %I:%M)|%(subject)\\n\" \"refs/tags/*\"",
                _settings.EtiquetasDir)
            .Split("\\n", StringSplitOptions.RemoveEmptyEntries).Select(GitTag.Parse)
            .Prepend(GitTag.Local);

            LeftGitVer = new(tags.ToList());
            RightGitVer = new(tags.ToList());

            CloseDialogCommand = new(ClosingDialog, () => acceptButton.IsEnabled);
        }

        private void LeftFetchFiles(Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            leftLabel.IsEnabled = false;
            acceptButton.IsEnabled = false;

            leftLabel.ItemsSource = FetchFiles((GitTag)LeftGitVer.CurrentItem);
            if (leftLabel.ItemsSource is not null)
            {
                leftLabel.IsEnabled = true;
                acceptButton.IsEnabled = true;
                leftLabel.SelectedIndex = 0;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        private void RightFetchFiles(Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            rightLabel.IsEnabled = false;
            acceptButton.IsEnabled = false;

            rightLabel.ItemsSource = FetchFiles((GitTag)RightGitVer.CurrentItem);
            if (rightLabel.ItemsSource is not null)
            {
                rightLabel.IsEnabled = true;
                acceptButton.IsEnabled = true;
                rightLabel.SelectedIndex = 1;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        private IEnumerable<LabelFile>? FetchFiles(GitTag current)
        {
            if (current == GitTag.Local)
            {
                return _files;
            }
            return LoadGitFile(current);
        }

        private IEnumerable<LabelFile> LoadGitFile(GitTag git)
        {
            string path = Path.Combine(Path.GetTempPath(), $"Visual Ternera - {git.Tag}");
            Directory.CreateDirectory(path);

            if (Git.RunGitCommand("tag", "--points-at HEAD", path) != git.Tag)
            {
                Git.RunGitCommand("init", "", path);
                Git.RunGitCommand("remote add origin", _settings.GitRepo, path);
                Git.RunGitCommand("fetch", "--all --tags --prune", path);
                Git.RunGitCommand("checkout", $"tags/{git.Tag}", path);
            }

            return Directory
                .GetFiles(path, $"*.{_settings.EtiquetasExtension}")
                .Select(f => new LabelFile(f));
        }

        private void ClosingDialog()
        {
            var sr = new SelectionResult(LeftLabel, RightLabel, (LabelDpi)DpiList.CurrentItem, (LabelSize)SizeList.CurrentItem);
            var result = new DialogResult
            {
                Parameters = new DialogParameters { { "SelectionResult", sr } },
                Result = ButtonResult.OK
            };
            RequestClose.Invoke(result);
        }

        public Boolean CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
