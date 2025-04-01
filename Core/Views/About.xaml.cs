using System.Reflection;
using System.Windows.Controls;

namespace Core.Views;

public partial class About : UserControl, IDialogAware
{
    public static string Title => "Acerca de Canelary";
    public DelegateCommand CloseDialogCommand => new(() => RequestClose.Invoke());
    public DialogCloseListener RequestClose { get; }

    public About()
    {
        InitializeComponent();
        DataContext = this;

        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
            .Select(x => x.InformationalVersion)
            .First();

        verionText.Text = $"Versión: {version}";
    }

    public Boolean CanCloseDialog() => true;

    public void OnDialogClosed()
    { }

    public void OnDialogOpened(IDialogParameters parameters)
    { }
}