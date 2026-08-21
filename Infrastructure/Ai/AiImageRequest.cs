namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Provider-agnostic image generation request. Concrete clients map this onto their
/// specific request shape.
/// </summary>
public sealed class AiImageRequest
{
    /// <summary>
    /// The text prompt describing the image to generate.
    /// </summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// Optional reference images the provider should use for likeness or style.
    /// Providers impose their own limits on how many are accepted per request.
    /// </summary>
    public IReadOnlyList<AiImageReference> References { get; init; } = Array.Empty<AiImageReference>();

    /// <summary>
    /// Optional override for the provider's default model. If null the client uses
    /// the model configured in its options.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Optional aspect ratio hint, e.g. "1:1", "16:9". Interpretation is provider-specific.
    /// </summary>
    public string? AspectRatio { get; init; }
}
