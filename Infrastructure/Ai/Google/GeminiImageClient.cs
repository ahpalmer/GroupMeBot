using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace GroupMeBot.Infrastructure.Ai.Google;

/// <summary>
/// Google Gemini implementation of <see cref="IAiImageClient"/> backed by the
/// Interactions API (POST /v1beta/interactions). Register via
/// <c>GoogleServiceCollectionExtensions.AddGoogleAiImageClient</c>.
/// </summary>
public sealed class GeminiImageClient : IAiImageClient
{
    private const string InteractionsPath = "/v1beta/interactions";

    /// <summary>
    /// Gemini's image models cap the number of character reference images per request.
    /// We only ever send one (the crawler's headshot), but clamp defensively so a
    /// caller can't turn a prompt-building bug into a 400.
    /// </summary>
    private const int MaxReferenceImages = 4;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiImageClient(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("x-goog-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _options.ApiKey);
        }
    }

    public async Task<AiImageResponse> GenerateImageAsync(
        AiImageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("An image prompt is required.", nameof(request));
        }

        var model = request.Model ?? _options.DefaultImageModel;

        var input = new List<GeminiInputItem> { GeminiInputItem.FromText(request.Prompt) };
        input.AddRange(request.References
            .Take(MaxReferenceImages)
            .Select(r => GeminiInputItem.FromImage(r.MediaType, r.Data)));

        var payload = new GeminiInteractionRequest
        {
            Model = model,
            Input = input,
            ResponseFormat = new GeminiResponseFormat
            {
                MimeType = _options.ImageMediaType,
                AspectRatio = request.AspectRatio ?? _options.DefaultAspectRatio,
                ImageSize = _options.ImageSize
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(InteractionsPath, payload, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Gemini API request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiInteractionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Gemini API returned an empty response body.");

        // Scan every step rather than indexing steps[0].content[0]: a refusal or a
        // safety block comes back 200 with text-only content, and multi-step responses
        // put the image after intermediate reasoning steps.
        var image = (result.Steps ?? new List<GeminiStep>())
            .SelectMany(s => s.Content ?? new List<GeminiContentItem>())
            .FirstOrDefault(c =>
                string.Equals(c.Type, "image", StringComparison.Ordinal)
                && !string.IsNullOrEmpty(c.Data));

        if (image is null)
        {
            // Surface any text the model returned instead — that's where a content
            // policy refusal explains itself.
            var explanation = string.Concat(
                (result.Steps ?? new List<GeminiStep>())
                    .SelectMany(s => s.Content ?? new List<GeminiContentItem>())
                    .Where(c => !string.IsNullOrEmpty(c.Text))
                    .Select(c => c.Text));

            throw new InvalidOperationException(
                string.IsNullOrEmpty(explanation)
                    ? "Gemini API returned no image content."
                    : $"Gemini API returned no image content: {explanation}");
        }

        return new AiImageResponse(
            Data: Convert.FromBase64String(image.Data!),
            MediaType: image.MimeType ?? _options.ImageMediaType,
            ModelUsed: result.Model ?? model);
    }
}
