using OrdersService.Domain.Entities;
namespace OrdersService.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Guid> AddOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<bool> SaveOrderAsync(Order order);
    }
}
