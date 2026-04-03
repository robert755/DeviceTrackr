namespace DeviceTrackr.Api.Configuration;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Google AI Studio / Gemini API key. Prefer env: <c>Gemini__ApiKey</c> (not committed to Git).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model id (override with Gemini__Model). Default works on most API keys; try gemini-2.0-flash if your project supports it.</summary>
    public string Model { get; set; } = "gemini-1.5-flash";
}
