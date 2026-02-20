using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;

namespace Dica56_gRPC.Models;

/// <summary>
/// Interface para o repositório de produtos
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetAllAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<List<Product>> SearchAsync(string query, string category, double minPrice, double maxPrice, int page, int pageSize);
}

/// <summary>
/// Implementação em memória do repositório de produtos
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<int, Product> _products;
    private int _nextId = 1;

    public InMemoryProductRepository()
    {
        _products = new ConcurrentDictionary<int, Product>();
        SeedData();
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<List<Product>> GetAllAsync()
    {
        return Task.FromResult(_products.Values.Where(p => p.IsActive).ToList());
    }

    public Task<Product> CreateAsync(Product product)
    {
        product.Id = Interlocked.Increment(ref _nextId);
        product.CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
        product.UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
        
        _products.TryAdd(product.Id, product);
        return Task.FromResult(product);
    }

    public Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
        _products.AddOrUpdate(product.Id, product, (key, oldValue) => product);
        return Task.FromResult(product);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return Task.FromResult(_products.TryRemove(id, out _));
    }

    public Task<List<Product>> SearchAsync(string query, string category, double minPrice, double maxPrice, int page, int pageSize)
    {
        var products = _products.Values.Where(p => p.IsActive);

        // Filtrar por query (nome ou descrição)
        if (!string.IsNullOrWhiteSpace(query))
        {
            products = products.Where(p => 
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // Filtrar por categoria
        if (!string.IsNullOrWhiteSpace(category))
        {
            products = products.Where(p => 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        // Filtrar por preço
        if (minPrice > 0)
        {
            products = products.Where(p => p.Price >= minPrice);
        }

        if (maxPrice > 0)
        {
            products = products.Where(p => p.Price <= maxPrice);
        }

        // Paginação
        if (page > 0 && pageSize > 0)
        {
            products = products.Skip((page - 1) * pageSize).Take(pageSize);
        }

        return Task.FromResult(products.ToList());
    }

    /// <summary>
    /// Popula o repositório com dados de exemplo
    /// </summary>
    private void SeedData()
    {
        var sampleProducts = new[]
        {
            new Product
            {
                Id = _nextId++,
                Name = "Smartphone Samsung Galaxy S24",
                Description = "Smartphone Android com 256GB de armazenamento e câmera tripla",
                Price = 3499.99,
                Category = "Eletrônicos",
                StockQuantity = 50,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                IsActive = true,
                Tags = { "smartphone", "android", "samsung", "5g" }
            },
            new Product
            {
                Id = _nextId++,
                Name = "Notebook Dell XPS 13",
                Description = "Ultrabook com processador Intel i7, 16GB RAM e SSD 512GB",
                Price = 6999.99,
                Category = "Computadores",
                StockQuantity = 25,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-25)),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                IsActive = true,
                Tags = { "notebook", "ultrabook", "dell", "intel" }
            },
            new Product
            {
                Id = _nextId++,
                Name = "Fone de Ouvido Sony WH-1000XM5",
                Description = "Fone sem fio com cancelamento de ruído ativo",
                Price = 1299.99,
                Category = "Áudio",
                StockQuantity = 100,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                IsActive = true,
                Tags = { "fone", "bluetooth", "sony", "cancelamento-ruido" }
            },
            new Product
            {
                Id = _nextId++,
                Name = "Smart TV Samsung 55\" 4K",
                Description = "Smart TV QLED 4K com HDR e sistema Tizen",
                Price = 2899.99,
                Category = "Eletrônicos",
                StockQuantity = 30,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-4)),
                IsActive = true,
                Tags = { "tv", "smart-tv", "4k", "samsung", "qled" }
            },
            new Product
            {
                Id = _nextId++,
                Name = "Mouse Gamer Logitech G Pro X",
                Description = "Mouse gamer sem fio com sensor HERO 25K",
                Price = 599.99,
                Category = "Periféricos",
                StockQuantity = 75,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                IsActive = true,
                Tags = { "mouse", "gamer", "logitech", "sem-fio" }
            }
        };

        foreach (var product in sampleProducts)
        {
            _products.TryAdd(product.Id, product);
        }
    }
}
