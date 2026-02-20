# Dica 34: Chamando APIs com Refit

## 📋 Problema
Chamar APIs REST manualmente com `HttpClient` pode ser verboso e propenso a erros:
- Muito código boilerplate para serialização/deserialização
- URLs e parâmetros construídos manualmente
- Tratamento manual de headers e autenticação
- Falta de type safety nas chamadas de API
- Dificuldade para testar e mockar

## ✅ Solução
Use **Refit** para criar clientes de API tipados de forma declarativa através de interfaces.

## 🔧 O que é Refit?
Refit é uma biblioteca que transforma interfaces C# em clientes REST tipados. Você define contratos de API usando interfaces com atributos, e o Refit gera automaticamente toda a implementação.

## 💡 Implementação

### 1. Instalação
```bash
dotnet add package Refit
dotnet add package Refit.HttpClientFactory
```

### 2. Definindo Contratos de API
```csharp
// Interface para API de usuários
public interface IUserApi
{
    [Get("/users")]
    Task<List<User>> GetUsersAsync();

    [Get("/users/{id}")]
    Task<User> GetUserAsync(int id);

    [Post("/users")]
    Task<User> CreateUserAsync([Body] CreateUserRequest request);

    [Put("/users/{id}")]
    Task<User> UpdateUserAsync(int id, [Body] UpdateUserRequest request);

    [Delete("/users/{id}")]
    Task DeleteUserAsync(int id);
}
```

### 3. Modelos de Dados
```csharp
public record User(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string Name,
    string Email
);

public record UpdateUserRequest(
    string Name,
    string Email,
    bool IsActive
);
```

### 4. Configuração e Uso
```csharp
// Registrar no DI Container
services.AddRefitClient<IUserApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"));

// Usar em serviços
public class UserService
{
    private readonly IUserApi _userApi;

    public UserService(IUserApi userApi)
    {
        _userApi = userApi;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userApi.GetUsersAsync();
    }
}
```

## 🎯 Recursos Avançados

### 1. Query Parameters
```csharp
[Get("/users")]
Task<PagedResult<User>> GetUsersAsync(
    int page = 1,
    int size = 10,
    string search = null,
    [Query] UserFilter filter = null
);
```

### 2. Headers Customizados
```csharp
[Get("/users/{id}")]
Task<User> GetUserAsync(
    int id,
    [Header("Authorization")] string token,
    [Header("X-Custom-Header")] string customValue
);
```

### 3. Autenticação Bearer Token
```csharp
[Get("/protected/data")]
[Headers("Authorization: Bearer")]
Task<SecureData> GetSecureDataAsync([Header("Authorization")] string token);
```

### 4. Multipart Form Data
```csharp
[Multipart]
[Post("/upload")]
Task<UploadResult> UploadFileAsync(
    [AliasAs("file")] StreamPart file,
    [AliasAs("metadata")] string metadata
);
```

### 5. Response Wrapping
```csharp
[Get("/users/{id}")]
Task<ApiResponse<User>> GetUserWithResponseAsync(int id);

// Uso
var response = await userApi.GetUserWithResponseAsync(1);
if (response.IsSuccessStatusCode)
{
    var user = response.Content;
    var headers = response.Headers;
}
```

## 🛠️ Configurações Avançadas

### 1. HttpClientFactory Integration
```csharp
// Configuração completa no Program.cs
builder.Services.AddRefitClient<IUserApi>(new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    })
})
.ConfigureHttpClient(c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
    c.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
```

### 2. Políticas de Retry com Polly
```csharp
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

### 3. Tratamento de Erros
```csharp
try
{
    var user = await userApi.GetUserAsync(999);
}
catch (ApiException ex)
{
    Console.WriteLine($"API Error: {ex.StatusCode}");
    Console.WriteLine($"Content: {ex.Content}");
    
    if (ex.StatusCode == HttpStatusCode.NotFound)
    {
        // Usuário não encontrado
    }
}
```

## 📊 Comparação: HttpClient vs Refit

### HttpClient Manual:
```csharp
public async Task<User> GetUserAsync(int id)
{
    var response = await _httpClient.GetAsync($"/users/{id}");
    
    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(json, _jsonOptions);
    }
    
    throw new HttpRequestException($"Error: {response.StatusCode}");
}
```

### Refit:
```csharp
[Get("/users/{id}")]
Task<User> GetUserAsync(int id);
```

## 🧪 Testabilidade

### 1. Mock para Testes
```csharp
[Test]
public async Task GetUserAsync_ShouldReturnUser()
{
    // Arrange
    var mockUserApi = new Mock<IUserApi>();
    var expectedUser = new User(1, "John", "john@test.com", true, DateTime.Now);
    
    mockUserApi.Setup(x => x.GetUserAsync(1))
              .ReturnsAsync(expectedUser);
    
    var service = new UserService(mockUserApi.Object);
    
    // Act
    var result = await service.GetUserAsync(1);
    
    // Assert
    Assert.Equal(expectedUser, result);
}
```

### 2. Testes de Integração
```csharp
[Test]
public async Task Integration_GetUsers_ShouldReturnData()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddRefitClient<IUserApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"));
    
    var provider = services.BuildServiceProvider();
    var userApi = provider.GetService<IUserApi>();
    
    // Act
    var users = await userApi.GetUsersAsync();
    
    // Assert
    Assert.NotEmpty(users);
}
```

## ⚠️ Melhores Práticas

### 1. ✅ Faça
- Use interfaces específicas por domínio de API
- Configure timeouts apropriados
- Implemente retry policies para resiliência
- Use records para DTOs imutáveis
- Trate erros de API adequadamente

### 2. ❌ Evite
- Interfaces muito grandes com muitos endpoints
- Deixar configurações padrão sem customização
- Ignorar tratamento de erros de rede
- Usar apenas uma interface para toda a API

## 🔗 Recursos Adicionais
- [Documentação oficial Refit](https://github.com/reactiveui/refit)
- [Refit com HttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- [Polly para resilência](https://github.com/App-vNext/Polly)
