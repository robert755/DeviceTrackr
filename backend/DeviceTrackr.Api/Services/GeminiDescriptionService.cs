using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DeviceTrackr.Api.Configuration;
using DeviceTrackr.Api.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeviceTrackr.Api.Services;

public class GeminiDescriptionService(
    HttpClient http,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    IOptions<GeminiOptions> options,
    ILogger<GeminiDescriptionService> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<(string? Text, string? ErrorHint)> GenerateDeviceDescriptionAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GeminiConfigHelper.ResolveApiKey(configuration, hostEnvironment.ContentRootPath);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = options.Value.ApiKey?.Trim();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (null, "API key is empty. Set Gemini:ApiKey in appsettings, or env Gemini__ApiKey, or user secrets.");
        }

        var model = configuration["Gemini:Model"]?.Trim();
        if (string.IsNullOrWhiteSpace(model))
        {
            model = string.IsNullOrWhiteSpace(options.Value.Model) ? "gemini-1.5-flash" : options.Value.Model.Trim();
        }

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var typeLabel = device.Type switch
        {
            DeviceType.Phone => "smartphone",
            DeviceType.Tablet => "tablet",
            DeviceType.Laptop => "laptop",
            _ => "device"
        };
        var prompt = new StringBuilder()
            .AppendLine("Write a short internal IT inventory description for this company-owned device.")
            .AppendLine("Requirements: 2-4 complete sentences, plain text only, no markdown, no bullet lists, professional but friendly tone.")
            .AppendLine("End each sentence properly. Use only the facts below; do not invent model numbers or prices.")
            .AppendLine()
            .AppendLine($"Name: {device.Name}")
            .AppendLine($"Manufacturer: {device.Manufacturer}")
            .AppendLine($"Form factor: {typeLabel}")
            .AppendLine($"OS: {device.OperatingSystem} {device.OsVersion}")
            .AppendLine($"Processor: {device.Processor}")
            .AppendLine($"RAM: {device.RamAmountGb} GB")
            .AppendLine()
            .AppendLine("Write the description in English.")
            .ToString();
        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.35,
                maxOutputTokens = 1024
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini HTTP request failed.");
            return (null, $"Network error calling Gemini: {ex.Message}");
        }

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var googleMsg = TryParseGoogleErrorMessage(responseText);
            var hint = googleMsg ?? $"HTTP {(int)response.StatusCode} from Gemini API.";
            logger.LogWarning("Gemini API error {Status}: {Body}", (int)response.StatusCode, responseText);
            return (null, hint);
        }

        try
        {
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;

            if (root.TryGetProperty("promptFeedback", out var feedback) &&
                feedback.TryGetProperty("blockReason", out var blockReason))
            {
                var reason = blockReason.GetString() ?? "unspecified";
                logger.LogWarning("Gemini blocked prompt: {Reason}. Body: {Body}", reason, responseText);
                return (null, $"Prompt was blocked ({reason}). Try different device text or check API safety settings.");
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                var err = TryParseGoogleErrorMessage(responseText) ?? "No candidates in response (empty or blocked).";
                logger.LogWarning("Gemini no candidates: {Body}", responseText);
                return (null, err);
            }

            var first = candidates[0];

            if (TryExtractCandidateText(first, out var extracted) && !string.IsNullOrWhiteSpace(extracted))
            {
                return (extracted.Trim(), null);
            }

            if (first.TryGetProperty("finishReason", out var finish))
            {
                var fr = finish.GetString() ?? finish.ToString();
                logger.LogWarning("Gemini no text, finishReason={Reason}. Body: {Body}", fr, responseText);
                return (null, $"Model returned no text (finishReason: {fr}).");
            }

            return (null, "Could not read generated text from Gemini response.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse Gemini response.");
            return (null, $"Invalid response from Gemini: {ex.Message}");
        }
    }

    private static bool TryExtractCandidateText(JsonElement candidate, out string? text)
    {
        text = null;
        if (!candidate.TryGetProperty("content", out var content))
        {
            return false;
        }

        if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
        {
            return false;
        }

        var sb = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textEl))
            {
                var segment = textEl.GetString();
                if (!string.IsNullOrEmpty(segment))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(segment);
                }
            }
        }

        text = sb.Length > 0 ? sb.ToString() : null;
        return !string.IsNullOrWhiteSpace(text);
    }

    private static string? TryParseGoogleErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err) && err.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
        }

        return null;
    }
}
