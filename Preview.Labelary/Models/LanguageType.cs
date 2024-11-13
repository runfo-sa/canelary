using System.Runtime.Serialization;

namespace PreviewLabelary.Models
{
    public enum LanguageType
    {
        [EnumMember(Value = "Chinese")]
        Chinese = 'J',

        [EnumMember(Value = "Japanese")]
        Japanese = 'J',

        [EnumMember(Value = "Korean")]
        Korean = 'J',

        [EnumMember(Value = "Cyrillic")]
        Cyrillic = 'N',

        [EnumMember(Value = "Greek")]
        Greek = 'N',

        [EnumMember(Value = "Arabic")]
        Arabic = 'L'
    }
}