using Core.FileTree;
using Core.Services;
using System.IO;
using Version.Git.Models;

namespace VersionGit.Models
{
    public class Git : IVersion
    {
        public IEnumerable<string> ListVersions()
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
                .GetFiles(Settings.Instance.EtiquetasDir, $"*.{SettingsService.Instance.EtiquetasExtension}")
                .Select(f => new LabelFile(f));
        }

        public IEnumerable<IFile>? ListFiles(string version)
        {
            if (version == "Local")
            {
                return ListFiles();
            }
            return LoadGitFiles(version);
        }

        private static IEnumerable<LabelFile> LoadGitFiles(string tag)
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
                .GetFiles(path, $"*.{SettingsService.Instance.EtiquetasExtension}")
                .Select(f => new LabelFile(f));
        }

        public void SaveFile(String path, String content)
        {
            new LabelFile(path).Write(content);
        }
    }
}
