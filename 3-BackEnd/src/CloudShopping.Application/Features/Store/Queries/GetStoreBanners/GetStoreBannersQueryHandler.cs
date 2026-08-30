using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Store.Queries.GetStoreBanners;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Store.Queries.GetStoreBanners
{
    public sealed class GetStoreBannersQueryHandler : IRequestHandler<GetStoreBannersQuery, Result<IEnumerable<StoreBannerResponse>>>
    {
        private readonly IStoreBannerRepository _bannerRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetStoreBannersQueryHandler(
            IStoreBannerRepository bannerRepository,
            ITenantProvider tenantProvider)
        {
            _bannerRepository = bannerRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<IEnumerable<StoreBannerResponse>>> Handle(GetStoreBannersQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var banners = await _bannerRepository.GetAllByTenantAsync(tenantId, cancellationToken);
            var response = banners.Select(b => new StoreBannerResponse(
                Id: b.Id,
                Title: b.Title,
                Subtitle: b.Subtitle,
                DiscountPercentage: b.DiscountPercentage,
                ButtonText: b.ButtonText,
                ButtonLink: b.ButtonLink,
                BackgroundColor: b.BackgroundColor,
                DisplayOrder: b.DisplayOrder
            ));
            return Result.Success(response);
        }
    }
}