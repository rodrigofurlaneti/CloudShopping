using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Abstractions.Services;
namespace CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress;
public sealed class GetPostalAddressQueryHandler(IPostalCodeLookup lookup) : IRequestHandler<GetPostalAddressQuery, Result<PostalAddress?>>
{
    public Task<Result<PostalAddress?>> Handle(GetPostalAddressQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private Task<PostalAddress?> ExecuteAsync(GetPostalAddressQuery request, CancellationToken ct) => lookup.FindAsync(request.ZipCode, ct);
}
