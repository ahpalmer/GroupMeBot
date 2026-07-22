using System.Text.Json.Serialization;

namespace GroupMeBot.Model;

public class GroupMeHistoryMessage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}

public class GroupMeMessagesResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("messages")]
    public List<GroupMeHistoryMessage>? Messages { get; set; }
}

public class GroupMeMessagesApiResponse
{
    [JsonPropertyName("response")]
    public GroupMeMessagesResponse? Response { get; set; }
}
