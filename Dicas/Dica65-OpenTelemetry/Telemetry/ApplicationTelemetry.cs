using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Dica65.OpenTelemetry.Telemetry;

/// <summary>
/// Classe central para gerenciar todas as atividades de telemetria da aplicação
/// </summary>
public static class ApplicationTelemetry
{
    // === INFORMAÇÕES DA APLICAÇÃO ===
    public const string ServiceName = "Dica65.OpenTelemetry";
    public const string ServiceVersion = "1.0.0";
    
    // === ACTIVITY SOURCE (TRACES) ===
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);
    
    // === METER (METRICS) ===
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);
    
    // === COUNTERS (CONTADORES) ===
    public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "http_requests_total",
        "requests",
        "Total number of HTTP requests");
    
    public static readonly Counter<long> ErrorCounter = Meter.CreateCounter<long>(
        "errors_total", 
        "errors",
        "Total number of errors");
    
    public static readonly Counter<long> BusinessOperationCounter = Meter.CreateCounter<long>(
        "business_operations_total",
        "operations", 
        "Total number of business operations");
    
    // === HISTOGRAMS (DISTRIBUIÇÕES) ===
    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "http_request_duration_ms",
        "milliseconds",
        "Duration of HTTP requests in milliseconds");
    
    public static readonly Histogram<double> DatabaseQueryDuration = Meter.CreateHistogram<double>(
        "database_query_duration_ms",
        "milliseconds", 
        "Duration of database queries in milliseconds");
    
    public static readonly Histogram<long> ProcessingItemsCount = Meter.CreateHistogram<long>(
        "processing_items_count",
        "items",
        "Number of items processed in batch operations");
    
    // === GAUGES (VALORES ATUAIS) ===
    public static readonly ObservableGauge<long> ActiveConnections = Meter.CreateObservableGauge<long>(
        "active_connections",
        () => GetActiveConnections(),
        "connections",
        "Number of active connections");
    
    public static readonly ObservableGauge<double> MemoryUsage = Meter.CreateObservableGauge<double>(
        "memory_usage_mb", 
        () => GetMemoryUsageMB(),
        "megabytes",
        "Current memory usage in megabytes");
    
    // === UP/DOWN COUNTERS (VALORES QUE SOBEM/DESCEM) ===
    public static readonly UpDownCounter<long> QueueSize = Meter.CreateUpDownCounter<long>(
        "queue_size",
        "items",
        "Current number of items in queue");
    
    // === MÉTODOS HELPER ===
    
    /// <summary>
    /// Inicia uma nova atividade de trace com tags padrão
    /// </summary>
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = ActivitySource.StartActivity(name, kind);
        activity?.SetTag("service.name", ServiceName);
        activity?.SetTag("service.version", ServiceVersion);
        return activity;
    }
    
    /// <summary>
    /// Registra uma métrica de requisição HTTP
    /// </summary>
    public static void RecordHttpRequest(string method, string endpoint, int statusCode, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("method", method),
            new("endpoint", endpoint), 
            new("status_code", statusCode)
        };
        
        RequestCounter.Add(1, tags);
        RequestDuration.Record(durationMs, tags);
        
        if (statusCode >= 400)
        {
            ErrorCounter.Add(1, tags);
        }
    }
    
    /// <summary>
    /// Registra uma operação de negócio
    /// </summary>
    public static void RecordBusinessOperation(string operationType, string result, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("operation_type", operationType),
            new("result", result)
        };
        
        BusinessOperationCounter.Add(1, tags);
        
        if (result == "error")
        {
            ErrorCounter.Add(1, new KeyValuePair<string, object?>[]
            {
                new("type", "business_operation"),
                new("operation", operationType)
            });
        }
    }
    
    /// <summary>
    /// Registra uma consulta de banco de dados
    /// </summary>
    public static void RecordDatabaseQuery(string queryType, string table, double durationMs, bool success)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("query_type", queryType),
            new("table", table),
            new("success", success)
        };
        
        DatabaseQueryDuration.Record(durationMs, tags);
        
        if (!success)
        {
            ErrorCounter.Add(1, new KeyValuePair<string, object?>[]
            {
                new("type", "database_error"),
                new("query_type", queryType),
                new("table", table)
            });
        }
    }
    
    /// <summary>
    /// Atualiza métricas de sistema
    /// </summary>
    public static void UpdateSystemMetrics(long activeConnectionsCount, long currentQueueSize)
    {
        // Para gauges observáveis, os valores são coletados automaticamente
        // mas podemos simular atualizações aqui para demonstração
    }
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    public static void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
    
    // === MÉTODOS PARA GAUGES OBSERVÁVEIS ===
    
    private static long GetActiveConnections()
    {
        // Simular contagem de conexões ativas
        return Random.Shared.Next(10, 100);
    }
    
    private static double GetMemoryUsageMB()
    {
        // Obter uso real de memória
        return GC.GetTotalMemory(false) / (1024.0 * 1024.0);
    }
}

/// <summary>
/// Tags padronizadas para telemetria
/// </summary>
public static class TelemetryTags
{
    // HTTP
    public const string HttpMethod = "http.method";
    public const string HttpStatusCode = "http.status_code";
    public const string HttpUrl = "http.url";
    public const string HttpUserAgent = "http.user_agent";
    
    // Database
    public const string DatabaseSystem = "db.system";
    public const string DatabaseName = "db.name";
    public const string DatabaseOperation = "db.operation";
    public const string DatabaseTable = "db.table";
    
    // Business
    public const string BusinessOperation = "business.operation";
    public const string BusinessDomain = "business.domain";
    public const string BusinessResult = "business.result";
    public const string BusinessUserId = "business.user_id";
    
    // System
    public const string SystemEnvironment = "system.environment";
    public const string SystemVersion = "system.version";
    public const string SystemHostname = "system.hostname";
}

/// <summary>
/// Helper para criar atividades específicas do domínio
/// </summary>
public static class ActivityHelper
{
    public static Activity? StartHttpActivity(string method, string url)
    {
        var activity = ApplicationTelemetry.StartActivity($"HTTP {method}", ActivityKind.Server);
        activity?.SetTag(TelemetryTags.HttpMethod, method);
        activity?.SetTag(TelemetryTags.HttpUrl, url);
        return activity;
    }
    
    public static Activity? StartDatabaseActivity(string operation, string table)
    {
        var activity = ApplicationTelemetry.StartActivity($"DB {operation}", ActivityKind.Client);
        activity?.SetTag(TelemetryTags.DatabaseSystem, "postgresql");
        activity?.SetTag(TelemetryTags.DatabaseOperation, operation);
        activity?.SetTag(TelemetryTags.DatabaseTable, table);
        return activity;
    }
    
    public static Activity? StartBusinessActivity(string operation, string domain)
    {
        var activity = ApplicationTelemetry.StartActivity($"Business {operation}", ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.BusinessOperation, operation);
        activity?.SetTag(TelemetryTags.BusinessDomain, domain);
        return activity;
    }
}
