using Comparator.Models;
using Core.Events;
using Core.Models;
using Core.Services.VersionModel;
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

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        private static readonly Lazy<IEventAggregator> _lazyEventAggregator =
            new(() => ContainerLocator.Container.Resolve<IEventAggregator>());

        private static IEventAggregator EventAggregator => _lazyEventAggregator.Value;

        public SelectLabelsDialog()
        {
            InitializeComponent();
            DataContext = this;

            CloseDialogCommand = new(ClosingDialog, () => acceptButton.IsEnabled);

            EventAggregator
             .GetEvent<RecvFilesEvent>()
             .Subscribe(RecvFiles);
        }

        private static void ClosingDialog()
        {
            EventAggregator.GetEvent<SendFilesEvent>().Publish();
        }

        private void RecvFiles(ComparasionFiles files)
        {
            var sr = new SelectionResult(
                files.Left,
                files.Right,
                (LabelDpi)DpiList.CurrentItem,
                (LabelSize)SizeList.CurrentItem
            );

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
