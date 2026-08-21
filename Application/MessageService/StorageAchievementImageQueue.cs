using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Application;

/// <summary>
/// Azure Storage Queue implementation of <see cref="IAchievementImageQueue"/>, backed
/// by the Functions host's own storage account.
/// </summary>
public class StorageAchievementImageQueue : IAchievementImageQueue
{
    public const string QueueName = "achievement-images";

    private readonly QueueClient _queueClient;
    private readonly ILogger<StorageAchievementImageQueue> _logger;
    private bool _queueEnsured;

    public StorageAchievementImageQueue(
        string connectionString,
        ILogger<StorageAchievementImageQueue> logger)
    {
        _logger = logger;

        // The Functions queue trigger extension expects Base64-encoded message bodies
        // by default; QueueClient sends plain text unless told otherwise. Mismatch here
        // makes the worker silently fail to deserialize.
        _queueClient = new QueueClient(
            connectionString,
            QueueName,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    }

    public async Task EnqueueAsync(
        AchievementImageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_queueEnsured)
        {
            await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            _queueEnsured = true;
        }

        // Fully qualified: GroupMeBot.Application has its own JsonSerializer (Newtonsoft-backed)
        // that shadows the BCL one, and the queue trigger binding deserializes with System.Text.Json.
        var payload = System.Text.Json.JsonSerializer.Serialize(request);
        await _queueClient.SendMessageAsync(payload, cancellationToken);

        _logger.LogInformation(
            "AchievementImageQueue-enqueued image request for {DisplayName} (message {MessageId})",
            request.DisplayName,
            request.MessageId);
    }
}
