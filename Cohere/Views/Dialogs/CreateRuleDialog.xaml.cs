using Cohere.Models;
using Core.Database.Model;
using Core.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace Cohere.Views
{
    public partial class CreateRuleDialog : UserControl, IDialogAware
    {
        public string Title => "Crear Regla";

        public ObservableCollection<ReglaAtributo> Atributos { get; set; } = [];
        public List<string> Etiquetas { get; set; }
        public List<string> AtributosOpciones { get; set; }

        private string _reglaNombre = string.Empty;
        public string ReglaNombre
        {
            get => _reglaNombre;
            set
            {
                _reglaNombre = value;
                CloseDialogCommand.RaiseCanExecuteChanged();
            }
        }

        public string Etiqueta { get; set; } = null!;

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        public CreateRuleDialog()
        {
            InitializeComponent();
            DataContext = this;

            Etiquetas = [.. Directory
                .GetFiles(SettingsService.Instance.EtiquetasDir, $"*.{SettingsService.Instance.EtiquetasExtension}")
                .Select(p => Path.GetFileNameWithoutExtension(p))
            ];

            // TODO!: Pass this to a config file
            AtributosOpciones = [
                "Codigo Senasa",
                "Temperatura",
                "Traducciones - Aleman",
                "Traducciones - Ingles",
                "Traducciones - Italiano",
                "Traducciones - Frances",
                "Traducciones - Español",
                "Traducciones - Portugues",
                "Traducciones - Ruso Metro",
                "Traducciones - Ruso",
                "Traducciones - Libre",
                "Traducciones - Chino Hex",
                "Traducciones - Chino",
                "EAN",
                "Definiciones Cuartos - Aleman",
                "Definiciones Cuartos - Ingles",
                "Definiciones Cuartos - Italiano",
                "Definiciones Cuartos - Frances",
                "Definiciones Cuartos - Español",
                "Definiciones Cuartos - Portugues",
                "Definiciones Cuartos - Ruso Metro",
                "Definiciones Cuartos - Ruso",
                "Definiciones Cuartos - Libre",
                "Definiciones Cuartos - Chino Hex",
                "Definiciones Cuartos - Chino"
            ];

            CloseDialogCommand = new(ClosingDialog, () => !ReglaNombre.IsNullOrEmpty());
        }

        private void AddAttribute(Object sender, System.Windows.RoutedEventArgs e)
        {
            Atributos.Add(new ReglaAtributo());
        }

        private void ClosingDialog()
        {
            var cr = new CreateRuleResult(Etiqueta, ReglaNombre, Atributos);
            var result = new DialogResult
            {
                Parameters = { { "Result", cr } },
                Result = ButtonResult.OK
            };
            RequestClose.Invoke(result);
        }

        public Boolean CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
