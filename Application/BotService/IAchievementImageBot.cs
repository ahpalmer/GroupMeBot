namespace GroupMeBot.Application;

/// <summary>
/// Generates and posts the image that accompanies an achievement. Runs on a
/// background worker, not on the GroupMe webhook request path.
/// </summary>
public interface IAchievementImageBot
{
    /// <summary>
    /// Generates the achievement image and posts it to the group. Never throws for a
    /// content refusal or a missing reference photo — the achievement text has already
    /// posted, so a failure here degrades to a text-only achievement.
    /// </summary>
    Task HandleAsync(AchievementImageRequest request, CancellationToken cancellationToken = default);
}
