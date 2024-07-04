using Cohere.Models;
using Cohere.Services;
using Core.Database;
using Core.Database.Model;
using Core.FileTree;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;

namespace Cohere.ViewModels
{
    public class ProductsListViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        public ObservableCollection<ListarProductos> ProductsList { get; set; } = [];

        private LabelFile? _currentLabel;
        public LabelFile? CurrentLabel
        {
            get => _currentLabel;
            set
            {
                SetProperty(ref _currentLabel, value);
                ChangeRuleCommand.RaiseCanExecuteChanged();
            }
        }

        private string _ruleName = string.Empty;
        public string RuleName
        {
            get => _ruleName;
            set => SetProperty(ref _ruleName, value);
        }

        public ICommandService CommandService { get; }

        public DelegateCommand ChangeRuleCommand { get; private set; }

        public ProductsListViewModel(ICommandService commandService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            CommandService = commandService;
            CommandService.OpenItemCommand.RegisterCommand(new DelegateCommand<object?>(OpenItem));
            CommandService.CreateRuleCommand.RegisterCommand(new DelegateCommand<CreateRuleResult>(CreateRule));

            ChangeRuleCommand = new(() => ChangeRule(CurrentLabel!), () => CurrentLabel != null);
        }

        private void OpenItem(object? item)
        {
            if (item is not null && item is LabelFile file)
            {
                CurrentLabel = file;
                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(CurrentLabel.Name);
                    var param = new SqlParameter("@Etiqueta", labelName);
                    var result = context.Database
                        .SqlQueryRaw<ListarProductos>("ide.ListarProductos @Etiqueta", param)
                        .ToList();

                    ProductsList.Clear();
                    foreach (var product in result)
                    {
                        ProductsList.Add(product);
                    }

                    var rule = context.Reglas.FirstOrDefault(r => r.Etiqueta == labelName);
                    if (rule != null)
                    {
                        RuleName = rule.Nombre;
                        IEnumerable<ReglaAtributo> attributes = [.. context.ReglasAtributos];
                        foreach (var attrib in attributes)
                        {
                            var labelParam = new SqlParameter("@Etiqueta", labelName);
                            var attribParam = new SqlParameter("@Var", attrib.AtributoNombre);
                            var rc = context.Database
                                .SqlQueryRaw<ReglaKeyValue>("ide.BuscarReglaAtributo @Etiqueta, @Var", labelParam, attribParam)
                                .ToList();
                            foreach (var keyValue in rc)
                            {
                                var prod = ProductsList.First(p => p.Codigo == keyValue.Codigo);

                                if (attrib.EsAtributoEstatico && keyValue.Valor is not null && keyValue.Valor != attrib.ValorEstatico)
                                {
                                    prod.Error = ProductoError.Incoherente;
                                }
                                else if (keyValue.Valor is null)
                                {
                                    prod.Error = ProductoError.Incompleto;
                                }

                                prod.Valores.Add(new Valor(attrib.AtributoNombre, keyValue.Valor, prod.Error));
                            }
                        }
                    }
                    else
                    {
                        RuleName = "No Existente";
                    }
                }

                var errorCount = ProductsList.Where(p => p.Error != ProductoError.Ninguno).Count();
                CommandService.RefreshErrorCount.Execute(new ErrorCounter(errorCount, ProductsList.Count));
            }
        }

        private void CreateRule(CreateRuleResult result)
        {
            using (var context = new IdeDbContext())
            {
                var rule = context.Reglas.FirstOrDefault(r => r.Etiqueta == result.Label);
                if (rule is not null)
                {
                    rule.Nombre = result.Rule;
                }
                else
                {
                    rule = context.Reglas.Add(new Regla()
                    {
                        Nombre = result.Rule,
                        Etiqueta = result.Label
                    }).Entity;
                }
                context.SaveChanges();
                RuleName = result.Rule;

                foreach (var attr in result.Attributes)
                {
                    attr.Reglas_Id = rule.Id;
                    context.ReglasAtributos.Add(attr);
                }
                context.SaveChanges();
            }
        }

        private void ChangeRule(LabelFile file)
        {
            _dialogService.ShowDialog("SelectRuleDialog", result =>
            {
                if (result.Result != ButtonResult.OK)
                {
                    return;
                }

                var rc = (ChangeRuleResult)result.Parameters["Result"];
                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(file.Name);
                    var rule = context.Reglas.FirstOrDefault(r => r.Etiqueta == labelName);

                    if (rc.Remove)
                    {
                        if (rule is not null)
                        {
                            context.Reglas.Remove(rule);
                            context.SaveChanges();
                            CommandService.OpenItemCommand.Execute(CurrentLabel);
                        }
                        return;
                    }

                    if (rule is not null)
                    {
                        rule.Nombre = rc.Rule;
                    }
                    else
                    {
                        context.Reglas.Add(new Regla()
                        {
                            Nombre = rc.Rule,
                            Etiqueta = labelName
                        });
                    }

                    context.SaveChanges();
                    CommandService.OpenItemCommand.Execute(CurrentLabel);
                }
            });
        }
    }
}
