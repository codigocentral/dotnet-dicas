using System.Diagnostics;
using Dica65.OpenTelemetry.Telemetry;

namespace Dica65.OpenTelemetry.Services;

/// <summary>
/// Serviço que simula operações de banco de dados com telemetria
/// </summary>
public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly Random _random = new();

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simula busca de usuários no banco
    /// </summary>
    public async Task<List<UserDto>> GetUsersAsync(int count = 10)
    {
        using var activity = ActivityHelper.StartDatabaseActivity("SELECT", "users");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Buscando {Count} usuários no banco", count);
            
            // Simular latência de banco
            var delayMs = _random.Next(50, 200);
            await Task.Delay(delayMs);
            
            // Simular falha ocasional
            if (_random.Next(1, 11) <= 1) // 10% de chance de falha
            {
                throw new InvalidOperationException("Database connection timeout");
            }
            
            var users = Enumerable.Range(1, count)
                .Select(i => new UserDto
                {
                    Id = i,
                    Name = $"User {i}",
                    Email = $"user{i}@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365))
                })
                .ToList();
            
            // Registrar métricas de sucesso
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("SELECT", "users", stopwatch.ElapsedMilliseconds, true);
            
            activity?.SetTag("records_returned", count);
            activity?.SetTag("query_success", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Retornados {Count} usuários em {Duration}ms", count, stopwatch.ElapsedMilliseconds);
            
            return users;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("SELECT", "users", stopwatch.ElapsedMilliseconds, false);
            
            activity?.SetTag("query_success", false);
            activity?.SetTag("error_message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro ao buscar usuários");
            throw;
        }
    }

    /// <summary>
    /// Simula criação de usuário
    /// </summary>
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        using var activity = ActivityHelper.StartDatabaseActivity("INSERT", "users");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Criando usuário: {Name}", request.Name);
            
            // Simular validações
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Nome é obrigatório");
            }
            
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Email é obrigatório");
            }
            
            // Simular latência de escrita
            var delayMs = _random.Next(100, 300);
            await Task.Delay(delayMs);
            
            // Simular falha ocasional
            if (_random.Next(1, 21) <= 1) // 5% de chance de falha
            {
                throw new InvalidOperationException("Duplicate email address");
            }
            
            var user = new UserDto
            {
                Id = _random.Next(1000, 9999),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("INSERT", "users", stopwatch.ElapsedMilliseconds, true);
            
            activity?.SetTag("user_id", user.Id);
            activity?.SetTag("query_success", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Usuário criado: ID {UserId} em {Duration}ms", user.Id, stopwatch.ElapsedMilliseconds);
            
            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("INSERT", "users", stopwatch.ElapsedMilliseconds, false);
            
            activity?.SetTag("query_success", false);
            activity?.SetTag("error_message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro ao criar usuário");
            throw;
        }
    }

    /// <summary>
    /// Simula atualização de usuário
    /// </summary>
    public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        using var activity = ActivityHelper.StartDatabaseActivity("UPDATE", "users");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Atualizando usuário {UserId}", userId);
            
            // Simular latência
            await Task.Delay(_random.Next(75, 150));
            
            // Simular usuário não encontrado
            if (_random.Next(1, 11) <= 1) // 10% de chance
            {
                throw new KeyNotFoundException($"User {userId} not found");
            }
            
            var user = new UserDto
            {
                Id = userId,
                Name = request.Name ?? $"User {userId}",
                Email = request.Email ?? $"user{userId}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 100))
            };
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("UPDATE", "users", stopwatch.ElapsedMilliseconds, true);
            
            activity?.SetTag("user_id", userId);
            activity?.SetTag("query_success", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return user;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordDatabaseQuery("UPDATE", "users", stopwatch.ElapsedMilliseconds, false);
            
            activity?.SetTag("user_id", userId);
            activity?.SetTag("query_success", false);
            activity?.SetTag("error_message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", userId);
            throw;
        }
    }
}

/// <summary>
/// Serviço que simula operações de negócio com telemetria
/// </summary>
public class BusinessService
{
    private readonly DatabaseService _databaseService;
    private readonly ExternalApiService _externalApiService;
    private readonly ILogger<BusinessService> _logger;
    private readonly Random _random = new();

    public BusinessService(
        DatabaseService databaseService,
        ExternalApiService externalApiService,
        ILogger<BusinessService> logger)
    {
        _databaseService = databaseService;
        _externalApiService = externalApiService;
        _logger = logger;
    }

    /// <summary>
    /// Processo de negócio: Registrar novo usuário com validações externas
    /// </summary>
    public async Task<UserRegistrationResult> RegisterUserAsync(CreateUserRequest request)
    {
        using var activity = ActivityHelper.StartBusinessActivity("user_registration", "user_management");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Iniciando registro de usuário: {Email}", request.Email);
            
            activity?.SetTag(TelemetryTags.BusinessUserId, request.Email);
            activity?.SetTag("registration_type", "standard");
            
            // 1. Validar email externamente
            _logger.LogInformation("Validando email externamente...");
            var emailValidation = await _externalApiService.ValidateEmailAsync(request.Email);
            
            if (!emailValidation.IsValid)
            {
                var result = new UserRegistrationResult
                {
                    Success = false,
                    ErrorMessage = "Email inválido",
                    User = null
                };
                
                stopwatch.Stop();
                ApplicationTelemetry.RecordBusinessOperation("user_registration", "validation_failed", stopwatch.ElapsedMilliseconds);
                
                activity?.SetTag(TelemetryTags.BusinessResult, "validation_failed");
                activity?.SetStatus(ActivityStatusCode.Error, "Email validation failed");
                
                return result;
            }
            
            // 2. Criar usuário no banco
            _logger.LogInformation("Criando usuário no banco...");
            var user = await _databaseService.CreateUserAsync(request);
            
            // 3. Enviar evento de boas-vindas
            _logger.LogInformation("Enviando evento de boas-vindas...");
            await _externalApiService.SendWelcomeEventAsync(user.Id, user.Email);
            
            var successResult = new UserRegistrationResult
            {
                Success = true,
                ErrorMessage = null,
                User = user
            };
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordBusinessOperation("user_registration", "success", stopwatch.ElapsedMilliseconds);
            
            activity?.SetTag(TelemetryTags.BusinessResult, "success");
            activity?.SetTag("user_id", user.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Usuário registrado com sucesso: ID {UserId} em {Duration}ms", 
                user.Id, stopwatch.ElapsedMilliseconds);
            
            return successResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordBusinessOperation("user_registration", "error", stopwatch.ElapsedMilliseconds);
            
            activity?.SetTag(TelemetryTags.BusinessResult, "error");
            activity?.SetTag("error_type", ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro no registro de usuário");
            
            return new UserRegistrationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                User = null
            };
        }
    }

    /// <summary>
    /// Operação de negócio: Processar lote de dados
    /// </summary>
    public async Task<BatchProcessingResult> ProcessDataBatchAsync(int batchSize = 100)
    {
        using var activity = ActivityHelper.StartBusinessActivity("batch_processing", "data_processing");
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Iniciando processamento de lote com {BatchSize} itens", batchSize);
            
            activity?.SetTag("batch_size", batchSize);
            
            var processedCount = 0;
            var errorCount = 0;
            
            // Simular processamento de itens
            for (int i = 0; i < batchSize; i++)
            {
                // Simular processamento individual
                await Task.Delay(_random.Next(5, 20));
                
                // Simular falhas ocasionais
                if (_random.Next(1, 21) <= 1) // 5% de chance de falha
                {
                    errorCount++;
                    _logger.LogWarning("Erro ao processar item {ItemIndex}", i);
                }
                else
                {
                    processedCount++;
                }
                
                // Atualizar métricas a cada 10 itens
                if (i % 10 == 0)
                {
                    ApplicationTelemetry.ProcessingItemsCount.Record(10, 
                        new KeyValuePair<string, object?>[]
                        {
                            new("batch_id", Guid.NewGuid().ToString()),
                            new("processing_type", "data_transformation")
                        });
                }
            }
            
            var result = new BatchProcessingResult
            {
                TotalItems = batchSize,
                ProcessedItems = processedCount,
                ErrorItems = errorCount,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
            
            stopwatch.Stop();
            
            var operationResult = errorCount == 0 ? "success" : 
                                errorCount < batchSize * 0.1 ? "partial_success" : "failed";
            
            ApplicationTelemetry.RecordBusinessOperation("batch_processing", operationResult, stopwatch.ElapsedMilliseconds);
            
            activity?.SetTag("processed_items", processedCount);
            activity?.SetTag("error_items", errorCount);
            activity?.SetTag(TelemetryTags.BusinessResult, operationResult);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Lote processado: {ProcessedItems}/{TotalItems} itens em {Duration}ms", 
                processedCount, batchSize, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordBusinessOperation("batch_processing", "error", stopwatch.ElapsedMilliseconds);
            
            activity?.SetTag(TelemetryTags.BusinessResult, "error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro no processamento de lote");
            throw;
        }
    }
}

/// <summary>
/// Serviço que simula chamadas para APIs externas
/// </summary>
public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;
    private readonly Random _random = new();

    public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Simula validação de email em serviço externo
    /// </summary>
    public async Task<EmailValidationResult> ValidateEmailAsync(string email)
    {
        using var activity = ApplicationTelemetry.StartActivity("External API: Email Validation", ActivityKind.Client);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Validando email: {Email}", email);
            
            activity?.SetTag("api.service", "email-validation");
            activity?.SetTag("email", email);
            
            // Simular chamada HTTP
            await Task.Delay(_random.Next(100, 500));
            
            // Simular falha ocasional
            if (_random.Next(1, 21) <= 1) // 5% de chance de falha
            {
                throw new HttpRequestException("Email validation service unavailable");
            }
            
            // Simular validação (emails com 'invalid' são considerados inválidos)
            var isValid = !email.Contains("invalid", StringComparison.OrdinalIgnoreCase);
            
            var result = new EmailValidationResult
            {
                Email = email,
                IsValid = isValid,
                Reason = isValid ? "Valid format" : "Invalid format"
            };
            
            stopwatch.Stop();
            
            activity?.SetTag("validation_result", isValid);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Email {Email} validado: {IsValid} em {Duration}ms", 
                email, isValid, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            activity?.SetTag("error_message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            _logger.LogError(ex, "Erro ao validar email {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Simula envio de evento de boas-vindas
    /// </summary>
    public async Task SendWelcomeEventAsync(int userId, string email)
    {
        using var activity = ApplicationTelemetry.StartActivity("External API: Welcome Event", ActivityKind.Producer);
        
        try
        {
            _logger.LogInformation("Enviando evento de boas-vindas para usuário {UserId}", userId);
            
            activity?.SetTag("api.service", "event-service");
            activity?.SetTag("event.type", "user.welcome");
            activity?.SetTag("user_id", userId);
            
            // Simular envio
            await Task.Delay(_random.Next(50, 200));
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation("Evento de boas-vindas enviado para usuário {UserId}", userId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Erro ao enviar evento de boas-vindas");
            throw;
        }
    }
}

// === DTOs ===

public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreateUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record UpdateUserRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
}

public record UserRegistrationResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public UserDto? User { get; init; }
}

public record BatchProcessingResult
{
    public int TotalItems { get; init; }
    public int ProcessedItems { get; init; }
    public int ErrorItems { get; init; }
    public long DurationMs { get; init; }
}

public record EmailValidationResult
{
    public string Email { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public string Reason { get; init; } = string.Empty;
}
