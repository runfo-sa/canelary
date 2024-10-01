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

        public IFile LeftLabel => (IFile)leftLabel.SelectedItem;
        public IFile RightLabel => (IFile)rightLabel.SelectedItem;

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        public SelectLabelsDialog()
        {
            InitializeComponent();
            DataContext = this;

            CloseDialogCommand = new(ClosingDialog, () => acceptButton.IsEnabled);

            var (files, versions) = VersionServiceProvider.Version.FetchFileVer();
            leftLabel.ItemsSource = files;
            leftVersion.ItemsSource = versions;
            rightLabel.ItemsSource = files;
            rightVersion.ItemsSource = versions;

            if (VersionServiceProvider.Version.FetchByFile())
            {
                leftLabel.SelectionChanged += LeftFetchFiles;
                rightLabel.SelectionChanged += RightFetchFiles;
            }
            else
            {
                leftVersion.SelectionChanged += LeftFetchFiles;
                rightVersion.SelectionChanged += RightFetchFiles;
            }
        }

        private void LeftFetchFiles(Object sender, SelectionChangedEventArgs e)
        {
            var (files, versions) = VersionServiceProvider.Version.FetchFileVer(LeftLabel, (string)leftVersion.SelectedItem);
            if (VersionServiceProvider.Version.FetchByFile())
            {
                leftVersion.ItemsSource = versions;
            }
            else
            {
                leftLabel.ItemsSource = files;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        private void RightFetchFiles(Object sender, SelectionChangedEventArgs e)
        {
            var (files, versions) = VersionServiceProvider.Version.FetchFileVer(RightLabel, (string)rightVersion.SelectedItem);
            if (VersionServiceProvider.Version.FetchByFile())
            {
                rightVersion.ItemsSource = versions;
            }
            else
            {
                rightLabel.ItemsSource = files;
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
