using GiphyDotNet;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GroupMeBot.Application;

public class GifBot : IGifBot
{
    private IMessageOutgoing _messageOutgoing;
    private IBotPostConfiguration _botPostConfiguration;
    private IGiphyBotPostConfig _giphyBotPostConfig;
    private ILogger _logger;

    public GifBot(IMessageOutgoing messageOutgoing, 
        IBotPostConfiguration botPostConfiguration, 
        IGiphyBotPostConfig giphyBotPostConfig,
        ILogger<GifBot> log)
    {
        _messageOutgoing = messageOutgoing;
        _botPostConfiguration = botPostConfiguration;
        _giphyBotPostConfig = giphyBotPostConfig;
        _logger = log;
    }

    /// <inheritdoc/>
    public async Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message)
    {
        _logger.LogInformation("GifBot-HandleIncomingTextAsync method start");
        _logger.LogInformation("GifBot-message.Text.Substring(4): {message.Text.Substring(4)}", message.Text.Substring(4));
        var giphy = new Giphy(_giphyBotPostConfig.GiphyBotId);
        string queryText = FixLongString(message.Text.Substring(4));
        var searchParameter = new SearchParameter()
        {
            Query = queryText,
            Limit = 1
        };

        // Returns gif results
        var gifResult = await giphy.GifSearch(searchParameter);

        if (string.IsNullOrEmpty(gifResult.Data[0].Images.Original.Url))
        {
            _logger.LogInformation("GifBot-HandleIncomingTextAsync: Giphy did not return a gif");
            return await _messageOutgoing.PostAsync($"Giphy did not return a gif.  Not my fault", _botPostConfiguration.BotId);
        }

        _logger.LogInformation("GifBot-HandleIncomingTextAsync small width was null: {gifResult.Data[0].Images.Original.Url}", gifResult.Data[0].Images.Original.Url);

        return await _messageOutgoing.PostAsync($"{gifResult.Data[0].Images.Original.Url}", _botPostConfiguration.BotId);

        // Todo: delete if gifs work.  If Gifs get too unwieldy then this code will use smaller gifs
        //if (!string.IsNullOrEmpty(gifResult.Data[0].Images.FixedWidthSmall.Url))
        //{
        //    _logger.LogInformation("GifBot-HandleIncomingTextAsync {gifResult.Data[0].Images.FixedWidthSmall.Url}", gifResult.Data[0].Images.FixedWidthSmall.Url);
        //    return await _messageOutgoing.PostAsync($"{gifResult.Data[0].Images.FixedWidthSmall.Url}", _botId);
        //}
    }

    static string FixLongString(string query)
    {
        query = query.Trim();

        if (query.Length > 50)
        {
            query = query.Substring(0, 50);
        }

        return query;
    }
}
