using GroupMeBot.Infrastructure.Ai;
using GroupMeBot.Infrastructure.Ai.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroupMeBot.Infrastructure.DependencyInjection;

/// <summary>
/// DI registration helpers for the Google Gemini provider. Kept separate from
/// <see cref="InfrastructureServiceCollectionExtensions"/> so the text and image
/// providers can be swapped independently.
/// </summary>
public static class GoogleServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Gemini implementation of <see cref="IAiImageClient"/> using an
    /// inline options configuration callback.
    /// </summary>
    public static IServiceCollection AddGoogleAiImageClient(
        this IServiceCollection services,
        Action<GeminiOptions> configure)
    {
        services.AddOptions<GeminiOptions>()
            .Configure(configure)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "Google:ApiKey is not configured.")
            .ValidateOnStart();
        services.AddHttpClient<IAiImageClient, GeminiImageClient>();
        return services;
    }

    /// <summary>
    /// Registers the Gemini implementation of <see cref="IAiImageClient"/> using a
    /// configuration section (defaults to "Google").
    /// </summary>
    public static IServiceCollection AddGoogleAiImageClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Google")
    {
        services.AddOptions<GeminiOptions>()
            .Bind(configuration.GetRequiredSection(sectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                $"{sectionName}:ApiKey is not configured.")
            .ValidateOnStart();
        services.AddHttpClient<IAiImageClient, GeminiImageClient>();
        return services;
    }
}
