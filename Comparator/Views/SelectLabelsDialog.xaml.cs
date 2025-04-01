using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

using Comparator.Models;

using Core.Events;
using Core.Models;
using Core.Services.VersionModel;

using MaterialDesignThemes.Wpf;

namespace Comparator.Views;

public partial class SelectLabelsDialog : UserControl
{
    public static string Title => "Seleccionar Etiquetas";

    public ListCollectionView DpiList { get; set; } = new(DpiConstants.All);
    public ListCollectionView SizeList { get; set; } = new(LabelSize.GetList(File.ReadAllText("SizeList.xml")));

    public DelegateCommand CloseDialogCommand { get; private set; }

    private static readonly Lazy<IEventAggregator> LazyEventAggregator =
        new(() => ContainerLocator.Container.Resolve<IEventAggregator>());

    private readonly string _dialogIdentifier;

    private static IEventAggregator EventAggregator => LazyEventAggregator.Value;

    public SelectLabelsDialog(string dialogIdentifier)
    {
        InitializeComponent();
        DataContext = this;

        _dialogIdentifier = dialogIdentifier;
        CloseDialogCommand = new(ClosingDialog, () => acceptButton.IsEnabled);

        EventAggregator
         .GetEvent<RecvFilesEvent>()
         .Subscribe(RecvFiles, files => files.Id == dialogIdentifier);
    }

    private void ClosingDialog()
    {
        ContentGrid.Visibility = System.Windows.Visibility.Collapsed;
        LoadingBar.Visibility = System.Windows.Visibility.Visible;
        EventAggregator
            .GetEvent<SendFilesEvent>()
            .Publish(_dialogIdentifier);
    }

    private void RecvFiles(ComparasionFiles files)
    {
        var selection = new SelectionResult(
            files.Left,
            files.Right,
            (LabelDpi)DpiList.CurrentItem,
            (LabelSize)SizeList.CurrentItem
        );

        DialogHost.CloseDialogCommand.Execute(selection, null);
    }
}