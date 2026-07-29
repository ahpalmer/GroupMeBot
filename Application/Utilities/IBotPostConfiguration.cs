using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeBot.Application;

public interface IBotPostConfiguration
{
    string BotPostUrl { get; set; }
    string BotId { get; set; }
    string GroupMeAccessToken { get; set; }
}
