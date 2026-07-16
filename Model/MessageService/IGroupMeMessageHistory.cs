namespace GroupMeBot.Model;

public interface IGroupMeMessageHistory
{
    Task<List<GroupMeHistoryMessage>> GetRecentMessagesAsync(string groupId, int limit = 20);
}
