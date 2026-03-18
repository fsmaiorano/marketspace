using Order.Api.Application.Order.GetOrdersByCustomer;
using Order.Api.Endpoints.Dto;
using Order.Test.Fixtures;
using System.Net;

namespace Order.Test.Endpoints;

public class GetOrdersByCustomerEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Customer_Has_Orders()
    {
        Guid customerId = Guid.NewGuid();
        OrderEntity order1 = await fixture.CreateOrder(customerId);
        OrderEntity order2 = await fixture.CreateOrder(customerId);

        HttpResponseMessage response = await DoGet($"/order/customer/{customerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Returns_List_Of_Orders_For_Customer()
    {
        Guid customerId = Guid.NewGuid();
        await fixture.CreateOrder(customerId);
        await fixture.CreateOrder(customerId);

        HttpResponseMessage response = await DoGet($"/order/customer/{customerId}");
        List<OrderDto>? result = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Returns_Empty_List_When_Customer_Has_No_Orders()
    {
        Guid customerId = Guid.NewGuid();

        HttpResponseMessage response = await DoGet($"/order/customer/{customerId}");
        List<OrderDto>? result = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_Orders_Belonging_Only_To_Requested_Customer()
    {
        Guid customerA = Guid.NewGuid();
        Guid customerB = Guid.NewGuid();

        await fixture.CreateOrder(customerA);
        await fixture.CreateOrder(customerA);
        await fixture.CreateOrder(customerB);

        HttpResponseMessage response = await DoGet($"/order/customer/{customerA}");
        List<OrderDto>? result = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.CustomerId == customerA);
    }

    [Fact]
    public async Task Respects_Limit_Query_Parameter()
    {
        Guid customerId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
            await fixture.CreateOrder(customerId);

        HttpResponseMessage response = await DoGet($"/order/customer/{customerId}?limit=2");
        List<OrderDto>? result = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Returns_Orders_With_Expected_Fields()
    {
        Guid customerId = Guid.NewGuid();
        await fixture.CreateOrder(customerId);

        HttpResponseMessage response = await DoGet($"/order/customer/{customerId}");
        List<OrderDto>? result = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

        OrderDto order = result!.First();
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().NotBeNullOrWhiteSpace();
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }
}
