using Comparator.Views;

using Core.Events;
using Core.Models;
using Core.Services;

namespace Comparator;

[Module(ModuleName = "Comparar", OnDemand = true)]
public class ComparatorModule(IEventAggregator eventAggregator) : IModule
{
    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private IContainerProvider? _container;

    public void OnInitialized(IContainerProvider containerProvider)
    {
        _container = containerProvider;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance(new ModuleLoaderService("Comparar", CreateWindow));
        containerRegistry.RegisterDialog<SelectLabelsDialog>();
    }

    private void CreateWindow(string name)
    {
        _eventAggregator.GetEvent<SendModuleEvent>().Publish(new ModuleTab("Comparador", _container?.Resolve<Views.Comparator>()!));
    }
}