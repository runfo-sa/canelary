using Core.Logger;
using Core.Services;
using LabelaryPreview.Models;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace LabelaryPreview
{
    /// <summary>
    /// Servicio que se comunica con la API de Labelary,
    /// para analizar y generar la muestra visual de una etiqueta.
    /// </summary>
    public partial class Labelary(string content) : IPreview
    {
        private const string START_METADATA = "^FX Start Metadata#Labelary";

        private string _content = content;
        public string Content => _content;

        private readonly StringBuilder _error = new();
        public string Error => _error.ToString();

        private Metadata? _metadata;

        public IPreview LoadVariables()
        {
            List<KeyValuePair<string, string?>> dictionary = BackendServiceProvider.Backend.GetValues(_metadata?.ProductId ?? 1);

            int startIdx;
            int endIdx = _content.LastIndexOf("@]");

            while (endIdx > 0)
            {
                startIdx = _content.LastIndexOf("[@", endIdx);
                if (startIdx > 0)
                {
                    var key = _content[(startIdx + 2)..endIdx];
                    _content = _content.Replace($"[@{key}@]", ParseVariable(key, ref dictionary), StringComparison.CurrentCultureIgnoreCase);
                }
                endIdx = _content.LastIndexOf("@]", startIdx);
            }
            return this;
        }

        public IPreview ParseMetadata()
        {
            if (!HasMetadata())
            {
                return this;
            }

            var startIdx = _content.IndexOf(START_METADATA);
            var endIdx = _content.IndexOf("^FX End Metadata", startIdx);

            if (endIdx > startIdx)
            {
                var rawMetadata = _content[(startIdx + START_METADATA.Length)..endIdx]
                    .Trim()
                    .Replace("^FX ", "");

                var deserializer = new DeserializerBuilder().Build();
                _metadata = deserializer
                    .Deserialize<Metadata>(rawMetadata);

                if (_metadata.Languages is not null)
                {
                    foreach (var language in _metadata.Languages)
                    {
                        _content = language.ParseContent(_content);
                    }
                }
            }

            return this;
        }

        public bool HasMetadata()
        {
            return _content.Contains(START_METADATA);
        }

        public async Task<List<byte[]?>?> Build(string dpi, string size)
        {
            int labelsCount = RegexLabel().Matches(_content).Count;
            List<byte[]?> labels = [];

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

                for (int i = 0; i < labelsCount; i++)
                {
                    string uri = $"http://api.labelary.com/v1/printers/{dpi}dpmm/labels/{size}/{i}/";
                    using HttpResponseMessage response = await client.PostAsync(uri, body);
                    response.EnsureSuccessStatusCode();
                    var label = await response.Content.ReadAsByteArrayAsync();
                    labels.Add(label);
                }
                return labels;
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
                Logger.Log(err.Message);
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

        [GeneratedRegex("\\^XA")]
        private static partial Regex RegexLabel();

        private string ParseVariable(string key, ref List<KeyValuePair<string, string?>> dictionary)
        {
            var parts = key.Split(';');
            var reg = dictionary.Find(v => v.Key.Equals(parts[0], StringComparison.CurrentCultureIgnoreCase));
            if (reg.Value != null)
            {
                parts[0] = reg.Value;
            }
            else
            {
                _error.AppendLine($"Variable [@{parts[0]}@] no esta cargada para el producto");
                return "";
            }

            if (parts.Length > 1)
            {
                var functions = parts[1].Split('-');
                foreach (var func in functions)
                {
                    var function = func[..2];
                    switch (function)
                    {
                        case "FK":
                            parts[0] = Convert.ToDecimal((double)Convert.ToInt32(parts[0]) / 1000.0)
                                .ToString(func[2..]);
                            break;

                        case "FF":
                            parts[0] = DateTime
                                .ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture)
                                .ToString(func[2..]);
                            break;

                        case "FD":
                            parts[0] = Convert.ToDecimal(parts[0]).ToString(func[2..]);
                            break;

                        case "FR":
                            char padChar = func[2];
                            parts[0] = parts[0].PadLeft(Convert.ToInt32(func[3..]), padChar);
                            break;

                        case "FC":
                            parts[0] = (func[2..4] == "SI") ? parts[0].Replace(",", "") : parts[0].Replace(".", ",");
                            break;

                        case "FP":
                            parts[0] = (func[2..4] == "SI") ? parts[0].Replace(".", "") : parts[0].Replace(",", ".");
                            break;

                        case "FI":
                            parts[0] = BackendServiceProvider.Backend.GetTranslation(func[2] - '0', parts[0]);
                            break;
                    }
                }
            }

            return parts[0];
        }
    }
}
