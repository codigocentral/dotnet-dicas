using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dica80.CleanArchitecture.Application.Common;
using Dica80.CleanArchitecture.Application.Mappings;

namespace Dica80.CleanArchitecture.Application;

/// <summary>
/// Extension methods to configure Application layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add AutoMapper
        services.AddAutoMapper(typeof(ApplicationMappingProfile));

        // Add FluentValidation - register all validators from assembly
        // services.AddScoped(typeof(IValidator<>), typeof(BaseValidator<>)); // Remove this problematic line
        
        // Register all concrete validators manually
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null && 
                       t.BaseType.IsGenericType && 
                       t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();
            
        foreach (var validatorType in validatorTypes)
        {
            var interfaceType = validatorType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, validatorType);
            }
        }

        // Add MediatR Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }
}

/// <summary>
/// Interface for application services
/// </summary>
public interface IApplicationService
{
    // Marker interface for application services
}

/// <summary>
/// Base application service for common functionality
/// </summary>
public abstract class BaseApplicationService : IApplicationService
{
    protected readonly IMediator Mediator;

    protected BaseApplicationService(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Sends a command or query through MediatR
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response</returns>
    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await Mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Sends a command through MediatR
    /// </summary>
    /// <param name="request">Command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        await Mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification through MediatR
    /// </summary>
    /// <param name="notification">Notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        await Mediator.Publish(notification, cancellationToken);
    }
}
