using Cohere.Models;
using Core.Services;
using Microsoft.Office.Interop.Word;
using System.IO;
using System.Windows.Controls;
using Range = Microsoft.Office.Interop.Word.Range;

namespace Cohere.Views
{
    public partial class GenerateRecallDialog : UserControl, IDialogAware
    {
        public static string Title => "Generar RE-CAL-22";

        private IEnumerable<ProductoMuestra> _products = null!;

        public DelegateCommand CloseDialogCommand => new(() =>
        {
            var wordApp = new Application();
            var doc = wordApp.Documents.Open(SettingsService.Instance.RecallTemplate);

            SetBookmark(doc, "destino", destino.Text);
            SetBookmark(doc, "autor", autor.Text);
            SetBookmark(doc, "detalle", detalle.Text);
            SetBookmark(doc, "etiqueta_after", etiqueta_after.Text);
            SetBookmark(doc, "etiqueta_before", etiqueta_before.Text);
            SetBookmark(doc, "fecha_solicitud", fecha_solicitud.Text);
            SetBookmark(doc, "impresora", impresora.Text);
            SetBookmark(doc, "motivo", motivo.Text);
            SetBookmark(doc, "observaciones", observaciones.Text);
            SetBookmark(doc, "solicitado", solicitado.Text);

            foreach (var prod in _products)
            {
                Row row = doc.Tables[2].Rows.Add();
                row.Cells[1].Range.Text = prod.Senasa;
                row.Cells[1].Range.Font.Bold = 0;
                row.Cells[2].Range.Text = prod.Name;
                row.Cells[2].Range.Font.Bold = 0;
                row.Cells[3].Range.Text = prod.Code;
                row.Cells[3].Range.Font.Bold = 0;
            }

            var timestamp = DateTime.Now.ToString("yyMMdd_HH-mm-ss");
            doc.SaveAs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"RE-CAL-22_{timestamp}.docx"));
            doc.Close();
            wordApp.Quit();
            RequestClose.Invoke();
        });

        public DialogCloseListener RequestClose { get; }

        public GenerateRecallDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static void SetBookmark(Document doc, string bookmark, string value)
        {
            Bookmark bkm = doc.Bookmarks[bookmark];
            Range range = bkm.Range;
            range.Text = value;
        }

        public Boolean CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("Products", out IEnumerable<ProductoMuestra>? products) && products is not null)
            {
                _products = products;
            }
        }
    }
}
