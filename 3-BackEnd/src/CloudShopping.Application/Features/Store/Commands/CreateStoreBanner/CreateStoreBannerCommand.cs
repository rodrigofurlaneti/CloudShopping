using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Store.Commands.CreateStoreBanner
{
    public sealed record CreateStoreBannerCommand(
        string Title,
        string? Subtitle,
        string? DiscountPercentage,
        string ButtonText,
        string ButtonLink,
        string BackgroundColor,
        int DisplayOrder
    ) : IRequest<Result<int>>;
}