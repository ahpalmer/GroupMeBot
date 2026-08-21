using System.Net;

namespace GroupMeBot.Application;

public interface IMessageOutgoing
{
    /// <summary>
    /// Posts a text message to the group
    /// </summary>
    /// <param name="text"></param>
    /// <param name="botId"></param>
    /// <returns></returns>
    public Task<HttpStatusCode> PostAsync(string text, string botId);

    /// <summary>
    /// Posts a message with attachments to the group. Pass an empty string for
    /// <paramref name="text"/> to post the attachment on its own.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="botId"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public Task<HttpStatusCode> PostAsync(string text, string botId, Attachment[] attachments);

    /// <summary>
    /// Posts a bot message to the service
    /// </summary>
    /// <param name="request">Request to post</param>
    /// <returns>Response code from the GroupMe service</returns>
    public Task<HttpStatusCode> PostBotMessage(CreateBotPostRequest request);

}
