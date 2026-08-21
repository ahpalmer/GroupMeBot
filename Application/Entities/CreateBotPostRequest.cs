using System.Runtime.Serialization;

namespace GroupMeBot.Application;

[DataContract]
public class CreateBotPostRequest
{
    public CreateBotPostRequest(string botId, string text)
        : this(botId, text, null)
    {
    }

    public CreateBotPostRequest(string botId, string text, Attachment[]? attachments)
    {
        BotId = botId;
        Text = text;
        Attachments = attachments;
    }

    /// <summary>
    /// Gets or sets the ID of the bot that is sending the message
    /// </summary>
    [DataMember(Name = "bot_id")]
    public string BotId { get; set; }

    /// <summary>
    /// Gets or sets the text of the message
    /// </summary>
    [DataMember(Name = "text")]
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the attachments for the message. Null when the message is text-only.
    /// </summary>
    /// <remarks>
    /// This class is [DataContract], so Json.NET serializes in opt-in mode: a property
    /// without [DataMember] is silently dropped from the wire payload. Any new field
    /// here needs the attribute.
    /// </remarks>
    [DataMember(Name = "attachments", EmitDefaultValue = false)]
    public Attachment[]? Attachments { get; set; }
}
