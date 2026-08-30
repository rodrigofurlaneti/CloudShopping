using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Store.Commands.DeleteStoreBanner
{
    public sealed record DeleteStoreBannerCommand(int Id) : IRequest<Result>;
}