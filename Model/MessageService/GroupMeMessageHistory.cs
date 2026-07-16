using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GroupMeBot.Model;

public class GroupMeMessageHistory : IGroupMeMessageHistory
{
    private readonly IBotPostConfiguration _config;
    private readonly ILogger<GroupMeMessageHistory> _logger;

    public GroupMeMessageHistory(IBotPostConfiguration config, ILogger<GroupMeMessageHistory> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<List<GroupMeHistoryMessage>> GetRecentMessagesAsync(string groupId, int limit = 20)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"https://api.groupme.com/v3/groups/{groupId}/messages?limit={limit}&token={_config.GroupMeAccessToken}";

            _logger.LogInformation("Fetching recent messages for group {GroupId}", groupId);
            var response = await client.GetAsync(url);

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
