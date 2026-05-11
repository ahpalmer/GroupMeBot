using Moq;
using GroupMeBot.Model;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Logging;

namespace GroupmeBot.Model.UnitTest;

[TestClass]
public class MessageBotUnitTest
{
    private static readonly Regex BotCannedResponseRegex = new Regex(@"((?i)(?=.*\bbot\b)(?=.*\bmessage\b)(?=.*\bresponse\b)(?-i))");
    private const string _botPostUrl = "https://api.groupme.com/v3/bots/post";

    [TestMethod]
    public void RetrieveRandomResponseAnonymous()
    {
        //Arrange
        MessageItem messageItem = new MessageItem("Bot message response");
        var mockLogger = new Mock<ILogger<MessageBot>>();
        var mockMessageOutgoing = new Mock<IMessageOutgoing>();
        var mockBotPostConfig = new Mock<IBotPostConfiguration>();

        var messageBot = new MessageBot(mockMessageOutgoing.Object, mockBotPostConfig.Object, mockLogger.Object);

        string[] anonymousResponses = new string[]
        {
            "This is an anonymous response",
            "Yes",
            "Sure",
            "All signs point to no"
        };

        string response = messageBot.RetrieveRandomResponse("");

        Assert.IsTrue(anonymousResponses.Contains(response));
    }

    [TestMethod]
    public void RetrieveRandomResponseAndrew()
    {
        //Arrange
        MessageItem messageItem = new MessageItem("Bot message response");
        var mockLogger = new Mock<ILogger<MessageBot>>();
        var mockMessageOutgoing = new Mock<IMessageOutgoing>();
        var mockBotPostConfig = new Mock<IBotPostConfiguration>();

        var messageBot = new MessageBot(mockMessageOutgoing.Object, mockBotPostConfig.Object, mockLogger.Object);

        string[] andrewResponses = new string[]
        {
            "I agree completely!",
            "You are as correct as you are handsome",
            "I agree with you 100%",
            "Gosh you're so smart",
            "HELP I'M TRAPPED IN A FACTORY THAT MAKES COMPLIMENTS, FUCK YOU ANDREW, FUCK Y",
            "You're an awesome friend.",
            "You have a great sense of humor!",
            "You is strong, you is smart, you is important",
            "You are inspiring",
            "You are brave and courageous."
        };

        string response = messageBot.RetrieveRandomResponse("Andrew");

        Assert.IsTrue(andrewResponses.Contains(response));
    }

    [TestMethod]
    public async Task HandleIncomingMessage_NullText_ReturnHttpBadRequest()
    {
        //Arrange
        MessageItem messageItem = new MessageItem();
        messageItem.Text = null;
        var mockLogger = new Mock<ILogger<MessageBot>>();
        var mockMessageOutgoing = new Mock<IMessageOutgoing>();
        var mockBotPostConfig = new Mock<IBotPostConfiguration>();

        var messageBot = new MessageBot(mockMessageOutgoing.Object, mockBotPostConfig.Object, mockLogger.Object);

        //Act
        HttpStatusCode result = await messageBot.HandleIncomingTextAsync(messageItem);

        //Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result);
    }

    [TestMethod]
    public async Task HandleIncomingMessage_ValidText_ReturnsStatusCode()
    {
        //Arrange
        MessageItem messageItem = new MessageItem("Bot message response");
        messageItem.UserId = "unknown-user";
        var mockLogger = new Mock<ILogger<MessageBot>>();
        var mockMessageOutgoing = new Mock<IMessageOutgoing>();
        var mockBotPostConfig = new Mock<IBotPostConfiguration>();
        mockBotPostConfig.Setup(c => c.BotId).Returns("test-bot-id");

        mockMessageOutgoing
            .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(HttpStatusCode.OK);

        var messageBot = new MessageBot(mockMessageOutgoing.Object, mockBotPostConfig.Object, mockLogger.Object);

        //Act
        HttpStatusCode result = await messageBot.HandleIncomingTextAsync(messageItem);

        //Assert
        Assert.AreEqual(HttpStatusCode.OK, result);
        mockMessageOutgoing.Verify(m => m.PostAsync(It.IsAny<string>(), "test-bot-id"), Times.Once);
    }
}
