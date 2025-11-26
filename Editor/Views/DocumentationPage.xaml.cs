using System.Text;
using System.Windows.Controls;

using Editor.Models;

using ComboBox = Editor.Models.ComboBox;
using TextBox = Editor.Models.TextBox;

namespace Editor.Views;

public partial class DocumentationPage : UserControl
{
    public DocumentationPage(ZplCommand command)
    {
        InitializeComponent();
        DataContext = this;

        CommandName = command.Name;
        LongDesc = command.LongDesc;
        ShortDesc = command.ShortDesc;
        Category = command.Category;

        var prm = new StringBuilder();

        foreach (var param in command.Parameters ?? [])
        {
            if (param is TextBox tb)
            {
                prm.Append($"{tb.Name} - {tb.Description}:\r\n\t{tb.AcceptedValue}\r\n");
            }
            else if (param is NumericBox nb)
            {
                prm.Append($"{nb.Name} - {nb.Description}:\r\n\t{nb.AcceptedValue}\r\n");
            }
            else if (param is ComboBox cb)
            {
                prm.Append($"{cb.Name} - {cb.Description}:\r\n");
                foreach (var p in cb.Values)
                {
                    prm.Append($"\t- {p.Value} = {p.Text}\r\n");
                }
            }
            prm.AppendLine();
        }

        Parameters = prm.ToString();
    }

    public string CommandName { get; }
    public string LongDesc { get; }
    public string ShortDesc { get; }
    public string Category { get; }
    public string Parameters { get; }
}