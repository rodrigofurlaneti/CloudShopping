using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Store;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Store.Commands.CreateStoreBanner
{
    public sealed class CreateStoreBannerCommandHandler : IRequestHandler<CreateStoreBannerCommand, Result<int>>
    {
        private readonly IStoreBannerRepository _bannerRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateStoreBannerCommandHandler> _logger;

        public CreateStoreBannerCommandHandler(
            IStoreBannerRepository bannerRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<CreateStoreBannerCommandHandler> logger)
        {
            _bannerRepository = bannerRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateStoreBannerCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var banner = StoreBanner.Create(
                tenantId: tenantId,
                title: request.Title,
                subtitle: request.Subtitle,
                discountPercentage: request.DiscountPercentage,
                buttonText: request.ButtonText,
                buttonLink: request.ButtonLink,
                backgroundColor: request.BackgroundColor,
                displayOrder: request.DisplayOrder
            );

            await _bannerRepository.AddAsync(banner, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Banner criado com sucesso. ID: {BannerId}, Tenant: {TenantId}", banner.Id, tenantId);
            return Result.Success(banner.Id);
        }
    }
}
