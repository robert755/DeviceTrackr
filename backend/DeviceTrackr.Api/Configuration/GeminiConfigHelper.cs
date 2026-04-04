using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DeviceTrackr.Api.Configuration;

// If Gemini__ApiKey is set but empty, it overrides appsettings.json; we fall back to reading the JSON files from disk.
public static class GeminiConfigHelper
{
    private static readonly string[] EnvVarNames =
    [
        "GEMINI_API_KEY",
        "Gemini__ApiKey",
        "GOOGLE_API_KEY"
    ];

    private static readonly string[] AppSettingsFiles =
    [
        "appsettings.json",
        "appsettings.Development.json"
    ];

    public static string? ResolveApiKey(IConfiguration configuration, string contentRootPath)
    {
        foreach (var envName in EnvVarNames)
        {
            var v = Environment.GetEnvironmentVariable(envName)?.Trim();
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v;
            }
        }

        var fromMerged = configuration["Gemini:ApiKey"]?.Trim();
        if (!string.IsNullOrWhiteSpace(fromMerged))
        {
            return fromMerged;
        }

        return ReadApiKeyFromAppSettingsFiles(contentRootPath);
    }

    private static string? ReadApiKeyFromAppSettingsFiles(string contentRootPath)
    {
        foreach (var fileName in AppSettingsFiles)
        {
            var path = Path.Combine(contentRootPath, fileName);
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("Gemini", out var gemini))
                {
                    continue;
                }

                if (!gemini.TryGetProperty("ApiKey", out var keyEl))
                {
                    continue;
                }

                var key = keyEl.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return key;
                }
            }
            catch
            {
            }
        }

        return null;
    }
}
