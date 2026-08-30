using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Store.Commands.UpdateStoreBanner
{
    public sealed record UpdateStoreBannerCommand(
        int Id,
        int? TenantId,
        string Title,
        string? Subtitle,
        string? DiscountPercentage,
        string ButtonText,
        string ButtonLink,
        string BackgroundColor,
        int DisplayOrder,
        bool IsActive
    ) : IRequest<Result>;
}