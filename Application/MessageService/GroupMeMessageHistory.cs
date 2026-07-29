using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Application;

public class GroupMeMessageHistory : IGroupMeMessageHistory
{
    private readonly IBotPostConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GroupMeMessageHistory> _logger;

    public GroupMeMessageHistory(
        IBotPostConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<GroupMeMessageHistory> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<GroupMeHistoryMessage>> GetRecentMessagesAsync(string groupId, int limit = 20)
    {
        var client = _httpClientFactory.CreateClient();
        var encodedGroupId = Uri.EscapeDataString(groupId);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.groupme.com/v3/groups/{encodedGroupId}/messages?limit={limit}");
        request.Headers.Add("X-Access-Token", _config.GroupMeAccessToken);

        _logger.LogInformation("Fetching recent messages for group {GroupId}", groupId);
        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch messages: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<GroupMeMessagesApiResponse>()
            ?? throw new InvalidOperationException("GroupMe returned an empty message-history response.");

        return apiResponse.Response?.Messages ?? new List<GroupMeHistoryMessage>();
    }
}
