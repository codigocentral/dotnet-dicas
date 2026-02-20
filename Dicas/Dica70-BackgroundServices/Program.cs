using Microsoft.OpenApi.Models;
using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;
using Dica70_BackgroundServices.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Configuração de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllers();

// Registrar serviços de fila
builder.Services.AddSingleton<IQueueService<EmailNotification>, ChannelQueueService<EmailNotification>>();
builder.Services.AddSingleton<IQueueService<DataProcessingJob>, PriorityQueueService<DataProcessingJob>>();

// Registrar serviço de tracking
builder.Services.AddSingleton<IJobTrackingService, JobTrackingService>();

// Registrar background services
builder.Services.AddHostedService<TimedBackgroundService>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DataProcessingBackgroundService>();

// Registrar singleton background service como singleton para poder injetar em controllers
builder.Services.AddSingleton<SingletonBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SingletonBackgroundService>());

// Registrar health monitoring service como singleton para acesso nos controllers
builder.Services.AddSingleton<HealthMonitoringService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<HealthMonitoringService>());

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dica 70 - Background Services API",
        Version = "v1",
        Description = @"
API demonstrando diferentes tipos de Background Services em .NET:

## Funcionalidades

### Background Services Implementados:
- **TimedBackgroundService**: Executa tarefas agendadas (limpeza automática)
- **EmailBackgroundService**: Processa fila de emails com prioridades
- **DataProcessingBackgroundService**: Processa jobs de dados em lote
- **HealthMonitoringService**: Monitora saúde do sistema
- **SingletonBackgroundService**: Executa tarefas críticas (apenas uma instância)

### Filas:
- **Email Queue**: Fila baseada em Channels para emails
- **Data Processing Queue**: Fila com prioridade para processamento de dados

### Monitoramento:
- Tracking de jobs com status e métricas
- Health checks automatizados
- Dashboard com visão geral do sistema

## Padrões Demonstrados:
- Timer-based background services
- Queue-based background services  
- Batch processing
- Health monitoring
- Singleton pattern for critical tasks
- Producer/Consumer pattern
- Retry mechanisms
- Graceful shutdown with CancellationToken",
        Contact = new OpenApiContact
        {
            Name = "Dicas C#",
            Url = new Uri("https://github.com/example/dicas-csharp")
        }
    });

    // Incluir comentários XML se existirem
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Background Services API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
        c.DocumentTitle = "Background Services - Dica 70";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Endpoints informativos
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/info", () => new
{
    Title = "Dica 70 - Background Services",
    Description = "Demonstração de diferentes tipos de Background Services em .NET",
    Version = "1.0",
    Endpoints = new
    {
        Swagger = "/swagger",
        Health = "/health",
        Jobs = "/api/jobs",
        Queue = "/api/queue", 
        Monitoring = "/api/monitoring"
    },
    BackgroundServices = new[]
    {
        "TimedBackgroundService - Tarefas agendadas",
        "EmailBackgroundService - Processamento de emails",
        "DataProcessingBackgroundService - Processamento de dados",
        "HealthMonitoringService - Monitoramento de saúde",
        "SingletonBackgroundService - Tarefas críticas singleton"
    }
});

Console.WriteLine("🚀 Background Services API iniciada");
Console.WriteLine("📊 Dashboard: http://localhost:5000");
Console.WriteLine("📋 Swagger: http://localhost:5000/swagger");
Console.WriteLine("❤️ Health: http://localhost:5000/health");

app.Run();
