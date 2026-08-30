using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders
{
    public sealed class GetTenantOrdersQueryValidator : AbstractValidator<GetTenantOrdersQuery>
    {
        public GetTenantOrdersQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("O ID do lojista (Tenant) é inválido.");
            RuleFor(x => x.OrderStatusId)
                .GreaterThan(0).When(x => x.OrderStatusId.HasValue)
                .WithMessage("O ID do status é inválido.");
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("A página deve ser maior que zero.");
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("O tamanho da página deve ser maior que zero.")
                .LessThanOrEqualTo(100).WithMessage("O tamanho máximo da página é 100.");
        }
    }
}
