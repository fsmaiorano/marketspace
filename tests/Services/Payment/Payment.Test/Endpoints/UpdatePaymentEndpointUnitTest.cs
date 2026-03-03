using Payment.Test.Base;
using Payment.Test.Fixtures;
using System.Net;

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

        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

        PaymentEntity? updatedPayment = await Context.Payments.FindAsync(payment.Id);
        updatedPayment.Should().NotBeNull();
    }
}
