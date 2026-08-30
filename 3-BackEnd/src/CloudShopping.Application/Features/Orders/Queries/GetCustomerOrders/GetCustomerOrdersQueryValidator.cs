using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders
{
    public sealed class GetCustomerOrdersQueryValidator : AbstractValidator<GetCustomerOrdersQuery>
    {
        public GetCustomerOrdersQueryValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("O ID do cliente é inválido.");
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("A página deve ser maior que zero.");
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("O tamanho da página deve ser maior que zero.")
                .LessThanOrEqualTo(100).WithMessage("O tamanho máximo da página é 100.");
        }
    }
}