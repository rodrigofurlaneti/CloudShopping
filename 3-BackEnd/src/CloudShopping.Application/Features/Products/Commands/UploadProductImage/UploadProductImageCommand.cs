using CloudShopping.Domain.Primitives.Results;
using MediatR;
using CloudShopping.Application.Abstractions.Files;
namespace CloudShopping.Application.Features.Products.Commands.UploadProductImage
{
    public sealed record UploadProductImageCommand(
            int ProductId,
            UploadFile File,
            bool IsPrimary,
            int DisplayOrder
        ) : IRequest<Result<string>>; // Retorna o caminho relativo da imagem salva
}
