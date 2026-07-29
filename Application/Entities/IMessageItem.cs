using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeBot.Application;

/// <summary>
/// A generic message item from the server - support for group and direct messages
/// </summary>
public interface IMessageItem
{
    /// <summary>
    /// Gets or sets the Text of the message
    /// </summary>
    [DataMember(Name = "text", EmitDefaultValue = false)]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user that sent the message
    /// </summary>
    [DataMember(Name = "user_id", EmitDefaultValue = false)]
    public string? UserId { get; set; }
    
    /// <summary>
    /// The id for the group this message belongs to
    /// </summary>
    [DataMember(Name = "group_id", EmitDefaultValue = false)]
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the variable "id" for the message.  Use is unspecified.
    /// </summary>
    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string? unspecifiedId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user who sent this message
    /// </summary>
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the ID of the system that sent the message
    /// </summary>
    [DataMember(Name = "sender_id", EmitDefaultValue = false)]
    public string? SystemSenderId { get; set; }

    /// <summary>
    /// Gets or sets the type for the message sender
    /// </summary>
    [DataMember(Name = "sender_type", EmitDefaultValue = false)]
    public string? SystemSenderType { get; set; }

    /// <summary>
    /// Gets or sets the source guid for this message
    /// </summary>
    [DataMember(Name = "source_guid", EmitDefaultValue = false)]
    public string? sourceGuid { get; set; }

    /// <summary>
    /// Displays whether this is a system message or not
    /// </summary>
    [DataMember(Name = "system", EmitDefaultValue = false)]
    public bool? IsSystem { get; set; }
}
