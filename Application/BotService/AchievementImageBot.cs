using GroupMeBot.Infrastructure.Ai;
using GroupMeBot.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupMeBot.Application;

public class AchievementImageBot : IAchievementImageBot
{
    private const string TrophyEmoji = "🏆";

    /// <summary>
    /// Style and framing shared by every achievement image, so the bit stays visually
    /// consistent across posts. The per-achievement detail is appended by
    /// <see cref="BuildPrompt"/>.
    /// </summary>
    private const string CardStyle = """
        Illustrate a single "achievement unlocked" card in the style of the Dungeon Crawler Carl books.
        Framing: a dark holographic UI panel floating against a dungeon backdrop, glowing neon border,
        faint scanlines and interface glyphs around the edges, ornate trophy iconography in the corners.
        The whole image is the card - do not render a screenshot of a monitor or a phone.
        Overall tone: humorous, absurd, over-the-top, gleefully mean-spirited sci-fi game show.
        Do not include real-world logos, watermarks, or signatures.
        """;

    private readonly IMessageOutgoing _messageOutgoing;
    private readonly IBotPostConfiguration _botPostConfiguration;
    private readonly IAiImageClient _imageClient;
    private readonly IPersonPhotoStore _photoStore;
    private readonly IGroupMeImageUploader _imageUploader;
    private readonly AchievementImageOptions _options;
    private readonly ILogger<AchievementImageBot> _logger;

    public AchievementImageBot(
        IMessageOutgoing messageOutgoing,
        IBotPostConfiguration botPostConfiguration,
        IAiImageClient imageClient,
        IPersonPhotoStore photoStore,
        IGroupMeImageUploader imageUploader,
        IOptions<AchievementImageOptions> options,
        ILogger<AchievementImageBot> logger)
    {
        _messageOutgoing = messageOutgoing;
        _botPostConfiguration = botPostConfiguration;
        _imageClient = imageClient;
        _photoStore = photoStore;
        _imageUploader = imageUploader;
        _options = options.Value;
        _logger = logger;
    }

    public async Task HandleAsync(
        AchievementImageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_options.ImagesEnabled)
        {
            _logger.LogInformation("AchievementImageBot-skipped, images are disabled");
            return;
        }

        try
        {
            var photo = string.IsNullOrWhiteSpace(request.UserId)
                ? null
                : await _photoStore.GetPhotoAsync(request.UserId, cancellationToken);

            var title = ExtractTitle(request.AchievementText);
            var prompt = BuildPrompt(request, hasReferencePhoto: photo is not null);

            var references = photo is null
                ? Array.Empty<AiImageReference>()
                : new[] { new AiImageReference(photo.Data, photo.MediaType) };

            _logger.LogInformation(
                "AchievementImageBot-generating image for {DisplayName} (reference photo: {HasPhoto})",
                request.DisplayName,
                photo is not null);

            var image = await _imageClient.GenerateImageAsync(
                new AiImageRequest { Prompt = prompt, References = references },
                cancellationToken);

            var pictureUrl = await _imageUploader.UploadAsync(image.Data, image.MediaType, cancellationToken);
            if (pictureUrl is null)
            {
                _logger.LogWarning("AchievementImageBot-image generated but upload failed, posting nothing");
                return;
            }

            var status = await _messageOutgoing.PostAsync(
                BuildCaption(title),
                _botPostConfiguration.BotId,
                new[] { Attachment.Image(pictureUrl) });

            _logger.LogInformation("AchievementImageBot-posted image, result: {Status}", status);
        }
        catch (Exception ex)
        {
            // The achievement text already landed. Swallow rather than rethrow: a
            // content-policy refusal fails identically on every queue retry, and
            // poison-queueing it just makes noise.
            _logger.LogError(ex, "AchievementImageBot-failed to generate or post achievement image");
        }
    }

    /// <summary>
    /// GroupMe's bot post API requires a non-empty <c>text</c>, so an image can never be
    /// posted bare. Repeating the title also re-ties the image to its achievement, which
    /// matters because the image lands well after the text and other messages may have
    /// arrived in between.
    /// </summary>
    internal static string BuildCaption(string? title) =>
        string.IsNullOrWhiteSpace(title) ? $"{TrophyEmoji} Achievement unlocked" : $"{TrophyEmoji} {title}";

    /// <summary>
    /// Builds the generation prompt. When there's no reference photo the crawler is
    /// drawn as a generic figure rather than a guess at someone's face.
    /// </summary>
    internal static string BuildPrompt(AchievementImageRequest request, bool hasReferencePhoto)
    {
        var title = ExtractTitle(request.AchievementText);
        var crawlerName = string.IsNullOrWhiteSpace(request.DisplayName) ? "the crawler" : request.DisplayName;

        var subject = hasReferencePhoto
            ? $"""
               Centered in the card is an exaggerated caricature of the person in the reference photo.
               Keep them clearly recognizable - preserve their face shape, hair, skin tone, facial hair,
               glasses, and any other distinguishing features - but render them as a bold comic-book
               caricature with exaggerated proportions and expression, never as a photograph.
               This is {crawlerName}, a crawler in the dungeon.
               """
            : $"""
               Centered in the card is an exaggerated comic-book caricature of a generic dungeon crawler
               representing {crawlerName}. Do not attempt a specific real person's likeness.
               """;

        var titleLine = string.IsNullOrWhiteSpace(title)
            ? "Render a short achievement title across the top of the card in a bold sci-fi display font."
            : $"""Render the text "{title}" across the top of the card as the achievement title, in a bold sci-fi display font, spelled exactly as given.""";

        return $"""
            {CardStyle}

            {subject}

            Depict them in a scene that illustrates the achievement below - read it and invent the
            visual gag yourself.

            {titleLine}

            The achievement:
            {request.AchievementText}
            """;
    }

    /// <summary>
    /// Pulls the achievement title out of the generated text. AchievementBot's system
    /// prompt pins a fixed shape where the title is its own line, in caps, led by a
    /// trophy emoji.
    /// </summary>
    internal static string? ExtractTitle(string achievementText)
    {
        if (string.IsNullOrWhiteSpace(achievementText))
        {
            return null;
        }

        var lines = achievementText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var titleLine = lines.FirstOrDefault(l => l.StartsWith(TrophyEmoji, StringComparison.Ordinal));

        // Fall back to the first all-caps line for the occasional response that drops
        // the emoji, then give up rather than putting flavor text in the banner.
        titleLine ??= lines
            .Skip(1)
            .FirstOrDefault(l => l.Any(char.IsLetter) && l.ToUpperInvariant() == l);

        if (titleLine is null)
        {
            return null;
        }

        // The trophy is a surrogate pair, so strip it as a string rather than a char.
        if (titleLine.StartsWith(TrophyEmoji, StringComparison.Ordinal))
        {
            titleLine = titleLine[TrophyEmoji.Length..];
        }

        return titleLine.Trim();
    }
}
