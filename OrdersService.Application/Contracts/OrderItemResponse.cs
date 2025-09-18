namespace OrdersService.Application.Contracts
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
