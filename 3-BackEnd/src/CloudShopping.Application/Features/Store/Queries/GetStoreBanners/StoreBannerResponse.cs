namespace CloudShopping.Application.Features.Store.Queries.GetStoreBanners
{
    public sealed record StoreBannerResponse(
        int Id,
        string Title,
        string? Subtitle,
        string? DiscountPercentage,
        string ButtonText,
        string ButtonLink,
        string BackgroundColor,
        int DisplayOrder
    );
}