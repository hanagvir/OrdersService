namespace OrdersService.Application.DTOsContracts
{
    public class OrderItemCreateRequest
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
