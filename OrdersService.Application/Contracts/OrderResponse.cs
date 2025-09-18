using System;
namespace OrdersService.Application.Contracts
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public List<OrderItemResponse> Items { get; set; } = [];
    }
}
