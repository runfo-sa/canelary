using System.IO;
using System.Windows.Controls;

using Cohere.Models;

using Core.Services;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Cohere.Views;

public partial class GenerateRecallDialog : UserControl, IDialogAware
{
    public static string Title => "Generar RE-CAL-22";

    private IEnumerable<ProductoMuestra> _products = null!;

    public DelegateCommand CloseDialogCommand => new(() =>
    {
        if (SettingsService.Instance.ReportTemplate is not null)
        {
            var dict = new Dictionary<string, string>()
            {
                {"destino", destino.Text},
                {"autor", autor.Text},
                {"detalle", detalle.Text},
                {"etiqueta_after", etiqueta_after.Text},
                {"etiqueta_before", etiqueta_before.Text},
                {"fecha_solicitud", fecha_solicitud.Text},
                {"impresora", impresora.Text},
                {"motivo", motivo.Text},
                {"observaciones", observaciones.Text},
                {"solicitado", solicitado.Text}
            };

            var timestamp = DateTime.Now.ToString("yyMMdd_HH-mm-ss");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"RE-CAL-22_{timestamp}.docx");
            File.Copy(SettingsService.Instance.ReportTemplate, path, true);

            using var doc = WordprocessingDocument.Open(path, true);

            foreach (var bookmark in doc.MainDocumentPart!.RootElement!.Descendants<BookmarkStart>())
            {
                if (dict.TryGetValue(bookmark.Name!, out var value))
                {
                    bookmark.Parent?.AppendChild(new Paragraph(new Run(new Text(value).ToList()).ToList()));
                }
            }

            var table = doc.MainDocumentPart.RootElement.Descendants<Table>().Last();
            foreach (var prod in _products)
            {
                var row = new TableRow();
                var cell1 = new TableCell(new Paragraph(new Run(new Text(prod.Senasa).ToList()).ToList()).ToList());
                var cell2 = new TableCell(new Paragraph(new Run(new Text(prod.Name))));
                var cell3 = new TableCell(new Paragraph(new Run(new Text(prod.Code))));

                row.Append(cell1, cell2, cell3);
                table.Append(row);
            }

            doc.Save();
        }

        RequestClose.Invoke();
    });

    public DialogCloseListener RequestClose { get; }

    public GenerateRecallDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Boolean CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue("Products", out IEnumerable<ProductoMuestra>? products) && products is not null)
        {
            _products = products;
        }
    }
}