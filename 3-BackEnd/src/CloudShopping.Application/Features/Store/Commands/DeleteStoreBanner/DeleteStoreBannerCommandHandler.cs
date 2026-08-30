using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Store.Commands.DeleteStoreBanner
{
    public sealed class DeleteStoreBannerCommandHandler : IRequestHandler<DeleteStoreBannerCommand, Result>
    {
        private readonly IStoreBannerRepository _bannerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteStoreBannerCommandHandler> _logger;

        public DeleteStoreBannerCommandHandler(
            IStoreBannerRepository bannerRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteStoreBannerCommandHandler> logger)
        {
            _bannerRepository = bannerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(DeleteStoreBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner is null)
                return Result.Failure(new Error("StoreBanner.NotFound", "Banner não encontrado."));
            _bannerRepository.Remove(banner);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Banner removido com sucesso. ID: {BannerId}", banner.Id);
            return Result.Success();
        }
    }
}