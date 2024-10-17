using Core.Services;
using System.IO;
using VersionGit.Models;
using VersionGit.Views;

namespace VersionGit
{
    [Module(ModuleName = "Git", OnDemand = false)]
    public class GitModule(IRegionManager regionManager) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            VersionServiceProvider.Set(new Git());
            SetRepository();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            VersionServiceProvider.SetView("Git", "Main#VersionRegion", typeof(VersionView), _regionManager);
            VersionServiceProvider.SetView("Git", "Publicar#Region", typeof(PublishView), _regionManager);
            VersionServiceProvider.SetView("Git", "Comparator#Selector", typeof(CompareSelectorView), _regionManager);
            containerRegistry.RegisterDialog<PushView>();
            containerRegistry.RegisterDialog<CreateBranch>();
        }

        private static void SetRepository()
        {
            if (Path.Exists(Settings.Instance.EtiquetasDir) &&
                GitInner.RunGitCommand("status", "", Settings.Instance.EtiquetasDir).ExitCode == 0)
            {
                return;
            }

            Array.ForEach(Directory.GetDirectories(Settings.Instance.EtiquetasDir), Directory.Delete);
            Array.ForEach(Directory.GetFiles(Settings.Instance.EtiquetasDir), File.Delete);

            GitInner.RunGitCommand("clone", Settings.Instance.GitRepo, Settings.Instance.EtiquetasDir);
        }
    }
}
