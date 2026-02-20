using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dica44.MediatR.Behaviors;

// ================================
// LOGGING PIPELINE BEHAVIOR
// ================================

/// <summary>
/// Pipeline behavior para logging de requisições
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;

        _logger.LogInformation("🔄 Iniciando {RequestName} -> {ResponseName}", requestName, responseName);
        _logger.LogDebug("📋 Request: {@Request}", request);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation("✅ Concluído {RequestName} em {Duration}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            _logger.LogDebug("📤 Response: {@Response}", response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "❌ Erro em {RequestName} após {Duration}ms: {ErrorMessage}", 
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            
            throw;
        }
    }
}

// ================================
// VALIDATION PIPELINE BEHAVIOR
// ================================

/// <summary>
/// Pipeline behavior para validação usando FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, 
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogDebug("🔍 Nenhum validator encontrado para {RequestName}", requestName);
            return await next();
        }

        _logger.LogDebug("🔍 Validando {RequestName} com {ValidatorCount} validators", 
            requestName, _validators.Count());

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning("❌ Validação falhou para {RequestName}: {ErrorCount} erros encontrados", 
                requestName, failures.Count);

            foreach (var failure in failures)
            {
                _logger.LogWarning("📋 Erro de validação - Propriedade: {PropertyName}, Mensagem: {ErrorMessage}", 
                    failure.PropertyName, failure.ErrorMessage);
            }

            throw new ValidationException(failures);
        }

        _logger.LogDebug("✅ Validação bem-sucedida para {RequestName}", requestName);
        return await next();
    }
}

// ================================
// PERFORMANCE PIPELINE BEHAVIOR
// ================================

/// <summary>
/// Pipeline behavior para monitoramento de performance
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IMediator _mediator;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var duration = stopwatch.Elapsed;

            // Log baseado na duração
            if (duration.TotalMilliseconds > 2000)
            {
                _logger.LogWarning("🐌 OPERAÇÃO LENTA - {RequestName} executado em {Duration}ms", 
                    requestName, duration.TotalMilliseconds);
            }
            else if (duration.TotalMilliseconds > 500)
            {
                _logger.LogInformation("⚠️ OPERAÇÃO MODERADA - {RequestName} executado em {Duration}ms", 
                    requestName, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug("⚡ OPERAÇÃO RÁPIDA - {RequestName} executado em {Duration}ms", 
                    requestName, duration.TotalMilliseconds);
            }

            // Publicar notification de operação completada (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mediator.Publish(new Notifications.OperationCompletedNotification(
                        requestName, duration, true), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao publicar OperationCompletedNotification");
                }
            }, CancellationToken.None);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;

            _logger.LogError("💥 OPERAÇÃO COM ERRO - {RequestName} falhou após {Duration}ms: {ErrorMessage}", 
                requestName, duration.TotalMilliseconds, ex.Message);

            // Publicar notification de operação com erro (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mediator.Publish(new Notifications.OperationCompletedNotification(
                        requestName, duration, false, ex.Message), CancellationToken.None);
                }
                catch (Exception publishEx)
                {
                    _logger.LogError(publishEx, "Erro ao publicar OperationCompletedNotification de erro");
                }
            }, CancellationToken.None);

            throw;
        }
    }
}

// ================================
// CACHING PIPELINE BEHAVIOR
// ================================

/// <summary>
/// Pipeline behavior para cache simples (apenas para demonstração)
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private static readonly Dictionary<string, (object Response, DateTime CachedAt)> _cache = new();
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public CachingBehavior(ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Só cachear queries (não commands)
        if (!requestName.Contains("Query"))
        {
            _logger.LogDebug("🚫 Cache ignorado para {RequestName} (não é query)", requestName);
            return await next();
        }

        var cacheKey = $"{requestName}:{request?.GetHashCode()}";

        // Verificar cache
        lock (_cache)
        {
            if (_cache.TryGetValue(cacheKey, out var cachedEntry))
            {
                if (DateTime.UtcNow - cachedEntry.CachedAt < _cacheExpiry)
                {
                    _logger.LogInformation("🎯 CACHE HIT - {RequestName} retornado do cache", requestName);
                    return (TResponse)cachedEntry.Response;
                }
                else
                {
                    _cache.Remove(cacheKey);
                    _logger.LogDebug("🗑️ Cache expirado removido para {RequestName}", requestName);
                }
            }
        }

        _logger.LogDebug("🔍 CACHE MISS - Executando {RequestName}", requestName);
        var response = await next();

        // Adicionar ao cache
        lock (_cache)
        {
            _cache[cacheKey] = (response!, DateTime.UtcNow);
            _logger.LogDebug("💾 Resultado adicionado ao cache para {RequestName}", requestName);
        }

        return response;
    }
}

// ================================
// RETRY PIPELINE BEHAVIOR
// ================================

/// <summary>
/// Pipeline behavior para retry automático em caso de falhas transitórias
/// </summary>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private const int MaxRetries = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(100);

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Só aplicar retry para operações específicas
        if (!ShouldRetry(requestName))
        {
            return await next();
        }

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 1)
                {
                    _logger.LogInformation("🔄 RETRY - Tentativa {Attempt}/{MaxRetries} para {RequestName}", 
                        attempt, MaxRetries, requestName);
                }

                var response = await next();
                
                if (attempt > 1)
                {
                    _logger.LogInformation("✅ RETRY SUCESSO - {RequestName} sucedeu na tentativa {Attempt}", 
                        requestName, attempt);
                }

                return response;
            }
            catch (Exception ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                
                _logger.LogWarning("⚠️ RETRY NECESSÁRIO - {RequestName} falhou na tentativa {Attempt}: {ErrorMessage}. Aguardando {Delay}ms", 
                    requestName, attempt, ex.Message, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    _logger.LogError("💥 RETRY ESGOTADO - {RequestName} falhou após {MaxRetries} tentativas: {ErrorMessage}", 
                        requestName, MaxRetries, ex.Message);
                }
                throw;
            }
        }

        // Este ponto nunca deve ser alcançado
        throw new InvalidOperationException("Retry behavior falhou inesperadamente");
    }

    private static bool ShouldRetry(string requestName)
    {
        // Por exemplo, só aplicar retry para operações específicas
        return requestName.Contains("Slow") || requestName.Contains("External");
    }

    private static bool IsTransientError(Exception ex)
    {
        // Determinar se o erro é transitório
        return ex is TaskCanceledException ||
               ex is TimeoutException ||
               (ex is InvalidOperationException && ex.Message.Contains("transitório"));
    }
}
