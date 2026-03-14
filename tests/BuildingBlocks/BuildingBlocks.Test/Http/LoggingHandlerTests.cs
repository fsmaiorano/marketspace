using BuildingBlocks.Http;
using Moq.Protected;
using System.Net;

namespace BuildingBlocks.Test.Http;

public class LoggingHandlerTests
{
    private static LoggingHandler BuildSut(
        Mock<ILogger<LoggingHandler>> loggerMock,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Exception? innerException = null)
    {
        Mock<HttpMessageHandler> innerHandlerMock = new();

        if (innerException is not null)
        {
            innerHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(innerException);
        }
        else
        {
            innerHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode));
        }

        LoggingHandler handler = new(loggerMock.Object)
        {
            InnerHandler = innerHandlerMock.Object
        };

        return handler;
    }

    [Fact]
    public async Task SendAsync_SuccessfulRequest_LogsInformation()
    {
        Mock<ILogger<LoggingHandler>> loggerMock = new();
        LoggingHandler sut = BuildSut(loggerMock);
        using HttpClient client = new(sut);

        await client.GetAsync("https://example.com/api/test");

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("GET")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_SuccessfulRequest_ReturnsResponse()
    {
        Mock<ILogger<LoggingHandler>> loggerMock = new();
        LoggingHandler sut = BuildSut(loggerMock, HttpStatusCode.Created);
        using HttpClient client = new(sut);

        HttpResponseMessage response = await client.GetAsync("https://example.com/api/resource");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SendAsync_WhenInnerThrows_LogsErrorAndRethrows()
    {
        Mock<ILogger<LoggingHandler>> loggerMock = new();
        HttpRequestException exception = new("connection refused");
        LoggingHandler sut = BuildSut(loggerMock, innerException: exception);
        using HttpClient client = new(sut);

        Func<Task> act = () => client.GetAsync("https://example.com/fail");

        await act.Should().ThrowAsync<HttpRequestException>();

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
