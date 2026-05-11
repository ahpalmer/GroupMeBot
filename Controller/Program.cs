using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Azure.Monitor.OpenTelemetry.Exporter;
using GroupMeBot.Model;

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

        var botPostUri = configuration["GroupMePostUri"];
        var giphyBotId = configuration["GiphyBotId"];
        var groupMeBotId = configuration["GroupMeBotId"];

        services.AddHttpClient();

        services.AddSingleton<IAnalysisBot, AnalysisBot>();
        services.AddSingleton<IMessageBot, MessageBot>();
        services.AddSingleton<IGifBot, GifBot>();
        services.AddSingleton<IMessageIncoming, MessageIncoming>();
        services.AddSingleton<IMessageOutgoing, MessageOutgoing>();
        services.AddSingleton<IBotPostConfiguration>(new BotPostConfiguration(botPostUri, groupMeBotId));
        services.AddSingleton<IGiphyBotPostConfig>(new GiphyBotPostConfig(giphyBotId));

        services.AddOpenTelemetry()
            .UseFunctionsWorkerDefaults()
            .UseAzureMonitorExporter();
    })
    .Build();

host.Run();
