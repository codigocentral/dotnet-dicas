using Dica34.ChamandoApisComRefit.Apis;
using Dica34.ChamandoApisComRefit.Services;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do JSON Serializer para Refit
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true
};

var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
};

// Política simples de retry
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Tentativa {retryCount} em {timespan} segundos");
            });
}

// Registrar APIs do Refit

// API de usuários (mock interno)
builder.Services.AddRefitClient<IUserApi>(refitSettings)
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(GetRetryPolicy());

// API de produtos (externa - FakeStore API)
builder.Services.AddRefitClient<IProductApi>(refitSettings)
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://fakestoreapi.com");
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(GetRetryPolicy());

// API segura (exemplo com headers)
builder.Services.AddRefitClient<ISecureApi>(refitSettings)
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://httpbin.org");
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(GetRetryPolicy());

// API para diferentes tipos de resposta
builder.Services.AddRefitClient<IResponseTypesApi>(refitSettings)
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://httpbin.org");
        c.Timeout = TimeSpan.FromSeconds(30);
    });

// Registrar serviços
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SecureService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Minimal API endpoints para demonstração

// Endpoint para buscar usuários
app.MapGet("/demo/users", async (UserService userService) =>
{
    try
    {
        var users = await userService.GetUsersAsync();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar usuários: {ex.Message}");
    }
})
.WithName("GetUsers")
.WithSummary("Busca todos os usuários");

// Endpoint para buscar usuário por ID
app.MapGet("/demo/users/{id:int}", async (int id, UserService userService) =>
{
    try
    {
        var user = await userService.GetUserAsync(id);
        return user != null ? Results.Ok(user) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar usuário: {ex.Message}");
    }
})
.WithName("GetUserById")
.WithSummary("Busca usuário por ID");

// Endpoint para buscar produtos
app.MapGet("/demo/products", async (ProductService productService) =>
{
    try
    {
        var products = await productService.GetAllProductsAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar produtos: {ex.Message}");
    }
})
.WithName("GetProducts")
.WithSummary("Busca todos os produtos");

// Endpoint para teste de autenticação
app.MapGet("/demo/secure-data", async (SecureService secureService) =>
{
    try
    {
        var token = "fake-jwt-token";
        var data = await secureService.GetSecureDataAsync(token);
        return Results.Ok(data);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar dados seguros: {ex.Message}");
    }
})
.WithName("GetSecureData")
.WithSummary("Testa chamada com autenticação");

// Endpoint para demonstrar diferentes tipos de resposta
app.MapGet("/demo/secure/bearer", async (SecureService secureService) =>
{
    try
    {
        var token = "fake-jwt-token";
        var data = await secureService.GetSecureDataAsync(token);
        return Results.Ok(data);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar dados seguros: {ex.Message}");
    }
})
.WithName("TestBearerAuth")
.WithSummary("Testa autenticação Bearer");

// Endpoint para demonstrar paginação
app.MapGet("/demo/users/search", async (string query, UserService userService, int limit = 10) =>
{
    try
    {
        var users = await userService.SearchUsersByNameAsync(query, limit);
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao buscar usuários: {ex.Message}");
    }
})
.WithName("SearchUsers")
.WithSummary("Busca usuários por nome");

app.Run();
