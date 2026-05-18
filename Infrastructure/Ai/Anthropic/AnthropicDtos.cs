using System.Text.Json.Serialization;

namespace GroupMeBot.Infrastructure.Ai.Anthropic;

// Internal wire-format DTOs for the Anthropic Messages API.
// Kept internal so callers depend only on the provider-agnostic types in GroupMeBot.Infrastructure.Ai.

internal sealed class AnthropicMessageRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("messages")]
    public List<AnthropicMessage> Messages { get; set; } = new();
}

internal sealed record AnthropicMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed class AnthropicMessageResponse
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("stop_reason")] public string? StopReason { get; set; }
    [JsonPropertyName("content")] public List<AnthropicContentBlock>? Content { get; set; }
    [JsonPropertyName("usage")] public AnthropicUsage? Usage { get; set; }
}

internal sealed class AnthropicContentBlock
{
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
}

internal sealed class AnthropicUsage
{
    [JsonPropertyName("input_tokens")] public int InputTokens { get; set; }
    [JsonPropertyName("output_tokens")] public int OutputTokens { get; set; }
}
