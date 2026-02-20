using Dica33.TestesSnapshotComVerify;
using Newtonsoft.Json;
using Xunit;

namespace Dica33.TestesSnapshotComVerify.Tests;

public class JsonSnapshotTests
{
    [Fact]
    public async Task ShouldSnapshotApiResponse()
    {
        // Arrange
        var generator = new ApiResponseGenerator();

        // Act
        var response = await generator.GenerateUserApiResponseAsync(1);

        // Assert
        await Verify(response)
            .ScrubMembers("RequestId", "Timestamp"); // Remove dados dinâmicos
    }

    [Fact]
    public Task ShouldSnapshotJsonConfiguration()
    {
        // Arrange
        var settings = new
        {
            Application = new
            {
                Name = "E-commerce API",
                Version = "2.1.0",
                Environment = "Production"
            },
            Database = new
            {
                Provider = "PostgreSQL",
                ConnectionString = "Host=prod-db;Database=ecommerce",
                PoolSize = 100,
                CommandTimeout = 30
            },
            Cache = new
            {
                Provider = "Redis",
                Host = "redis-cluster.internal",
                DefaultTtl = TimeSpan.FromMinutes(30),
                KeyPrefix = "ecomm:"
            },
            Security = new
            {
                JwtSecret = "super-secret-key-for-production",
                TokenExpiry = TimeSpan.FromHours(24),
                AllowedOrigins = new[] { "https://app.ecommerce.com", "https://admin.ecommerce.com" },
                RequireHttps = true
            },
            Features = new Dictionary<string, object>
            {
                ["PaymentGateway"] = new { Provider = "Stripe", PublicKey = "pk_live_xyz" },
                ["EmailService"] = new { Provider = "SendGrid", ApiKey = "SG.xyz" },
                ["Analytics"] = new { Provider = "Google Analytics", TrackingId = "GA-123456" },
                ["Monitoring"] = new { Provider = "Application Insights", ConnectionString = "InstrumentationKey=abc" }
            }
        };

        // Act & Assert
        return Verify(settings)
            .ScrubMembers("JwtSecret", "ApiKey", "ConnectionString"); // Remove dados sensíveis
    }

    [Fact]
    public Task ShouldSnapshotComplexNestedJson()
    {
        // Arrange
        var complexData = new
        {
            Metadata = new
            {
                Version = "1.0",
                CreatedBy = "System",
                CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0),
                Tags = new[] { "production", "api", "v2" }
            },
            Data = new
            {
                Users = new[]
                {
                    new
                    {
                        Id = 1,
                        Profile = new
                        {
                            PersonalInfo = new { Name = "Ana Silva", Age = 28, Country = "Brasil" },
                            Preferences = new
                            {
                                Language = "pt-BR",
                                Currency = "BRL",
                                Notifications = new
                                {
                                    Email = true,
                                    Push = false,
                                    SMS = true
                                }
                            },
                            Activity = new
                            {
                                LastLogin = new DateTime(2024, 1, 20, 9, 15, 0),
                                TotalOrders = 5,
                                TotalSpent = 1250.99m,
                                FavoriteCategories = new[] { "Electronics", "Books", "Home" }
                            }
                        }
                    },
                    new
                    {
                        Id = 2,
                        Profile = new
                        {
                            PersonalInfo = new { Name = "Carlos Santos", Age = 35, Country = "Brasil" },
                            Preferences = new
                            {
                                Language = "pt-BR",
                                Currency = "BRL",
                                Notifications = new
                                {
                                    Email = false,
                                    Push = true,
                                    SMS = false
                                }
                            },
                            Activity = new
                            {
                                LastLogin = new DateTime(2024, 1, 19, 18, 30, 0),
                                TotalOrders = 12,
                                TotalSpent = 2890.45m,
                                FavoriteCategories = new[] { "Sports", "Electronics" }
                            }
                        }
                    }
                },
                Summary = new
                {
                    TotalUsers = 2,
                    ActiveUsers = 2,
                    TotalRevenue = 4141.44m,
                    AverageOrderValue = 243.61m,
                    TopCategories = new[]
                    {
                        new { Category = "Electronics", Orders = 8, Revenue = 2100.00m },
                        new { Category = "Books", Orders = 3, Revenue = 240.00m },
                        new { Category = "Sports", Orders = 6, Revenue = 1801.44m }
                    }
                }
            }
        };

        // Act & Assert
        return Verify(complexData);
    }

    [Fact]
    public Task ShouldSnapshotErrorResponse()
    {
        // Arrange
        var errorResponse = new
        {
            Success = false,
            Error = new
            {
                Code = "VALIDATION_ERROR",
                Message = "Os dados fornecidos são inválidos",
                Details = new[]
                {
                    new { Field = "Email", Message = "Email é obrigatório" },
                    new { Field = "Password", Message = "Senha deve ter pelo menos 8 caracteres" },
                    new { Field = "Age", Message = "Idade deve ser maior que 18" }
                },
                Timestamp = new DateTime(2024, 1, 15, 14, 30, 0),
                RequestId = "req_12345",
                TraceId = "trace_abcdef"
            },
            Meta = new
            {
                ApiVersion = "2.1",
                Documentation = "https://api.docs.com/errors",
                SupportContact = "support@company.com"
            }
        };

        // Act & Assert
        return Verify(errorResponse)
            .ScrubMembers("RequestId", "TraceId"); // Remove IDs únicos
    }

    [Fact]
    public Task ShouldSnapshotPaginatedResponse()
    {
        // Arrange
        var paginatedResponse = new
        {
            Data = new[]
            {
                new { Id = 1, Name = "Produto A", Price = 100.00m },
                new { Id = 2, Name = "Produto B", Price = 200.00m },
                new { Id = 3, Name = "Produto C", Price = 300.00m },
                new { Id = 4, Name = "Produto D", Price = 400.00m },
                new { Id = 5, Name = "Produto E", Price = 500.00m }
            },
            Pagination = new
            {
                CurrentPage = 1,
                PageSize = 5,
                TotalItems = 50,
                TotalPages = 10,
                HasPrevious = false,
                HasNext = true,
                Links = new
                {
                    First = "/api/products?page=1&size=5",
                    Previous = (string?)null,
                    Current = "/api/products?page=1&size=5",
                    Next = "/api/products?page=2&size=5",
                    Last = "/api/products?page=10&size=5"
                }
            },
            Meta = new
            {
                Query = new
                {
                    Filters = new { Category = "Electronics", MinPrice = 50.00m },
                    Sort = "price_asc",
                    Search = ""
                },
                ExecutionTime = TimeSpan.FromMilliseconds(45),
                CacheHit = false
            }
        };

        // Act & Assert
        return Verify(paginatedResponse)
            .ScrubMembers("ExecutionTime"); // Remove tempo de execução
    }

    [Fact]
    public Task ShouldSnapshotWebhookPayload()
    {
        // Arrange
        var webhookPayload = new
        {
            EventType = "order.completed",
            EventId = "evt_1234567890",
            Timestamp = new DateTime(2024, 1, 15, 10, 30, 0),
            Data = new
            {
                Order = new
                {
                    Id = 1001,
                    Status = "completed",
                    Customer = new
                    {
                        Id = 123,
                        Email = "customer@email.com",
                        Name = "João Silva"
                    },
                    Items = new[]
                    {
                        new { ProductId = 1, Quantity = 2, Price = 100.00m },
                        new { ProductId = 2, Quantity = 1, Price = 50.00m }
                    },
                    Payment = new
                    {
                        Method = "credit_card",
                        Amount = 250.00m,
                        Currency = "BRL",
                        Status = "paid",
                        TransactionId = "txn_abcdef123456"
                    },
                    Shipping = new
                    {
                        Method = "express",
                        Cost = 20.00m,
                        EstimatedDelivery = new DateTime(2024, 1, 18),
                        TrackingCode = "BR123456789"
                    }
                }
            },
            Signature = "sha256=abc123def456",
            Delivery = new
            {
                AttemptNumber = 1,
                MaxAttempts = 3,
                NextRetry = (DateTime?)null
            }
        };

        // Act & Assert
        return Verify(webhookPayload)
            .ScrubMembers("EventId", "TransactionId", "Signature", "TrackingCode"); // Remove dados únicos
    }
}
