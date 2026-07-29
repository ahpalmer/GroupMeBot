using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GroupMeBot.Application;
using GroupMeBot.Infrastructure.DependencyInjection;

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
                ["Anthropic:ApiKey"] = "test-api-key"
            })
            .Build();

        services.AddLogging();
        services.AddHttpClient();
        services.AddSingleton<IAnalysisBot, AnalysisBot>();
        services.AddSingleton<IMessageBot, MessageBot>();
        services.AddSingleton<IGifBot, GifBot>();
        services.AddSingleton<IAchievementBot, AchievementBot>();
        services.AddSingleton<IGroupMeMessageHistory, GroupMeMessageHistory>();
        services.AddSingleton<IMessageIncoming, MessageIncoming>();
        services.AddSingleton<IMessageOutgoing, MessageOutgoing>();
        services.AddSingleton<IBotPostConfiguration>(new BotPostConfiguration(
            configuration["GroupMePostUri"], configuration["GroupMeBotId"], configuration["GroupMeAccessToken"]));
        services.AddSingleton<IGiphyBotPostConfig>(new GiphyBotPostConfig(
            configuration["GiphyBotId"]));
        services.AddAnthropicAiClient(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Assert - all services resolve without errors
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAnalysisBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGifBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IAchievementBot>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGroupMeMessageHistory>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageIncoming>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageOutgoing>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IBotPostConfiguration>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IGiphyBotPostConfig>());
    }
}
