namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// Provider-agnostic image generation result. <paramref name="MediaType"/> is an IANA
/// media type such as "image/jpeg" or "image/png".
/// </summary>
public sealed record AiImageResponse(
    byte[] Data,
    string MediaType,
    string ModelUsed);
