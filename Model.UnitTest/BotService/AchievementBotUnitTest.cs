using Moq;
using GroupMeBot.Model;
using GroupMeBot.Infrastructure.Ai;
using System.Net;
using Microsoft.Extensions.Logging;

namespace GroupmeBot.Model.UnitTest;

[TestClass]
public class AchievementBotUnitTest
{
    private Mock<IMessageOutgoing> _mockMessageOutgoing = null!;
    private Mock<IBotPostConfiguration> _mockBotPostConfig = null!;
    private Mock<IAiCompletionClient> _mockAiClient = null!;
    private Mock<IGroupMeMessageHistory> _mockMessageHistory = null!;
    private Mock<ILogger<AchievementBot>> _mockLogger = null!;
    private AchievementBot _achievementBot = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockMessageOutgoing = new Mock<IMessageOutgoing>();
        _mockBotPostConfig = new Mock<IBotPostConfiguration>();
        _mockAiClient = new Mock<IAiCompletionClient>();
        _mockMessageHistory = new Mock<IGroupMeMessageHistory>();
        _mockLogger = new Mock<ILogger<AchievementBot>>();

        _mockBotPostConfig.Setup(c => c.BotId).Returns("test-bot-id");

        _achievementBot = new AchievementBot(
            _mockMessageOutgoing.Object,
            _mockBotPostConfig.Object,
            _mockAiClient.Object,
            _mockMessageHistory.Object,
            _mockLogger.Object);
    }

    [TestMethod]
    public async Task HandleIncomingText_ManualTrigger_PostsAchievementAboutRequester()
    {
        // Arrange
        var message = new MessageItem("bot achievement post")
        {
            DisplayName = "Andrew",
            GroupId = "test-group",
            UserId = "4635437"
        };

        _mockMessageHistory
            .Setup(h => h.GetRecentMessagesAsync("test-group", 20))
            .ReturnsAsync(new List<GroupMeHistoryMessage>
            {
                new() { Name = "Logan", Text = "Anyone want tacos?" },
                new() { Name = "Andrew", Text = "bot achievement post" }
            });

        _mockAiClient
            .Setup(a => a.GetCompletionAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCompletionResponse("New Achievement!\n🏆 THE AUDACITY\nYou asked for your own achievement. Reward: Mirror of Self-Admiration", "claude-sonnet-4-5", null, null, null));

        _mockMessageOutgoing
            .Setup(m => m.PostAsync(It.IsAny<string>(), "test-bot-id"))
            .ReturnsAsync(HttpStatusCode.OK);

        // Act
        var result = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: true);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result);
        _mockAiClient.Verify(a => a.GetCompletionAsync(
            It.Is<AiCompletionRequest>(r => r.Messages[0].Content.Contains("Andrew")),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageOutgoing.Verify(m => m.PostAsync(It.IsAny<string>(), "test-bot-id"), Times.Once);
    }

    [TestMethod]
    public async Task HandleIncomingText_RandomTrigger_PostsAchievementAboutConversation()
    {
        // Arrange
        var message = new MessageItem("I just ate 3 pizzas")
        {
            DisplayName = "Logan",
            GroupId = "test-group",
            UserId = "20597076"
        };

        _mockMessageHistory
            .Setup(h => h.GetRecentMessagesAsync("test-group", 20))
            .ReturnsAsync(new List<GroupMeHistoryMessage>
            {
                new() { Name = "Sean", Text = "What did you have for lunch?" },
                new() { Name = "Logan", Text = "I just ate 3 pizzas" }
            });

        _mockAiClient
            .Setup(a => a.GetCompletionAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCompletionResponse("New Achievement!\n🏆 BOTTOMLESS PIT\nReward: Antacid of Regret", "claude-sonnet-4-5", null, null, null));

        _mockMessageOutgoing
            .Setup(m => m.PostAsync(It.IsAny<string>(), "test-bot-id"))
            .ReturnsAsync(HttpStatusCode.OK);

        // Act
        var result = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: false);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result);
        _mockAiClient.Verify(a => a.GetCompletionAsync(
            It.Is<AiCompletionRequest>(r => r.Messages[0].Content.Contains("Logan") && r.Messages[0].Content.Contains("3 pizzas")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleIncomingText_EmptyHistory_StillGeneratesAchievement()
    {
        // Arrange
        var message = new MessageItem("hello")
        {
            DisplayName = "Jordan",
            GroupId = "test-group",
            UserId = "11900950"
        };

        _mockMessageHistory
            .Setup(h => h.GetRecentMessagesAsync("test-group", 20))
            .ReturnsAsync(new List<GroupMeHistoryMessage>());

        _mockAiClient
            .Setup(a => a.GetCompletionAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCompletionResponse("New Achievement!\n🏆 GHOST TOWN\nReward: Echo of Silence", "claude-sonnet-4-5", null, null, null));

        _mockMessageOutgoing
            .Setup(m => m.PostAsync(It.IsAny<string>(), "test-bot-id"))
            .ReturnsAsync(HttpStatusCode.OK);

        // Act
        var result = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: false);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result);
    }

    [TestMethod]
    public async Task HandleIncomingText_AiClientThrows_ReturnsBadRequest()
    {
        // Arrange
        var message = new MessageItem("test")
        {
            DisplayName = "Hayden",
            GroupId = "test-group",
            UserId = "84706251"
        };

        _mockMessageHistory
            .Setup(h => h.GetRecentMessagesAsync("test-group", 20))
            .ReturnsAsync(new List<GroupMeHistoryMessage>());

        _mockAiClient
            .Setup(a => a.GetCompletionAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API error"));

        // Act
        var result = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: false);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result);
        _mockMessageOutgoing.Verify(m => m.PostAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleIncomingText_MessageHistoryThrows_ReturnsBadRequestWithoutCallingAi()
    {
        var message = new MessageItem("test")
        {
            DisplayName = "Hayden",
            GroupId = "test-group",
            UserId = "84706251"
        };

        _mockMessageHistory
            .Setup(h => h.GetRecentMessagesAsync("test-group", 20))
            .ThrowsAsync(new HttpRequestException("GroupMe API error"));

        var result = await _achievementBot.HandleIncomingTextAsync(message, isManualTrigger: false);

        Assert.AreEqual(HttpStatusCode.BadRequest, result);
        _mockAiClient.Verify(
            client => client.GetCompletionAsync(
                It.IsAny<AiCompletionRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _mockMessageOutgoing.Verify(
            outgoing => outgoing.PostAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
