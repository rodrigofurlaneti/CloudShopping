using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.UploadProductImage;
public sealed class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        RuleFor(x=>x.ProductId).GreaterThan(0);
        RuleFor(x=>x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x=>x.File).NotNull();
        When(x=>x.File!=null,()=>
        {
            RuleFor(x=>x.File.FileName).NotEmpty().MaximumLength(255);
            RuleFor(x=>x.File.Length).GreaterThan(0).LessThanOrEqualTo(10_000_000);
            RuleFor(x=>x.File.Content).Must(s=>s!=null&&s.CanRead).WithMessage("Arquivo deve estar aberto para leitura.");
        });
    }
}
