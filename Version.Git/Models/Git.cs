using Core.FileTree;
using Core.Services;
using System.IO;

namespace VersionGit.Models
{
    public class Git : IVersion
    {
        public string? FolderPath { get; set; }

        public IEnumerable<string> ListVersions(IFile? file = null)
        {
            return GitInner.RunGitCommand(
                 "for-each-ref",
                 "--format=\"%(refname:short)|%(creatordate:format:%Y/%m/%d %I:%M)|%(subject)\\n\" \"refs/tags/*\"",
                 Settings.Instance.EtiquetasDir)
             .Message
             .Split("\\n", StringSplitOptions.RemoveEmptyEntries)
             .Select(s => GitTag.Parse(s).Tag)
             .Prepend("Local");
        }

        public IEnumerable<IFile> ListFiles(string version = "Local")
        {
            if (version != "Local")
            {
                return LoadGitFiles(version);
            }
            return Directory
                .GetFiles(Settings.Instance.EtiquetasDir, $"*.{SettingsService.Instance.Extension}")
                .Select(f => new LabelFile(f));
        }

        public static IEnumerable<LabelFile> LoadGitFiles(string tag)
        {
            string path = Path.Combine(Path.GetTempPath(), $"Visual Ternera - {tag}");
            Directory.CreateDirectory(path);

            if (GitInner.RunGitCommand("tag", "--points-at HEAD", path).Message != tag)
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

        public bool SaveFile(string path, string content)
        {
            new LabelFile(path).Write(content);
            return true;
        }
    }
}
