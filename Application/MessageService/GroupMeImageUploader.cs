using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Application;

public class GroupMeImageUploader : IGroupMeImageUploader
{
    private const string ImageServiceUrl = "https://image.groupme.com/pictures";

    private readonly IBotPostConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GroupMeImageUploader> _logger;

    public GroupMeImageUploader(
        IBotPostConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<GroupMeImageUploader> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> UploadAsync(
        byte[] image,
        string mediaType,
        CancellationToken cancellationToken = default)
    {
        if (image is null || image.Length == 0)
        {
            _logger.LogWarning("GroupMeImageUploader-refusing to upload an empty image");
            return null;
        }

        var client = _httpClientFactory.CreateClient();

        using var content = new MultipartFormDataContent();
        var filePart = new ByteArrayContent(image);
        filePart.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(filePart, "file", "achievement" + ExtensionFor(mediaType));

        using var request = new HttpRequestMessage(HttpMethod.Post, ImageServiceUrl)
        {
            Content = content
        };
        // Header, never a query string — the token must not end up in logs or proxies.
        request.Headers.Add("X-Access-Token", _config.GroupMeAccessToken);

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "GroupMeImageUploader-upload failed ({StatusCode}): {Body}",
                response.StatusCode,
                body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<GroupMeImageUploadResponse>(cancellationToken: cancellationToken);
        var url = result?.Payload?.PictureUrl;

        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogError("GroupMeImageUploader-upload succeeded but no picture_url was returned");
            return null;
        }

        _logger.LogInformation("GroupMeImageUploader-uploaded image to {PictureUrl}", url);
        return url;
    }

    private static string ExtensionFor(string mediaType) => mediaType switch
    {
        "image/png" => ".png",
        "image/gif" => ".gif",
        _ => ".jpg"
    };

    private sealed class GroupMeImageUploadResponse
    {
        [JsonPropertyName("payload")] public GroupMeImagePayload? Payload { get; set; }
    }

    private sealed class GroupMeImagePayload
    {
        [JsonPropertyName("picture_url")] public string? PictureUrl { get; set; }
    }
}
