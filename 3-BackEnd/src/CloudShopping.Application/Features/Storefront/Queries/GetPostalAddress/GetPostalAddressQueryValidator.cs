using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress;
public sealed class GetPostalAddressQueryValidator : AbstractValidator<GetPostalAddressQuery>
{
    public GetPostalAddressQueryValidator() => RuleFor(x => x.ZipCode).NotEmpty().Matches("^[0-9]{8}$");
}
