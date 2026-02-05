using Payment.Api.Application.Payment.PatchPaymentStatus;
using Payment.Test.Fixtures;

namespace Payment.Test.Endpoints;

public class PatchPaymentStatusEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Patch_Payment_Status_Endpoint()
    {
        PaymentEntity? Payment = PaymentBuilder.CreatePaymentFaker().Generate();

        Context.Payments.Add(Payment);
        await Context.SaveChangesAsync();

        PatchPaymentStatusCommand command = PaymentBuilder.CreatePatchPaymentStatusCommandFaker(Payment.Id.Value).Generate();

        HttpResponseMessage response = await DoPatch("/Payment/status", command);
        Result<PatchPaymentStatusResult>? result = await response.Content.ReadFromJsonAsync<Result<PatchPaymentStatusResult>>();

        PaymentEntity? updatedPayment = await Context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == Payment.Id);

        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
        updatedPayment?.Status.Should().Be(command.Status);
    }
}
