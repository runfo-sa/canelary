using System.Reflection;
using System.Windows.Controls;

namespace Core.View
{
    public partial class About : UserControl, IDialogAware
    {
        public string Title => "Acerca de Visual Ternera";
        public DelegateCommand CloseDialogCommand => new(() => RequestClose.Invoke());
        public DialogCloseListener RequestClose { get; }

        public About()
        {
            InitializeComponent();
            DataContext = this;
            verionText.Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
