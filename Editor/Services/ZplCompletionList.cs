using System.IO;
using System.Xml.Serialization;

using Editor.Models;

namespace Editor.Services;

public static class ZplCompletionList
{
    private static Dictionary<string, ZplCompletionData>? _cache;

    public static Dictionary<string, ZplCompletionData> List => _cache ??= LoadList();

    private static Dictionary<string, ZplCompletionData> LoadList()
    {
        XmlSerializer serializer = new(typeof(Root));
        using StringReader reader = new(File.ReadAllText("zplHelp.xml").Replace("\\r\\n", Environment.NewLine));
        var root = (Root)serializer.Deserialize(reader)!;
        var dict = new Dictionary<string, ZplCompletionData>();
        foreach (var item in root.Commands)
        {
            dict.Add(item.Name, new ZplCompletionData(item));
        }
        return dict;
    }
}