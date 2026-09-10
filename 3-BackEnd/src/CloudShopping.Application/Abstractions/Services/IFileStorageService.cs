using CloudShopping.Application.Abstractions.Files;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveProductImageAsync(int tenantId, int productId, UploadFile file, CancellationToken cancellationToken);
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken);
    }
}
