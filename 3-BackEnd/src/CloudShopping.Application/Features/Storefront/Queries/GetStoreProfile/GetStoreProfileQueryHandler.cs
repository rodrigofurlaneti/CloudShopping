using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProfile;
public sealed class GetStoreProfileQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreProfileQuery, Result<StoreProfileView>>
{
    public Task<Result<StoreProfileView>> Handle(GetStoreProfileQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<StoreProfileView> ExecuteAsync(GetStoreProfileQuery request, CancellationToken ct)
    {
        var c = await repository.Customer(request.CustomerId, ct) ?? throw new KeyNotFoundException();
        return new(c.Email, c.Company == null ? "B2C" : "B2B", c.Individual?.FullName ?? c.Company?.CompanyName ?? "", c.Individual?.TaxId ?? c.Company?.BusinessTaxId ?? "");
    }
}
