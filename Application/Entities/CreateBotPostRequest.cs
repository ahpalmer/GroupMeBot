using System.Runtime.Serialization;

namespace GroupMeBot.Application;

[DataContract]
public class CreateBotPostRequest
{
    public CreateBotPostRequest(string botId, string text)
    {
        BotId = botId;
        Text = text;
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
    /// Gets or sets the attachments for the message
    /// </summary>
    //[DataMember(Name = "attachments")]
    //public Attachment[] Attachments { get; set; }

}
