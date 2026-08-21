using GroupMeBot.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Presentation;

/// <summary>
/// Background worker that generates and posts achievement images. Kept off the
/// BasicResponse HTTP path because image generation takes tens of seconds and GroupMe
/// retries slow webhook callbacks.
/// </summary>
public class AchievementImageWorker
{
    private readonly IAchievementImageBot _achievementImageBot;
    private readonly ILogger _logger;

    public AchievementImageWorker(
        IAchievementImageBot achievementImageBot,
        ILogger<AchievementImageWorker> logger)
    {
        _achievementImageBot = achievementImageBot;
        _logger = logger;
    }

    [Function("AchievementImageWorker")]
    public async Task Run(
        [QueueTrigger(StorageAchievementImageQueue.QueueName, Connection = "AzureWebJobsStorage")]
        AchievementImageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AchievementImageWorker picked up an image request for {DisplayName}",
            request?.DisplayName);

        if (request is null)
        {
            return;
        }

        await _achievementImageBot.HandleAsync(request, cancellationToken);
    }
}
