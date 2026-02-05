using Payment.Api.Domain.Entities;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Repositories;

public interface IPaymentRepository
{
    Task<int> AddAsync(PaymentEntity payment, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(PaymentEntity payment, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(PaymentId id, CancellationToken cancellationToken = default);

    Task<PaymentEntity?> GetByIdAsync(PaymentId id, bool isTrackingEnabled = true,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentEntity>> GetAllCreatedPaymentsAsync(bool isTrackingEnabled = true, CancellationToken cancellationToken = default);
    Task<int> PatchStatusAsync(PaymentEntity payment, CancellationToken cancellationToken = default);
}
