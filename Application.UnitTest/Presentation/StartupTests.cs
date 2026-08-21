using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GroupMeBot.Application;
using GroupMeBot.Infrastructure.Ai;
using GroupMeBot.Infrastructure.DependencyInjection;
using GroupMeBot.Infrastructure.Storage;

namespace GroupMeBot.Tests;

[TestClass]
public class StartupTests
{
    [TestMethod]
    public void ServiceRegistration_ResolvesExpectedServices()
    {
        // Arrange - build a service collection mirroring Program.cs registrations
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GroupMePostUri"] = "https://api.groupme.com/v3/bots/post",
                ["GiphyBotId"] = "test-giphy-id",
                ["GroupMeBotId"] = "test-bot-id",
                ["GroupMeAccessToken"] = "test-access-token",
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["Anthropic:ApiKey"] = "test-api-key",
                ["Google:ApiKey"] = "test-google-key",
                ["Achievement:ImagesEnabled"] = "true",
                ["AchievementPhotos:ContainerName"] = "achievement-people",
                ["AchievementPhotos:People:4635437"] = "Andrew"
            })
            .Build();

        services.AddLogging();
        services.AddHttpClient();
        services.AddSingleton<IAnalysisBot, AnalysisBot>();
        services.AddSingleton<IMessageBot, MessageBot>();
        services.AddSingleton<IGifBot, GifBot>();
        services.AddSingleton<IAchievementBot, AchievementBot>();
        services.AddSingleton<IAchievementImageBot, AchievementImageBot>();
        services.AddSingleton<IGroupMeMessageHistory, GroupMeMessageHistory>();
        services.AddSingleton<IGroupMeImageUploader, GroupMeImageUploader>();
        services.AddSingleton<IMessageIncoming, MessageIncoming>();
        services.AddSingleton<IMessageOutgoing, MessageOutgoing>();
        services.AddSingleton<IBotPostConfiguration>(new BotPostConfiguration(
            configuration["GroupMePostUri"], configuration["GroupMeBotId"], configuration["GroupMeAccessToken"]));
        services.AddSingleton<IGiphyBotPostConfig>(new GiphyBotPostConfig(
            configuration["GiphyBotId"]));
        services.AddSingleton<IAchievementImageQueue>(sp => new StorageAchievementImageQueue(
            configuration["AzureWebJobsStorage"]!,
            sp.GetRequiredService<ILogger<StorageAchievementImageQueue>>()));
        services.Configure<AchievementImageOptions>(configuration.GetSection("Achievement"));
        services.AddAnthropicAiClient(configuration);
        services.AddGoogleAiImageClient(configuration);
        services.AddBlobPersonPhotoStore(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Assert - all services resolve without errors
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAnalysisBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGifBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAchievementBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAchievementImageBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGroupMeMessageHistory>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGroupMeImageUploader>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageIncoming>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageOutgoing>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IBotPostConfiguration>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGiphyBotPostConfig>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAchievementImageQueue>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAiCompletionClient>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAiImageClient>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IPersonPhotoStore>());
    }
}
