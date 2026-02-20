# Dica 66: Rate Limiting em ASP.NET Core

## 📋 Sobre

Esta dica demonstra como implementar **Rate Limiting** (limitação de taxa) em ASP.NET Core, uma funcionalidade essencial para proteger APIs contra abuso, garantir fair use de recursos e melhorar a performance e disponibilidade do sistema.

## 🎯 Conceitos Demonstrados

### 1. **Estratégias de Rate Limiting**
- **Fixed Window**: Janela fixa de tempo com limite de requisições
- **Sliding Window**: Janela deslizante mais suave que fixed window
- **Token Bucket**: Sistema de tokens que permite rajadas controladas
- **Concurrency Limiter**: Controla número de operações simultâneas

### 2. **Políticas Implementadas**
- **PerIP**: Limite por endereço IP (Sliding Window)
- **PerUser**: Limite por usuário autenticado (Token Bucket)
- **PerTier**: Limite baseado no tier do usuário (Fixed Window)
- **ConcurrentOperations**: Limite de operações simultâneas
- **Custom**: Rate limiting customizado via middleware

### 3. **Casos de Uso**
- Proteção contra ataques DDoS
- Fair use de recursos entre usuários
- Monetização baseada em uso (tiers)
- Controle de operações pesadas
- Proteção de recursos críticos

## 🏗️ Estrutura do Projeto

```
Dica66-RateLimiting/
├── Controllers/
│   ├── WeatherController.cs      # Endpoints com diferentes políticas
│   ├── ResourceController.cs     # Operações pesadas e customizadas
│   └── DemoController.cs         # Demonstrações e testes
├── Services/
│   └── RateLimitPolicyService.cs # Configuração das políticas
├── Middleware/
│   └── RateLimitMiddleware.cs    # Middleware customizado
├── Models/
│   └── ApiResponse.cs            # Modelos de resposta
└── Program.cs                    # Configuração da aplicação
```

## 🚀 Funcionalidades

### **1. Política Per IP (Sliding Window)**
```csharp
[EnableRateLimiting("PerIP")]
public ActionResult<ApiResponse<IEnumerable<WeatherForecast>>> GetWeatherForecast()
```
- **Limite**: 20 requisições por minuto
- **Tipo**: Sliding Window com 4 segmentos
- **Partição**: Por endereço IP
- **Uso**: Proteção geral contra abuso

### **2. Política Per User (Token Bucket)**
```csharp
[EnableRateLimiting("PerUser")]
public async Task<ActionResult<ApiResponse<WeatherForecast>>> GetDetailedWeatherForecast()
```
- **Limite**: 50 tokens iniciais
- **Reposição**: 10 tokens a cada 30 segundos
- **Partição**: Por usuário autenticado
- **Uso**: Operações que consomem mais recursos

### **3. Política Per Tier (Fixed Window)**
```csharp
[EnableRateLimiting("PerTier")]
public ActionResult<ApiResponse<object>> GetPremiumWeatherData()
```
- **Free**: 10 requisições/minuto
- **Premium**: 100 requisições/minuto
- **Enterprise**: 1000 requisições/minuto
- **Uso**: Monetização e diferenciação de serviços

### **4. Política de Concorrência**
```csharp
[EnableRateLimiting("ConcurrentOperations")]
public async Task<ActionResult<ApiResponse<string>>> ProcessHeavyOperation()
```
- **Limite**: 5 operações simultâneas
- **Fila**: Até 10 requisições aguardando
- **Uso**: Operações que consomem muitos recursos

### **5. Rate Limiting Customizado**
```csharp
public class CustomRateLimitMiddleware
{
    // Implementação própria para casos específicos
    private static bool CheckRateLimit(string clientId, int limit, TimeSpan window)
}
```
- **Limite**: 5 requisições por minuto para endpoints `/api/custom*`
- **Implementação**: Middleware customizado
- **Uso**: Casos específicos que necessitam lógica própria

## 📊 Configuração das Políticas

### **RateLimitPolicyService.cs**
```csharp
services.AddRateLimiter(limiterOptions =>
{
    // Política Global
    limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Políticas específicas...
});
```

### **Headers de Resposta**
```json
{
  "X-RateLimit-Policy": "PerIP",
  "X-RateLimit-Limit": "20",
  "X-RateLimit-Remaining": "15",
  "X-RateLimit-Reset": "2025-07-10T23:56:34.000Z",
  "Retry-After": "60"
}
```

## 🧪 Como Testar

### **1. Executar a Aplicação**
```bash
dotnet run
```
Acesse: http://localhost:5000

### **2. Testar Rate Limit por IP**
```bash
# Fazer múltiplas requisições rapidamente
for i in {1..25}; do 
  curl -s http://localhost:5000/api/weather | jq '.rateLimit'
done
```

### **3. Testar Rate Limit por Tier**
```bash
# Tier Free (10 req/min)
curl -H "X-User-Tier: Free" http://localhost:5000/api/weather/premium

# Tier Premium (100 req/min)
curl -H "X-User-Tier: Premium" http://localhost:5000/api/weather/premium

# Tier Enterprise (1000 req/min)
curl -H "X-User-Tier: Enterprise" http://localhost:5000/api/weather/premium
```

### **4. Testar Rate Limit Customizado**
```bash
# Limite de 5 req/min para endpoints custom
for i in {1..8}; do 
  curl -w "\nStatus: %{http_code}\n" http://localhost:5000/api/resource/custom-limit
done
```

### **5. Testar Operações Concorrentes**
```bash
# Enviar múltiplas requisições simultâneas
for i in {1..10}; do 
  curl -X POST -H "Content-Type: application/json" \
       -d '"test data"' \
       http://localhost:5000/api/resource/heavy-operation &
done
wait
```

## 📈 Endpoints de Demonstração

### **Cenários Disponíveis**
- `GET /api/demo/scenarios` - Lista todos os cenários de teste
- `GET /api/demo/status` - Status das políticas ativas
- `GET /api/demo/test/{count}` - Endpoint para teste rápido

### **Monitoramento**
- `GET /health` - Health check da aplicação
- `GET /api/policies` - Políticas disponíveis
- Headers de resposta com informações de rate limiting

## 🔧 Configuração Avançada

### **1. Resposta Personalizada para Rate Limit Excedido**
```csharp
limiterOptions.OnRejected = async (context, token) =>
{
    context.HttpContext.Response.StatusCode = 429;
    
    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
    {
        context.HttpContext.Response.Headers.RetryAfter = 
            ((int)retryAfter.TotalSeconds).ToString();
    }

    await context.HttpContext.Response.WriteAsync("""
        {
            "success": false,
            "message": "Rate limit exceeded. Too many requests.",
            "statusCode": 429,
            "timestamp": "{DateTime.UtcNow:O}"
        }
        """, cancellationToken: token);
};
```

### **2. Rate Limiting Dinâmico**
```csharp
limiterOptions.AddPolicy("PerTier", httpContext =>
{
    var userTier = GetUserTier(httpContext);
    
    return userTier switch
    {
        "Free" => RateLimitPartition.GetFixedWindowLimiter(...),
        "Premium" => RateLimitPartition.GetFixedWindowLimiter(...),
        "Enterprise" => RateLimitPartition.GetFixedWindowLimiter(...),
        _ => RateLimitPartition.GetFixedWindowLimiter(...)
    };
});
```

## 🎯 Casos de Uso Reais

### **1. API Pública**
- Rate limit por API key
- Diferentes tiers de serviço
- Proteção contra abuso

### **2. Microserviços**
- Rate limit entre serviços
- Controle de operações custosas
- Circuit breaker patterns

### **3. E-commerce**
- Rate limit em checkout
- Proteção contra bots
- Fair use durante promoções

### **4. IoT e Telemetria**
- Rate limit por device
- Controle de ingesta de dados
- Balanceamento de carga

## 📚 Conceitos Importantes

### **Fixed Window vs Sliding Window**
- **Fixed Window**: Mais simples, pode ter "thundering herd"
- **Sliding Window**: Mais suave, distribui melhor a carga

### **Token Bucket**
- Permite rajadas controladas
- Ideal para operações que podem ser feitas em lote
- Mais flexível que windows fixas

### **Concurrency Limiter**
- Controla operações simultâneas
- Ideal para operações I/O intensivas
- Previne esgotamento de recursos

### **Particionamento**
- Por IP, usuário, API key, etc.
- Define como as requisições são agrupadas
- Crucial para efetividade do rate limiting

## ⚠️ Considerações de Produção

### **1. Armazenamento de Estado**
- Em produção, use Redis ou similar
- Rate limiting distribuído
- Estado compartilhado entre instâncias

### **2. Performance**
- Rate limiting deve ser rápido
- Cache de configurações
- Monitoramento de overhead

### **3. Observabilidade**
- Métricas de rate limiting
- Alertas para limites excedidos
- Logs para análise de padrões

### **4. Configuração**
- Limites configuráveis por ambiente
- A/B testing de políticas
- Ajustes baseados em métricas

## 🔗 Recursos Adicionais

- [ASP.NET Core Rate Limiting](https://docs.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [System.Threading.RateLimiting](https://docs.microsoft.com/en-us/dotnet/api/system.threading.ratelimiting)
- [Rate Limiting Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/throttling)

## 🎉 Conclusão

Rate Limiting é uma funcionalidade essencial para APIs modernas, permitindo:

- **Proteção**: Contra abuso e ataques
- **Performance**: Controle de recursos e carga
- **Monetização**: Diferenciação de tiers de serviço
- **Disponibilidade**: Garantia de fair use

Esta implementação demonstra as principais estratégias e casos de uso, fornecendo uma base sólida para implementação em projetos reais.
