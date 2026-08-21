namespace GroupMeBot.Infrastructure.Storage;

/// <summary>
/// A reference headshot for one chat member, used to give generated achievement
/// images a recognizable likeness. <paramref name="MediaType"/> is an IANA media
/// type such as "image/jpeg".
/// </summary>
public sealed record PersonPhoto(string UserId, byte[] Data, string MediaType);
