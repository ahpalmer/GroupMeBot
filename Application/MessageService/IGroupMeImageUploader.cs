namespace GroupMeBot.Application;

/// <summary>
/// Uploads image bytes to GroupMe's image service so they can be attached to a bot
/// message. GroupMe rejects attachment URLs that aren't hosted on i.groupme.com, so
/// every generated image has to go through here first.
/// </summary>
public interface IGroupMeImageUploader
{
    /// <summary>
    /// Uploads an image and returns its i.groupme.com URL, or null if the upload failed.
    /// </summary>
    Task<string?> UploadAsync(byte[] image, string mediaType, CancellationToken cancellationToken = default);
}
