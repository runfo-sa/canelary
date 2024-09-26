namespace PreviewLabelary.Models
{
    public record struct Language(LanguageType LanguageType, char Letter)
    {
        public readonly string ParseContent(string content)
        {
            return LanguageType switch
            {
                LanguageType.Chinese | LanguageType.Japanese | LanguageType.Korean => content.Replace(
                    $"^A{Letter}",
                    $"^A{(char)LanguageType.Chinese}",
                    StringComparison.CurrentCultureIgnoreCase
                ),
                LanguageType.Cyrillic | LanguageType.Greek => content.Replace(
                    $"^A{Letter}",
                    $"^A{(char)LanguageType.Cyrillic}",
                    StringComparison.CurrentCultureIgnoreCase
                ),
                LanguageType.Arabic => content.Replace(
                    $"^A{Letter}",
                    $"^A{(char)LanguageType.Arabic}",
                    StringComparison.CurrentCultureIgnoreCase
                ),
                _ => content
            };
        }
    }
}
