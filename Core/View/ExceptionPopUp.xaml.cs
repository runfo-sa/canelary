using AdonisUI.Controls;

namespace Core.View
{
    public partial class ExceptionPopUp : AdonisWindow
    {
        public ExceptionPopUp(string exceptionMessage)
        {
            InitializeComponent();
            errorMessage.Text = exceptionMessage;
        }

        private void Confirm(Object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
