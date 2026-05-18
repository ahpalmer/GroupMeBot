using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace GroupMeBot.Infrastructure.Ai.Anthropic;

/// <summary>
/// Anthropic Claude implementation of <see cref="IAiCompletionClient"/> backed by the
/// Messages API (POST /v1/messages). Register via
/// <c>InfrastructureServiceCollectionExtensions.AddAnthropicAiClient</c>.
/// </summary>
public sealed class AnthropicCompletionClient : IAiCompletionClient
{
    private const string MessagesPath = "/v1/messages";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;

    public AnthropicCompletionClient(HttpClient httpClient, IOptions<AnthropicOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("anthropic-version"))
        {
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);
        }
    }

    public async Task<AiCompletionResponse> GetCompletionAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Anthropic's Messages API expects system as a top-level field, not a message role.
        // If the caller embedded system entries in Messages, fold them into the system field.
        var systemFromMessages = request.Messages
            .Where(m => m.Role == AiRole.System)
            .Select(m => m.Content)
            .ToList();

        var system = request.System;
        if (string.IsNullOrEmpty(system) && systemFromMessages.Count > 0)
        {
            system = string.Join("\n\n", systemFromMessages);
        }

        var payload = new AnthropicMessageRequest
        {
            Model = request.Model ?? _options.DefaultModel,
            MaxTokens = request.MaxOutputTokens ?? _options.DefaultMaxOutputTokens,
            Temperature = request.Temperature,
            System = system,
            Messages = request.Messages
                .Where(m => m.Role != AiRole.System)
                .Select(m => new AnthropicMessage(MapRole(m.Role), m.Content))
                .ToList()
        };

        using var response = await _httpClient.PostAsJsonAsync(MessagesPath, payload, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Anthropic API request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<AnthropicMessageResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Anthropic API returned an empty response body.");

        var text = string.Concat(
            (result.Content ?? new List<AnthropicContentBlock>())
                .Where(c => string.Equals(c.Type, "text", StringComparison.Ordinal))
                .Select(c => c.Text));

        return new AiCompletionResponse(
            Text: text,
            ModelUsed: result.Model ?? payload.Model,
            InputTokens: result.Usage?.InputTokens,
            OutputTokens: result.Usage?.OutputTokens,
            StopReason: result.StopReason);
    }

    private static string MapRole(AiRole role) => role switch
    {
        AiRole.User => "user",
        AiRole.Assistant => "assistant",
        _ => throw new ArgumentOutOfRangeException(
            nameof(role), role, "Anthropic messages only accept 'user' or 'assistant' roles.")
    };
}
