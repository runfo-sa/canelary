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
            VersionServiceProvider.SetView("Git", "Main#VersionRegion", typeof(VersionView), _regionManager);
            VersionServiceProvider.SetView("Git", "Publicar#Region", typeof(PublishView), _regionManager);
        }
    }
}
