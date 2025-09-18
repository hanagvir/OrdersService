using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrdersService.Application.Interfaces;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Data;

namespace OrdersService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDbContext _dbContext;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrdersDbContext dbContext, ILogger<OrderRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Guid> AddOrderAsync(Order order)
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
            return order.Id;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            _logger.LogDebug("Fetching order from database. OrderId: {OrderId}", orderId);
            return await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> SaveOrderAsync(Order order)
        {
            _logger.LogDebug("Saving updated order to database. OrderId: {OrderId}", order.Id);
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
            _logger.LogDebug("Order updated successfully. OrderId: {OrderId}", order.Id);
            return true;
        }
    }
}
