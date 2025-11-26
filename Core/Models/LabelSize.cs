using System.IO;
using System.Xml.Serialization;

namespace Core.Models;

public record struct LabelSize(string Display, string Value)
{
    public static LabelSize[] GetList(string content)
    {
        XmlSerializer serializer = new(typeof(SizeList));
        using StringReader reader = new(content);
        var list = (SizeList)serializer.Deserialize(reader)!;
        return list.Items;
    }
}

public class SizeList
{
    public LabelSize[] Items { get; set; } = [
            new LabelSize("Primaria - [10x8cm]", "4x3"),
            new LabelSize("Caja - [10x15cm]", "4x6"),
        ];
}