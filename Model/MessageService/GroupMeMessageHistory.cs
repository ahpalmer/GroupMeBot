using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GroupMeBot.Model;

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
        try
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
                return new List<GroupMeHistoryMessage>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<GroupMeMessagesApiResponse>(content);
            return apiResponse?.Response?.Messages ?? new List<GroupMeHistoryMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching message history");
            return new List<GroupMeHistoryMessage>();
        }
    }
}
