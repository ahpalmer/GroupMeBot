using System.Net;
using System.Text;
using GroupMeBot.Application;
using Microsoft.Extensions.Logging;
using Moq;

namespace GroupMeBot.Tests;

[TestClass]
public class GroupMeImageUploaderTests
{
    private static readonly byte[] SampleImage = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

    [TestMethod]
    public async Task Upload_SendsAccessTokenInHeaderOnly()
    {
        const string accessToken = "sensitive-access-token";
        var handler = new CapturingHttpMessageHandler();
        var uploader = CreateUploader(handler, accessToken);

        var url = await uploader.UploadAsync(SampleImage, "image/jpeg");

        Assert.AreEqual("https://i.groupme.com/123x456.jpeg.abcdef", url);
        Assert.AreEqual(accessToken, handler.AccessToken);
        Assert.IsNotNull(handler.RequestUri);
        Assert.IsFalse(handler.RequestUri.Query.Contains(accessToken, StringComparison.Ordinal));
        Assert.IsFalse(handler.RequestUri.Query.Contains("token=", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Upload_PostsMultipartToTheImageService()
    {
        var handler = new CapturingHttpMessageHandler();
        var uploader = CreateUploader(handler, "token");

        await uploader.UploadAsync(SampleImage, "image/jpeg");

        Assert.AreEqual(HttpMethod.Post, handler.Method);
        Assert.AreEqual("https://image.groupme.com/pictures", handler.RequestUri!.ToString());
        StringAssert.StartsWith(handler.ContentType, "multipart/form-data");
    }

    [TestMethod]
    public async Task Upload_UnsuccessfulResponse_ReturnsNull()
    {
        // The achievement text already posted, so a failed upload must degrade quietly
        var handler = new CapturingHttpMessageHandler(HttpStatusCode.Unauthorized);
        var uploader = CreateUploader(handler, "invalid-token");

        var url = await uploader.UploadAsync(SampleImage, "image/jpeg");

        Assert.IsNull(url);
    }

    [TestMethod]
    public async Task Upload_ResponseWithoutPictureUrl_ReturnsNull()
    {
        var handler = new CapturingHttpMessageHandler(responseJson: """{"payload":{}}""");
        var uploader = CreateUploader(handler, "token");

        var url = await uploader.UploadAsync(SampleImage, "image/jpeg");

        Assert.IsNull(url);
    }

    [TestMethod]
    public async Task Upload_EmptyImage_ReturnsNullWithoutCallingTheService()
    {
        var handler = new CapturingHttpMessageHandler();
        var uploader = CreateUploader(handler, "token");

        var url = await uploader.UploadAsync(Array.Empty<byte>(), "image/jpeg");

        Assert.IsNull(url);
        Assert.IsNull(handler.RequestUri);
    }

    private static GroupMeImageUploader CreateUploader(HttpMessageHandler handler, string accessToken)
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handler));

        var configuration = new Mock<IBotPostConfiguration>();
        configuration.Setup(config => config.GroupMeAccessToken).Returns(accessToken);

        return new GroupMeImageUploader(
            configuration.Object,
            clientFactory.Object,
            Mock.Of<ILogger<GroupMeImageUploader>>());
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private const string DefaultResponseJson = """
            {
              "payload": {
                "url": "https://i.groupme.com/123x456.jpeg.abcdef",
                "picture_url": "https://i.groupme.com/123x456.jpeg.abcdef"
              }
            }
            """;

        private readonly HttpStatusCode _statusCode;
        private readonly string _responseJson;

        public CapturingHttpMessageHandler(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string responseJson = DefaultResponseJson)
        {
            _statusCode = statusCode;
            _responseJson = responseJson;
        }

        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? AccessToken { get; private set; }
        public string? ContentType { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Method = request.Method;
            AccessToken = request.Headers.GetValues("X-Access-Token").Single();
            ContentType = request.Content?.Headers.ContentType?.ToString();

            if (_statusCode != HttpStatusCode.OK)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent("nope", Encoding.UTF8, "text/plain")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
