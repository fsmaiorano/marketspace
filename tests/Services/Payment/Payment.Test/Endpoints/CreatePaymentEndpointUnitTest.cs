using Payment.Api.Domain.ValueObjects;
using Payment.Test.Base;
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
        
        // As it is a create operation, we could also verify if it exists in DB, getting the ID from result
        PaymentEntity? createdPayment = await Context.Payments.FindAsync(PaymentId.Of(result.Data.PaymentId));
        createdPayment.Should().NotBeNull();
        createdPayment!.OrderId.Should().Be(command.OrderId);
    }
}
