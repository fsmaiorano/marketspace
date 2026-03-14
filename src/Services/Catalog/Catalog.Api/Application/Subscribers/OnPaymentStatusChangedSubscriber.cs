using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Catalog.Api.Application.Catalog.UpdateStock;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.ValueObjects;

namespace Catalog.Api.Application.Subscribers;

/// <summary>
/// Handles payment status changes:
/// - Authorized/Captured  → confirms reservation (removes from Reserved)
/// - Failed/Rejected/Cancelled/Refunded/Chargeback → releases reservation back to Available
/// </summary>
public class OnPaymentStatusChangedSubscriber(
    IAppLogger<OnPaymentStatusChangedSubscriber> logger,
    ICatalogRepository repository,
    IEventBus eventBus)
    : IIntegrationEventHandler<PaymentStatusChangedIntegrationEvent>
{
    // PaymentStatus: 3=Authorized, 4=Captured
    private static readonly HashSet<int> ConfirmStatuses = [3, 4];
    // PaymentStatus: 5=Failed, 6=Rejected, 7=Cancelled, 8=Refunded, 9=Chargeback
    private static readonly HashSet<int> ReleaseStatuses = [5, 6, 7, 8, 9];

    public async Task HandleAsync(PaymentStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        bool isConfirm = ConfirmStatuses.Contains(@event.PaymentStatus);
        bool isRelease = ReleaseStatuses.Contains(@event.PaymentStatus);

        if (!isConfirm && !isRelease)
            return;

        string operation = isConfirm ? "Confirming" : "Releasing";
        logger.LogInformation(LogTypeEnum.Application,
            "Payment {Status} — {Operation} reservation for order {OrderId}, {ItemCount} item(s). CorrelationId: {CorrelationId}",
            @event.PaymentStatusName, operation, @event.OrderId, @event.Items.Count, @event.CorrelationId);

        foreach (OrderItemData item in @event.Items)
        {
            CatalogEntity? entity = await repository.GetByIdAsync(
                CatalogId.Of(item.CatalogId), isTrackingEnabled: true);

            if (entity is null)
            {
                logger.LogError(LogTypeEnum.Business, null,
                    "Catalog {CatalogId} not found while processing payment status change.", item.CatalogId);
                continue;
            }

            try
            {
                if (isConfirm)
                    entity.ConfirmReservation(item.Quantity);
                else
                    entity.ReleaseReservation(item.Quantity);

                await repository.UpdateAsync(entity);

                logger.LogInformation(LogTypeEnum.Business,
                    "Reservation {Operation} for catalog {CatalogId} — {Quantity} unit(s). Available: {Available}, Reserved: {Reserved}",
                    operation.ToLowerInvariant(), item.CatalogId, item.Quantity,
                    entity.Stock.Available, entity.Stock.Reserved);

                await eventBus.PublishAsync(new CatalogStockUpdatedIntegrationEvent
                {
                    CatalogId = item.CatalogId,
                    MerchantId = entity.MerchantId,
                    ProductName = entity.Name,
                    Available = entity.Stock.Available,
                    Reserved = entity.Stock.Reserved,
                    CorrelationId = @event.CorrelationId
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(LogTypeEnum.Exception, ex,
                    "Error processing reservation {Operation} for catalog {CatalogId}.",
                    operation.ToLowerInvariant(), item.CatalogId);
            }
        }
    }
}
