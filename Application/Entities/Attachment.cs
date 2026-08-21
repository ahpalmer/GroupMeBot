using System.Runtime.Serialization;

namespace GroupMeBot.Application;

/// <summary>
/// An attachment on an outgoing GroupMe bot message. Image URLs must be hosted on
/// GroupMe's own image service (i.groupme.com) — arbitrary URLs are rejected. Use
/// <see cref="IGroupMeImageUploader"/> to get one.
/// </summary>
[DataContract]
public class Attachment
{
    public Attachment()
    {
    }

    public Attachment(string type, string url)
    {
        Type = type;
        Url = url;
    }

    /// <summary>
    /// Gets or sets the attachment type, e.g. "image".
    /// </summary>
    [DataMember(Name = "type")]
    public string Type { get; set; } = "image";

    /// <summary>
    /// Gets or sets the i.groupme.com URL of the attached media
    /// </summary>
    [DataMember(Name = "url")]
    public string Url { get; set; } = string.Empty;

    public static Attachment Image(string url) => new("image", url);
}
