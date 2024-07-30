using Core.Database;
using Core.Database.IdeDbModels;
using Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cohere.Views
{
    public partial class AlterRuleDialog : UserControl, IDialogAware
    {
        public static string Title => "Modificar Regla";

        public ObservableCollection<RuleAttributes> Attributes { get; set; } = [];
        public List<string> AttributesList { get; set; }
        public List<Rule> Rules { get; set; }
        public Rule Rule { get; set; }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; }
        }

        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }

        public AlterRuleDialog()
        {
            InitializeComponent();
            DataContext = this;

            using var context = new IdeDbContext();
            Rules = [.. context.Rule];
            Rule = Rules[0];
            Description = Rule.Description;
            Attributes = [.. context.RuleAttributes.Where(r => r.RuleId == Rule.Id)];
            AttributesList = BackendServiceProvider.Backend.GetAttributes();

            CloseDialogCommand = new(ClosingDialog);
        }

        private void AddAttribute(Object sender, System.Windows.RoutedEventArgs e)
        {
            Attributes.Add(new RuleAttributes() { RuleId = Rule.Id });
        }

        private void ClosingDialog()
        {
            using (var context = new IdeDbContext())
            {
                if (Rule.Description != Description)
                {
                    Rule.Description = Description;
                    context.Rule.Update(Rule);
                    context.SaveChanges();
                }

                foreach (var attr in Attributes)
                {
                    if (context.RuleAttributes.Contains(attr))
                    {
                        context.RuleAttributes.Update(attr);
                    }
                    else
                    {
                        context.RuleAttributes.Add(attr);
                    }
                }
                context.SaveChanges();
            }

            RequestClose.Invoke();
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }

        private void RuleChanged(Object sender, SelectionChangedEventArgs e)
        {
            Attributes.Clear();
            using var context = new IdeDbContext();
            IEnumerable<RuleAttributes> attributes = [.. context.RuleAttributes.Where(r => r.RuleId == Rule.Id)];
            foreach (var attr in attributes)
            {
                Attributes.Add(attr);
            }
        }
    }
}
