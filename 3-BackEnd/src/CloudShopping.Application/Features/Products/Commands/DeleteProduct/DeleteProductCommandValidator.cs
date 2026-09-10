using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.DeleteProduct;
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
    }
}
