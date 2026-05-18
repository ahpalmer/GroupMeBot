namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Provider-agnostic completion result. Token counts are optional because not every
/// provider returns them.
/// </summary>
public sealed record AiCompletionResponse(
    string Text,
    string ModelUsed,
    int? InputTokens,
    int? OutputTokens,
    string? StopReason);
