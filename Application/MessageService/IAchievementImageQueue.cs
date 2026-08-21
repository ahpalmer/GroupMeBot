namespace GroupMeBot.Application;

/// <summary>
/// Hands off achievement image generation to a background worker so the GroupMe
/// webhook can return immediately. Image generation takes tens of seconds; GroupMe
/// retries slow callbacks.
/// </summary>
public interface IAchievementImageQueue
{
    Task EnqueueAsync(AchievementImageRequest request, CancellationToken cancellationToken = default);
}
