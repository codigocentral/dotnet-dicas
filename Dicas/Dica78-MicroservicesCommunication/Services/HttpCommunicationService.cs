using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dica78.MicroservicesCommunication.Services;

public class HttpCommunicationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpCommunicationService> _logger;

    public HttpCommunicationService(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpCommunicationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task DemonstrateHttpPatterns()
    {
        await DemonstrateBasicHttpCall();
        await DemonstrateResilientHttpCall();
        await DemonstrateParallelCalls();
        await DemonstrateRequestResponse();
    }

    private async Task DemonstrateBasicHttpCall()
    {
        Console.WriteLine("📤 1. Basic HTTP Call");
        Console.WriteLine("---------------------");

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync("users/1");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(content);
                
                Console.WriteLine($"✅ Usuário obtido: {user.GetProperty("name").GetString()}");
                Console.WriteLine($"   Email: {user.GetProperty("email").GetString()}");
            }
            else
            {
                Console.WriteLine($"❌ Falha na requisição: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na chamada HTTP básica");
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task DemonstrateResilientHttpCall()
    {
        Console.WriteLine("🔄 2. Resilient HTTP Call (com retry)");
        Console.WriteLine("--------------------------------------");

        try
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            
            // Simula endpoint que pode falhar
            var response = await client.GetAsync("status/500"); // Retorna 500 para testar retry
            
            Console.WriteLine($"Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Falha após retries: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task DemonstrateParallelCalls()
    {
        Console.WriteLine("⚡ 3. Parallel HTTP Calls");
        Console.WriteLine("-------------------------");

        var userClient = _httpClientFactory.CreateClient("UserService");
        var notificationClient = _httpClientFactory.CreateClient("NotificationService");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Executa múltiplas chamadas em paralelo
            var tasks = new[]
            {
                GetUserAsync(userClient, 1),
                GetUserAsync(userClient, 2),
                GetUserAsync(userClient, 3),
                GetNotificationAsync(notificationClient)
            };

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            Console.WriteLine($"✅ {results.Length} chamadas concluídas em {stopwatch.ElapsedMilliseconds}ms");
            
            foreach (var result in results.Where(r => r != null))
            {
                Console.WriteLine($"   📦 {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro nas chamadas paralelas: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task DemonstrateRequestResponse()
    {
        Console.WriteLine("📨 4. Request/Response with Timeout");
        Console.WriteLine("-----------------------------------");

        try
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            
            // POST com timeout
            var requestData = new { userId = 1, productId = 123, quantity = 2 };
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await client.PostAsync("post", content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Pedido criado com sucesso");
                Console.WriteLine($"   Response Length: {responseContent.Length} chars");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("⏰ Request cancelado por timeout");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na requisição: {ex.Message}");
        }

        Console.WriteLine();
    }

    private async Task<string?> GetUserAsync(HttpClient client, int userId)
    {
        try
        {
            var response = await client.GetAsync($"users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(content);
                return $"User {userId}: {user.GetProperty("name").GetString()}";
            }
        }
        catch
        {
            // Ignora erros em chamadas paralelas para demo
        }
        return null;
    }

    private async Task<string?> GetNotificationAsync(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync("users?page=1");
            if (response.IsSuccessStatusCode)
            {
                return "Notifications: Service available";
            }
        }
        catch
        {
            // Ignora erros em chamadas paralelas para demo
        }
        return null;
    }
}
