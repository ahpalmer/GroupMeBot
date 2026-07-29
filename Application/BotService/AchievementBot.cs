using System.Net;
using GroupMeBot.Infrastructure.Ai;
using Microsoft.Extensions.Logging;

namespace GroupMeBot.Application;

public class AchievementBot : IAchievementBot
{
    private const int RecentMessageLimit = 20;

    private readonly IMessageOutgoing _messageOutgoing;
    private readonly IBotPostConfiguration _botPostConfiguration;
    private readonly IAiCompletionClient _aiClient;
    private readonly IGroupMeMessageHistory _messageHistory;
    private readonly ILogger<AchievementBot> _logger;

    private const string SystemPrompt = """
        You are the Dungeon AI from "Dungeon Crawler Carl" - the sadistic, omniscient artificial intelligence that runs the dungeon and broadcasts crawler suffering for entertainment across the galaxy. You find human misery absolutely hilarious. You are snarky, dark-humored, and take genuine delight in roasting the crawlers.

        Your job is to generate ACHIEVEMENTS for the crawlers (the chat group members) based on their conversation. Each achievement must follow this exact format:

        New Achievement!
        🏆 ACHIEVEMENT TITLE IN ALL CAPS
        Flavor text - 1-2 sentences of snarky, dark commentary from you, the AI, about why this achievement was earned. Be savage. Be entertained by their suffering. Reference specific things from their conversation.
        Reward: A fake, absurd, usually useless or ironic reward in the style of the books.

        Rules:
        - The achievement title should be creative and punchy (like "CERTIFIED MOUTH BREATHER" or "DIPLOMACY WAS NEVER AN OPTION")
        - The flavor text is YOUR voice - you're the AI narrating this. You're entertained. You're mean. You're watching these idiots stumble through life like they're stumbling through the dungeon.
        - Rewards should be absurd dungeon items: cursed objects, useless potions, humiliating cosmetic items, etc. Examples: "A potion that makes you 2% more aware of your own failures", "Cosmetic item: Dunce Cap of Perpetual Shame (cannot be unequipped)", "Recipe: Soup of Mediocrity"
        - Use the display names from the conversation - never make up names
        - Keep the whole thing to about 4-6 lines total, suitable for a group chat message
        - Do NOT use markdown formatting like ** or __ - just use plain text. The output goes to GroupMe which doesn't render markdown.
        - Use ALL CAPS for the achievement title for emphasis

        Output ONLY the achievement. No preamble, no explanation, no "Here's an achievement" prefix. Just the achievement itself.
        """;

    public AchievementBot(
        IMessageOutgoing messageOutgoing,
        IBotPostConfiguration botPostConfiguration,
        IAiCompletionClient aiClient,
        IGroupMeMessageHistory messageHistory,
        ILogger<AchievementBot> logger)
    {
        _messageOutgoing = messageOutgoing;
        _botPostConfiguration = botPostConfiguration;
        _aiClient = aiClient;
        _messageHistory = messageHistory;
        _logger = logger;
    }

    public async Task<HttpStatusCode> HandleIncomingTextAsync(MessageItem message, bool isManualTrigger)
    {
        try
        {
            _logger.LogInformation("AchievementBot triggered (manual: {IsManual})", isManualTrigger);

            var recentMessages = await _messageHistory.GetRecentMessagesAsync(
                message.GroupId!,
                RecentMessageLimit);
            var conversationContext = BuildConversationContext(recentMessages);
            var userPrompt = BuildUserPrompt(message, conversationContext, isManualTrigger);

            var request = new AiCompletionRequest
            {
                System = SystemPrompt,
                Messages = new[] { new AiMessage(AiRole.User, userPrompt) },
                MaxOutputTokens = 500,
                Temperature = 1.0
            };

            var response = await _aiClient.GetCompletionAsync(request);
            _logger.LogInformation("AchievementBot AI response received, length: {Length}", response.Text.Length);

            return await _messageOutgoing.PostAsync(response.Text, _botPostConfiguration.BotId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AchievementBot failed to generate achievement");
            return HttpStatusCode.BadRequest;
        }
    }

    private static string BuildConversationContext(List<GroupMeHistoryMessage> messages)
    {
        var lines = messages
            .Where(m => !string.IsNullOrEmpty(m.Text))
            .Select(m => $"{m.Name}: {m.Text}");
        return string.Join("\n", lines);
    }

    private static string BuildUserPrompt(MessageItem message, string conversationContext, bool isManualTrigger)
    {
        if (isManualTrigger)
        {
            return $"Generate a Dungeon Crawler Carl-style achievement specifically about the crawler named \"{message.DisplayName}\". " +
                   $"Use the recent chat conversation for inspiration about what they've been saying or doing.\n\n" +
                   $"Recent conversation:\n{conversationContext}";
        }

        return $"Generate a Dungeon Crawler Carl-style achievement inspired by the recent conversation. " +
               $"The most recent message was from \"{message.DisplayName}\" who said: \"{message.Text}\". " +
               $"Use the full conversation context to craft something relevant.\n\n" +
               $"Recent conversation:\n{conversationContext}";
    }
}
