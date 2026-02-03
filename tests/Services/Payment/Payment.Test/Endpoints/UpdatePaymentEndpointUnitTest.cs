using Payment.Test.Base;
using Payment.Test.Fixtures;

namespace Payment.Test.Endpoints;

public class UpdatePaymentEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Payment_Is_Updated_Successfully()
    {
        PaymentEntity payment = PaymentBuilder.CreatePaymentFaker().Generate();
        await Context.Payments.AddAsync(payment);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        UpdatePaymentCommand command = PaymentBuilder.CreateUpdatePaymentCommandFaker(payment.Id.Value).Generate();
        
        HttpResponseMessage response = await DoPut("/payment", command);
        
        response.EnsureSuccessStatusCode();
        Result<UpdatePaymentResult>? result = await response.Content.ReadFromJsonAsync<Result<UpdatePaymentResult>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();

        PaymentEntity? updatedPayment = await Context.Payments.FindAsync(payment.Id);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(command.Status);
    }
}
