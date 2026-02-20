# Dica 32: Usando HttpClient Corretamente

## 📋 Problema

Muitos desenvolvedores cometem erros comuns ao usar `HttpClient` que podem levar a:

1. **Socket Exhaustion**: Criando um novo `HttpClient` a cada requisição
2. **Problemas de DNS**: Reutilizando um `HttpClient` estático que não atualiza DNS
3. **Performance Issues**: Não aproveitando conexões reutilizáveis

## ❌ Prática Incorreta

```csharp
// ❌ RUIM: Criar HttpClient a cada requisição
public async Task<string> GetDataAsync()
{
    using var client = new HttpClient(); // Socket exhaustion!
    return await client.GetStringAsync("https://api.example.com/data");
}

// ❌ RUIM: HttpClient estático (DNS não atualiza)
private static readonly HttpClient _client = new HttpClient();
```

## ✅ Soluções Corretas

### Solução 1: HttpClient de Longa Duração com PooledConnectionLifetime

```csharp
private static readonly HttpClient _client = new HttpClient(new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Resolve problema de DNS
});
```

### Solução 2: HttpClientFactory (Recomendado)

```csharp
// Em Startup.cs ou Program.cs
services.AddHttpClient();

// No seu serviço
public class ApiService
{
    private readonly HttpClient _httpClient;
    
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
```

## 🎯 Pontos-Chave

- ✅ Use `IHttpClientFactory` para criar instâncias
- ✅ Configure `PooledConnectionLifetime` para clientes de longa duração
- ✅ Reutilize `HttpMessageHandler` 
- ❌ Nunca crie `HttpClient` a cada requisição
- ❌ Evite `HttpClient` estático sem configuração DNS

## 🏃‍♂️ Como Executar

```bash
cd Dicas/Dica32-UsandoHttpClientCorretamente/Dica32.UsandoHttpClientCorretamente
dotnet run
```

## 📊 Comparação de Performance

O projeto demonstra:
- Diferenças de performance entre as abordagens
- Monitoramento de sockets em uso
- Impacto na resolução DNS
- Melhores práticas com HttpClientFactory
