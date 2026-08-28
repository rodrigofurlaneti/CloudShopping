using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Carts.Commands
{
    public sealed class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AddCartItemCommandHandler(ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken);
            if (cart is null) return Result.Failure(new Error("Cart.NotFound", "Carrinho não encontrado."));
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null) return Result.Failure(new Error("Product.NotFound", "Produto não encontrado."));
            cart.AddOrUpdateItem(product.Id, request.Quantity, product.Price);
            _cartRepository.Update(cart);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}