using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands.ChangeCustomerEmail
{
    public sealed record ChangeCustomerEmailCommand(
        int CustomerId,
        string NewEmail) : IRequest<Result>;
}
