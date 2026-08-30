using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders
{
    public sealed class GetPaginatedTenantOrdersQueryValidator : AbstractValidator<GetPaginatedTenantOrdersQuery>
    {
        public GetPaginatedTenantOrdersQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("A página deve ser maior que zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("O tamanho da página deve ser maior que zero.")
                .LessThanOrEqualTo(100).WithMessage("O tamanho máximo por página é 100 pedidos.");
        }
    }
}
