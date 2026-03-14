using BuildingBlocks.Services.Correlation;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Test.Services;

public class CorrelationIdServiceTests
{
    [Fact]
    public void GetCorrelationId_WithNoHttpContext_ReturnsNonEmptyString()
    {
        IHttpContextAccessor accessor = new HttpContextAccessor(); // HttpContext is null
        CorrelationIdService sut = new(accessor);

        string id = sut.GetCorrelationId();

        id.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetCorrelationId_WithNoHttpContext_ReturnsValidGuid()
    {
        IHttpContextAccessor accessor = new HttpContextAccessor();
        CorrelationIdService sut = new(accessor);

        string id = sut.GetCorrelationId();

        Guid.TryParse(id, out _).Should().BeTrue("CorrelationId must be a valid GUID");
    }

    [Fact]
    public void GetCorrelationId_WhenContextItemSet_ReturnsContextValue()
    {
        const string expected = "test-correlation-id-123";
        DefaultHttpContext httpContext = new();
        httpContext.Items["CorrelationId"] = expected;

        IHttpContextAccessor accessor = new HttpContextAccessor { HttpContext = httpContext };
        CorrelationIdService sut = new(accessor);

        string id = sut.GetCorrelationId();

        id.Should().Be(expected);
    }

    [Fact]
    public void GetCorrelationId_WhenNoContextItem_FallsBackToPrivateField()
    {
        const string expected = "fallback-correlation-id";
        IHttpContextAccessor accessor = new HttpContextAccessor(); // no HttpContext
        CorrelationIdService sut = new(accessor);

        sut.SetCorrelationId(expected);
        string id = sut.GetCorrelationId();

        id.Should().Be(expected);
    }

    [Fact]
    public void SetCorrelationId_WithHttpContext_WritesContextItem()
    {
        const string expected = "my-correlation-id";
        DefaultHttpContext httpContext = new();
        IHttpContextAccessor accessor = new HttpContextAccessor { HttpContext = httpContext };
        CorrelationIdService sut = new(accessor);

        sut.SetCorrelationId(expected);

        httpContext.Items["CorrelationId"].Should().Be(expected);
    }

    [Fact]
    public void SetCorrelationId_WithNoHttpContext_DoesNotThrow()
    {
        IHttpContextAccessor accessor = new HttpContextAccessor();
        CorrelationIdService sut = new(accessor);

        Action act = () => sut.SetCorrelationId("some-id");

        act.Should().NotThrow();
    }

    [Fact]
    public void SetThenGet_WithoutHttpContext_ReturnsSameId()
    {
        const string expected = "background-worker-id";
        IHttpContextAccessor accessor = new HttpContextAccessor();
        CorrelationIdService sut = new(accessor);

        sut.SetCorrelationId(expected);

        sut.GetCorrelationId().Should().Be(expected);
    }
}
