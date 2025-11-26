using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

using Core.Database.ServiceDbModels;
using Core.Logger;
using Core.Services;

using Microsoft.IdentityModel.Tokens;

using PreviewLabelary.Models;

using YamlDotNet.Serialization;

namespace PreviewLabelary;

/// <summary>
/// Servicio que se comunica con la API de Labelary,
/// para analizar y generar la muestra visual de una etiqueta.
/// </summary>
public partial class Labelary(string content) : IPreview
{
    private const string START_METADATA = "^FX Start Metadata#Labelary";
    private const int MAX_RETRIES = 5;

    public string Content { get; private set; } = content;

    private StringBuilder _error = new();
    public string Error => _error.ToString();

    private Metadata? _metadata;
    private readonly HttpClient _httpClient = new();

    public IPreview LoadVariables<T>(int? id = null, T? extraData = null) where T : class
    {
        Content = BackendServiceProvider.Backend.LoadVariables(Content, id ?? _metadata?.ProductId ?? 0, ref _error, extraData);
        return this;
    }

    public IPreview LoadVariables(int? id = null)
    {
        return LoadVariables<object>(id, null);
    }

    public IPreview ParseMetadata()
    {
        if (!HasMetadata())
        {
            return this;
        }

        var startIdx = Content.IndexOf(START_METADATA);
        var endIdx = Content.IndexOf("^FX End Metadata", startIdx);

        if (endIdx > startIdx)
        {
            var rawMetadata = Content[(startIdx + START_METADATA.Length)..endIdx]
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
                        Content = language.ParseContent(Content);
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
        return Content.Contains(START_METADATA);
    }

    public async Task<List<byte[]?>?> Build(string dpi, string size)
    {
        int labelsCount = RegexLabel().Count(Content);
        List<byte[]?> labels = [];
        using StringContent body = new(
            Content,
            Encoding.ASCII,
            "application/x-www-form-urlencoded"
        );

        for (int i = 0; i < labelsCount; i++)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    string uri = $"http://api.labelary.com/v1/printers/{dpi}dpmm/labels/{size}/{i}/";
                    using HttpResponseMessage response = await _httpClient.PostAsync(uri, body);

                    if (response.IsSuccessStatusCode)
                    {
                        var label = await response.Content.ReadAsByteArrayAsync();
                        labels.Add(label);
                        break;
                    }
                    else if (retryCount < MAX_RETRIES)
                    {
                        retryCount++;

                        TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                        if (response.Headers.RetryAfter?.Delta.HasValue == true)
                        {
                            delay = response.Headers.RetryAfter.Delta.Value;
                        }

                        Trace.TraceWarning($"Request failed with status code {response.StatusCode}. Retrying in {delay.TotalSeconds} seconds...");
                        await Task.Delay(delay);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex) when (retryCount < MAX_RETRIES)
                {
                    retryCount++;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    Trace.TraceWarning($"Request failed: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");
                    await Task.Delay(delay);
                }
            }
        }

        return labels.IsNullOrEmpty() ? null : labels;
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