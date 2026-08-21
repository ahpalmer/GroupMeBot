using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupMeBot.Infrastructure.Storage;

/// <summary>
/// Reads reference headshots from a private Azure Blob Storage container, one blob
/// per member named <c>{userId}.jpg</c>. The photos are deliberately not committed to
/// the repository, which is public.
/// </summary>
public sealed class BlobPersonPhotoStore : IPersonPhotoStore
{
    private readonly BlobContainerClient _container;
    private readonly IMemoryCache _cache;
    private readonly PersonPhotoOptions _options;
    private readonly ILogger<BlobPersonPhotoStore> _logger;

    public BlobPersonPhotoStore(
        IMemoryCache cache,
        IOptions<PersonPhotoOptions> options,
        ILogger<BlobPersonPhotoStore> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _container = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
    }

    public async Task<PersonPhoto?> GetPhotoAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var cacheKey = $"{nameof(BlobPersonPhotoStore)}:{userId}";

        // The cache stores misses as well as hits, so an unknown member costs one blob
        // probe per cache window rather than one per achievement.
        if (_cache.TryGetValue<PersonPhoto?>(cacheKey, out var cached))
        {
            return cached;
        }

        var photo = await DownloadAsync(userId, cancellationToken);
        _cache.Set(cacheKey, photo, _options.CacheDuration);
        return photo;
    }

    private async Task<PersonPhoto?> DownloadAsync(string userId, CancellationToken cancellationToken)
    {
        var blobName = userId + _options.BlobExtension;

        try
        {
            var blob = _container.GetBlobClient(blobName);
            var download = await blob.DownloadContentAsync(cancellationToken);

            var mediaType = string.IsNullOrWhiteSpace(download.Value.Details.ContentType)
                ? _options.DefaultMediaType
                : download.Value.Details.ContentType;

            _logger.LogInformation(
                "PersonPhotoStore-loaded reference photo for user {UserId} ({Bytes} bytes)",
                userId,
                download.Value.Content.ToMemory().Length);

            return new PersonPhoto(userId, download.Value.Content.ToArray(), mediaType);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("PersonPhotoStore-no reference photo for user {UserId}", userId);
            return null;
        }
        catch (RequestFailedException ex)
        {
            // A misconfigured container or an auth failure shouldn't take the whole
            // image down — degrade to the generic-figure path.
            _logger.LogError(ex, "PersonPhotoStore-failed to read reference photo for user {UserId}", userId);
            return null;
        }
    }
}
