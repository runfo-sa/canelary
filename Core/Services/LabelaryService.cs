using Core.Models;
using Core.Services.LabelaryModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using YamlDotNet.Serialization;

namespace Core.Services
{
    /// <summary>
    /// Servicio que se comunica con la API de Labelary,
    /// para analizar y generar la muestra visual de una etiqueta.
    /// </summary>
    public class LabelaryService(string content) : IPreviewService
    {
        private string _content = content;
        public string Content => _content;

        private readonly StringBuilder _error = new();
        public string Error => _error.ToString();

        public IPreviewService LoadVariables()
        {
            Dictionary<string, string> keyValues = BackendServiceProvider.Backend.GetValues();

            int startIdx;
            int endIdx = _content.LastIndexOf("@]");

            while (endIdx > 0)
            {
                startIdx = _content.LastIndexOf("[@", endIdx);
                if (startIdx > 0)
                {
                    var key = _content[(startIdx + 2)..endIdx].ToLower();
                    if (keyValues.TryGetValue(key, out var value))
                    {
                        _content = _content.Replace($"[@{key}@]", value, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        _content = _content.Replace($"[@{key}@]", "", StringComparison.CurrentCultureIgnoreCase);
                        _error.AppendLine($"Variable [@{key}@] no esta cargada para el producto");
                    }
                }
                endIdx = _content.LastIndexOf("@]", startIdx);
            }
            return this;
        }

        public IPreviewService LoadFonts()
        {
            var startIdx = _content.IndexOf("^FX Start Metadata");
            var endIdx = _content.IndexOf("^FX End Metadata");

            if (startIdx > 0 && endIdx > startIdx)
            {
                var rawMetadata = _content[(startIdx + "^FX Start Metadata".Length)..endIdx]
                    .Trim()
                    .Replace("^FX ", "");

                var deserializer = new DeserializerBuilder().Build();
                var metadata = deserializer
                    .Deserialize<LabelMetadata<LabelaryLanguage>>(rawMetadata);

                if (metadata.Languages is not null)
                {
                    foreach (var language in metadata.Languages)
                    {
                        _content = language.ParseContent(_content);
                    }
                }
            }

            return this;
        }

        public async Task<byte[]?> Build(string dpi, string size)
        {
            try
            {
                using HttpClient client = new()
                {
                    Timeout = TimeSpan.FromSeconds(10.0)
                };

                using StringContent body = new(
                    @_content,
                    Encoding.ASCII,
                    "application/x-www-form-urlencoded"
                );

                string uri = $"http://api.labelary.com/v1/printers/{dpi}dpmm/labels/{size}/0/";
                using HttpResponseMessage response = await client.PostAsync(uri, body);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception err)
            {
                Trace.TraceError(err.Message);
            }

            return null;
        }

        public async Task<string[]?> Linting(string codigo, string dpi, string size)
        {
            try
            {
                using HttpClient client = new()
                {
                    Timeout = TimeSpan.FromSeconds(10.0)
                };

                using StringContent body = new(
                    codigo,
                    Encoding.ASCII,
                    "application/x-www-form-urlencoded"
                );

                string uri = $"http://api.labelary.com/v1/printers/{dpi}dpmm/labels/{size}/0/";
                client.DefaultRequestHeaders.Add("X-Linter", "On");

                using HttpResponseMessage response = await client.PostAsync(uri, body);
                response.EnsureSuccessStatusCode();
                return WarningParse(response.Headers.GetValues("X-Warnings").First());
            }
            catch (Exception err)
            {
                Logger.Logger.Log(err.Message);
            }

            return null;
        }

        private static string[] WarningParse(string warnings)
        {
            int count = 0;
            int offset = 0;
            int oldOffset = 0;
            List<string> warningsList = [];

            foreach (char c in warnings)
            {
                if (c == '|') count++;
                if (count == 5)
                {
                    warningsList.Add(warnings[oldOffset..offset]);
                    oldOffset = offset + 1;
                    count = 0;
                }
                offset++;
            }

            return [.. warningsList];
        }
    }
}
