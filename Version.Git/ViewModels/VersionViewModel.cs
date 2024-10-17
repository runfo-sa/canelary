using Core.Logger;
using System.IO;
using VersionGit.Models;

namespace VersionGit.ViewModels
{
    public class VersionViewModel : BindableBase
    {
        public GitTag GitTag { get; set; }
        public string GitTagUri { get; set; } = string.Empty;

        private bool _needsUpdate = false;
        public bool NeedsUpdate
        {
            get => _needsUpdate;
            set => SetProperty(ref _needsUpdate, value);
        }

        public DelegateCommand UpdateCommand { get; private set; }

        public VersionViewModel()
        {
            var tag = GitInner.GetLastTag();
            if (tag is GitTag gtag)
            {
                GitTag = gtag;
                GitTagUri = Path.Combine(Settings.Instance.GitRepo, $"releases/tag/{GitTag.Tag}");
            }

            CheckUpdate();
            UpdateCommand = new(Update);
        }

        private void CheckUpdate()
        {
            GitInner.RunGitCommand("fetch", "--all", Settings.Instance.EtiquetasDir);

            var branch = GitInner.RunGitCommand("branch", "--show-current", Settings.Instance.EtiquetasDir).Message;
            var localHead = GitInner.RunGitCommand("rev-parse", "HEAD", Settings.Instance.EtiquetasDir).Message;
            var remoteHead = GitInner.RunGitCommand("ls-remote", $"origin refs/heads/{branch}", Settings.Instance.EtiquetasDir)
                .Message
                .Split('\t')[0]
                .Trim();

            NeedsUpdate = (localHead != remoteHead);
        }

        private void Update()
        {
            if (!NeedsUpdate)
            {
                return;
            }

            var branch = GitInner.RunGitCommand("branch", "--show-current", Settings.Instance.EtiquetasDir).Message;
            var rc = GitInner.RunGitCommand("reset", $"--hard origin/{branch}", Settings.Instance.EtiquetasDir);
            if (rc.ExitCode != 0)
            {
                Logger.Log($"No se pudo actualizar el repositorio porque: {rc.Error}");
            }

            CheckUpdate();
        }
    }
}
