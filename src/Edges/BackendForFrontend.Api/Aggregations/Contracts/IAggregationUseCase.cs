using BackendForFrontend.Api.Aggregations.Dtos;

namespace BackendForFrontend.Api.Aggregations.Contracts;

public interface ICustomerDashboardUseCase
{
    Task<CustomerDashboardResponse> GetCustomerDashboardAsync(Guid customerId);
}

public interface IOrderSummaryUseCase
{
    Task<OrderSummaryResponse> GetOrderSummaryAsync(Guid orderId);
}
