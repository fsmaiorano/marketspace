using Payment.Test.Fixtures;

namespace Payment.Test.Endpoints;

public class CreatePaymentEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Payment_Is_Created_Successfully()
    {
        CreatePaymentCommand command = PaymentBuilder.CreateCreatePaymentCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/payment", command);
        
        response.EnsureSuccessStatusCode();
        Result<CreatePaymentResult>? result = await response.Content.ReadFromJsonAsync<Result<CreatePaymentResult>>();
        
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
