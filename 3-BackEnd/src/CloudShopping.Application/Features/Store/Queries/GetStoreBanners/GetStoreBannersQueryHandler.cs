using CloudShopping.Application.Abstractions.Caching;
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
        private readonly ICacheService _cache;

        public GetStoreBannersQueryHandler(
            IStoreBannerRepository bannerRepository,
            ITenantProvider tenantProvider,
            ICacheService cache)
        {
            _bannerRepository = bannerRepository;
            _tenantProvider = tenantProvider;
            _cache = cache;
        }

        public async Task<Result<IEnumerable<StoreBannerResponse>>> Handle(GetStoreBannersQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Cache Longo (storebanners, tarefa de cache Redis). TTL Médio (não Longo):
            // quando o tenant não tem banners próprios, o repositório cai para os
            // banners globais (TenantId == null) — uma mudança nesses banners globais
            // não é invalidada nas listas já cacheadas por tenant (ver comentário nos
            // Command Handlers), então um TTL mais curto limita a janela de defasagem
            // nesse caso específico.
            var cacheKey = CacheKeys.TenantList(tenantId, CacheKeys.StoreBanners);
            var response = await _cache.GetOrCreateAsync(cacheKey, CacheTtl.Medium, async ct =>
            {
                var banners = await _bannerRepository.GetAllByTenantAsync(tenantId, ct);
                return banners.Select(b => new StoreBannerResponse(
                    Id: b.Id,
                    Title: b.Title,
                    Subtitle: b.Subtitle,
                    DiscountPercentage: b.DiscountPercentage,
                    ButtonText: b.ButtonText,
                    ButtonLink: b.ButtonLink,
                    BackgroundColor: b.BackgroundColor,
                    DisplayOrder: b.DisplayOrder
                )).ToList();
            }, cancellationToken);

            return Result.Success<IEnumerable<StoreBannerResponse>>(response ?? new List<StoreBannerResponse>());
        }
    }
}
