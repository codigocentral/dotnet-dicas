using Dica67_APIVersioning.Models;

namespace Dica67_APIVersioning.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(CreateProductRequest request);
}

public interface IProductServiceV2
{
    Task<IEnumerable<ProductV2>> GetAllProductsAsync();
    Task<ProductV2?> GetProductByIdAsync(int id);
    Task<ProductV2> CreateProductAsync(CreateProductV2Request request);
    Task<IEnumerable<ProductV2>> GetProductsByCategoryAsync(string category);
}

public interface IProductServiceV3
{
    Task<IEnumerable<ProductV3>> GetAllProductsAsync();
    Task<ProductV3?> GetProductBySkuAsync(string sku);
    Task<ProductV3> CreateProductAsync(CreateProductV3Request request);
    Task<IEnumerable<ProductV3>> SearchProductsAsync(string query);
    Task<ProductV3> UpdateStockAsync(string sku, int quantity);
}

public class ProductService : IProductService, IProductServiceV2, IProductServiceV3
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 999.99m, Description = "High-performance laptop" },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m, Description = "Wireless mouse" },
        new Product { Id = 3, Name = "Keyboard", Price = 79.99m, Description = "Mechanical keyboard" }
    };

    private static readonly List<ProductV2> _productsV2 = new()
    {
        new ProductV2 
        { 
            Id = 1, 
            Name = "Laptop", 
            Price = 999.99m, 
            Description = "High-performance laptop",
            Category = "Electronics",
            Tags = new[] { "computer", "portable", "work" },
            Rating = new ProductRating { Average = 4.5, Count = 123 },
            Inventory = new ProductInventory { Stock = 50, Reserved = 5 }
        },
        new ProductV2 
        { 
            Id = 2, 
            Name = "Mouse", 
            Price = 29.99m, 
            Description = "Wireless mouse",
            Category = "Accessories",
            Tags = new[] { "wireless", "ergonomic" },
            Rating = new ProductRating { Average = 4.2, Count = 89 },
            Inventory = new ProductInventory { Stock = 200, Reserved = 10 }
        }
    };

    private static readonly List<ProductV3> _productsV3 = new()
    {
        new ProductV3
        {
            Sku = "LAPTOP-001",
            Title = "MacBook Pro 16\"",
            Price = new Money { Amount = 2499.99m, Currency = "USD" },
            Summary = "Professional laptop with M3 chip",
            Metadata = new ProductMetadata 
            { 
                Brand = "Apple", 
                Model = "MacBook Pro 16\" M3",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            Availability = new ProductAvailability 
            { 
                InStock = true, 
                Quantity = 25 
            }
        }
    };

    // V1 Implementation
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        await Task.Delay(10); // Simulate async work
        return _products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        await Task.Delay(10);
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        await Task.Delay(10);
        var product = new Product
        {
            Id = _products.Max(p => p.Id) + 1,
            Name = request.Name,
            Price = request.Price,
            Description = request.Description
        };
        _products.Add(product);
        return product;
    }

    // V2 Implementation
    Task<IEnumerable<ProductV2>> IProductServiceV2.GetAllProductsAsync()
    {
        return Task.FromResult<IEnumerable<ProductV2>>(_productsV2);
    }

    Task<ProductV2?> IProductServiceV2.GetProductByIdAsync(int id)
    {
        return Task.FromResult(_productsV2.FirstOrDefault(p => p.Id == id));
    }

    Task<ProductV2> IProductServiceV2.CreateProductAsync(CreateProductV2Request request)
    {
        var product = new ProductV2
        {
            Id = _productsV2.Max(p => p.Id) + 1,
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Category = request.Category,
            Tags = request.Tags,
            Rating = new ProductRating { Average = 0, Count = 0 },
            Inventory = new ProductInventory { Stock = 0, Reserved = 0 }
        };
        _productsV2.Add(product);
        return Task.FromResult(product);
    }

    public async Task<IEnumerable<ProductV2>> GetProductsByCategoryAsync(string category)
    {
        await Task.Delay(10);
        return _productsV2.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    // V3 Implementation
    Task<IEnumerable<ProductV3>> IProductServiceV3.GetAllProductsAsync()
    {
        return Task.FromResult<IEnumerable<ProductV3>>(_productsV3);
    }

    public async Task<ProductV3?> GetProductBySkuAsync(string sku)
    {
        await Task.Delay(10);
        return _productsV3.FirstOrDefault(p => p.Sku == sku);
    }

    Task<ProductV3> IProductServiceV3.CreateProductAsync(CreateProductV3Request request)
    {
        var product = new ProductV3
        {
            Sku = $"PROD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Title = request.Title,
            Price = request.Price,
            Summary = request.Summary,
            Metadata = new ProductMetadata
            {
                Brand = request.Brand,
                Model = request.Model,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Availability = new ProductAvailability
            {
                InStock = request.InitialStock > 0,
                Quantity = request.InitialStock
            }
        };
        _productsV3.Add(product);
        return Task.FromResult(product);
    }

    public async Task<IEnumerable<ProductV3>> SearchProductsAsync(string query)
    {
        await Task.Delay(10);
        return _productsV3.Where(p => 
            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Metadata.Brand.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ProductV3> UpdateStockAsync(string sku, int quantity)
    {
        await Task.Delay(10);
        var product = _productsV3.FirstOrDefault(p => p.Sku == sku);
        if (product == null)
            throw new ArgumentException($"Product with SKU {sku} not found");

        var updatedProduct = product with
        {
            Availability = product.Availability with
            {
                Quantity = quantity,
                InStock = quantity > 0
            },
            Metadata = product.Metadata with
            {
                UpdatedAt = DateTime.UtcNow
            }
        };

        var index = _productsV3.IndexOf(product);
        _productsV3[index] = updatedProduct;
        
        return updatedProduct;
    }
}
