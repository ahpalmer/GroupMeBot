using System.Text.Json.Serialization;

namespace GroupMeBot.Application;

/// <summary>
/// Queue payload describing an achievement image to generate. Enqueued by
/// <c>AchievementBot</c> once the achievement text has posted, and consumed by the
/// AchievementImageWorker function.
/// </summary>
/// <remarks>
/// Serialized with System.Text.Json (not the Newtonsoft path the GroupMe DTOs use),
/// because that's what the Functions queue trigger binding deserializes with.
/// </remarks>
public sealed class AchievementImageRequest
{
    [JsonPropertyName("groupId")] public string? GroupId { get; set; }

    [JsonPropertyName("userId")] public string? UserId { get; set; }

    [JsonPropertyName("displayName")] public string? DisplayName { get; set; }

    [JsonPropertyName("achievementText")] public string AchievementText { get; set; } = string.Empty;

    /// <summary>
    /// The GroupMe id of the message that triggered the achievement. Carried for log
    /// correlation only.
    /// </summary>
    [JsonPropertyName("messageId")] public string? MessageId { get; set; }
}
