using BuildingBlocks.Middlewares;
using BuildingBlocks.Services.Correlation;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Test.Middlewares;

public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private static (CorrelationIdMiddleware middleware, Mock<ICorrelationIdService> serviceMock, DefaultHttpContext ctx)
        BuildSut(bool addIncomingHeader = false, string incomingId = "existing-id")
    {
        DefaultHttpContext ctx = new();
        ctx.Response.Body = new System.IO.MemoryStream(); // required for OnStarting

        if (addIncomingHeader)
            ctx.Request.Headers[CorrelationIdHeader] = incomingId;

        Mock<ICorrelationIdService> serviceMock = new();

        RequestDelegate next = _ => Task.CompletedTask;
        CorrelationIdMiddleware middleware = new(next);

        return (middleware, serviceMock, ctx);
    }

    [Fact]
    public async Task InvokeAsync_NoIncomingHeader_GeneratesNewCorrelationId()
    {
        var (middleware, serviceMock, ctx) = BuildSut();
        string? capturedId = null;
        serviceMock.Setup(s => s.SetCorrelationId(It.IsAny<string>()))
                   .Callback<string>(id => capturedId = id);

        await middleware.InvokeAsync(ctx, serviceMock.Object);

        capturedId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(capturedId, out _).Should().BeTrue("generated ID must be a valid GUID");
    }

    [Fact]
    public async Task InvokeAsync_WithIncomingHeader_UsesExistingId()
    {
        const string existingId = "my-existing-cid-abc123";
        var (middleware, serviceMock, ctx) = BuildSut(addIncomingHeader: true, incomingId: existingId);
        string? capturedId = null;
        serviceMock.Setup(s => s.SetCorrelationId(It.IsAny<string>()))
                   .Callback<string>(id => capturedId = id);

        await middleware.InvokeAsync(ctx, serviceMock.Object);

        capturedId.Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_CallsSetCorrelationIdOnService()
    {
        var (middleware, serviceMock, ctx) = BuildSut();

        await middleware.InvokeAsync(ctx, serviceMock.Object);

        serviceMock.Verify(s => s.SetCorrelationId(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SetsRequestHeaderWhenAbsent()
    {
        var (middleware, serviceMock, ctx) = BuildSut();

        await middleware.InvokeAsync(ctx, serviceMock.Object);

        ctx.Request.Headers.ContainsKey(CorrelationIdHeader).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        bool nextCalled = false;
        DefaultHttpContext ctx = new();
        ctx.Response.Body = new System.IO.MemoryStream();

        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        CorrelationIdMiddleware middleware = new(next);
        Mock<ICorrelationIdService> serviceMock = new();

        await middleware.InvokeAsync(ctx, serviceMock.Object);

        nextCalled.Should().BeTrue();
    }
}
