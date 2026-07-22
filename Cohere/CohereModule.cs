using Cohere.Views;

using Core.Events;
using Core.Models;
using Core.Services;

namespace Cohere;

[Module(ModuleName = "Verificar", OnDemand = true)]
public class CohereModule(IRegionManager regionManager, IEventAggregator eventAggregator) : IModule
{
    private readonly IRegionManager _regionManager = regionManager;
    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private IContainerProvider? _container;

    public void OnInitialized(IContainerProvider containerProvider)
    {
        _container = containerProvider;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance(new ModuleLoaderService("Verificar", CreateWindow));
        containerRegistry.RegisterDialog<CreateRuleDialog>();
        containerRegistry.RegisterDialog<AlterRuleDialog>();
        containerRegistry.RegisterDialog<SelectRuleDialog>();
        containerRegistry.RegisterDialog<GenerateSample>();

        _regionManager.RegisterViewWithRegion("Cohere#MenuRegion", typeof(Menu));
        _regionManager.RegisterViewWithRegion("Cohere#TreeRegion", typeof(Tree));
        _regionManager.RegisterViewWithRegion("Cohere#ProductListRegion", typeof(ProductsList));
        _regionManager.RegisterViewWithRegion("Cohere#ProductReportRegion", typeof(Views.ProductReport));
    }

    private void CreateWindow(string name)
    {
        _eventAggregator.GetEvent<SendModuleEvent>().Publish(new ModuleTab("Verificador", _container?.Resolve<Views.Cohere>()!));
    }
}