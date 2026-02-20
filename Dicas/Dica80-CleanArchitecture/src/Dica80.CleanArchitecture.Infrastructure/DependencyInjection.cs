using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Infrastructure.Data;
using Dica80.CleanArchitecture.Infrastructure.Repositories;

namespace Dica80.CleanArchitecture.Infrastructure;

/// <summary>
/// Extension methods to configure Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        AddDatabase(services, configuration);

        // Add Repositories
        AddRepositories(services);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Use In-Memory database for development/testing
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("CleanArchitectureDb"));
        }
        else
        {
            // Use SQL Server for production
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
        }
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, SimpleUnitOfWork>();

        // Register Repositories
        services.AddScoped<IUserRepository, SimpleUserRepository>();
        services.AddScoped<IProjectRepository, SimpleProjectRepository>();
        services.AddScoped<ITaskRepository, SimpleTaskRepository>();
        services.AddScoped<ICommentRepository, SimpleCommentRepository>();
    }

    /// <summary>
    /// Adds Infrastructure layer services with In-Memory database for testing
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureForTesting(this IServiceCollection services)
    {
        // Add In-Memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Add Repositories
        AddRepositories(services);

        return services;
    }
}

/// <summary>
/// Database initialization and seeding
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Ensures database is created and seeded with initial data
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>Task</returns>
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed initial data if database is empty
        if (!await context.Users.AnyAsync())
        {
            await SeedDataAsync(context);
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context)
    {
        // Seed initial users
        var adminUser = Domain.Entities.User.Create(
            "System",
            "Administrator",
            "admin@cleanarch.com",
            "hashed_password_admin");

        var managerUser = Domain.Entities.User.Create(
            "Project",
            "Manager",
            "manager@cleanarch.com",
            "hashed_password_manager");

        var memberUser = Domain.Entities.User.Create(
            "Team",
            "Member",
            "member@cleanarch.com",
            "hashed_password_member");

        await context.Users.AddRangeAsync(adminUser, managerUser, memberUser);
        await context.SaveChangesAsync();

        // Seed initial project
        var sampleProject = Domain.Entities.Project.Create(
            "Sample Clean Architecture Project",
            "A demonstration project showcasing Clean Architecture principles",
            managerUser.Id,
            Domain.ValueObjects.Money.Create(50000, "USD"));

        await context.Projects.AddAsync(sampleProject);
        await context.SaveChangesAsync();

        // Seed initial tasks
        var task1 = Domain.Entities.TaskItem.Create(
            "Implement Domain Layer",
            "Create entities, value objects, and domain events",
            Domain.ValueObjects.Priority.High,
            sampleProject.Id,
            memberUser.Id);

        var task2 = Domain.Entities.TaskItem.Create(
            "Implement Application Layer",
            "Create CQRS commands, queries, and handlers",
            Domain.ValueObjects.Priority.High,
            sampleProject.Id,
            memberUser.Id);

        var task3 = Domain.Entities.TaskItem.Create(
            "Implement Infrastructure Layer",
            "Create Entity Framework configurations and repositories",
            Domain.ValueObjects.Priority.Medium,
            sampleProject.Id,
            memberUser.Id);

        await context.Tasks.AddRangeAsync(task1, task2, task3);
        await context.SaveChangesAsync();

        // Mark first task as completed
        task1.Complete();
        await context.SaveChangesAsync();
    }
}
