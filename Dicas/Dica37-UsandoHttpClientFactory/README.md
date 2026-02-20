# Dica 37: Usando HttpClientFactory

## 📖 Problema

O uso direto do `HttpClient` pode causar sérios problemas de performance e esgotamento de recursos (socket exhaustion), especialmente em aplicações que fazem muitas requisições HTTP.

## ✅ Solução

Use o `HttpClientFactory` para gerenciar instâncias de `HttpClient` de forma eficiente, com pooling automático e integração com injeção de dependência.

## 🎯 Cenários de Uso

### 1. HttpClient Básico
```csharp
services.AddHttpClient("BasicClient");

// Uso
var client = httpClientFactory.CreateClient("BasicClient");
var response = await client.GetStringAsync("https://api.exemplo.com/dados");
```

### 2. HttpClient Nomeado
```csharp
services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://api.exemplo.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "MeuApp/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### 3. HttpClient Tipado
```csharp
services.AddHttpClient<UsuarioService>(client =>
{
    client.BaseAddress = new Uri("https://api.usuarios.com/");
});

public class UsuarioService
{
    private readonly HttpClient _httpClient;
    
    public UsuarioService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Usuario> ObterUsuarioAsync(int id)
    {
        var response = await _httpClient.GetAsync($"usuarios/{id}");
        // processamento...
    }
}
```

## 🔄 Integração com Polly (Resilência)

```csharp
services.AddHttpClient<ApiService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

## 📊 Comparação: HttpClient vs HttpClientFactory

| Aspecto | HttpClient Direto | HttpClientFactory |
|---------|------------------|-------------------|
| **Gestão de Sockets** | Manual (propenso a vazamentos) | Automática |
| **DNS Changes** | Pode não respeitar | Respeita mudanças |
| **Configuração** | Repetitiva | Centralizada |
| **Testabilidade** | Difícil | Fácil |
| **Resilência** | Manual | Integrada (Polly) |
| **Performance** | Pode degradar | Otimizada |

## ⚡ Benefícios

1. **Evita Socket Exhaustion**: Pool automático de conexões
2. **Respeita DNS**: Renovação automática de conexões
3. **Configuração Centralizada**: DI container gerencia tudo
4. **Políticas de Resilência**: Integração nativa com Polly
5. **Melhor Testabilidade**: Fácil mock para testes
6. **Logging Integrado**: Observabilidade nativa

## 🚨 Problemas do HttpClient Tradicional

```csharp
// ❌ EVITE - pode causar esgotamento de sockets
using var client = new HttpClient();
var response = await client.GetAsync("https://api.exemplo.com");

// ❌ EVITE - instância estática não respeita DNS
private static readonly HttpClient _client = new HttpClient();
```

## ✅ Uso Correto com HttpClientFactory

```csharp
// Configuração no Program.cs
builder.Services.AddHttpClient();

// Uso no serviço
public class ApiService
{
    private readonly IHttpClientFactory _factory;
    
    public ApiService(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<string> FazerRequisicaoAsync()
    {
        var client = _factory.CreateClient();
        return await client.GetStringAsync("https://api.exemplo.com");
    }
}
```

## 🎯 Quando Usar Cada Tipo

- **HttpClient Básico**: Requisições simples e pontuais
- **HttpClient Nomeado**: APIs diferentes com configurações específicas
- **HttpClient Tipado**: Serviços dedicados a APIs específicas
- **Com Polly**: APIs externas que podem falhar ou ter latência alta

## 🏗️ Exemplo Prático: Múltiplas APIs

```csharp
// Configuração
services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "MeuApp");
});

services.AddHttpClient("JsonPlaceholder", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

// Uso
var githubClient = factory.CreateClient("GitHub");
var jsonClient = factory.CreateClient("JsonPlaceholder");
```

O `HttpClientFactory` é essencial para aplicações modernas que fazem requisições HTTP, fornecendo performance, resilência e facilidade de manutenção.
