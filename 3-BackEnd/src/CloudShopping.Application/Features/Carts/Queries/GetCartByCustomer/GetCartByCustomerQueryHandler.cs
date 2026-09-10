using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Carts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Carts.Queries.GetCartByCustomer
{
    public sealed class GetCartByCustomerQueryHandler : IRequestHandler<GetCartByCustomerQuery, Result<Cart?>>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartByCustomerQueryHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public Task<Result<Cart?>> Handle(GetCartByCustomerQuery request, CancellationToken cancellationToken) => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);
    private async Task<Cart?> ExecuteAsync(GetCartByCustomerQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
            return cart;
        }
    }
}
