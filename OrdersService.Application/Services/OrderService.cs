using Microsoft.Extensions.Logging;
using OrdersService.Application.Contracts;
using OrdersService.Application.DTOsContracts;
using OrdersService.Application.Interfaces;
using OrdersService.Application.Services;
using OrdersService.Domain.Entities;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateOrderAsync(string customerId, List<OrderItemCreateRequest> items)
    {
        _logger.LogInformation("Creating order for CustomerId: {CustomerId} with {ItemCount} items", customerId, items.Count);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(x => x.Quantity * x.UnitPrice);
        var orderId = await _orderRepository.AddOrderAsync(order);

        _logger.LogInformation("Order persisted. OrderId: {OrderId}", orderId);
        return orderId;
    }

    public async Task<bool> AddItemToOrderAsync(Guid orderId, OrderItemCreateRequest request)
    {
        _logger.LogInformation("Adding item to order. OrderId: {OrderId}, Sku: {Sku}", orderId, request.Sku);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        }

        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Sku = request.Sku,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        order.Items ??= new List<OrderItem>();
        order.Items.Add(item);
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        order.UpdatedAt = DateTime.UtcNow;

        var success = await _orderRepository.SaveOrderAsync(order);
        _logger.LogInformation("Item added and order updated. Success: {Success}", success);
        return success;
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null) return null;

        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<OrderActionResult> ConfirmOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return OrderActionResult.NotFound;

        if (order.Status != OrderStatus.Draft)
            return OrderActionResult.InvalidState;

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.SaveOrderAsync(order);
        return OrderActionResult.Success;
    }

    public async Task<OrderActionResult> CancelOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return OrderActionResult.NotFound;

        if (order.Status != OrderStatus.Draft)
            return OrderActionResult.InvalidState;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.SaveOrderAsync(order);
        return OrderActionResult.Success;
    }

    public async Task<OrderActionResult> DeleteOrderItemAsync(Guid orderId, Guid itemId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return OrderActionResult.NotFound;

        if (order.Status != OrderStatus.Draft)
            return OrderActionResult.InvalidState;

        var item = order.Items?.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return OrderActionResult.NotFound;

        order.Items.Remove(item);
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.SaveOrderAsync(order);
        return OrderActionResult.Success;
    }
}