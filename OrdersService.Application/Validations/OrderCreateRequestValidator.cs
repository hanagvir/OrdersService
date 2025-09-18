using FluentValidation;
using OrdersService.Application.DTOsContracts;

public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotNull().NotEmpty();
    }
}