using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Model;

public class MessageIncoming : IMessageIncoming
{
    private IMessageBot _messageBot;
    private IAnalysisBot _analysisBot;
    private IGifBot _gifBot;
    private IAchievementBot _achievementBot;
    private IBotPostConfiguration _botPostConfiguration;
    private ILogger _logger;

    private static readonly Regex _botAnalysisRegex = new Regex(@"((?i)(\bbot\b.*\banalysis\b)|(\banalysis\b.*\bbot\b)(?-i))");
    private static readonly Regex _botMessageRegex = new Regex(@"((?i)(\bbot\b.*\bmessage\b)|(\bmessage\b.*\bbot\b)(?-i))");
    private static readonly Regex _botAchievementRegex = new Regex(@"(?i)\bbot\b.*\bachievement\b.*\bpost\b(?-i)");

    public MessageIncoming(
        IMessageBot messageBot,
        IAnalysisBot analysisBot,
        IGifBot gifBot,
        IAchievementBot achievementBot,
        IBotPostConfiguration botPostConfiguration,
        ILogger<MessageIncoming> logger)
    {
        this._messageBot = messageBot;
        this._analysisBot = analysisBot;
        this._gifBot = gifBot;
        this._achievementBot = achievementBot;
        this._botPostConfiguration = botPostConfiguration;
        this._logger = logger;
    }

    public async Task<IActionResult> ParseIncomingRequestAsync(HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Parse Incoming Request-Attempting to parse incoming request");
            if (req == null)
            {
                return new BadRequestObjectResult("No request was received");
            }

            string content = string.Empty;
            using (StreamReader sr = new StreamReader(req.Body))
            {
                _logger.LogInformation("Parse Incoming Request-reading HttpRequest");
                content = await sr.ReadToEndAsync();
            }

            MessageItem message = new MessageItem();

            _logger.LogInformation($"Parse Incoming Request-read HttpRequest: {content}");
            if (content != null)
            {
                message = JsonConvert.DeserializeObject<MessageItem>(content)!;
                _logger.LogInformation($"Parse Incoming Request-Message text content: {message.Text}");
            }

            // Todo: I'm not sure if this should be UserId or SenderId.  Guess we'll find out!
            // Delete the wrong one once you've figured it out
            if (message.UserId == _botPostConfiguration.BotId)
            {
                _logger.LogInformation($"CRITICAL INFORMATION: message.UserId == _botId");
                _logger.LogInformation($"Parse Incoming Request-returning OKObjectResult No response required, message is from bot");
                return new OkObjectResult("No response required, message is from bot");
            }
            if (message.SystemSenderId == _botPostConfiguration.BotId)
            {
                _logger.LogInformation($"CRITICAL INFORMATION: message.SystemSenderId == _botId");
                _logger.LogInformation($"Parse Incoming Request-returning OKObjectResult No response required, message is from bot");
                return new OkObjectResult("No response required, message is from bot");
            }
            if (message.Text == null)
            {
                _logger.LogInformation($"Parse Incoming Request-returning BadRequestObjectResult No text was received");
                return new BadRequestObjectResult("No text was received");
            }

            _logger.LogInformation($"Parse Incoming Request-attempting regex match");

            // Achievement bot - manual trigger ("bot achievement post")
            Match achievementRegex = _botAchievementRegex.Match(message.Text);
            if (achievementRegex.Success)
            {
                _logger.LogInformation("Parse Incoming Request-achievement manual trigger");
                var achievementStatus = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: true);
                return achievementStatus == HttpStatusCode.OK
                    ? new OkObjectResult(achievementStatus)
                    : new BadRequestObjectResult(achievementStatus);
            }

            // Achievement bot - random trigger (1/50 chance)
            if (Random.Shared.Next(50) == 0)
            {
                _logger.LogInformation("Parse Incoming Request-achievement random trigger fired");
                var achievementStatus = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: false);
                _logger.LogInformation("Parse Incoming Request-achievement random trigger result: {Status}", achievementStatus);
                // Don't return - let normal command processing continue
            }

            if (message.Text.StartsWith("Gif:") || message.Text.StartsWith("gif:") || message.Text.StartsWith("GIF:"))
            {
                _logger.LogInformation($"Gif: Parse Incoming Request-gif match successful");
                var status = await _gifBot.HandleIncomingTextAsync(message);
                if (status == HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Gif: Parse Incoming Request-returning OKObjectResult because HttpStatusCode Ok");
                    return new OkObjectResult(status);
                }
                else if (status == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation($"Gif: Parse Incoming Request-returning BadRequestObjectResult because HttpStatusCode BadRequest");
                    return new BadRequestObjectResult(status);
                }
            }

            Match analysisRegex = _botAnalysisRegex.Match(message.Text);
            if (analysisRegex.Success)
            {
                _logger.LogInformation($"Parse Incoming Request-analysis regex match successful");
                return new OkObjectResult(_analysisBot);
            }

            Match messageRegex = _botMessageRegex.Match(message.Text);
            if (messageRegex.Success)
            {
                _logger.LogInformation($"Parse Incoming Request-regex match successful");

                _logger.LogInformation($"Parse Incoming Request-HandleIncomingTextAsync");
                var status = await _messageBot.HandleIncomingTextAsync(message);
                if (status == HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Parse Incoming Request-returning OKObjectResult because HttpStatusCode Ok");
                    return new OkObjectResult(status);
                }
                else if (status == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation($"Parse Incoming Request-returning BadRequestObjectResult because HttpStatusCode BadRequest");
                    return new BadRequestObjectResult(status);
                }
                else
                {
                    _logger.LogInformation($"HttpStatusCode was neither OK nor BadRequest: {status}");
                }
            }

            _logger.LogInformation($"Parse Incoming Request-final log, didn't trip any if statements, return OkObjectResult");
            return new OkObjectResult("No response criteria met, no response required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parse Incoming Request-Exception caught: {ex}", ex);
            await Console.Out.WriteLineAsync(ex.ToString());
            return new BadRequestObjectResult("An error occurred");
        }
    }

    public string ParseIncomingHeadersAsync(HttpRequest req)
    {
        if (req == null)
        {
            return "";
        }

        var builder = new StringBuilder(Environment.NewLine);
        foreach (var header in req.Headers)
        {
            builder.AppendLine($"{header.Key}: {header.Value}");
        }
        var headersDump = builder.ToString();

        return headersDump;
    }
}
