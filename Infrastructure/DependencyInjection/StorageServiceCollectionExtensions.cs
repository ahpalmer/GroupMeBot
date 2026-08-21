using GroupMeBot.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroupMeBot.Infrastructure.DependencyInjection;

/// <summary>
/// DI registration helpers for Azure Storage-backed infrastructure.
/// </summary>
public static class StorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers the blob-backed <see cref="IPersonPhotoStore"/> using a configuration
    /// section (defaults to "AchievementPhotos"). When the section supplies no
    /// <c>ConnectionString</c>, falls back to <c>AzureWebJobsStorage</c> so the photos
    /// live in the Functions host's own storage account.
    /// </summary>
    public static IServiceCollection AddBlobPersonPhotoStore(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "AchievementPhotos")
    {
        services.AddMemoryCache();

        services.AddOptions<PersonPhotoOptions>()
            .Bind(configuration.GetSection(sectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    options.ConnectionString = configuration["AzureWebJobsStorage"] ?? string.Empty;
                }
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                $"{sectionName}:ConnectionString is not configured and AzureWebJobsStorage is unset.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ContainerName),
                $"{sectionName}:ContainerName is not configured.")
            .ValidateOnStart();

        services.AddSingleton<IPersonPhotoStore, BlobPersonPhotoStore>();
        return services;
    }
}
