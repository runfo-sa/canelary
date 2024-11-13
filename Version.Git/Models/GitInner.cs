using System.Diagnostics;
using System.Text;

namespace VersionGit.Models
{
    public static class GitInner
    {
        public static bool EnableGit { get; set; } = true;

        public static ProcessRecord RunGitCommand(string command, string args, string workingDirectory)
        {
            if (!EnableGit)
            {
                return new ProcessRecord(0, $"No git found|{DateTime.Now}|No git found", "No git found");
            }

            StringBuilder stdo = new();
            StringBuilder stde = new();

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"{command} {args}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                }
            };
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                stdo.Append($"{proc.StandardOutput.ReadLine()}");
            }

            while (!proc.StandardError.EndOfStream)
            {
                stde.Append($"{proc.StandardError.ReadLine()}");
            }

            proc.WaitForExit();
            return new ProcessRecord(proc.ExitCode, stdo.ToString(), stde.ToString());
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
                .Message
                .Split("\\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(GitTag.Parse);

            return tags.LastOrDefault();
        }
    }
}