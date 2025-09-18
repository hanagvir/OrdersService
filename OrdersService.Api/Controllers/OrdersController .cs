using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Application.Contracts;
using OrdersService.Application.DTOsContracts;
using OrdersService.Application.Services;

namespace OrdersService.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController(IOrderService orderService, ILogger<OrdersController> logger) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;
        private readonly ILogger<OrdersController> _logger = logger;

        /// <summary>
        /// Creates a new order with items.
        /// </summary>
        /// <param name="request">Order creation request.</param>
        /// <returns>
        /// Returns 201 Created with the order ID if successful.<br/>
        /// Returns 400 Bad Request if validation fails.
        /// </returns>
        /// <response code="201">Order created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            _logger.LogInformation("Received CreateOrder request for CustomerId: {CustomerId}", request.CustomerId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var orderId = await _orderService.CreateOrderAsync(request.CustomerId, request.Items);
                _logger.LogInformation("Order created successfully. OrderId: {OrderId}", orderId);
                return CreatedAtAction(nameof(CreateOrder), new { id = orderId }, new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for CustomerId: {CustomerId}", request.CustomerId);
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        /// <summary>
        /// Adds an item to an existing order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <param name="request">Order item creation request.</param>
        /// <returns>
        /// Returns 200 OK if the item was added.<br/>
        /// Returns 404 Not Found if the order does not exist.<br/>
        /// Returns 400 Bad Request if validation fails.
        /// </returns>
        /// <response code="200">Item added successfully.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("{id}/items")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> AddItemToOrder([FromRoute] Guid id, [FromBody] OrderItemCreateRequest request)
        {
            _logger.LogInformation("Received AddItemToOrder request for OrderId: {OrderId}, Sku: {Sku}", id, request.Sku);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _orderService.AddItemToOrderAsync(id, request);
                _logger.LogInformation("Item added to order successfully. OrderId: {OrderId}, Sku: {Sku}", id, request.Sku);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found. OrderId: {OrderId}", id);
                return NotFound(ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error adding item to order. OrderId: {OrderId}, Sku: {Sku}", id, request.Sku);
                return Conflict("A concurrency error occurred while adding the item to the order. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to order. OrderId: {OrderId}, Sku: {Sku}", id, request.Sku);
                return StatusCode(500, "An error occurred while adding the item to the order.");
            }
        }

        /// <summary>
        /// Gets an order by its ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>
        /// Returns 200 OK with the order if found.<br/>
        /// Returns 404 Not Found if the order does not exist.
        /// </returns>
        /// <response code="200">Order found.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
        {
            _logger.LogInformation("Received GetOrderById request for OrderId: {OrderId}", id);

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found. OrderId: {OrderId}", id);
                    return NotFound($"Order with ID '{id}' not found.");
                }

                _logger.LogInformation("Order retrieved successfully. OrderId: {OrderId}", id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order. OrderId: {OrderId}", id);
                return StatusCode(500, "An error occurred while retrieving the order.");
            }
        }

        /// <summary>
        /// Confirms an order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>
        /// Returns 200 OK if confirmed.<br/>
        /// Returns 404 Not Found if not found.<br/>
        /// Returns 409 Conflict if already confirmed/canceled.<br/>
        /// Returns 500 Internal server error.
        /// </returns>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> ConfirmOrder([FromRoute] Guid id)
        {
            _logger.LogInformation("Received ConfirmOrder request for OrderId: {OrderId}", id);

            try
            {
                var result = await _orderService.ConfirmOrderAsync(id);
                if (result == OrderActionResult.NotFound)
                    return NotFound($"Order with ID '{id}' not found.");
                if (result == OrderActionResult.InvalidState)
                    return Conflict("Order cannot be confirmed in its current state.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order. OrderId: {OrderId}", id);
                return StatusCode(500, "An error occurred while confirming the order.");
            }
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>
        /// Returns 200 OK if canceled.<br/>
        /// Returns 404 Not Found if not found.<br/>
        /// Returns 409 Conflict if already confirmed/canceled.<br/>
        /// Returns 500 Internal server error.
        /// </returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
        {
            _logger.LogInformation("Received CancelOrder request for OrderId: {OrderId}", id);

            try
            {
                var result = await _orderService.CancelOrderAsync(id);
                if (result == OrderActionResult.NotFound)
                    return NotFound($"Order with ID '{id}' not found.");
                if (result == OrderActionResult.InvalidState)
                    return Conflict("Order cannot be canceled in its current state.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order. OrderId: {OrderId}", id);
                return StatusCode(500, "An error occurred while canceling the order.");
            }
        }

        /// <summary>
        /// Deletes an item from an order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <param name="itemId">Order item ID.</param>
        /// <returns>
        /// Returns 200 OK if deleted.<br/>
        /// Returns 404 Not Found if order or item does not exist.<br/>
        /// Returns 409 Conflict if order cannot be modified.<br/>
        /// Returns 500 Internal server error.
        /// </returns>
        [HttpDelete("{id}/items/{itemId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeleteOrderItem([FromRoute] Guid id, [FromRoute] Guid itemId)
        {
            _logger.LogInformation("Received DeleteOrderItem request for OrderId: {OrderId}, ItemId: {ItemId}", id, itemId);

            try
            {
                var result = await _orderService.DeleteOrderItemAsync(id, itemId);
                if (result == OrderActionResult.NotFound)
                    return NotFound($"Order or item not found. OrderId: '{id}', ItemId: '{itemId}'.");
                if (result == OrderActionResult.InvalidState)
                    return Conflict("Order cannot be modified in its current state.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item from order. OrderId: {OrderId}, ItemId: {ItemId}", id, itemId);
                return StatusCode(500, "An error occurred while deleting the item from the order.");
            }
        }
    }
}