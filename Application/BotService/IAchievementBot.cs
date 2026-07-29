using System.Net;

namespace GroupMeBot.Application;

public interface IAchievementBot
{
    Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message, bool isManualTrigger);
}
