using MediatR;
using Microsoft.Extensions.Logging;
using Dica44.MediatR.Models;

namespace Dica44.MediatR.Notifications;

// ================================
// USER CREATED NOTIFICATION
// ================================

/// <summary>
/// Notification disparada quando um usuário é criado
/// </summary>
public record UserCreatedNotification(User User) : INotification;

/// <summary>
/// Handler para enviar email de boas-vindas
/// </summary>
public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando email de boas-vindas para: {Email}", notification.User.Email);

        // Simular envio de email
        await Task.Delay(200, cancellationToken);

        _logger.LogInformation("Email de boas-vindas enviado com sucesso para: {Email}", notification.User.Email);
    }
}

/// <summary>
/// Handler para registrar auditoria
/// </summary>
public class AuditUserCreatedHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<AuditUserCreatedHandler> _logger;

    public AuditUserCreatedHandler(ILogger<AuditUserCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registrando auditoria para usuário criado: {UserId}", notification.User.Id);

        // Simular registro de auditoria
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Auditoria registrada - Usuário: {UserId}, Nome: {Name}, Email: {Email}, Data: {CreatedAt}", 
            notification.User.Id, notification.User.Name, notification.User.Email, notification.User.CreatedAt);
    }
}

/// <summary>
/// Handler para integração com sistemas externos
/// </summary>
public class ExternalSystemIntegrationHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<ExternalSystemIntegrationHandler> _logger;

    public ExternalSystemIntegrationHandler(ILogger<ExternalSystemIntegrationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sincronizando usuário com sistemas externos: {UserId}", notification.User.Id);

        // Simular integração com sistema externo
        await Task.Delay(300, cancellationToken);

        _logger.LogInformation("Usuário sincronizado com sistemas externos: {UserId}", notification.User.Id);
    }
}

// ================================
// USER UPDATED NOTIFICATION
// ================================

/// <summary>
/// Notification disparada quando um usuário é atualizado
/// </summary>
public record UserUpdatedNotification(User User, string[] ChangedFields) : INotification;

/// <summary>
/// Handler para invalidar cache
/// </summary>
public class InvalidateCacheHandler : INotificationHandler<UserUpdatedNotification>
{
    private readonly ILogger<InvalidateCacheHandler> _logger;

    public InvalidateCacheHandler(ILogger<InvalidateCacheHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invalidando cache para usuário: {UserId}", notification.User.Id);

        // Simular invalidação de cache
        await Task.Delay(50, cancellationToken);

        _logger.LogInformation("Cache invalidado - Usuário: {UserId}, Campos alterados: {ChangedFields}", 
            notification.User.Id, string.Join(", ", notification.ChangedFields));
    }
}

/// <summary>
/// Handler para notificar usuário sobre alterações
/// </summary>
public class NotifyUserChangesHandler : INotificationHandler<UserUpdatedNotification>
{
    private readonly ILogger<NotifyUserChangesHandler> _logger;

    public NotifyUserChangesHandler(ILogger<NotifyUserChangesHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notificando usuário sobre alterações: {UserId}", notification.User.Id);

        // Simular envio de notificação
        await Task.Delay(150, cancellationToken);

        _logger.LogInformation("Usuário notificado sobre alterações - Email: {Email}, Campos: {ChangedFields}", 
            notification.User.Email, string.Join(", ", notification.ChangedFields));
    }
}

// ================================
// SYSTEM NOTIFICATION
// ================================

/// <summary>
/// Notification para eventos do sistema
/// </summary>
public record SystemNotification(
    string EventType,
    string Message,
    DateTime Timestamp,
    Dictionary<string, object>? Metadata = null) : INotification;

/// <summary>
/// Handler para log de eventos do sistema
/// </summary>
public class SystemLogHandler : INotificationHandler<SystemNotification>
{
    private readonly ILogger<SystemLogHandler> _logger;

    public SystemLogHandler(ILogger<SystemLogHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SystemNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evento do sistema - Tipo: {EventType}, Mensagem: {Message}, Timestamp: {Timestamp}", 
            notification.EventType, notification.Message, notification.Timestamp);

        if (notification.Metadata != null && notification.Metadata.Any())
        {
            foreach (var metadata in notification.Metadata)
            {
                _logger.LogDebug("Metadata - {Key}: {Value}", metadata.Key, metadata.Value);
            }
        }

        // Simular processamento de log
        await Task.Delay(25, cancellationToken);
    }
}

/// <summary>
/// Handler para métricas do sistema
/// </summary>
public class SystemMetricsHandler : INotificationHandler<SystemNotification>
{
    private readonly ILogger<SystemMetricsHandler> _logger;

    public SystemMetricsHandler(ILogger<SystemMetricsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SystemNotification notification, CancellationToken cancellationToken)
    {
        // Simular coleta de métricas
        await Task.Delay(10, cancellationToken);

        _logger.LogDebug("Métrica coletada - EventType: {EventType}, Timestamp: {Timestamp}", 
            notification.EventType, notification.Timestamp);
    }
}

// ================================
// OPERATION COMPLETED NOTIFICATION
// ================================

/// <summary>
/// Notification disparada quando uma operação é concluída
/// </summary>
public record OperationCompletedNotification(
    string OperationName,
    TimeSpan Duration,
    bool Success,
    string? ErrorMessage = null) : INotification;

/// <summary>
/// Handler para monitoramento de performance
/// </summary>
public class PerformanceMonitoringHandler : INotificationHandler<OperationCompletedNotification>
{
    private readonly ILogger<PerformanceMonitoringHandler> _logger;

    public PerformanceMonitoringHandler(ILogger<PerformanceMonitoringHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OperationCompletedNotification notification, CancellationToken cancellationToken)
    {
        var logLevel = notification.Duration.TotalMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information;
        
        _logger.Log(logLevel, "Performance - Operação: {OperationName}, Duração: {Duration}ms, Sucesso: {Success}", 
            notification.OperationName, notification.Duration.TotalMilliseconds, notification.Success);

        if (!notification.Success && !string.IsNullOrEmpty(notification.ErrorMessage))
        {
            _logger.LogError("Erro na operação {OperationName}: {ErrorMessage}", 
                notification.OperationName, notification.ErrorMessage);
        }

        // Simular envio para sistema de monitoramento
        await Task.Delay(15, cancellationToken);
    }
}

/// <summary>
/// Handler para alertas do sistema
/// </summary>
public class SystemAlertsHandler : INotificationHandler<OperationCompletedNotification>
{
    private readonly ILogger<SystemAlertsHandler> _logger;

    public SystemAlertsHandler(ILogger<SystemAlertsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OperationCompletedNotification notification, CancellationToken cancellationToken)
    {
        // Verificar se deve disparar alerta
        var shouldAlert = !notification.Success || notification.Duration.TotalSeconds > 5;

        if (shouldAlert)
        {
            _logger.LogWarning("ALERTA - Operação: {OperationName}, Duração: {Duration}ms, Sucesso: {Success}", 
                notification.OperationName, notification.Duration.TotalMilliseconds, notification.Success);

            // Simular envio de alerta
            await Task.Delay(100, cancellationToken);

            _logger.LogInformation("Alerta enviado para operação: {OperationName}", notification.OperationName);
        }
        else
        {
            await Task.Delay(5, cancellationToken);
        }
    }
}
