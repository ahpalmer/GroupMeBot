using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Azure.Monitor.OpenTelemetry.Exporter;
using GroupMeBot.Application;
using GroupMeBot.Infrastructure.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var botPostConfiguration = new BotPostConfiguration(
            configuration.GetRequiredValue("GroupMePostUri"),
            configuration.GetRequiredValue("GroupMeBotId"),
            configuration.GetRequiredValue("GroupMeAccessToken"));
        var giphyBotPostConfig = new GiphyBotPostConfig(
            configuration.GetRequiredValue("GiphyBotId"));

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
        services.AddSingleton<IBotPostConfiguration>(botPostConfiguration);
        services.AddSingleton<IGiphyBotPostConfig>(giphyBotPostConfig);

        services.AddSingleton<IAchievementImageQueue>(sp => new StorageAchievementImageQueue(
            configuration.GetRequiredValue("AzureWebJobsStorage"),
            sp.GetRequiredService<ILogger<StorageAchievementImageQueue>>()));

        services.Configure<AchievementImageOptions>(configuration.GetSection("Achievement"));

        services.AddAnthropicAiClient(configuration);
        services.AddGoogleAiImageClient(configuration);
        services.AddBlobPersonPhotoStore(configuration);

        services.AddOpenTelemetry()
            .UseFunctionsWorkerDefaults()
            .UseAzureMonitorExporter();
    })
    .Build();

host.Run();
