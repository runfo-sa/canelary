using Core.Events;

namespace Core.Services;

/// <summary>
/// Servicio encargado de vincular los modulos con una sola instancia correspondiente.
/// </summary>
public class ModuleLoaderService
{
    private static readonly Lazy<IEventAggregator> LazyEventAggregator =
        new(() => ContainerLocator.Container.Resolve<IEventAggregator>());

    private static IEventAggregator EventAggregator => LazyEventAggregator.Value;

    public ModuleLoaderService(string moduleName, Action<string> action)
    {
        EventAggregator
            .GetEvent<LoadModuleEvent>()
            .Subscribe(action, ThreadOption.UIThread, true, name => name == moduleName);
    }
}