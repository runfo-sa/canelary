namespace VersionGit.Models
{
    public record struct ProcessRecord(int ExitCode, string Message, string Error);
}
