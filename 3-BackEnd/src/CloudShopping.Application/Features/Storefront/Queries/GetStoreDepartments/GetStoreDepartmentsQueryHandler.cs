using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreDepartments;
public sealed class GetStoreDepartmentsQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreDepartmentsQuery, Result<IReadOnlyList<StoreDepartmentView>>>
{
    public Task<Result<IReadOnlyList<StoreDepartmentView>>> Handle(GetStoreDepartmentsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<StoreDepartmentView>> ExecuteAsync(GetStoreDepartmentsQuery request, CancellationToken ct)
    {
        return await repository.Departments(ct);
    }
}
