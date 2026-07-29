using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GroupMeBot.Application;

namespace GroupMeBot.Presentation;

public class BasicResponse
{
    private readonly IMessageIncoming _messageIncoming;
    private readonly ILogger _logger;

    public BasicResponse(IMessageIncoming messageIncoming, ILogger<BasicResponse> logger)
    {
        _messageIncoming = messageIncoming;
        _logger = logger;
    }

    /// <summary>
    /// This is the main Azure Function. It receives an HTTP request, passes it to the Application layer, and returns an appropriate HTTP response to the client.
    /// </summary>
    [Function("BasicResponse")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "GroupMeBot/BasicResponse")] HttpRequest req)
    {
        _logger.LogInformation("GroupMeBot trigger processed a request.");

        _logger.LogInformation("GroupMeBot trigger message attempt to parse incoming request");

        _logger.LogInformation("GroupMeBot trigger message: http request body: {Request}", req);
        IActionResult httpResponse = await _messageIncoming.ParseIncomingRequestAsync(req);

        return httpResponse;
    }
}
