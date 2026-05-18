namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// A single turn in an AI conversation. Provider-agnostic; concrete clients translate
/// this into whatever the underlying API expects.
/// </summary>
public sealed record AiMessage(AiRole Role, string Content);
