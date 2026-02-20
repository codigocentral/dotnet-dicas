using Dica66_RateLimiting.Services;
using Dica66_RateLimiting.Middleware;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Dica 66 - Rate Limiting API", 
        Version = "v1",
        Description = "Demonstração de Rate Limiting em ASP.NET Core com diferentes estratégias e políticas"
    });
    
    // Incluir comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configurar Rate Limiting
RateLimitPolicyService.ConfigureRateLimitPolicies(builder.Services);

// Health Checks
builder.Services.AddHealthChecks();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rate Limiting API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

// Middleware de métricas (antes do rate limiting)
app.UseMiddleware<RateLimitMetricsMiddleware>();

// Rate Limiting (deve vir cedo no pipeline)
app.UseRateLimiter();

// Middleware customizado de rate limiting
app.UseMiddleware<CustomRateLimitMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

// Endpoint de demonstração sem controller
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/ping", () => new { 
    Message = "Pong!", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}).WithName("Ping")
  .RequireRateLimiting("PerIP");

// Endpoint para status das políticas
app.MapGet("/api/policies", () => new {
    AvailablePolicies = new[] { "PerIP", "PerUser", "PerTier", "ConcurrentOperations" },
    GlobalPolicy = "Active",
    CustomMiddleware = "Active",
    Timestamp = DateTime.UtcNow
}).WithName("GetPolicies");

Console.WriteLine("🚀 Rate Limiting API iniciada!");
Console.WriteLine("📊 Swagger UI: http://localhost:5000");
Console.WriteLine("🏥 Health Check: http://localhost:5000/health");
Console.WriteLine("📋 Políticas disponíveis: PerIP, PerUser, PerTier, ConcurrentOperations");

app.Run();
