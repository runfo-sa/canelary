using Cohere.Models;
using Core.Database;
using Core.Database.Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cohere.Views
{
    public partial class SelectRuleDialog : UserControl, IDialogAware
    {
        public string Title => "Seleccionar Regla";

        public ObservableCollection<Regla> Reglas { get; set; }
        public Regla? SelectedRule { get; set; }
        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CloseDialogCommand { get; private set; }
        public DelegateCommand RemoveRuleCommand { get; private set; }

        public SelectRuleDialog()
        {
            InitializeComponent();
            DataContext = this;
            using (var context = new IdeDbContext())
            {
                Reglas = [.. context.Reglas];
            }

            CloseDialogCommand = new(() => ClosingDialog());
            RemoveRuleCommand = new(() => ClosingDialog(true));
        }

        private void ClosingDialog(bool removeRule = false)
        {
            var rc = new ChangeRuleResult(removeRule, SelectedRule?.Nombre ?? "");
            var result = new DialogResult
            {
                Parameters = new DialogParameters { { "Result", rc } },
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
