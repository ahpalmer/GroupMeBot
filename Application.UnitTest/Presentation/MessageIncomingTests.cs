using System.Text;
using GroupMeBot.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GroupMeBot.Tests;

[TestClass]
public class MessageIncomingTests
{
    [TestMethod]
    [DataRow("bot achievement")]
    [DataRow("achievement bot")]
    public async Task ParseIncomingRequest_ManualAchievementAccepted_ReturnsOk(string triggerText)
    {
        var messageBot = new Mock<IMessageBot>();
        var analysisBot = new Mock<IAnalysisBot>();
        var gifBot = new Mock<IGifBot>();
        var achievementBot = new Mock<IAchievementBot>();
        var botConfiguration = new Mock<IBotPostConfiguration>();
        var logger = new Mock<ILogger<MessageIncoming>>();

        achievementBot
            .Setup(bot => bot.HandleIncomingTextAsync(It.IsAny<MessageItem>(), true))
            .ReturnsAsync(System.Net.HttpStatusCode.Accepted);

        var messageIncoming = new MessageIncoming(
            messageBot.Object,
            analysisBot.Object,
            gifBot.Object,
            achievementBot.Object,
            botConfiguration.Object,
            logger.Object);

        var payload = $$"""
            {
              "text": "{{triggerText}}",
              "group_id": "89303421",
              "name": "Andrew",
              "sender_id": "4635437",
              "sender_type": "user",
              "user_id": "4635437"
            }
            """;

        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        var result = await messageIncoming.ParseIncomingRequestAsync(request);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        achievementBot.Verify(
            bot => bot.HandleIncomingTextAsync(It.IsAny<MessageItem>(), true),
            Times.Once);
    }

    [TestMethod]
    public async Task ParseIncomingRequest_BotSender_DoesNotInvokeResponseBots()
    {
        var messageBot = new Mock<IMessageBot>();
        var analysisBot = new Mock<IAnalysisBot>();
        var gifBot = new Mock<IGifBot>();
        var achievementBot = new Mock<IAchievementBot>();
        var botConfiguration = new Mock<IBotPostConfiguration>();
        var logger = new Mock<ILogger<MessageIncoming>>();

        var messageIncoming = new MessageIncoming(
            messageBot.Object,
            analysisBot.Object,
            gifBot.Object,
            achievementBot.Object,
            botConfiguration.Object,
            logger.Object);

        const string payload = """
            {
              "attachments": [],
              "avatar_url": null,
              "created_at": 1784746413,
              "group_id": "89303421",
              "id": "178474641398640389",
              "name": "LoganInsultBotTest",
              "sender_id": "872126",
              "sender_type": "bot",
              "source_guid": "9032b4d0682c013fb6315ab5348d1504",
              "system": false,
              "text": "New Achievement!\n🏆 BEGGING THE MACHINE GOD FOR SCRAPS\nAndrew Palmer typed \"bot achievement post\" into the void.",
              "user_id": "872126"
            }
            """;

        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        var result = await messageIncoming.ParseIncomingRequestAsync(request);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        achievementBot.Verify(
            bot => bot.HandleIncomingTextAsync(It.IsAny<MessageItem>(), It.IsAny<bool>()),
            Times.Never);
        gifBot.Verify(
            bot => bot.HandleIncomingTextAsync(It.IsAny<MessageItem>()),
            Times.Never);
        messageBot.Verify(
            bot => bot.HandleIncomingTextAsync(It.IsAny<MessageItem>()),
            Times.Never);
    }
}
