using Core.Logger;
using Core.Services;
using PreviewLabelary.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace PreviewLabelary
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

        private StringBuilder _error = new();
        public string Error => _error.ToString();

        private Metadata? _metadata;

        public IPreview LoadVariables(int? id = null)
        {
            _content = BackendServiceProvider.Backend.LoadVariables(_content, id ?? _metadata?.ProductId ?? 0, ref _error);
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

                try
                {
                    _metadata = deserializer.Deserialize<Metadata>(rawMetadata);
                    if (_metadata.Languages is not null)
                    {
                        foreach (var language in _metadata.Languages)
                        {
                            _content = language.ParseContent(_content);
                        }
                    }
                }
                catch (Exception e)
                {
                    _error.AppendLine("Ocurrio el siguiente problema al parsear la metadata:");
                    _error.AppendLine(e.Message);
                    var inner = e.InnerException;
                    while (inner != null)
                    {
                        _error.AppendLine(inner.Message);
                        inner = inner.InnerException;
                    }
                    _error.AppendLine();
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
    }
}