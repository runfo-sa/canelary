using System.Diagnostics;
using System.Text;
using Version.Git.Models;

namespace VersionGit.Models
{
    public static class GitInner
    {
        public static string RunGitCommand(string command, string args, string workingDirectory)
        {
            StringBuilder sb = new();

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"{command} {args}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                }
            };
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                sb.Append($"{proc.StandardOutput.ReadLine()}");
            }

            proc.WaitForExit();
            return sb.ToString();
        }

        /// <summary>
        /// Devuelve la metadata de la ultima version publicada en Git.
        /// </summary>
        public static GitTag? GetLastTag()
        {
            var tags = RunGitCommand(
                "for-each-ref",
                "--format=\"%(refname:short)|%(creatordate:format:%Y/%m/%d %I:%M)|%(subject)\\n\" \"refs/tags/*\"",
                Settings.Instance.EtiquetasDir)
                .Split("\\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(GitTag.Parse);

            return tags.LastOrDefault();
        }
    }
}
