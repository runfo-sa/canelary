using Comparator.Models;
using Core.FileTree;
using Core.Models;
using Core.Services;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace Comparator.Views
{
    public partial class SelectLabelsDialog : UserControl, IDialogAware
    {
        public static string Title => "Seleccionar Etiquetas";

        public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);
        public ListCollectionView SizeList { get; set; } = new(LabelSize.GetList(File.ReadAllText("SizeList.xml")));

        public ListCollectionView LeftVersion { get; set; }
        public ListCollectionView RightVersion { get; set; }

        public IFile LeftLabel => (IFile)leftLabel.SelectedItem;
        public IFile RightLabel => (IFile)rightLabel.SelectedItem;

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        public SelectLabelsDialog()
        {
            InitializeComponent();
            DataContext = this;

            IEnumerable<IFile> _files = VersionServiceProvider.Version.ListFiles();

            leftLabel.ItemsSource = _files;
            rightLabel.ItemsSource = _files;

            LeftVersion = new(VersionServiceProvider.Version.ListVersions().ToList());
            RightVersion = new(VersionServiceProvider.Version.ListVersions().ToList());

            CloseDialogCommand = new(ClosingDialog, () => acceptButton.IsEnabled);
        }

        private void LeftFetchFiles(Object sender, SelectionChangedEventArgs e)
        {
            leftLabel.IsEnabled = false;
            acceptButton.IsEnabled = false;

            leftLabel.ItemsSource = VersionServiceProvider.Version.ListFiles((string)LeftVersion.CurrentItem);
            if (leftLabel.ItemsSource is not null)
            {
                leftLabel.IsEnabled = true;
                acceptButton.IsEnabled = true;
                leftLabel.SelectedIndex = 0;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        private void RightFetchFiles(Object sender, SelectionChangedEventArgs e)
        {
            rightLabel.IsEnabled = false;
            acceptButton.IsEnabled = false;

            rightLabel.ItemsSource = VersionServiceProvider.Version.ListFiles((string)RightVersion.CurrentItem);
            if (rightLabel.ItemsSource is not null)
            {
                rightLabel.IsEnabled = true;
                acceptButton.IsEnabled = true;
                rightLabel.SelectedIndex = 1;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
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
