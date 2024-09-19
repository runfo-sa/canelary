using BackendTwins.Models;
using Core.Services;

namespace BackendTwins
{
    [Module(ModuleName = "TwinsBackend#False", OnDemand = false)]
    public class TwinsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            BackendServiceProvider.Set(new Twins());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
