using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Products
{
    public sealed class Product : AggregateRoot<int>, IMultiTenant
    {
        public int TenantId { get; private set; }
        public int DepartmentId { get; private set; }
        public string Sku { get; private set; }
        public string Slug { get; private set; } = "produto-"+Guid.NewGuid().ToString("N");
        public string Description { get; private set; } = "";
        public string? Brand { get; private set; }
        public decimal WeightKg { get; private set; }
        public decimal WidthCm { get; private set; }
        public decimal HeightCm { get; private set; }
        public decimal LengthCm { get; private set; }
        public string? FamilyCode { get; private set; }
        public string? VariantLabel { get; private set; }
        public string AttributesJson { get; private set; } = "{}";
        public void ConfigureCatalog(string slug,string description,string? brand,decimal weight,decimal width,decimal height,decimal length,string? family,string? variant,string attributesJson)
        {
            if(!System.Text.RegularExpressions.Regex.IsMatch(slug??"","^[a-z0-9]+(?:-[a-z0-9]+)*$")||slug!.Length is <3 or >150||description==null||description.Length>10000||brand?.Length>100)
                throw new ArgumentException("Descrição, marca ou URL do produto inválida.");
            if(weight<0||weight>10000||width<0||width>10000||height<0||height>10000||length<0||length>10000||decimal.Round(weight,3)!=weight||decimal.Round(width,2)!=width||decimal.Round(height,2)!=height||decimal.Round(length,2)!=length)
                throw new ArgumentException("Peso ou dimensões inválidos.");
            if((family==null)!=(variant==null)||family!=null&&(!System.Text.RegularExpressions.Regex.IsMatch(family,"^[A-Za-z0-9_-]{1,50}$")||string.IsNullOrWhiteSpace(variant)||variant.Length>100))
                throw new ArgumentException("Informe código de família e identificação da variante juntos.");
            Slug=slug;Description=description;Brand=brand;WeightKg=weight;WidthCm=width;HeightCm=height;LengthCm=length;FamilyCode=family;VariantLabel=variant;AttributesJson=attributesJson;UpdateTimestamp();
        }
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public int PhysicalStock { get; private set; }
        public int ReservedStock { get; private set; }
        public int AvailableStock => PhysicalStock - ReservedStock;
        public StockLocation? Location { get; private set; }
        public int Version { get; private set; }

        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        private Product() { }

        public static Product Create(int tenantId, int departmentId, string sku, string name, decimal price, int initialStock = 0, StockLocation? location = null)
        {
            if (departmentId <= 0) throw new ArgumentException("O departamento é obrigatório.");
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU é obrigatório.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do produto é obrigatório.");
            if (price <= 0) throw new ArgumentException("O preço deve ser maior que zero.");
            if (initialStock < 0) throw new ArgumentException("O estoque inicial não pode ser negativo.");

            return new Product
            {
                TenantId = tenantId,
                DepartmentId = departmentId,
                Sku = sku,
                Name = name,
                Price = price,
                PhysicalStock = initialStock,
                ReservedStock = 0,
                Location = location,
                Version = 1
            };
        }

        public void UpdateDetails(string name, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do produto é obrigatório.");
            if (price <= 0) throw new ArgumentException("O preço deve ser maior que zero.");

            Name = name;
            Price = price;
            UpdateTimestamp();
        }

        public void ChangeDepartment(int newDepartmentId)
        {
            if (newDepartmentId <= 0) throw new ArgumentException("O novo departamento é obrigatório.");

            DepartmentId = newDepartmentId;
            UpdateTimestamp();
        }

        public void UpdateLocation(StockLocation newLocation)
        {
            Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));
            UpdateTimestamp();
        }

        public void ClearLocation()
        {
            Location = null;
            UpdateTimestamp();
        }

        public void AddPhysicalStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("A quantidade de entrada deve ser maior que zero.");

            PhysicalStock += quantity;
            UpdateTimestamp();
        }

        public void ReserveStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantidade inválida.");
            if (AvailableStock < quantity) throw new InvalidOperationException("Estoque indisponível.");

            ReservedStock += quantity;
            UpdateTimestamp();
        }

        public void CommitReservedStock(int quantity)
        {
            if (ReservedStock < quantity) throw new InvalidOperationException("Quantidade reservada inconsistente.");

            PhysicalStock -= quantity;
            ReservedStock -= quantity;
            UpdateTimestamp();
        }

        public void ReleaseReservedStock(int quantity)
        {
            if (ReservedStock < quantity) throw new InvalidOperationException("Quantidade reservada inconsistente.");

            ReservedStock -= quantity;
            UpdateTimestamp();
        }

        public void AdjustInventory(int actualPhysicalQuantity)
        {
            if (actualPhysicalQuantity < 0)
                throw new ArgumentException("O estoque físico não pode ser negativo.");

            if (actualPhysicalQuantity < ReservedStock)
                throw new InvalidOperationException($"Não é possível ajustar o estoque para {actualPhysicalQuantity} pois já existem {ReservedStock} unidades reservadas para clientes.");

            PhysicalStock = actualPhysicalQuantity;
            UpdateTimestamp();
        }

        public ProductImage AddImage(string fileName, string filePath, bool isPrimary, int displayOrder)
        {
            if (isPrimary)
            {
                foreach (var img in _images) img.SetAsPrimary(false);
            }

            var image = ProductImage.Create(Id, fileName, filePath, isPrimary || !_images.Any(), displayOrder);
            _images.Add(image);
            UpdateTimestamp();

            return image;
        }

        public void RemoveImage(int productImageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == productImageId);
            if (image is null) throw new InvalidOperationException("Imagem não encontrada.");

            _images.Remove(image);
            UpdateTimestamp();
        }
    }
}
