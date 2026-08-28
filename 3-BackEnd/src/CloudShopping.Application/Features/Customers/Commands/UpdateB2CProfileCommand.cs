using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record UpdateB2CProfileCommand(
        int CustomerId,
        string FullName,
        DateTime? BirthDate) : IRequest<Result>;
}
