using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders
{
    public sealed class GetPaginatedTenantOrdersQueryValidator : AbstractValidator<GetPaginatedTenantOrdersQuery>
    {
        public GetPaginatedTenantOrdersQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("A página deve ser maior que zero.");

            // Limite elevado para 500: este endpoint alimenta o quadro Kanban do backoffice,
            // que carrega o board inteiro em uma única chamada (pageSize padrão do
            // controller é 200) em vez de paginar página a página como uma listagem pública.
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("O tamanho da página deve ser maior que zero.")
                .LessThanOrEqualTo(500).WithMessage("O tamanho máximo por página é 500 pedidos.");
        }
    }
}
