using Core.Services;
using TwinsBackend.Models;

namespace TwinsBackend
{
    [Module(ModuleName = "TwinsBackend#False", OnDemand = false)]
    public class TwinsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            BackendServiceProvider.Add(new Twins());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
