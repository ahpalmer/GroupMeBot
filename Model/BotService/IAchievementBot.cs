using System.Net;

namespace GroupMeBot.Model;

public interface IAchievementBot
{
    Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message, bool isManualTrigger);
}
