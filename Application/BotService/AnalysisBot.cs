using Microsoft.Extensions.Logging;

namespace GroupMeBot.Application;

public class AnalysisBot: IAnalysisBot
{
    public ILogger Log { get; set; }
    public AnalysisBot(ILogger<AnalysisBot> log)
    {
        Log = log;
    }
}