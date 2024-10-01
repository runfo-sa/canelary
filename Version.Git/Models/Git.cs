using Core.FileTree;
using Core.Services;
using System.Diagnostics;
using System.IO;
using Version.Git.Models;

namespace VersionGit.Models
{
    public class Git : IVersion
    {
        public string? FolderPath { get; set; }

        public static IEnumerable<string> ListVersions()
        {
            return GitInner.RunGitCommand(
                 "for-each-ref",
                 "--format=\"%(refname:short)|%(creatordate:format:%Y/%m/%d %I:%M)|%(subject)\\n\" \"refs/tags/*\"",
                 Settings.Instance.EtiquetasDir)
             .Split("\\n", StringSplitOptions.RemoveEmptyEntries)
             .Select(s => GitTag.Parse(s).Tag)
             .Prepend("Local");
        }

        public IEnumerable<IFile> ListFiles()
        {
            return Directory
                .GetFiles(Settings.Instance.EtiquetasDir, $"*.{SettingsService.Instance.Extension}")
                .Select(f => new LabelFile(f));
        }

        public (IEnumerable<IFile>, IEnumerable<String>) FetchFileVer(IFile? file = null, String version = "Local")
        {
            var versions = ListVersions();

            if (version == "Local")
            {
                return (ListFiles(), versions);
            }
            return (LoadGitFiles(version), versions);
        }

        public static IEnumerable<LabelFile> LoadGitFiles(string tag)
        {
            string path = Path.Combine(Path.GetTempPath(), $"Visual Ternera - tag");
            Directory.CreateDirectory(path);

            if (GitInner.RunGitCommand("tag", "--points-at HEAD", path) != tag)
            {
                GitInner.RunGitCommand("init", "", path);
                GitInner.RunGitCommand("remote add origin", Settings.Instance.GitRepo, path);
                GitInner.RunGitCommand("fetch", "--all --tags --prune", path);
                GitInner.RunGitCommand("checkout", $"tags/{tag}", path);
            }

            return Directory
                .GetFiles(path, $"*.{SettingsService.Instance.Extension}")
                .Select(f => new LabelFile(f));
        }

        public bool SaveFile(String path, String content)
        {
            new LabelFile(path).Write(content);
            return true;
        }

        public Boolean FetchByFile() => false;

        public void Publish()
        {
            Trace.WriteLine(FolderPath);
        }
    }
}
