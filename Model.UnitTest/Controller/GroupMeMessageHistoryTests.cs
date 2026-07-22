using System.Net;
using System.Text;
using GroupMeBot.Model;
using Microsoft.Extensions.Logging;
using Moq;

namespace GroupMeBot.Tests;

[TestClass]
public class GroupMeMessageHistoryTests
{
    [TestMethod]
    public async Task GetRecentMessages_SendsAccessTokenInHeaderOnly()
    {
        const string accessToken = "sensitive-access-token";
        var handler = new CapturingHttpMessageHandler();
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handler));

        var configuration = new Mock<IBotPostConfiguration>();
        configuration.Setup(config => config.GroupMeAccessToken).Returns(accessToken);

        var history = new GroupMeMessageHistory(
            configuration.Object,
            clientFactory.Object,
            Mock.Of<ILogger<GroupMeMessageHistory>>());

        var messages = await history.GetRecentMessagesAsync("89303421");

        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(accessToken, handler.AccessToken);
        Assert.IsNotNull(handler.RequestUri);
        Assert.IsFalse(handler.RequestUri.Query.Contains(accessToken, StringComparison.Ordinal));
        Assert.IsFalse(handler.RequestUri.Query.Contains("token=", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string? AccessToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            AccessToken = request.Headers.GetValues("X-Access-Token").Single();

            const string responseJson = """
                {
                  "response": {
                    "count": 1,
                    "messages": [
                      {
                        "id": "message-id",
                        "name": "Andrew",
                        "text": "hello",
                        "user_id": "user-id"
                      }
                    ]
                  }
                }
                """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
