using CloudShopping.Domain.Primitives;
using System;

namespace CloudShopping.Domain.Entities.Products
{
    public sealed class ProductImage : AuditableEntity<int>
    {
        public int ProductId { get; private set; }
        public string FileName { get; private set; } // Nome físico do arquivo salvo
        public string FilePath { get; private set; } // Caminho relativo (ex: uploads/1/products/45/foto.jpg)
        public bool IsPrimary { get; private set; }   // Indica se é a foto principal/capa
        public int DisplayOrder { get; private set; }  // Ordem de exibição na galeria

        private ProductImage() { }

        public static ProductImage Create(int productId, string fileName, string filePath, bool isPrimary, int displayOrder)
        {
            if (productId <= 0) throw new ArgumentException("ID do produto inválido.");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Nome do arquivo obrigatório.");
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho do arquivo obrigatório.");
            return new ProductImage
            {
                ProductId = productId,
                FileName = fileName.Trim(),
                FilePath = filePath.Trim().Replace("\\", "/"), // Padroniza barras para web
                IsPrimary = isPrimary,
                DisplayOrder = displayOrder
            };
        }
        public void SetAsPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
            UpdateTimestamp();
        }
        public void UpdateOrder(int order)
        {
            DisplayOrder = order;
            UpdateTimestamp();
        }
    }
}
