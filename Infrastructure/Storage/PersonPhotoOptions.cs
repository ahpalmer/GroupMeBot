namespace GroupMeBot.Infrastructure.Storage;

/// <summary>
/// Configuration for <see cref="BlobPersonPhotoStore"/>. Bind from the
/// "AchievementPhotos" configuration section.
/// </summary>
public sealed class PersonPhotoOptions
{
    /// <summary>
    /// Storage account connection string. Defaults to the Functions host's own
    /// storage account so no extra Azure resource is needed.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Private blob container holding one headshot per member.
    /// </summary>
    public string ContainerName { get; set; } = "achievement-people";

    /// <summary>
    /// Blob extension appended to the user id to form the blob name.
    /// </summary>
    public string BlobExtension { get; set; } = ".jpg";

    /// <summary>
    /// Media type reported for downloaded photos when the blob carries no
    /// Content-Type of its own.
    /// </summary>
    public string DefaultMediaType { get; set; } = "image/jpeg";

    /// <summary>
    /// How long a downloaded photo (or a confirmed absence) is cached in memory.
    /// Photos effectively never change, so this is long by design — the point is to
    /// avoid a blob round trip on every achievement, not to stay fresh.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(12);

    /// <summary>
    /// GroupMe user id to display name, for members who have a reference photo.
    /// </summary>
    public Dictionary<string, string> People { get; set; } = new();
}
