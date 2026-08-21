namespace GroupMeBot.Infrastructure.Ai.Google;

/// <summary>
/// Configuration for the Google Gemini implementation of <see cref="IAiImageClient"/>.
/// Bind from configuration (e.g. "Google" section) or configure via DI extension.
/// </summary>
public sealed class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";

    /// <summary>
    /// Model id used when <see cref="AiImageRequest.Model"/> is not set. The Pro image
    /// tier renders in-image text (the achievement title) noticeably better than the
    /// Flash tiers; drop to gemini-3.1-flash-image to trade that for cost and latency.
    /// </summary>
    public string DefaultImageModel { get; set; } = "gemini-3-pro-image";

    /// <summary>
    /// Media type requested for generated images. GroupMe's image service accepts
    /// JPEG and PNG; JPEG keeps the upload small.
    /// </summary>
    public string ImageMediaType { get; set; } = "image/jpeg";

    /// <summary>
    /// Aspect ratio used when <see cref="AiImageRequest.AspectRatio"/> is not set.
    /// </summary>
    public string DefaultAspectRatio { get; set; } = "1:1";

    /// <summary>
    /// Resolution tier passed as image_size, e.g. "1K" or "2K".
    /// </summary>
    public string ImageSize { get; set; } = "1K";
}
