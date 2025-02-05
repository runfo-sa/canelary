using System.Collections.ObjectModel;
using System.Windows.Controls;

using Core.Database;
using Core.Database.IdeDbModels;
using Core.Services;

using Microsoft.IdentityModel.Tokens;

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

        public string? Description { get; set; }

        public string Etiqueta { get; set; } = null!;

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public CreateRuleDialog()
        {
            InitializeComponent();
            DataContext = this;

            Etiquetas = [.. VersionServiceProvider.Version.ListFiles().Select(f => f.Name)];

            AttributesList = BackendServiceProvider.Backend.GetAttributes();

            CloseDialogCommand = new(ClosingDialog, () => !RuleName.IsNullOrEmpty());
            CancelCommand = new(() => RequestClose.Invoke(ButtonResult.Cancel));
        }

        private void AddAttribute(Object sender, System.Windows.RoutedEventArgs e)
        {
            Attributes.Add(new RuleAttributes());
        }

        private void ClosingDialog()
        {
            if (CreateRule(Etiqueta, RuleName, Description))
            {
                RequestClose.Invoke();
            }
        }

        private bool CreateRule(string label, string ruleName, string? description)
        {
            using (var context = new IdeDbContext())
            {
                if (context.Rule.FirstOrDefault(r => r.Name == ruleName) != null)
                {
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
                    context.RuleLabel.Add(new RuleLabel()
                    {
                        LabelName = label,
                        RuleId = rule.Id
                    });
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

        public void OnDialogClosed()
        { }

        public void OnDialogOpened(IDialogParameters parameters)
        { }
    }
}