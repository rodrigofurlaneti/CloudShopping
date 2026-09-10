using MediatR;
using CloudShopping.Application.Abstractions.Services;
namespace CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress;
public sealed class GetPostalAddressQueryHandler(IPostalCodeLookup lookup) : IRequestHandler<GetPostalAddressQuery, PostalAddress?>
{
    public Task<PostalAddress?> Handle(GetPostalAddressQuery request, CancellationToken ct) => lookup.FindAsync(request.ZipCode, ct);
}
