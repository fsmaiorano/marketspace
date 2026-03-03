using Payment.Test.Base;
using Payment.Test.Fixtures;
using System.Net;

namespace Payment.Test.Endpoints;

public class DeletePaymentEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Payment_Is_Deleted_Successfully()
    {
        PaymentEntity payment = PaymentBuilder.CreatePaymentFaker().Generate();
        await Context.Payments.AddAsync(payment);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        HttpResponseMessage response = await DoDelete($"/payment/{payment.Id.Value}");
        
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));
        
        PaymentEntity? deletedPayment = await Context.Payments.FindAsync(payment.Id);
        deletedPayment.Should().BeNull();
    }
}
