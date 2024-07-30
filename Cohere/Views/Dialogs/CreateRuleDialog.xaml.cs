using Core.Database;
using Core.Database.IdeDbModels;
using Core.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace Cohere.Views
{
    public partial class CreateRuleDialog : UserControl, IDialogAware
    {
        public static string Title => "Crear Regla";

        public ObservableCollection<RuleAttributes> Attributes { get; set; } = [];
        public List<string> Etiquetas { get; set; }
        public List<string> AttributesList { get; set; }

        private string _ruleName = string.Empty;
        public string RuleName
        {
            get => _ruleName;
            set
            {
                _ruleName = value;
                CloseDialogCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; }
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

            AttributesList = BackendServiceProvider.Backend.GetAttributes();

            CloseDialogCommand = new(ClosingDialog, () => !RuleName.IsNullOrEmpty());
        }

        private void AddAttribute(Object sender, System.Windows.RoutedEventArgs e)
        {
            Attributes.Add(new RuleAttributes());
        }

        private void ClosingDialog()
        {
            if (CreateRule(Etiqueta, RuleName, Attributes, Description))
            {
                RequestClose.Invoke();
            }
        }

        private bool CreateRule(string label, string ruleName, IEnumerable<RuleAttributes> attributes, string? description)
        {
            using (var context = new IdeDbContext())
            {
                if (context.Rule.FirstOrDefault(r => r.Name == ruleName) != null)
                {
                    Trace.WriteLine("Regla ya existente");
                    return false;
                }

                var rule = context.Rule.Add(new Rule()
                {
                    Name = ruleName,
                    Description = description
                }).Entity;
                context.SaveChanges();

                var ruleLabel = context.RuleLabel.FirstOrDefault(r => r.LabelName == label);
                if (ruleLabel != null)
                {
                    ruleLabel.RuleId = rule.Id;
                }
                else
                {
                    ruleLabel = context.RuleLabel.Add(new RuleLabel()
                    {
                        LabelName = label,
                        RuleId = rule.Id
                    }).Entity;
                }
                context.SaveChanges();

                foreach (var attr in Attributes)
                {
                    attr.RuleId = rule.Id;
                    context.RuleAttributes.Add(attr);
                }
                context.SaveChanges();
            }

            return true;
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
