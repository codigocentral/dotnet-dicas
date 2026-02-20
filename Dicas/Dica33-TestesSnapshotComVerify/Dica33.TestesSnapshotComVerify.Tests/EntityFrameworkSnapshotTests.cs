using Dica33.TestesSnapshotComVerify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dica33.TestesSnapshotComVerify.Tests;

public class EntityFrameworkSnapshotTests
{
    [Fact]
    public async Task ShouldSnapshotDatabaseQuery()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Act
        var users = await context.Users
            .Include(u => u.Orders)
            .ThenInclude(o => o.Products)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();

        // Assert
        await Verify(users)
            .IgnoreMembers("Id", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task ShouldSnapshotDatabaseSchema()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var model = context.Model;
        var entityTypes = model.GetEntityTypes()
            .Select(et => new
            {
                Name = et.ClrType.Name,
                TableName = et.GetTableName(),
                Properties = et.GetProperties()
                    .Select(p => new
                    {
                        Name = p.Name,
                        Type = p.ClrType.Name,
                        IsNullable = p.IsNullable,
                        MaxLength = p.GetMaxLength(),
                        IsKey = p.IsKey(),
                        IsForeignKey = p.IsForeignKey()
                    })
                    .OrderBy(p => p.Name)
                    .ToList(),
                Indexes = et.GetIndexes()
                    .Select(i => new
                    {
                        Properties = i.Properties.Select(p => p.Name).ToList(),
                        IsUnique = i.IsUnique
                    })
                    .ToList(),
                ForeignKeys = et.GetForeignKeys()
                    .Select(fk => new
                    {
                        Properties = fk.Properties.Select(p => p.Name).ToList(),
                        PrincipalEntityType = fk.PrincipalEntityType.ClrType.Name,
                        PrincipalProperties = fk.PrincipalKey.Properties.Select(p => p.Name).ToList(),
                        DeleteBehavior = fk.DeleteBehavior.ToString()
                    })
                    .ToList()
            })
            .OrderBy(et => et.Name)
            .ToList();

        // Assert
        await Verify(entityTypes);
    }

    [Fact]
    public async Task ShouldSnapshotQueryExecutionPlan()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Act
        var query = context.Users
            .Include(u => u.Orders)
            .ThenInclude(o => o.Products)
            .Where(u => u.Email.Contains("gmail"))
            .Where(u => u.Orders.Any(o => o.TotalAmount > 100))
            .OrderByDescending(u => u.Orders.Sum(o => o.TotalAmount));

        var queryString = query.ToQueryString();
        
        // Simulate execution plan (in real scenario, you'd get actual execution plan)
        var executionPlan = new
        {
            Query = queryString,
            EstimatedCost = 150.75,
            EstimatedRows = 25,
            Operations = new object[]
            {
                new { Type = "TableScan", Table = "Users", EstimatedCost = 45.2 },
                new { Type = "IndexSeek", Table = "Orders", Index = "IX_Orders_UserId", EstimatedCost = 32.1 },
                new { Type = "NestedLoop", EstimatedCost = 73.45 }
            },
            Warnings = new[]
            {
                "Missing index on Users.Email column may impact performance",
                "Complex aggregation in ORDER BY clause"
            }
        };

        // Assert
        await Verify(executionPlan);
    }

    [Fact]
    public async Task ShouldSnapshotChangeTracking()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Act
        var user = await context.Users.FirstAsync();
        user.Name = "Updated Name";
        user.Email = "updated@email.com";

        var newOrder = new Order
        {
            UserId = user.Id,
            TotalAmount = 299.99m,
            OrderDate = DateTime.UtcNow,
            Products = new List<Product>
            {
                new() { Name = "New Product", Price = 299.99m, Category = "Electronics" }
            }
        };
        context.Orders.Add(newOrder);

        var trackedEntities = context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .Select(e => new
            {
                EntityType = e.Entity.GetType().Name,
                State = e.State.ToString(),
                OriginalValues = e.State == EntityState.Modified 
                    ? e.OriginalValues.Properties
                        .Where(p => e.Property(p.Name).IsModified)
                        .ToDictionary(p => p.Name, p => e.OriginalValues[p])
                    : null,
                CurrentValues = e.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => e.CurrentValues[p]),
                ModifiedProperties = e.State == EntityState.Modified
                    ? e.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList()
                    : null
            })
            .ToList();

        // Assert
        await Verify(trackedEntities)
            .IgnoreMembers("Id", "CreatedAt", "UpdatedAt", "OrderDate");
    }

    [Fact]
    public async Task ShouldSnapshotMigrationScript()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act - Simulate migration script generation
        var migrationScript = new
        {
            Version = "20240115_AddUserPreferences",
            UpScript = """
                -- Add UserPreferences table
                CREATE TABLE UserPreferences (
                    Id int NOT NULL IDENTITY(1,1),
                    UserId int NOT NULL,
                    Theme nvarchar(50) NOT NULL DEFAULT 'light',
                    Language nvarchar(10) NOT NULL DEFAULT 'en-US',
                    Notifications bit NOT NULL DEFAULT 1,
                    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT PK_UserPreferences PRIMARY KEY (Id),
                    CONSTRAINT FK_UserPreferences_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
                );

                -- Add index for faster lookups
                CREATE UNIQUE INDEX IX_UserPreferences_UserId ON UserPreferences (UserId);

                -- Add default preferences for existing users
                INSERT INTO UserPreferences (UserId, Theme, Language, Notifications)
                SELECT Id, 'light', 'en-US', 1 FROM Users;
                """,
            DownScript = """
                -- Remove UserPreferences table
                DROP TABLE UserPreferences;
                """,
            AffectedTables = new[] { "UserPreferences", "Users" },
            EstimatedExecutionTime = "< 1 second",
            DataLossWarning = false,
            RequiresDowntime = false
        };

        // Assert
        await Verify(migrationScript);
    }

    [Fact]
    public async Task ShouldSnapshotDatabaseConstraints()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act - Simulate constraint validation
        var constraintViolations = new List<object>();

        try
        {
            // Test unique constraint violation
            var user1 = new User { Name = "Test User", Email = "test@email.com", IsActive = true };
            var user2 = new User { Name = "Another User", Email = "test@email.com", IsActive = true };
            
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            constraintViolations.Add(new
            {
                ConstraintType = "Unique",
                Table = "Users",
                Column = "Email",
                Value = "test@email.com",
                Error = ex.Message
            });
        }

        // Reset context
        context.ChangeTracker.Clear();

        try
        {
            // Test foreign key constraint violation
            var invalidOrder = new Order
            {
                UserId = 99999, // Non-existent user
                TotalAmount = 100.00m,
                OrderDate = DateTime.UtcNow
            };
            
            context.Orders.Add(invalidOrder);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            constraintViolations.Add(new
            {
                ConstraintType = "ForeignKey", 
                Table = "Orders",
                Column = "UserId",
                Value = 99999,
                ReferencedTable = "Users",
                Error = ex.Message
            });
        }

        var constraintReport = new
        {
            Timestamp = DateTime.UtcNow,
            DatabaseName = "TestDatabase",
            Violations = constraintViolations,
            Summary = new
            {
                TotalViolations = constraintViolations.Count,
                ViolationsByType = constraintViolations
                    .GroupBy(v => ((dynamic)v).ConstraintType)
                    .ToDictionary(g => g.Key, g => g.Count())
            }
        };

        // Assert
        await Verify(constraintReport)
            .IgnoreMembers("Timestamp");
    }

    private static ECommerceContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ECommerceContext(options);
    }

    private static async Task SeedTestData(ECommerceContext context)
    {
        var users = new[]
        {
            new User { Name = "João Silva", Email = "joao@gmail.com", IsActive = true },
            new User { Name = "Maria Santos", Email = "maria@hotmail.com", IsActive = true },
            new User { Name = "Pedro Costa", Email = "pedro@gmail.com", IsActive = false }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var products = new[]
        {
            new Product { Name = "Notebook", Price = 2500.00m, Category = "Electronics" },
            new Product { Name = "Mouse", Price = 50.00m, Category = "Electronics" },
            new Product { Name = "Book", Price = 25.00m, Category = "Education" }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var orders = new[]
        {
            new Order 
            { 
                UserId = users[0].Id, 
                TotalAmount = 2550.00m, 
                OrderDate = DateTime.UtcNow.AddDays(-5),
                Products = new List<Product> { products[0], products[1] }
            },
            new Order 
            { 
                UserId = users[1].Id, 
                TotalAmount = 75.00m, 
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Products = new List<Product> { products[1], products[2] }
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }
}
