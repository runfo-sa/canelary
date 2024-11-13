using System.IO;
using YamlDotNet.Serialization;

namespace VersionDatabase.Models
{
    public class Settings
    {
        [YamlMember(Description = " - Cadena de conexión con la base de datos SQL Server")]
        public string SqlConnection { get; set; }

        [YamlMember(Description = " - Esquema default en la base de datos")]
        public string Schema { get; set; }

        private static readonly Lazy<Settings> Lazy =
            new(() =>
            {
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<Settings>(File.ReadAllText("Modules\\Version.Database.Settings.yaml"));
            });

        public static Settings Instance => Lazy.Value;

        public static void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Instance);
            File.WriteAllText("Modules\\Version.Database.Settings.yaml", yaml);
        }
    }
}