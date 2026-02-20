using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Text.Json;

Console.WriteLine("==== Dica 37: Usando HttpClientFactory ====\n");

// Configuração do Host com DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // 1. HttpClient básico
        services.AddHttpClient("BasicClient");

        // 2. HttpClient nomeado com configuração
        services.AddHttpClient("JsonPlaceholderClient", client =>
        {
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "DicasCSharp-HttpClientFactory/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // 3. HttpClient tipado
        services.AddHttpClient<UsuarioService>(client =>
        {
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy()) // Adiciona política de retry
        .AddPolicyHandler(GetCircuitBreakerPolicy()); // Adiciona circuit breaker

        // 4. HttpClient com Polly (resilência)
        services.AddHttpClient("ResilientClient", client =>
        {
            client.BaseAddress = new Uri("https://httpbin.org/");
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        // Registrar serviços
        services.AddScoped<ApiService>();
        services.AddScoped<DemoService>();
    })
    .Build();

// Executar demonstrações
var demoService = host.Services.GetRequiredService<DemoService>();
await demoService.ExecutarDemonstracaoAsync();

await host.StopAsync();

// Políticas de resilência com Polly
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"🔄 Tentativa {retryCount} após {timespan}s");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (exception, duration) =>
            {
                Console.WriteLine($"🔌 Circuit Breaker aberto por {duration}s");
            },
            onReset: () =>
            {
                Console.WriteLine("🔌 Circuit Breaker fechado");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(5, Polly.Timeout.TimeoutStrategy.Optimistic);
}

// Modelos para demonstração
public record Usuario(int Id, string Name, string Email, string Phone);
public record Post(int Id, int UserId, string Title, string Body);

// Serviço com HttpClient tipado
public class UsuarioService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(HttpClient httpClient, ILogger<UsuarioService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Usuario?> ObterUsuarioAsync(int id)
    {
        try
        {
            _logger.LogInformation("Buscando usuário {UserId}", id);
            
            var response = await _httpClient.GetAsync($"users/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var usuario = JsonSerializer.Deserialize<Usuario>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Usuário {UserId} encontrado: {UserName}", id, usuario?.Name);
                return usuario;
            }
            
            _logger.LogWarning("Usuário {UserId} não encontrado. Status: {StatusCode}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário {UserId}", id);
            throw;
        }
    }

    public async Task<List<Post>> ObterPostsDoUsuarioAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Buscando posts do usuário {UserId}", userId);
            
            var response = await _httpClient.GetAsync($"posts?userId={userId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var posts = JsonSerializer.Deserialize<List<Post>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Post>();
            
            _logger.LogInformation("Encontrados {PostCount} posts para o usuário {UserId}", posts.Count, userId);
            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar posts do usuário {UserId}", userId);
            throw;
        }
    }
}

// Serviço que demonstra diferentes usos do HttpClientFactory
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> FazerRequisicaoBasicaAsync()
    {
        var client = _httpClientFactory.CreateClient("BasicClient");
        
        _logger.LogInformation("Fazendo requisição básica");
        var response = await client.GetStringAsync("https://api.github.com/users/github");
        
        return response;
    }

    public async Task<string> FazerRequisicaoComClienteNomeadoAsync()
    {
        var client = _httpClientFactory.CreateClient("JsonPlaceholderClient");
        
        _logger.LogInformation("Fazendo requisição com cliente nomeado");
        var response = await client.GetStringAsync("posts/1");
        
        return response;
    }

    public async Task TestarResilienciaAsync()
    {
        var client = _httpClientFactory.CreateClient("ResilientClient");
        
        try
        {
            _logger.LogInformation("Testando resilência - tentando endpoint que falha");
            
            // Simula endpoint que falha 50% das vezes
            var response = await client.GetAsync("status/500");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Requisição bem-sucedida após retries");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Falha após todas as tentativas de retry");
        }
    }
}

// Serviço principal da demonstração
public class DemoService
{
    private readonly UsuarioService _usuarioService;
    private readonly ApiService _apiService;
    private readonly ILogger<DemoService> _logger;

    public DemoService(UsuarioService usuarioService, ApiService apiService, ILogger<DemoService> logger)
    {
        _usuarioService = usuarioService;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task ExecutarDemonstracaoAsync()
    {
        Console.WriteLine("🔧 1. HttpClient Básico");
        Console.WriteLine("────────────────────────");
        try
        {
            var githubResponse = await _apiService.FazerRequisicaoBasicaAsync();
            var githubUser = JsonSerializer.Deserialize<JsonElement>(githubResponse);
            Console.WriteLine($"✅ GitHub API respondeu: {githubUser.GetProperty("login").GetString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }

        Console.WriteLine("\n📝 2. HttpClient Nomeado");
        Console.WriteLine("─────────────────────────");
        try
        {
            var postResponse = await _apiService.FazerRequisicaoComClienteNomeadoAsync();
            var post = JsonSerializer.Deserialize<Post>(postResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Console.WriteLine($"✅ Post obtido: \"{post?.Title?.Substring(0, Math.Min(50, post.Title?.Length ?? 0))}...\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }

        Console.WriteLine("\n👤 3. HttpClient Tipado (UsuarioService)");
        Console.WriteLine("──────────────────────────────────────────");
        try
        {
            var usuario = await _usuarioService.ObterUsuarioAsync(1);
            if (usuario != null)
            {
                Console.WriteLine($"✅ Usuário: {usuario.Name} ({usuario.Email})");
                
                var posts = await _usuarioService.ObterPostsDoUsuarioAsync(usuario.Id);
                Console.WriteLine($"✅ Posts do usuário: {posts.Count} encontrados");
                
                if (posts.Any())
                {
                    Console.WriteLine($"   Primeiro post: \"{posts.First().Title}\"");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }

        Console.WriteLine("\n🔄 4. Testando Resilência (Polly)");
        Console.WriteLine("──────────────────────────────────");
        await _apiService.TestarResilienciaAsync();

        Console.WriteLine("\n💡 5. Boas Práticas Demonstradas");
        Console.WriteLine("───────────────────────────────────");
        Console.WriteLine("✅ Uso do HttpClientFactory (evita esgotamento de sockets)");
        Console.WriteLine("✅ HttpClient tipado para serviços específicos");
        Console.WriteLine("✅ Configuração centralizada de clientes HTTP");
        Console.WriteLine("✅ Políticas de resilência com Polly (retry, circuit breaker)");
        Console.WriteLine("✅ Logging estruturado para monitoramento");
        Console.WriteLine("✅ Configuração de timeout e headers personalizados");
        Console.WriteLine("✅ Injeção de dependência para testabilidade");

        Console.WriteLine("\n📋 6. Comparação: HttpClient vs HttpClientFactory");
        Console.WriteLine("────────────────────────────────────────────────────");
        
        Console.WriteLine("❌ Problemas do HttpClient tradicional:");
        Console.WriteLine("   • Esgotamento de sockets (socket exhaustion)");
        Console.WriteLine("   • Não respeita mudanças de DNS");
        Console.WriteLine("   • Gestão manual de recursos");
        Console.WriteLine("   • Dificuldade para implementar políticas de retry");

        Console.WriteLine("\n✅ Vantagens do HttpClientFactory:");
        Console.WriteLine("   • Pool de HttpClient instances");
        Console.WriteLine("   • Gestão automática do ciclo de vida");
        Console.WriteLine("   • Integração nativa com DI");
        Console.WriteLine("   • Suporte a políticas de resilência");
        Console.WriteLine("   • Configuração centralizada");
        Console.WriteLine("   • Melhor testabilidade");

        Console.WriteLine("\n🏗️ 7. Cenários de Uso");
        Console.WriteLine("──────────────────────");
        Console.WriteLine("🔹 HttpClient básico: Requisições simples e pontuais");
        Console.WriteLine("🔹 HttpClient nomeado: APIs diferentes com configurações específicas");
        Console.WriteLine("🔹 HttpClient tipado: Serviços dedicados a APIs específicas");
        Console.WriteLine("🔹 Com Polly: APIs externas que podem falhar");

        Console.WriteLine("\n=== Demonstração concluída ===");
    }
}
