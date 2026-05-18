namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Abstraction over an external AI completion API. Depend on this from the
/// application/core layers so the concrete provider can be swapped (Anthropic,
/// OpenAI, Gemini, etc.) without touching business logic.
/// </summary>
public interface IAiCompletionClient
{
    Task<AiCompletionResponse> GetCompletionAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}
