using GiphyDotNet;
using System.Net;

namespace GroupMeBot.Application;

public interface IGifBot
{
    /// <summary>
    /// Handles incoming text that asks for a gif and sends the requested gif to the groupme chat.
    /// </summary>
    /// <param name="message"></param>
    /// <returns>HttpStatusCode</returns>
    public Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message);

}
