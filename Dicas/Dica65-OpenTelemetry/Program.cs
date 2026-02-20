using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Dica65.OpenTelemetry.Services;
using Dica65.OpenTelemetry.Telemetry;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE LOGGING ===
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(ApplicationTelemetry.ServiceName, ApplicationTelemetry.ServiceVersion));
    
    options.AddConsoleExporter();
});

// === REGISTRO DE SERVIÇOS ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP Client para serviços externos
builder.Services.AddHttpClient<ExternalApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Dica65.OpenTelemetry/1.0.0");
});

// Serviços da aplicação
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<BusinessService>();
builder.Services.AddScoped<ExternalApiService>();

// === CONFIGURAÇÃO DO OPENTELEMETRY ===
var otelServiceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? ApplicationTelemetry.ServiceName;
var otelServiceVersion = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceVersion") ?? ApplicationTelemetry.ServiceVersion;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
        resourceBuilder
            .AddService(
                serviceName: otelServiceName,
                serviceVersion: otelServiceVersion,
                serviceNamespace: builder.Configuration.GetValue<string>("OpenTelemetry:ServiceNamespace"))
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
                new KeyValuePair<string, object>("host.name", Environment.MachineName),
                new KeyValuePair<string, object>("process.id", Environment.ProcessId),
                new KeyValuePair<string, object>("telemetry.sdk.name", "opentelemetry"),
                new KeyValuePair<string, object>("telemetry.sdk.language", "dotnet"),
                new KeyValuePair<string, object>("telemetry.sdk.version", "1.9.0")
            });
    })
    
    // === CONFIGURAÇÃO DE TRACES ===
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            // Adicionar source da aplicação
            .AddSource(ApplicationTelemetry.ActivitySource.Name)
            
            // Instrumentação automática do ASP.NET Core
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("http.request.method", request.Method);
                    activity.SetTag("http.request.scheme", request.Scheme);
                    activity.SetTag("http.request.host", request.Host.ToString());
                    activity.SetTag("http.request.path", request.Path);
                    
                    if (request.Headers.UserAgent.Any())
                    {
                        activity.SetTag("http.user_agent", request.Headers.UserAgent.ToString());
                    }
                };
                
                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag("http.response.status_code", response.StatusCode);
                };
                
                // Filtrar health checks e métricas
                options.Filter = context =>
                {
                    var path = context.Request.Path.Value;
                    return !path?.Contains("/health") == true && 
                           !path?.Contains("/metrics") == true;
                };
            })
            
            // Instrumentação automática do HttpClient
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    activity.SetTag("http.client.method", request.Method.ToString());
                    activity.SetTag("http.client.url", request.RequestUri?.ToString());
                };
                
                options.EnrichWithHttpResponseMessage = (activity, response) =>
                {
                    activity.SetTag("http.client.status_code", (int)response.StatusCode);
                    activity.SetTag("http.client.response_size", response.Content.Headers.ContentLength);
                };
            })
            
            // Sampling - controla quantos traces são coletados
            .SetSampler(new TraceIdRatioBasedSampler(
                builder.Configuration.GetValue<double>("OpenTelemetry:Sampling:Ratio", 1.0)))
            
            // Exportadores
            .AddConsoleExporter();
        
        // Exportador Jaeger (se habilitado) - comentado para evitar dependências
        /*
        if (builder.Configuration.GetValue<bool>("OpenTelemetry:Exporters:Jaeger:Enabled"))
        {
            tracingBuilder.AddJaegerExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration.GetValue<string>("OpenTelemetry:Exporters:Jaeger:Endpoint") 
                    ?? "http://localhost:14268/api/traces");
            });
        }
        */
    })
    
    // === CONFIGURAÇÃO DE MÉTRICAS ===
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            // Adicionar meter da aplicação
            .AddMeter(ApplicationTelemetry.Meter.Name)
            
            // Instrumentação automática do ASP.NET Core
            .AddAspNetCoreInstrumentation()
            
            // Instrumentação automática do HttpClient
            .AddHttpClientInstrumentation()
            
            // Exportador de console
            .AddConsoleExporter();
        
        // Exportador Prometheus (se habilitado)
        if (builder.Configuration.GetValue<bool>("OpenTelemetry:Exporters:Prometheus:Enabled"))
        {
            metricsBuilder.AddPrometheusExporter();
        }
    });

var app = builder.Build();

// === CONFIGURAÇÃO DO PIPELINE ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de telemetria customizado
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        
        // Adicionar tags customizadas à atividade atual
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("custom.request_id", context.TraceIdentifier);
            Activity.Current.SetTag("custom.request_duration_ms", stopwatch.ElapsedMilliseconds);
            
            if (context.User.Identity?.IsAuthenticated == true)
            {
                Activity.Current.SetTag("custom.user_authenticated", true);
                Activity.Current.SetTag("custom.user_name", context.User.Identity.Name);
            }
        }
    }
});

app.UseRouting();
app.MapControllers();

// Endpoint para métricas Prometheus
if (builder.Configuration.GetValue<bool>("OpenTelemetry:Exporters:Prometheus:Enabled"))
{
    app.MapPrometheusScrapingEndpoint();
}

// Endpoint de health check simples
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    service = ApplicationTelemetry.ServiceName,
    version = ApplicationTelemetry.ServiceVersion
}));

// Endpoint de demonstração - geração de traces e métricas
app.MapGet("/demo", async (DatabaseService databaseService, BusinessService businessService) =>
{
    using var activity = ApplicationTelemetry.StartActivity("Demo Endpoint");
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        activity?.SetTag("demo.type", "comprehensive");
        
        var results = new List<object>();
        
        // 1. Buscar usuários
        var users = await databaseService.GetUsersAsync(5);
        results.Add(new { operation = "get_users", count = users.Count });
        
        // 2. Processar lote pequeno
        var batchResult = await businessService.ProcessDataBatchAsync(20);
        results.Add(new { operation = "process_batch", processed = batchResult.ProcessedItems });
        
        // 3. Registrar novo usuário
        var registrationResult = await businessService.RegisterUserAsync(new()
        {
            Name = $"Demo User {Random.Shared.Next(1000, 9999)}",
            Email = $"demo{Random.Shared.Next(1000, 9999)}@example.com"
        });
        
        results.Add(new { operation = "register_user", success = registrationResult.Success });
        
        stopwatch.Stop();
        
        activity?.SetTag("demo.operations_completed", results.Count);
        activity?.SetStatus(ActivityStatusCode.Ok);
        
        // Registrar métricas customizadas
        ApplicationTelemetry.QueueSize.Add(Random.Shared.Next(-5, 10)); // Simular mudança na fila
        
        return Results.Ok(new
        {
            message = "Demo executado com sucesso",
            duration_ms = stopwatch.ElapsedMilliseconds,
            operations = results,
            trace_id = Activity.Current?.TraceId.ToString(),
            span_id = Activity.Current?.SpanId.ToString()
        });
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        return Results.Problem("Erro na demonstração: " + ex.Message);
    }
});

// Configurar observabilidade para gauges
var timer = new Timer(state =>
{
    // Simular valores de gauge
    var random = new Random();
    
    // Atualizar métricas observáveis (simuladas)
    ApplicationTelemetry.QueueSize.Add(random.Next(-2, 5)); // Variação na fila
    
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

Console.WriteLine("==== Dica 65: OpenTelemetry ====");
Console.WriteLine();
Console.WriteLine("🚀 Aplicação iniciada com OpenTelemetry configurado!");
Console.WriteLine();
Console.WriteLine("📊 Endpoints disponíveis:");
Console.WriteLine("  • GET  /health           - Health check");
Console.WriteLine("  • GET  /demo             - Demonstração completa");
Console.WriteLine("  • GET  /api/users        - Listar usuários");
Console.WriteLine("  • POST /api/users        - Criar usuário");
Console.WriteLine("  • GET  /api/users/{id}   - Buscar usuário");
Console.WriteLine("  • PUT  /api/users/{id}   - Atualizar usuário");
Console.WriteLine("  • POST /api/processing/batch - Processar lote");
Console.WriteLine("  • POST /api/processing/complex - Operação complexa");

if (builder.Configuration.GetValue<bool>("OpenTelemetry:Exporters:Prometheus:Enabled"))
{
    Console.WriteLine("  • GET  /metrics          - Métricas Prometheus");
}

Console.WriteLine();
Console.WriteLine("🔍 Observabilidade configurada:");
Console.WriteLine("  • Traces: Console + Instrumentação automática");
Console.WriteLine("  • Metrics: Console + Prometheus (se habilitado)");
Console.WriteLine("  • Logs: Console com correlação de traces");
Console.WriteLine();
Console.WriteLine("💡 Teste os endpoints para gerar telemetria!");
Console.WriteLine();

app.Run();

// Cleanup
ApplicationTelemetry.Dispose();
