namespace GroupMeBot.Infrastructure.Storage;

/// <summary>
/// Supplies reference headshots for chat members by GroupMe user id.
/// </summary>
public interface IPersonPhotoStore
{
    /// <summary>
    /// Returns the reference photo for <paramref name="userId"/>, or null if no photo
    /// exists for that member. Callers fall back to a generic figure rather than
    /// treating a missing photo as an error.
    /// </summary>
    Task<PersonPhoto?> GetPhotoAsync(string userId, CancellationToken cancellationToken = default);
}
