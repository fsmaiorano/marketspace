using BuildingBlocks.Http;
using BuildingBlocks.Services.Correlation;
using Moq.Protected;
using System.Net;

namespace BuildingBlocks.Test.Http;

public class CorrelationIdHandlerTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private static (CorrelationIdHandler handler, Mock<HttpMessageHandler> innerMock) BuildSut(
        string correlationId = "test-cid-001")
    {
        Mock<ICorrelationIdService> serviceMock = new();
        serviceMock.Setup(s => s.GetCorrelationId()).Returns(correlationId);

        Mock<HttpMessageHandler> innerMock = new();
        innerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        CorrelationIdHandler handler = new(serviceMock.Object)
        {
            InnerHandler = innerMock.Object
        };

        return (handler, innerMock);
    }

    [Fact]
    public async Task SendAsync_NoExistingHeader_AddsCorrelationIdHeader()
    {
        const string expectedCid = "cid-12345-abcde";
        var (handler, innerMock) = BuildSut(expectedCid);
        using HttpClient client = new(handler);

        await client.GetAsync("https://example.com/api/orders");

        innerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains(CorrelationIdHeader) &&
                req.Headers.GetValues(CorrelationIdHeader).First() == expectedCid),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_ExistingHeader_DoesNotOverwrite()
    {
        const string existingCid = "already-set-cid";
        var (handler, innerMock) = BuildSut("service-generated-cid");
        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add(CorrelationIdHeader, existingCid);

        await client.GetAsync("https://example.com/api/basket");

        innerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains(CorrelationIdHeader) &&
                req.Headers.GetValues(CorrelationIdHeader).First() == existingCid),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_ReturnsInnerHandlerResponse()
    {
        var (handler, _) = BuildSut();
        using HttpClient client = new(handler);

        HttpResponseMessage response = await client.GetAsync("https://example.com/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
