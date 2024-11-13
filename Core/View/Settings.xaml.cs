using System.Windows.Controls;

namespace Core.View
{
    public partial class Settings : UserControl, IDialogAware
    {
        public static string Title => "Ajustes";

        public Settings()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DialogCloseListener RequestClose { get; }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed()
        { }

        public void OnDialogOpened(IDialogParameters parameters)
        { }
    }
}