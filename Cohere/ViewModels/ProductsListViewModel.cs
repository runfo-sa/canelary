using Cohere.Models;
using Cohere.Services;
using Core.Database;
using Core.Database.IdeDbModels;
using Core.FileTree;
using Core.Models;
using Core.Services;
using Core.Services.BackendModel;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Cohere.ViewModels
{
    public class ProductsListViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        public ObservableCollection<Product> ProductsList { get; set; } = [];

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
            CommandService.RefreshListCommand.RegisterCommand(new DelegateCommand(() =>
            {
                if (CurrentLabel != null)
                {
                    CommandService.OpenItemCommand.Execute(CurrentLabel);
                }
            }));

            ChangeRuleCommand = new(() => ChangeRule(CurrentLabel!), () => CurrentLabel != null);
        }

        private void OpenItem(object? item)
        {
            if (item is not null && item is LabelFile file)
            {
                ProductsList.Clear();
                CurrentLabel = file;
                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(CurrentLabel.Name);
                    var result = BackendServiceProvider.Backend.GetProducts(labelName);
                    foreach (var product in result)
                    {
                        ProductsList.Add(product);
                    }

                    var ruleLabel = context.RuleLabel.FirstOrDefault(r => r.LabelName == labelName);
                    if (ruleLabel != null)
                    {
                        var rule = context.Rule.First(r => r.Id == ruleLabel.RuleId);
                        RuleName = rule.Name;

                        var attributes = context.RuleAttributes
                            .Where(r => r.RuleId == rule.Id)
                            .ToList();

                        foreach (var attribute in attributes)
                        {
                            if (!attribute.FixedValue.IsNullOrEmpty())
                            {
                                var regex = new Regex(attribute.FixedValue!);
                                attribute.Regex = regex;
                            }
                        }

                        var values = BackendServiceProvider.Backend.GetValues([.. ProductsList], attributes);
                        foreach (var val in values)
                        {
                            var prod = val.Key;
                            var dict = val.Value;

                            foreach (var reg in dict!)
                            {
                                var error = ProductError.None;
                                var attribute = attributes.Find(a => a.Name == reg.Key);

                                if (attribute?.Regex != null && reg.Value != null && !attribute.Regex.IsMatch(reg.Value))
                                {
                                    error = ProductError.Incoherent;
                                }
                                else if (reg.Value.IsNullOrEmpty())
                                {
                                    error = ProductError.Incomplete;
                                }

                                prod.Attributes.Add(new ProductReport(reg.Key, reg.Value, error, attribute?.Comments));
                                prod.Error = (error > prod.Error) ? error : prod.Error;
                            }
                        }
                    }
                    else
                    {
                        RuleName = "No hay regla aplicada";
                    }
                }

                var errorCount = ProductsList.Where(p => p.Error != ProductError.None).Count();
                CommandService.RefreshErrorCount.Execute(new ErrorCounter(errorCount, ProductsList.Count));
            }
        }

        private void ChangeRule(LabelFile file)
        {
            _dialogService.Show("SelectRuleDialog", result =>
            {
                if (result.Result != ButtonResult.OK)
                {
                    return;
                }

                var rc = (ChangeRuleResult)result.Parameters["Result"];
                using (var context = new IdeDbContext())
                {
                    var labelName = Path.GetFileNameWithoutExtension(file.Name);

                    if (rc.Remove)
                    {
                        var removeRule = context.RuleLabel.FirstOrDefault(r => r.LabelName == labelName);
                        if (removeRule != null)
                        {
                            context.RuleLabel.Remove(removeRule);
                            context.SaveChanges();
                            CommandService.OpenItemCommand.Execute(CurrentLabel);
                        }
                        return;
                    }

                    var ruleLabel = context.RuleLabel.FirstOrDefault(r => r.LabelName == labelName);
                    if (ruleLabel is not null)
                    {
                        ruleLabel.RuleId = (int)rc.Rule!;
                    }
                    else
                    {
                        context.RuleLabel.Add(new RuleLabel()
                        {
                            LabelName = labelName,
                            RuleId = (int)rc.Rule!
                        });
                    }

                    context.SaveChanges();
                    CommandService.OpenItemCommand.Execute(CurrentLabel);
                }
            });
        }
    }
}
