using Core.Models;
using System.Windows.Controls;
using System.Windows.Data;

namespace Editor.Views.Dialogs
{
    public partial class ResizeLabelDialog : UserControl, IDialogAware
    {
        public static string Title => "Cambiar Resolución";

        public string? LabelName { get; private set; }

        public ListCollectionView FromDpi { get; set; } = new(DpiConstants.All);
        public ListCollectionView ToDpi { get; set; } = new(DpiConstants.All);

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        public ResizeLabelDialog()
        {
            InitializeComponent();
            DataContext = this;

            CloseDialogCommand = new DelegateCommand(() =>
            {
                var result = new DialogResult
                {
                    Parameters = new DialogParameters
                    {
                        { "FromDpi", (LabelDpi)FromDpi.CurrentItem },
                        { "ToDpi", (LabelDpi)ToDpi.CurrentItem }
                    },
                    Result = ButtonResult.OK
                };
                RequestClose.Invoke(result);
            });
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("LabelName", out string? labelName) && labelName is not null)
            {
                LabelName = labelName;
            }
        }
    }
}
