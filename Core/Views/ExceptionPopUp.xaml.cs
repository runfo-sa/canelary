using System.Windows;

namespace Core.Views;

public partial class ExceptionPopUp : Window
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