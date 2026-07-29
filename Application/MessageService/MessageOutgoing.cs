using Microsoft.Extensions.Logging;
using System.Net;

namespace GroupMeBot.Application;

public class MessageOutgoing : IMessageOutgoing
{
    private IBotPostConfiguration _botPostConfiguration;
    private ILogger _logger;

    public MessageOutgoing(IBotPostConfiguration botPostConfiguration, ILogger<MessageOutgoing> logger)
    {
        _botPostConfiguration = botPostConfiguration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<HttpStatusCode> PostAsync(string text, string botId)
    {
        try
        {
            var post = new CreateBotPostRequest(botId, text);
            return await PostBotMessage(post);
        }
        catch (Exception ex)
        {
            _logger.LogError($"MessageOutgoing-PostAsync method failed, {ex}");
            return HttpStatusCode.BadRequest;
        }
    }

    /// <inheritdoc/>
    public async Task<HttpStatusCode> PostBotMessage(CreateBotPostRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        using (HttpContent content = JsonSerializer.SerializeToJson(request))
        {
            var client = new HttpClient();
            HttpResponseMessage result = await client.PostAsync(_botPostConfiguration.BotPostUrl, content);
            return result != null ? result.StatusCode : HttpStatusCode.BadRequest;
        }

    }
}

