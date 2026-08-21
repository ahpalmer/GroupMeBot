namespace GroupMeBot.Application;

/// <summary>
/// Configuration for <see cref="AchievementImageBot"/>. Bind from the "Achievement"
/// configuration section.
/// </summary>
public sealed class AchievementImageOptions
{
    /// <summary>
    /// Kill switch. When false the bot posts achievement text only and never calls the
    /// image provider — flip it from Azure app settings without a redeploy.
    /// </summary>
    public bool ImagesEnabled { get; set; } = true;
}
