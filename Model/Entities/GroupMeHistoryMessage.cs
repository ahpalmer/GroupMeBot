using System.Runtime.Serialization;

namespace GroupMeBot.Model;

[DataContract]
public class GroupMeHistoryMessage
{
    [DataMember(Name = "id")]
    public string? Id { get; set; }

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "text")]
    public string? Text { get; set; }

    [DataMember(Name = "user_id")]
    public string? UserId { get; set; }
}

[DataContract]
public class GroupMeMessagesResponse
{
    [DataMember(Name = "count")]
    public int Count { get; set; }

    [DataMember(Name = "messages")]
    public List<GroupMeHistoryMessage>? Messages { get; set; }
}

[DataContract]
public class GroupMeMessagesApiResponse
{
    [DataMember(Name = "response")]
    public GroupMeMessagesResponse? Response { get; set; }
}
