using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.DTOs;
using OrdersService.Models;
using OrdersService.Services;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var orderId = await _orderService.CreateOrderAsync(request.CustomerId);
            return CreatedAtAction(nameof(GetOrder), new { id = orderId }, new { id = orderId });
        }

        [HttpPost("{orderId}/items")]
        public async Task<IActionResult> AddItem(Guid orderId, [FromBody] OrderItemCreateRequest request)
        {
            var order = await _dbContext.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Sku = request.Sku,
                Quantity = request.Quantity,
                UnitPrice = 0 // יתעדכן ע"י ה־worker
            };

            order.Items.Add(item);

            // עדכון TotalAmount
            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, item);
        }

        // Placeholder for GET /api/orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetOrder(Guid id) => Ok();
    }
}
