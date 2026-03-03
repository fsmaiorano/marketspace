using Payment.Api.Endpoints.Dto;
using Payment.Test.Base;
using Payment.Test.Fixtures;

namespace Payment.Test.Endpoints;

public class GetPaymentByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Payment_Exists()
    {
        PaymentEntity payment = PaymentBuilder.CreatePaymentFaker().Generate();
        await Context.Payments.AddAsync(payment);
        await Context.SaveChangesAsync();

        HttpResponseMessage response = await DoGet($"/payment/{payment.Id.Value}");
        PaymentDto? result = await response.Content.ReadFromJsonAsync<PaymentDto>();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_NotFound_When_Payment_Does_Not_Exist()
    {
        HttpResponseMessage response = await DoGet($"/payment/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}