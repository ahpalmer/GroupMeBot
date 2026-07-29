using System.Runtime.Serialization;

namespace GroupMeBot.Application;

/// <inheritdoc/>
[DataContract]
public class MessageItem : IMessageItem
{
    public MessageItem() 
    {
    }

    public MessageItem(string text)
    {
        Text = text;
    }

    /// <inheritdoc/>
    [DataMember(Name = "text", EmitDefaultValue = false)]
    public string? Text { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "user_id", EmitDefaultValue = false)]
    public string? UserId { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "group_id", EmitDefaultValue = false)]
    public string? GroupId { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string? unspecifiedId { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "sender_id", EmitDefaultValue = false)]
    public string? SystemSenderId { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "sender_type", EmitDefaultValue = false)]
    public string? SystemSenderType { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "source_guid", EmitDefaultValue = false)]
    public string? sourceGuid { get; set; }

    /// <inheritdoc/>
    [DataMember(Name = "system", EmitDefaultValue = false)]
    public bool? IsSystem { get; set; }
}