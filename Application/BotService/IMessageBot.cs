using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GroupMeBot.Application;

public interface IMessageBot
{
    /// <summary>
    /// Parses incoming text and decides how to respond
    /// </summary>
    /// <returns>returns an ObjectResult (that inherits from IActionResult) that will be sent to the GroupMe</returns>
    public Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message);
}
