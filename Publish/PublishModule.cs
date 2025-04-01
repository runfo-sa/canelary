using Core.Events;
using Core.Models;
using Core.Services;

namespace Publish;

[Module(ModuleName = "Publicar", OnDemand = true)]
public class PublishModule(IEventAggregator eventAggregator) : IModule
{
    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private IContainerProvider? _container;

    public void OnInitialized(IContainerProvider containerProvider)
    {
        _container = containerProvider;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance(new ModuleLoaderService("Publicar", CreateWindow));
    }

    private void CreateWindow(string name)
    {
        _eventAggregator.GetEvent<SendModuleEvent>().Publish(new ModuleTab("Publicar", _container?.Resolve<Views.Publish>()!));
    }
}