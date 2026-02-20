namespace Dica61.DependencyInjection.Interfaces;

// Interface base para repositórios
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Interface específica para usuários
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

// Interface para serviços de domínio
public interface IUserService
{
    Task<User?> GetUserAsync(int id);
    Task<User> CreateUserAsync(string name, string email);
    Task<User> UpdateUserAsync(int id, string name, string email);
    Task DeleteUserAsync(int id);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

// Interface para cache
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

// Interface para logging customizado
public interface IAuditLogger
{
    Task LogActionAsync(string action, string details, int? userId = null);
    Task LogErrorAsync(string error, Exception? exception = null);
}

// Interface para notificações
public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendPushNotificationAsync(int userId, string title, string message);
}

// Interface para APIs externas
public interface IExternalApiService
{
    Task<T?> GetAsync<T>(string endpoint) where T : class;
    Task<T?> PostAsync<T>(string endpoint, object data) where T : class;
}

// Interface para validação de business rules
public interface IBusinessRuleValidator
{
    Task<ValidationResult> ValidateUserCreationAsync(string name, string email);
    Task<ValidationResult> ValidateUserUpdateAsync(int id, string name, string email);
}

// Interface para trabalho em background
public interface IBackgroundTaskService
{
    Task QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
}

// Interface para factories
public interface IUserFactory
{
    User CreateUser(string name, string email);
    User CreateUser(UserDto dto);
}

// Interface para serviços com scoped lifetime
public interface IScopedService
{
    Guid InstanceId { get; }
    DateTime CreatedAt { get; }
    Task<string> ProcessScopedOperationAsync();
}

// Interface para serviços singleton
public interface ISingletonService
{
    Guid InstanceId { get; }
    DateTime CreatedAt { get; }
    int RequestCount { get; }
    void IncrementRequestCount();
}

// Interface para serviços transient
public interface ITransientService
{
    Guid InstanceId { get; }
    DateTime CreatedAt { get; }
    Task<string> ProcessTransientOperationAsync();
}

// Interface genérica para handlers
public interface IHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}

// Interface para estratégias de processamento
public interface IProcessingStrategy
{
    string StrategyName { get; }
    Task<ProcessingResult> ProcessAsync(ProcessingRequest request);
}

// Modelo de domínio
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

// DTOs
public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ProcessingRequest
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public string Result { get; set; } = string.Empty;
    public string ProcessedBy { get; set; } = string.Empty;
}
