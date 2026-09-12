using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Store.Commands.UpdateStoreBanner
{
    public sealed class UpdateStoreBannerCommandHandler : IRequestHandler<UpdateStoreBannerCommand, Result>
    {
        private readonly IStoreBannerRepository _bannerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateStoreBannerCommandHandler> _logger;

        public UpdateStoreBannerCommandHandler(
            IStoreBannerRepository bannerRepository,
            IUnitOfWork unitOfWork,
            ICacheService cache,
            ILogger<UpdateStoreBannerCommandHandler> logger)
        {
            _bannerRepository = bannerRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateStoreBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner is null)
                return Result.Failure(new Error("StoreBanner.NotFound", "Banner não encontrado."));

            banner.Update(
                tenantId: banner.TenantId,
                title: request.Title,
                subtitle: request.Subtitle,
                discountPercentage: request.DiscountPercentage,
                buttonText: request.ButtonText,
                buttonLink: request.ButtonLink,
                backgroundColor: request.BackgroundColor,
                displayOrder: request.DisplayOrder,
                isActive: request.IsActive
            );

            _bannerRepository.Update(banner);
            await _unitOfWork.CommitAsync(cancellationToken);

            // banner.TenantId é nulo apenas para os banners padrão globais do sistema;
            // ver nota em GetStoreBannersQueryHandler sobre a janela de defasagem desse
            // caso (mitigada com TTL Médio em vez de Longo).
            if (banner.TenantId is { } tenantId)
                await _cache.RemoveAsync(CacheKeys.TenantList(tenantId, CacheKeys.StoreBanners), cancellationToken);

            _logger.LogInformation("Banner atualizado com sucesso. ID: {BannerId}", banner.Id);
            return Result.Success();
        }
    }
}
