using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Carts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Carts.Queries.GetCartByCustomer
{
    public sealed class GetCartByCustomerQueryHandler : IRequestHandler<GetCartByCustomerQuery, Cart?>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartByCustomerQueryHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Cart?> Handle(GetCartByCustomerQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
            return cart;
        }
    }
}
