using Core.Services.SettingsModel;
using System.IO;
using YamlDotNet.Serialization;

namespace Core.Services
{
    public class SettingsService
    {
        [YamlMember(Description = " - Dirección de etiquetas")]
        public required string EtiquetasDir { get; set; }

        [YamlMember(Description = " - Cadena de conexión con la base de datos SQL Server")]
        public required string SqlConnection { get; set; }

        [YamlMember(Description = " - Dirección al archivo template de RE-CAL-22")]
        public string? RecallTemplate { get; set; }

        [YamlMember(Description = " - Tema del editor, posibles modos: dark, light")]
        public Theme Theme { get; set; } = Theme.Dark;

        [YamlMember(Description = " - Region a usar por el programa, para darle formato a las fechas y numeros")]
        public string Culture { get; set; } = "es-MX";

        [YamlMember(Description = " - Repositorio donde se almacenan globalmente las eitquetas")]
        public required string GitRepo { get; set; }

        [YamlMember(Description = " - Extensión de los archivos de etiqueta, sirve para filtrar")]
        public string EtiquetasExtension { get; set; } = "e01";

        [YamlMember(Description = " - Lista de directorios virtuales, por los cuales se deberian filtrar la carpeta de etiquetas")]
        public required List<FilterDirectory> VirtualDirectories { get; set; }

        [YamlMember(Description = " - Modulo utilizado para generar la preview de etiquetas")]
        public string PreviewEngine { get; set; } = "Labelary";

        [YamlMember(Description = " - Backend utilizado")]
        public string Backend { get; set; } = "Twins";

        private static readonly Lazy<SettingsService> Lazy =
            new(() =>
            {
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<SettingsService>(File.ReadAllText("Settings.yaml"));
            });

        public static SettingsService Instance => Lazy.Value;

        public static void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Instance);
            File.WriteAllText("Settings.yaml", yaml);
        }
    }
}
