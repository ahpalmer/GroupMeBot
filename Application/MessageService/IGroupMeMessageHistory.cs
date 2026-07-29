namespace GroupMeBot.Application;

public interface IGroupMeMessageHistory
{
    Task<List<GroupMeHistoryMessage>> GetRecentMessagesAsync(string groupId, int limit = 20);
}
