namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Provider-agnostic completion request. Concrete clients map this onto their
/// specific request shape (e.g. Anthropic Messages, OpenAI Chat Completions, etc.).
/// </summary>
public sealed class AiCompletionRequest
{
    /// <summary>
    /// Conversation turns. A <see cref="AiRole.System"/> entry here will be merged into
    /// <see cref="System"/> by providers that don't allow system as a message role.
    /// </summary>
    public IReadOnlyList<AiMessage> Messages { get; init; } = Array.Empty<AiMessage>();

    /// <summary>
    /// System prompt / instructions. Optional. If null, providers may infer from
    /// any <see cref="AiRole.System"/> entries in <see cref="Messages"/>.
    /// </summary>
    public string? System { get; init; }

    /// <summary>
    /// Optional override for the provider's default model. If null the client uses
    /// the model configured in its options.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Optional cap on output tokens. If null the client uses its configured default.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Optional sampling temperature. Range is provider-specific (usually 0..1 or 0..2).
    /// </summary>
    public double? Temperature { get; init; }
}
