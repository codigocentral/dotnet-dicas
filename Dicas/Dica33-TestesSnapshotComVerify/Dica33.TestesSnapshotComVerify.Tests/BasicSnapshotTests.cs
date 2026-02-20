using Dica33.TestesSnapshotComVerify;
using Newtonsoft.Json;
using Xunit;

namespace Dica33.TestesSnapshotComVerify.Tests;

public class BasicSnapshotTests
{
    [Fact]
    public Task ShouldSnapshotSimpleUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "João Silva",
            Email = "joao@email.com",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        // Act & Assert
        return Verify(user);
    }

    [Fact]
    public Task ShouldSnapshotUserList()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "Ana Costa", Email = "ana@email.com", IsActive = true, CreatedAt = new DateTime(2024, 1, 10) },
            new() { Id = 2, Name = "Carlos Santos", Email = "carlos@email.com", IsActive = false, CreatedAt = new DateTime(2024, 1, 11) },
            new() { Id = 3, Name = "Maria Silva", Email = "maria@email.com", IsActive = true, CreatedAt = new DateTime(2024, 1, 12) }
        };

        // Act & Assert
        return Verify(users);
    }

    [Fact]
    public Task ShouldSnapshotProductCatalog()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop Dell", Price = 2500.00m, Category = "Electronics" },
            new() { Id = 2, Name = "Mouse Logitech", Price = 50.00m, Category = "Electronics" },
            new() { Id = 3, Name = "Livro Clean Code", Price = 80.00m, Category = "Books" },
            new() { Id = 4, Name = "Cadeira Ergonômica", Price = 800.00m, Category = "Furniture" }
        };

        // Act & Assert
        return Verify(products);
    }

    [Fact]
    public Task ShouldSnapshotComplexOrder()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "João Silva",
            Email = "joao@email.com",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 2500.00m, Category = "Electronics" },
            new() { Id = 2, Name = "Mouse", Price = 50.00m, Category = "Electronics" }
        };

        var order = new Order
        {
            Id = 1001,
            UserId = 1,
            User = user,
            Items = new List<OrderItem>
            {
                new() { Id = 1, ProductId = 1, Product = products[0], Quantity = 1, UnitPrice = products[0].Price },
                new() { Id = 2, ProductId = 2, Product = products[1], Quantity = 2, UnitPrice = products[1].Price }
            },
            CreatedAt = new DateTime(2024, 1, 20, 14, 45, 0),
            Status = "Confirmed"
        };

        // Act & Assert
        return Verify(order)
            .IgnoreMembers("Orders", "OrderItems"); // Evitar referências circulares
    }

    [Fact]
    public Task ShouldSnapshotApplicationConfiguration()
    {
        // Arrange
        var config = new ApplicationConfig
        {
            Database = new DatabaseConfig
            {
                ConnectionString = "Server=localhost;Database=ProductionDB",
                CommandTimeout = 30,
                EnableRetry = true
            },
            Features = new Dictionary<string, bool>
            {
                ["EnableLogging"] = true,
                ["EnableCaching"] = true,
                ["EnableMetrics"] = true,
                ["EnableDebugMode"] = false
            },
            ApiSettings = new ApiSettings
            {
                BaseUrl = "https://api.production.com",
                Timeout = TimeSpan.FromSeconds(30),
                MaxRetries = 3
            }
        };

        // Act & Assert
        return Verify(config);
    }

    [Fact]
    public Task ShouldSnapshotOrderSummary()
    {
        // Arrange
        var orderSummary = new
        {
            OrderId = 1001,
            CustomerName = "João Silva",
            TotalItems = 3,
            TotalAmount = 2600.00m,
            Discount = 100.00m,
            FinalAmount = 2500.00m,
            PaymentMethod = "Credit Card",
            ShippingAddress = new
            {
                Street = "Rua das Flores, 123",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01234-567",
                Country = "Brasil"
            },
            Items = new[]
            {
                new { ProductName = "Laptop", Quantity = 1, UnitPrice = 2500.00m, Subtotal = 2500.00m },
                new { ProductName = "Mouse", Quantity = 2, UnitPrice = 50.00m, Subtotal = 100.00m }
            }
        };

        // Act & Assert
        return Verify(orderSummary);
    }
}
