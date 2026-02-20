using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Dica33.TestesSnapshotComVerify;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("📸 Dica 33: Testes de Snapshot com Verify");
        Console.WriteLine("=========================================\n");

        Console.WriteLine("Este projeto demonstra como usar a biblioteca Verify para");
        Console.WriteLine("implementar testes de snapshot robustos.\n");

        // Demonstrar diferentes tipos de dados para snapshot
        DemonstrarDadosParaSnapshot();
        
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        
        // Demonstrar geração de conteúdo complexo
        DemonstrarConteudoComplexo();
        
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        
        // Demonstrar Entity Framework
        await DemonstrarEntityFramework();
        
        Console.WriteLine("\n✅ Demonstração concluída!");
        Console.WriteLine("\n💡 Para ver os testes de snapshot em ação:");
        Console.WriteLine("   cd ../Dica33.TestesSnapshotComVerify.Tests");
        Console.WriteLine("   dotnet test");
        Console.WriteLine("\n📋 Os testes irão gerar arquivos .received.txt na primeira execução");
        Console.WriteLine("   que devem ser revisados e aprovados como .verified.txt");
    }

    static void DemonstrarDadosParaSnapshot()
    {
        Console.WriteLine("🔹 DADOS IDEAIS PARA SNAPSHOT TESTING");
        Console.WriteLine("=====================================\n");

        // Objeto simples
        var user = new User
        {
            Id = 1,
            Name = "João Silva",
            Email = "joao@email.com",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        Console.WriteLine("📊 Objeto User:");
        Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));
        Console.WriteLine();

        // Lista de produtos
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 2500.00m, Category = "Electronics" },
            new() { Id = 2, Name = "Mouse", Price = 50.00m, Category = "Electronics" },
            new() { Id = 3, Name = "Livro C#", Price = 80.00m, Category = "Books" }
        };

        Console.WriteLine("📦 Lista de Produtos:");
        Console.WriteLine(JsonConvert.SerializeObject(products, Formatting.Indented));
        Console.WriteLine();

        // Objeto aninhado complexo
        var order = new Order
        {
            Id = 1001,
            UserId = 1,
            User = user,
            Items = products.Select(p => new OrderItem
            {
                ProductId = p.Id,
                Product = p,
                Quantity = 2,
                UnitPrice = p.Price
            }).ToList(),
            CreatedAt = new DateTime(2024, 1, 20, 14, 45, 0),
            Status = "Confirmed"
        };

        Console.WriteLine("🛒 Pedido Complexo:");
        Console.WriteLine(JsonConvert.SerializeObject(order, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
    }

    static void DemonstrarConteudoComplexo()
    {
        Console.WriteLine("🔹 CONTEÚDO COMPLEXO PARA SNAPSHOT");
        Console.WriteLine("==================================\n");

        var reportGenerator = new ReportGenerator();
        var apiResponseGenerator = new ApiResponseGenerator();

        // Gerar relatório HTML
        var htmlReport = reportGenerator.GenerateUserReportAsync(1).Result;
        Console.WriteLine("📄 Relatório HTML gerado:");
        Console.WriteLine(htmlReport.Length > 200 ? htmlReport.Substring(0, 200) + "..." : htmlReport);
        Console.WriteLine();

        // Gerar resposta de API
        var apiResponse = apiResponseGenerator.GenerateUserApiResponseAsync(1).Result;
        Console.WriteLine("🌐 Resposta de API:");
        Console.WriteLine(JsonConvert.SerializeObject(apiResponse, Formatting.Indented));
        Console.WriteLine();

        // Configuração complexa
        var config = new ApplicationConfig
        {
            Database = new DatabaseConfig
            {
                ConnectionString = "Server=localhost;Database=TestDB",
                CommandTimeout = 30,
                EnableRetry = true
            },
            Features = new Dictionary<string, bool>
            {
                ["EnableLogging"] = true,
                ["EnableCaching"] = false,
                ["EnableMetrics"] = true
            },
            ApiSettings = new ApiSettings
            {
                BaseUrl = "https://api.example.com",
                Timeout = TimeSpan.FromSeconds(30),
                MaxRetries = 3
            }
        };

        Console.WriteLine("⚙️ Configuração da Aplicação:");
        Console.WriteLine(JsonConvert.SerializeObject(config, Formatting.Indented));
    }

    static async Task DemonstrarEntityFramework()
    {
        Console.WriteLine("🔹 ENTITY FRAMEWORK E QUERIES SQL");
        Console.WriteLine("==================================\n");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "SnapshotTestDb")
            .Options;

        using var context = new AppDbContext(options);

        // Seed data
        var users = new List<User>
        {
            new() { Id = 1, Name = "Ana Costa", Email = "ana@email.com", IsActive = true },
            new() { Id = 2, Name = "Carlos Santos", Email = "carlos@email.com", IsActive = false },
            new() { Id = 3, Name = "Maria Silva", Email = "maria@email.com", IsActive = true }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        Console.WriteLine("💾 Dados inseridos no Entity Framework");
        Console.WriteLine("📊 Usuários ativos encontrados:");

        var activeUsers = await context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();

        foreach (var user in activeUsers)
        {
            Console.WriteLine($"   • {user.Name} ({user.Email})");
        }

        Console.WriteLine("\n💡 As queries SQL geradas pelo EF podem ser capturadas em snapshots!");
    }
}

// Models
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<Order> Orders { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<OrderItem> OrderItems { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public decimal Subtotal => Quantity * UnitPrice;
}

// Services
public class ReportGenerator
{
    public async Task<string> GenerateUserReportAsync(int userId)
    {
        await Task.Delay(10); // Simular operação assíncrona

        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Relatório do Usuário #{userId}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 10px; }}
        .content {{ margin: 20px 0; }}
        .footer {{ font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Relatório do Usuário</h1>
        <p>ID: {userId}</p>
    </div>
    <div class=""content"">
        <h2>Informações Gerais</h2>
        <p>Este é um relatório gerado automaticamente.</p>
        <p>Data de geração: {DateTime.Now:yyyy-MM-dd}</p>
        
        <h2>Estatísticas</h2>
        <ul>
            <li>Total de pedidos: 5</li>
            <li>Valor total gasto: R$ 1.250,00</li>
            <li>Último acesso: 2024-01-20</li>
        </ul>
    </div>
    <div class=""footer"">
        <p>Relatório gerado pelo sistema em {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>
</body>
</html>";
    }
}

public class ApiResponseGenerator
{
    public async Task<object> GenerateUserApiResponseAsync(int userId)
    {
        await Task.Delay(10); // Simular operação assíncrona

        return new
        {
            Success = true,
            Data = new
            {
                User = new
                {
                    Id = userId,
                    Name = "João Silva",
                    Email = "joao@email.com",
                    Profile = new
                    {
                        IsActive = true,
                        LastLogin = "2024-01-20T10:30:00Z",
                        Preferences = new
                        {
                            Language = "pt-BR",
                            Theme = "dark",
                            Notifications = true
                        }
                    }
                },
                Statistics = new
                {
                    TotalOrders = 5,
                    TotalSpent = 1250.00m,
                    AverageOrderValue = 250.00m,
                    FavoriteCategory = "Electronics"
                }
            },
            Meta = new
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Version = "1.0",
                RequestId = Guid.NewGuid().ToString()
            }
        };
    }
}

// Configuration classes
public class ApplicationConfig
{
    public DatabaseConfig Database { get; set; } = new();
    public Dictionary<string, bool> Features { get; set; } = new();
    public ApiSettings ApiSettings { get; set; } = new();
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; }
    public bool EnableRetry { get; set; }
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; }
    public int MaxRetries { get; set; }
}

// Entity Framework Context
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(e => e.UserId);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.Items)
                  .HasForeignKey(e => e.OrderId);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.ProductId);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}

public class ECommerceContext : DbContext
{
    public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Category).HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne<User>()
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Products)
                .WithMany()
                .UsingEntity("OrderProducts");
        });

        base.OnModelCreating(modelBuilder);
    }
}
