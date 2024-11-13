using Core.Services;
using Microsoft.IdentityModel.Tokens;
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
            if (Settings.Instance.GitRepo.IsNullOrEmpty())
            {
                GitInner.EnableGit = false;
            }
            else
            {
                SetRepository();
            }
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
            if (Path.Exists(Settings.Instance.EtiquetasDir))
            {
                try
                {
                    var rc = GitInner.RunGitCommand("status", "", Settings.Instance.EtiquetasDir);
                    if (rc.ExitCode == 0)
                    {
                        return;
                    }
                }
                catch
                {
                    GitInner.EnableGit = false;
                    return;
                }
            }

            var parent = Directory.GetParent(Settings.Instance.EtiquetasDir);

            if (parent != null)
            {
                if (Path.Exists(Settings.Instance.EtiquetasDir))
                {
                    Directory.Delete(Settings.Instance.EtiquetasDir, true);
                }

                GitInner.RunGitCommand("clone", Settings.Instance.GitRepo, parent.FullName);
            }
        }
    }
}