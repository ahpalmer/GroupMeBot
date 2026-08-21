namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Abstraction over an external AI image generation API. Depend on this from the
/// application/core layers so the concrete provider can be swapped (Gemini, OpenAI,
/// etc.) without touching business logic.
/// </summary>
/// <remarks>
/// Deliberately separate from <see cref="IAiCompletionClient"/>: no single provider
/// currently serves both well. Anthropic has no image generation API at all, so the
/// bot uses Claude for achievement text and Gemini for achievement images.
/// </remarks>
public interface IAiImageClient
{
    Task<AiImageResponse> GenerateImageAsync(
        AiImageRequest request,
        CancellationToken cancellationToken = default);
}
