using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using CloudShopping.Application.Abstractions.Services;

namespace CloudShopping.Infrastructure.Services
{
    public sealed class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> SaveProductImageAsync(int tenantId, int productId, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("O arquivo de imagem é inválido.");
            var relativeFolder = Path.Combine("uploads", tenantId.ToString(), "products", productId.ToString());
            var absoluteFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativeFolder);
            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName).Replace(" ", "_")}.jpg";
            var absolutePath = Path.Combine(absoluteFolder, uniqueFileName);
            using (var inputStream = file.OpenReadStream())
            {
                using (var image = await Image.LoadAsync(inputStream, cancellationToken))
                {
                    int maxWidth = 1200;
                    if (image.Width > maxWidth)
                    {
                        var multiplier = (double)maxWidth / image.Width;
                        int newHeight = (int)(image.Height * multiplier);
                        image.Mutate(x => x.Resize(maxWidth, newHeight));
                    }
                    var encoder = new JpegEncoder
                    {
                        Quality = 78
                    };
                    await image.SaveAsync(absolutePath, encoder, cancellationToken);
                }
            }
            return Path.Combine(relativeFolder, uniqueFileName).Replace("\\", "/");
        }

        public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return Task.CompletedTask;
            var absolutePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), filePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
            return Task.CompletedTask;
        }
    }
}