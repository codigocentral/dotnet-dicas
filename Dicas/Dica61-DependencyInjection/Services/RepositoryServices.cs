using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dica61.DependencyInjection.Interfaces;

namespace Dica61.DependencyInjection.Services;

// Implementação do repositório em memória
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly ILogger<InMemoryUserRepository> _logger;
    private int _nextId = 1;

    public InMemoryUserRepository(ILogger<InMemoryUserRepository> logger)
    {
        _logger = logger;
        
        // Dados iniciais para demonstração
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, Name = "João Silva", Email = "joao@example.com", CreatedAt = DateTime.Now.AddDays(-30), IsActive = true },
            new User { Id = _nextId++, Name = "Maria Santos", Email = "maria@example.com", CreatedAt = DateTime.Now.AddDays(-15), IsActive = true },
            new User { Id = _nextId++, Name = "Pedro Costa", Email = "pedro@example.com", CreatedAt = DateTime.Now.AddDays(-7), IsActive = false }
        });
    }

    public Task<User?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Buscando usuário por ID: {UserId}", id);
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        _logger.LogDebug("Buscando todos os usuários");
        return Task.FromResult<IEnumerable<User>>(_users.ToList());
    }

    public Task<User> CreateAsync(User entity)
    {
        _logger.LogInformation("Criando novo usuário: {UserName}", entity.Name);
        entity.Id = _nextId++;
        entity.CreatedAt = DateTime.Now;
        _users.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<User> UpdateAsync(User entity)
    {
        _logger.LogInformation("Atualizando usuário: {UserId}", entity.Id);
        var existingUser = _users.FirstOrDefault(u => u.Id == entity.Id);
        if (existingUser != null)
        {
            existingUser.Name = entity.Name;
            existingUser.Email = entity.Email;
            existingUser.IsActive = entity.IsActive;
        }
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(int id)
    {
        _logger.LogInformation("Removendo usuário: {UserId}", id);
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
        }
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogDebug("Buscando usuário por email: {Email}", email);
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogDebug("Buscando usuários ativos");
        var activeUsers = _users.Where(u => u.IsActive).ToList();
        return Task.FromResult<IEnumerable<User>>(activeUsers);
    }
}

// Implementação do cache em memória
public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, CacheItem> _cache = new();
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        _logger.LogDebug("Buscando item no cache: {CacheKey}", key);
        
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogDebug("Item encontrado no cache: {CacheKey}", key);
                return Task.FromResult(item.Value as T);
            }
            else
            {
                _logger.LogDebug("Item expirado no cache, removendo: {CacheKey}", key);
                _cache.Remove(key);
            }
        }

        _logger.LogDebug("Item não encontrado no cache: {CacheKey}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var exp = expiration ?? _defaultExpiration;
        var item = new CacheItem
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(exp)
        };

        _cache[key] = item;
        _logger.LogDebug("Item adicionado ao cache: {CacheKey}, expira em: {ExpiresAt}", key, item.ExpiresAt);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        if (_cache.Remove(key))
        {
            _logger.LogDebug("Item removido do cache: {CacheKey}", key);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        var exists = _cache.ContainsKey(key) && _cache[key].ExpiresAt > DateTime.UtcNow;
        return Task.FromResult(exists);
    }

    private class CacheItem
    {
        public object Value { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}

// Implementação do audit logger
public class DatabaseAuditLogger : IAuditLogger
{
    private readonly ILogger<DatabaseAuditLogger> _logger;

    public DatabaseAuditLogger(ILogger<DatabaseAuditLogger> logger)
    {
        _logger = logger;
    }

    public Task LogActionAsync(string action, string details, int? userId = null)
    {
        _logger.LogInformation("AUDIT: {Action} | User: {UserId} | Details: {Details}", 
            action, userId ?? 0, details);
        
        // Aqui seria gravado no banco de dados
        return Task.CompletedTask;
    }

    public Task LogErrorAsync(string error, Exception? exception = null)
    {
        _logger.LogError(exception, "AUDIT ERROR: {Error}", error);
        
        // Aqui seria gravado no banco de dados
        return Task.CompletedTask;
    }
}

// Implementação do serviço de notificação
public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Enviando email para: {To}, Assunto: {Subject}", to, subject);
        
        // Simulação do envio de email
        return Task.Delay(100);
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("Enviando SMS para: {PhoneNumber}, Mensagem: {Message}", phoneNumber, message);
        
        // Simulação do envio de SMS
        return Task.Delay(50);
    }

    public Task SendPushNotificationAsync(int userId, string title, string message)
    {
        _logger.LogInformation("Enviando push notification para usuário: {UserId}, Título: {Title}", userId, title);
        
        // Simulação do envio de push notification
        return Task.Delay(30);
    }
}
