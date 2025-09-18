# OrdersService

A RESTful API for managing customer orders, built with ASP.NET Core (.NET 9, C# 13).

## Features

- Create, update, confirm, and cancel orders
- Add and remove items from orders
- Retrieve order details
- Optimistic concurrency handling
- Validation with FluentValidation
- Structured error handling and logging

## Endpoints

- `POST /api/orders` — Create a new order
- `POST /api/orders/{id}/items` — Add item to order
- `DELETE /api/orders/{id}/items/{itemId}` — Remove item from order
- `GET /api/orders/{id}` — Get order by ID
- `POST /api/orders/{id}/confirm` — Confirm order
- `POST /api/orders/{id}/cancel` — Cancel order

## Getting Started

1. **Clone the repository**
2. **Install dependencies**
3. **Run the application**
4. **API Documentation**
   - Swagger UI available at `/swagger` when running locally.

## Project Structure

- `OrdersService.Api` — ASP.NET Core Web API controllers
- `OrdersService.Application` — Business logic and DTOs
- `OrdersService.Domain` — Domain entities and enums
- `OrdersService.Infrastructure` — Data access and repositories

## Testing

Run unit tests with:

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License.