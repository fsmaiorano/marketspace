using Payment.Test.Fixtures;
using System.Net;

namespace Payment.Test.Endpoints;

public class CreatePaymentEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Payment_Is_Created_Successfully()
    {
        CreatePaymentCommand command = PaymentBuilder.CreateCreatePaymentCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/payment", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.Created));
    }
}