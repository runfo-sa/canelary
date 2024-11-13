using Cohere.Models;
using Core.Database;
using Core.Database.IdeDbModels;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cohere.Views
{
    public partial class SelectRuleDialog : UserControl, IDialogAware
    {
        public static string Title => "Seleccionar Regla";

        public ObservableCollection<Rule> Rules { get; set; }
        public Rule? SelectedRule { get; set; }
        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }
        public DelegateCommand RemoveRuleCommand { get; private set; }

        public SelectRuleDialog()
        {
            InitializeComponent();
            DataContext = this;
            using (var context = new IdeDbContext())
            {
                Rules = [.. context.Rule];
            }

            CloseDialogCommand = new(() => ClosingDialog());
            RemoveRuleCommand = new(() => ClosingDialog(true));
        }

        private void ClosingDialog(bool removeRule = false)
        {
            var rc = new ChangeRuleResult(removeRule, SelectedRule?.Id);
            var result = new DialogResult
            {
                Parameters = new DialogParameters { { "Result", rc } },
                Result = ButtonResult.OK
            };
            RequestClose.Invoke(result);
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed()
        { }

        public void OnDialogOpened(IDialogParameters parameters)
        { }
    }
}