using System.Windows;
using System.Windows.Controls;

namespace Editor.Views
{
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void CloseWindow(Object sender, System.Windows.RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}