using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Store
{
    public sealed class StoreBanner : Entity<int>
    {
        public int? TenantId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Subtitle { get; private set; }
        public string? DiscountPercentage { get; private set; }
        public string ButtonText { get; private set; } = "Ver ofertas";
        public string ButtonLink { get; private set; } = "#";
        public string BackgroundColor { get; private set; } = "#f95d00";
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        private StoreBanner() { }

        public static StoreBanner Create(
            int? tenantId,
            string title,
            string? subtitle,
            string? discountPercentage,
            string buttonText,
            string buttonLink,
            string backgroundColor,
            int displayOrder)
        {
            return new StoreBanner
            {
                TenantId = tenantId,
                Title = title,
                Subtitle = subtitle,
                DiscountPercentage = discountPercentage,
                ButtonText = buttonText,
                ButtonLink = buttonLink,
                BackgroundColor = backgroundColor,
                DisplayOrder = displayOrder,
                IsActive = true
            };
        }

        public void Update(
            int? tenantId,
            string title,
            string? subtitle,
            string? discountPercentage,
            string buttonText,
            string buttonLink,
            string backgroundColor,
            int displayOrder,
            bool isActive)
        {
            TenantId = tenantId;
            Title = title;
            Subtitle = subtitle;
            DiscountPercentage = discountPercentage;
            ButtonText = buttonText;
            ButtonLink = buttonLink;
            BackgroundColor = backgroundColor;
            DisplayOrder = displayOrder;
            IsActive = isActive;
        }
    }
}