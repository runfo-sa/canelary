namespace Core.Models
{
    public record struct LabelDpi(string Display, string Value);

    public static class DpiConstants
    {
        public static List<LabelDpi> All =>
        [
            new LabelDpi("6dpmm (150 dpi)","6"),
            new LabelDpi("8dpmm (203 dpi)","8"),
            new LabelDpi("12dpmm (300 dpi)","12"),
            new LabelDpi("24dpmm (600 dpi)","24"),
        ];
    }
}