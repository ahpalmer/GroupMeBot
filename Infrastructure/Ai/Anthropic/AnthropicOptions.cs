namespace GroupMeBot.Infrastructure.Ai.Anthropic;

/// <summary>
/// Configuration for the Anthropic Claude implementation of <see cref="IAiCompletionClient"/>.
/// Bind from configuration (e.g. "Anthropic" section) or configure via DI extension.
/// </summary>
public sealed class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    /// <summary>
    /// Anthropic requires an explicit API version header. See
    /// https://docs.anthropic.com/en/api/versioning
    /// </summary>
    public string ApiVersion { get; set; } = "2023-06-01";

    /// <summary>
    /// Model id used when <see cref="AiCompletionRequest.Model"/> is not set.
    /// </summary>
    public string DefaultModel { get; set; } = "claude-sonnet-5";

    /// <summary>
    /// max_tokens used when <see cref="AiCompletionRequest.MaxOutputTokens"/> is not set.
    /// Anthropic's API requires this field on every request.
    /// </summary>
    public int DefaultMaxOutputTokens { get; set; } = 1024;
}
