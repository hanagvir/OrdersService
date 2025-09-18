namespace OrdersService.Application.DTOsContracts
{
    public class OrderCreateRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<OrderItemCreateRequest> Items { get; set; } = new();
    }
}
