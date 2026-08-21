namespace GroupMeBot.Infrastructure.Ai;

/// <summary>
/// An input image supplied alongside a generation prompt, used by the provider as a
/// likeness/style reference. <paramref name="MediaType"/> is an IANA media type such
/// as "image/jpeg" or "image/png".
/// </summary>
public sealed record AiImageReference(byte[] Data, string MediaType);
