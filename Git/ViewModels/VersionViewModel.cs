using System.IO;
using Version.Git.Models;
using VersionGit.Models;

namespace VersionGit.ViewModels
{
    public class VersionViewModel : BindableBase
    {
        public GitTag GitTag { get; set; }
        public string GitTagUri { get; set; } = string.Empty;

        public VersionViewModel()
        {
            var tag = GitInner.GetLastTag();
            if (tag is not null)
            {
                GitTag = (GitTag)tag;
                GitTagUri = Path.Combine(Settings.Instance.GitRepo, $"releases/tag/{GitTag.Tag}");
            }
        }
    }
}
