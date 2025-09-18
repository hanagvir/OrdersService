using OrdersService.Application.Contracts;
using OrdersService.Application.DTOsContracts;

namespace OrdersService.Application.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(string customerId, List<OrderItemCreateRequest> items);
        Task<bool> AddItemToOrderAsync(Guid orderId, OrderItemCreateRequest request);
        Task<OrderResponse?> GetOrderByIdAsync(Guid orderId);
        Task<OrderActionResult> ConfirmOrderAsync(Guid orderId);
        Task<OrderActionResult> CancelOrderAsync(Guid orderId);
        Task<OrderActionResult> DeleteOrderItemAsync(Guid orderId, Guid itemId);

    }
}
