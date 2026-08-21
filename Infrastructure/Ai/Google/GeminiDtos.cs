using System.Text.Json.Serialization;

namespace GroupMeBot.Infrastructure.Ai.Google;

// Internal wire-format DTOs for the Gemini Interactions API (POST /v1beta/interactions).
// Kept internal so callers depend only on the provider-agnostic types in GroupMeBot.Infrastructure.Ai.

internal sealed class GeminiInteractionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public List<GeminiInputItem> Input { get; set; } = new();

    [JsonPropertyName("response_format")]
    public GeminiResponseFormat? ResponseFormat { get; set; }
}

/// <summary>
/// One entry in the input array. Text entries carry <see cref="Text"/>; image entries
/// carry <see cref="MimeType"/> and base64 <see cref="Data"/>. Unused properties are
/// omitted from the payload by the serializer's null-ignore policy.
/// </summary>
internal sealed class GeminiInputItem
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("mime_type")] public string? MimeType { get; set; }
    [JsonPropertyName("data")] public string? Data { get; set; }

    public static GeminiInputItem FromText(string text) =>
        new() { Type = "text", Text = text };

    public static GeminiInputItem FromImage(string mediaType, byte[] data) =>
        new() { Type = "image", MimeType = mediaType, Data = Convert.ToBase64String(data) };
}

internal sealed class GeminiResponseFormat
{
    [JsonPropertyName("type")] public string Type { get; set; } = "image";
    [JsonPropertyName("mime_type")] public string? MimeType { get; set; }
    [JsonPropertyName("aspect_ratio")] public string? AspectRatio { get; set; }
    [JsonPropertyName("image_size")] public string? ImageSize { get; set; }
}

internal sealed class GeminiInteractionResponse
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("steps")] public List<GeminiStep>? Steps { get; set; }
}

internal sealed class GeminiStep
{
    [JsonPropertyName("content")] public List<GeminiContentItem>? Content { get; set; }
}

internal sealed class GeminiContentItem
{
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("mime_type")] public string? MimeType { get; set; }
    [JsonPropertyName("data")] public string? Data { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
}
