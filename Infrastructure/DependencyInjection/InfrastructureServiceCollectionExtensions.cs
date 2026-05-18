using GroupMeBot.Infrastructure.Ai;
using GroupMeBot.Infrastructure.Ai.Anthropic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroupMeBot.Infrastructure.DependencyInjection;

/// <summary>
/// DI registration helpers for the Infrastructure layer. Keep registrations here so
/// composition roots (e.g. the Controller's Program.cs) don't need to know about
/// concrete provider classes.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Anthropic Claude implementation of <see cref="IAiCompletionClient"/>
    /// using an inline options configuration callback.
    /// </summary>
    public static IServiceCollection AddAnthropicAiClient(
        this IServiceCollection services,
        Action<AnthropicOptions> configure)
    {
        services.AddOptions<AnthropicOptions>().Configure(configure);
        services.AddHttpClient<IAiCompletionClient, AnthropicCompletionClient>();
        return services;
    }

    /// <summary>
    /// Registers the Anthropic Claude implementation of <see cref="IAiCompletionClient"/>
    /// using a configuration section (defaults to "Anthropic").
    /// </summary>
    public static IServiceCollection AddAnthropicAiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Anthropic")
    {
        services.AddOptions<AnthropicOptions>().Bind(configuration.GetSection(sectionName));
        services.AddHttpClient<IAiCompletionClient, AnthropicCompletionClient>();
        return services;
    }
}
