using Payment.Test.Base;
using Payment.Test.Fixtures;

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
        
        response.EnsureSuccessStatusCode();
        Result<DeletePaymentResult>? result = await response.Content.ReadFromJsonAsync<Result<DeletePaymentResult>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();

        PaymentEntity? deletedPayment = await Context.Payments.FindAsync(payment.Id);
        deletedPayment.Should().BeNull();
    }
}
