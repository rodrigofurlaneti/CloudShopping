using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.CancelStoreOrder;
public sealed class CancelStoreOrderCommandHandler(ICustomerPaymentCancellation payments) : IRequestHandler<CancelStoreOrderCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CancelStoreOrderCommand request, CancellationToken ct)
        => CommandExecution.Run<Unit>(async () =>
    {
        await payments.Cancel(request.Id, request.CustomerId, ct); return Unit.Value;
    
    });
}
