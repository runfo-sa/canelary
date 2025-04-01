using System.Diagnostics;
using System.Windows.Controls;

using Main;

namespace VersionGit.Views;

public partial class VersionView : UserControl
{
    public VersionView()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(Object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception)
        {
            App.ResolveException(null,
                new UnhandledExceptionEventArgs(
                    new Exception("Problemas para abrir el enlace, asegurarse de configurar el link al repositorio."),
                    false));
        }
    }
}