using Core.Services;
using VersionGit.Models;
using VersionGit.Views;

namespace VersionGit
{
    [Module(ModuleName = "Git#False", OnDemand = false)]
    public class GitModule(IRegionManager regionManager) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            VersionServiceProvider.Set(new Git());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _regionManager.RegisterViewWithRegion("Main#VersionRegion", typeof(VersionView));
        }
    }
}
