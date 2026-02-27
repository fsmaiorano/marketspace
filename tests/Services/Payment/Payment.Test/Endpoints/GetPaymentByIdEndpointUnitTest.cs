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
        Result<GetPaymentByIdResult>? result = await response.Content.ReadFromJsonAsync<Result<GetPaymentByIdResult>>();
        
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.Payment.Id.Should().Be(payment.Id.Value);
    }

    [Fact]
    public async Task Returns_NotFound_When_Payment_Does_Not_Exist()
    {
        HttpResponseMessage response = await DoGet($"/payment/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
