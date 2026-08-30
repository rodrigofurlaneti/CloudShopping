using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Products.Commands.UploadProductImage
{
    public sealed record UploadProductImageCommand(
            int ProductId,
            IFormFile File,
            bool IsPrimary,
            int DisplayOrder
        ) : IRequest<Result<string>>; // Retorna o caminho relativo da imagem salva
}
