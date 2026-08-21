using Moq;
using GroupMeBot.Application;
using GroupMeBot.Infrastructure.Ai;
using GroupMeBot.Infrastructure.Storage;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupMeBot.Application.UnitTest;

[TestClass]
public class AchievementImageBotUnitTest
{
    private const string Achievement =
        "New Achievement!\n🏆 CERTIFIED MOUTH BREATHER\nYou typed for nine minutes and said nothing.\nReward: Dunce Cap of Perpetual Shame";

    private Mock<IMessageOutgoing> _mockMessageOutgoing = null!;
    private Mock<IBotPostConfiguration> _mockBotPostConfig = null!;
    private Mock<IAiImageClient> _mockImageClient = null!;
    private Mock<IPersonPhotoStore> _mockPhotoStore = null!;
    private Mock<IGroupMeImageUploader> _mockUploader = null!;
    private Mock<ILogger<AchievementImageBot>> _mockLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockMessageOutgoing = new Mock<IMessageOutgoing>();
        _mockBotPostConfig = new Mock<IBotPostConfiguration>();
        _mockImageClient = new Mock<IAiImageClient>();
        _mockPhotoStore = new Mock<IPersonPhotoStore>();
        _mockUploader = new Mock<IGroupMeImageUploader>();
        _mockLogger = new Mock<ILogger<AchievementImageBot>>();

        _mockBotPostConfig.Setup(c => c.BotId).Returns("test-bot-id");
    }

    private AchievementImageBot CreateBot(bool imagesEnabled = true) => new(
        _mockMessageOutgoing.Object,
        _mockBotPostConfig.Object,
        _mockImageClient.Object,
        _mockPhotoStore.Object,
        _mockUploader.Object,
        Options.Create(new AchievementImageOptions { ImagesEnabled = imagesEnabled }),
        _mockLogger.Object);

    private static AchievementImageRequest Request() => new()
    {
        GroupId = "test-group",
        UserId = "4635437",
        DisplayName = "Andrew",
        AchievementText = Achievement,
        MessageId = "message-1"
    };

    [TestMethod]
    public async Task HandleAsync_WithReferencePhoto_PostsImageAttachment()
    {
        // Arrange
        _mockPhotoStore
            .Setup(p => p.GetPhotoAsync("4635437", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonPhoto("4635437", new byte[] { 1, 2, 3 }, "image/jpeg"));

        _mockImageClient
            .Setup(c => c.GenerateImageAsync(It.IsAny<AiImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiImageResponse(new byte[] { 9, 9 }, "image/jpeg", "gemini-3-pro-image"));

        _mockUploader
            .Setup(u => u.UploadAsync(It.IsAny<byte[]>(), "image/jpeg", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://i.groupme.com/abc");

        // Act
        await CreateBot().HandleAsync(Request());

        // Assert
        _mockImageClient.Verify(c => c.GenerateImageAsync(
            It.Is<AiImageRequest>(r => r.References.Count == 1 && r.Prompt.Contains("Andrew")),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockMessageOutgoing.Verify(m => m.PostAsync(
            "🏆 CERTIFIED MOUTH BREATHER",
            "test-bot-id",
            It.Is<Attachment[]>(a => a.Length == 1
                && a[0].Type == "image"
                && a[0].Url == "https://i.groupme.com/abc")), Times.Once);
    }

    [TestMethod]
    public void BuildCaption_IsNeverEmpty()
    {
        // GroupMe's bot post API rejects an empty text field, even with an attachment
        Assert.AreEqual("🏆 CERTIFIED MOUTH BREATHER", AchievementImageBot.BuildCaption("CERTIFIED MOUTH BREATHER"));
        Assert.AreEqual("🏆 Achievement unlocked", AchievementImageBot.BuildCaption(null));
        Assert.AreEqual("🏆 Achievement unlocked", AchievementImageBot.BuildCaption("   "));
    }

    [TestMethod]
    public async Task HandleAsync_NoReferencePhoto_StillGeneratesAndPosts()
    {
        // Arrange - Hayden and anyone new have no photo; they get a generic figure
        _mockPhotoStore
            .Setup(p => p.GetPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhoto?)null);

        _mockImageClient
            .Setup(c => c.GenerateImageAsync(It.IsAny<AiImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiImageResponse(new byte[] { 9 }, "image/jpeg", "gemini-3-pro-image"));

        _mockUploader
            .Setup(u => u.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://i.groupme.com/def");

        // Act
        await CreateBot().HandleAsync(Request());

        // Assert
        _mockImageClient.Verify(c => c.GenerateImageAsync(
            It.Is<AiImageRequest>(r => r.References.Count == 0 && r.Prompt.Contains("generic dungeon crawler")),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockMessageOutgoing.Verify(m => m.PostAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ImageClientThrows_SwallowsAndPostsNothing()
    {
        // Arrange - a content-policy refusal fails identically on every retry, so
        // rethrowing would only poison-queue the message
        _mockPhotoStore
            .Setup(p => p.GetPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhoto?)null);

        _mockImageClient
            .Setup(c => c.GenerateImageAsync(It.IsAny<AiImageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Gemini refused"));

        // Act
        await CreateBot().HandleAsync(Request());

        // Assert
        _mockMessageOutgoing.Verify(m => m.PostAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_UploadFails_PostsNothing()
    {
        // Arrange
        _mockPhotoStore
            .Setup(p => p.GetPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhoto?)null);

        _mockImageClient
            .Setup(c => c.GenerateImageAsync(It.IsAny<AiImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiImageResponse(new byte[] { 9 }, "image/jpeg", "gemini-3-pro-image"));

        _mockUploader
            .Setup(u => u.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        await CreateBot().HandleAsync(Request());

        // Assert
        _mockMessageOutgoing.Verify(m => m.PostAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_ImagesDisabled_DoesNotCallImageProvider()
    {
        // Act
        await CreateBot(imagesEnabled: false).HandleAsync(Request());

        // Assert
        _mockPhotoStore.Verify(
            p => p.GetPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockImageClient.Verify(
            c => c.GenerateImageAsync(It.IsAny<AiImageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockMessageOutgoing.Verify(
            m => m.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()), Times.Never);
    }

    [TestMethod]
    [DataRow("New Achievement!\n🏆 CERTIFIED MOUTH BREATHER\nflavor", "CERTIFIED MOUTH BREATHER")]
    [DataRow("New Achievement!\n🏆   SPACED OUT  \nflavor", "SPACED OUT")]
    [DataRow("New Achievement!\nDIPLOMACY WAS NEVER AN OPTION\nflavor", "DIPLOMACY WAS NEVER AN OPTION")]
    public void ExtractTitle_ReturnsTheTitleLine(string achievement, string expected)
    {
        Assert.AreEqual(expected, AchievementImageBot.ExtractTitle(achievement));
    }

    [TestMethod]
    public void ExtractTitle_NoTitleLine_ReturnsNull()
    {
        // Nothing all-caps and no trophy: better to render no banner than to put
        // flavor text in it
        Assert.IsNull(AchievementImageBot.ExtractTitle("New Achievement!\nsome flavor text here"));
        Assert.IsNull(AchievementImageBot.ExtractTitle(string.Empty));
    }

    [TestMethod]
    public void BuildPrompt_WithoutTitle_StillAsksForAnAchievementCard()
    {
        var request = new AchievementImageRequest
        {
            DisplayName = "Logan",
            AchievementText = "New Achievement!\nno title line here"
        };

        var prompt = AchievementImageBot.BuildPrompt(request, hasReferencePhoto: true);

        StringAssert.Contains(prompt, "Dungeon Crawler Carl");
        StringAssert.Contains(prompt, "Logan");
    }
}
