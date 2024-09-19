using System.IO;
using YamlDotNet.Serialization;

namespace Version.Git.Models
{
    public class Settings
    {
        [YamlMember(Description = " - Dirección de etiquetas")]
        public required string EtiquetasDir { get; set; }

        [YamlMember(Description = " - Repositorio donde se almacenan globalmente las etiquetas")]
        public required string GitRepo { get; set; }

        private static readonly Lazy<Settings> Lazy =
            new(() =>
            {
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<Settings>(File.ReadAllText("Modules\\Version.Git.Settings.yaml"));
            });

        public static Settings Instance => Lazy.Value;

        public static void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Instance);
            File.WriteAllText("Modules\\Version.Git.Settings.yaml", yaml);
        }
    }
}
