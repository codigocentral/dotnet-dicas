using System.Text.Json.Serialization;

namespace Dica57.MessageQueues.Models;

/// <summary>
/// Modelo base para mensagens do sistema
/// </summary>
public class BaseMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
    public string ReplyTo { get; set; } = string.Empty;
}

/// <summary>
/// Mensagem de pedido
/// </summary>
public class OrderMessage : BaseMessage
{
    public int OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Created";
}

/// <summary>
/// Item de pedido
/// </summary>
public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

/// <summary>
/// Mensagem de notificação
/// </summary>
public class NotificationMessage : BaseMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Email;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

/// <summary>
/// Tipos de notificação
/// </summary>
public enum NotificationType
{
    Email,
    SMS,
    Push,
    Slack,
    Teams
}

/// <summary>
/// Prioridade de notificação
/// </summary>
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Mensagem de evento de usuário
/// </summary>
public class UserEventMessage : BaseMessage
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // Created, Updated, Deleted, LoggedIn, LoggedOut
    public Dictionary<string, object> EventData { get; set; } = new();
}

/// <summary>
/// Mensagem de resposta de pagamento
/// </summary>
public class PaymentResponseMessage : BaseMessage
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string TransactionId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Status de pagamento
/// </summary>
public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Refunded
}

/// <summary>
/// Mensagem de métrica de sistema
/// </summary>
public class SystemMetricMessage : BaseMessage
{
    public string ServiceName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}

/// <summary>
/// Configuração para Message Brokers
/// </summary>
public class MessageBrokerConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Resultado de processamento de mensagem
/// </summary>
public class MessageProcessingResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Estatísticas de Message Queue
/// </summary>
public class QueueStatistics
{
    public string QueueName { get; set; } = string.Empty;
    public long MessagesProduced { get; set; }
    public long MessagesConsumed { get; set; }
    public long MessagesInQueue { get; set; }
    public long DeadLetterMessages { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public DateTime LastMessageTime { get; set; }
}
