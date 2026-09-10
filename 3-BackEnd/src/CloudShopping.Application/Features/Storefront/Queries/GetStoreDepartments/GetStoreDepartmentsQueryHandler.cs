using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreDepartments;
public sealed class GetStoreDepartmentsQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreDepartmentsQuery, IReadOnlyList<StoreDepartmentView>>
{
    public async Task<IReadOnlyList<StoreDepartmentView>> Handle(GetStoreDepartmentsQuery request, CancellationToken ct)
    {
        return await repository.Departments(ct);
    }
}
