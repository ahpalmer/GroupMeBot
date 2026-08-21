using Microsoft.Extensions.Logging;
using System.Net;

namespace GroupMeBot.Application;

public class MessageOutgoing : IMessageOutgoing
{
    private IBotPostConfiguration _botPostConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;
    private ILogger _logger;

    public MessageOutgoing(
        IBotPostConfiguration botPostConfiguration,
        IHttpClientFactory httpClientFactory,
        ILogger<MessageOutgoing> logger)
    {
        _botPostConfiguration = botPostConfiguration;
        _httpClientFactory = httpClientFactory;
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
    public async Task<HttpStatusCode> PostAsync(string text, string botId, Attachment[] attachments)
    {
        try
        {
            var post = new CreateBotPostRequest(botId, text, attachments);
            return await PostBotMessage(post);
        }
        catch (Exception ex)
        {
            _logger.LogError($"MessageOutgoing-PostAsync with attachments failed, {ex}");
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
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage result = await client.PostAsync(_botPostConfiguration.BotPostUrl, content);
            return result != null ? result.StatusCode : HttpStatusCode.BadRequest;
        }

    }
}

