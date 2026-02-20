using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Dica80.CleanArchitecture.Infrastructure.Data;

namespace Dica80.CleanArchitecture.WebAPI.Extensions;

/// <summary>
/// Extension methods for configuring API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API-specific services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Configure JSON options
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = true;
        });

        // Configure API behavior options
        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Customize model validation error response
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                var response = new
                {
                    Error = "Validation failed",
                    ValidationErrors = errors,
                    StatusCode = 400,
                    Timestamp = DateTime.UtcNow
                };

                return new BadRequestObjectResult(response);
            };
        });

        // Add health checks
        services.AddHealthChecks();

        return services;
    }
}

/// <summary>
/// Extension methods for application pipeline configuration
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application pipeline with health checks
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    Status = report.Status.ToString(),
                    Checks = report.Entries.Select(x => new
                    {
                        Name = x.Key,
                        Status = x.Value.Status.ToString(),
                        Duration = x.Value.Duration.TotalMilliseconds,
                        Description = x.Value.Description,
                        Error = x.Value.Exception?.Message
                    }),
                    TotalDuration = report.TotalDuration.TotalMilliseconds
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        });

        return app;
    }
}
