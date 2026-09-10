using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreContext;
public sealed class GetStoreContextQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreContextQuery, Result<StoreContextView>>
{
    public Task<Result<StoreContextView>> Handle(GetStoreContextQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<StoreContextView> ExecuteAsync(GetStoreContextQuery request, CancellationToken ct)
    {
        return await repository.Context(ct);
    }
}
