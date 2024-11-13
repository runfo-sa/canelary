namespace Editor.Models
{
    public record struct LintingInfo(int Offset, int Length, string Message)
    {
        public static LintingInfo Parse(string content)
        {
            var fields = content.Split('|');
            return new LintingInfo(int.Parse(fields[0]), int.Parse(fields[1]), fields[4]);
        }
    }
}