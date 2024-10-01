using Core.Services;
using VersionDatabase.Models;
using VersionDatabase.Views;

namespace VersionDatabase
{
    [Module(ModuleName = "Database#False", OnDemand = false)]
    public class DatabaseModule(IRegionManager regionManager) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            VersionServiceProvider.Set(new Database());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            VersionServiceProvider.SetView("Database", "Main#VersionRegion", typeof(VersionView), _regionManager);
            VersionServiceProvider.SetView("Database", "Publicar#Region", typeof(PublishView), _regionManager);
        }
    }
}
