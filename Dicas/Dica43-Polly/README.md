# 🛡️ Dica 43: Polly - Padrões de Resiliência em .NET

## 📋 Visão Geral

Esta dica demonstra como usar a biblioteca **Polly** para implementar padrões de resiliência em aplicações .NET, incluindo:

- ✅ **Retry Policy** - Tentativas automáticas com Exponential Backoff
- ✅ **Circuit Breaker** - Proteção contra falhas em cascata
- ✅ **Timeout Policy** - Controle de tempo limite
- ✅ **Bulkhead Policy** - Isolamento de recursos e controle de paralelismo
- ✅ **Fallback Policy** - Respostas alternativas quando falhas ocorrem
- ✅ **Políticas Combinadas** - Composição de múltiplas estratégias
- ✅ **HttpClientFactory Integration** - Integração nativa com HttpClient

## 🎯 Objetivos de Aprendizado

### **1. Padrões de Resiliência Fundamentais**
- Entender quando e como usar cada política
- Configurar políticas appropriadas para diferentes cenários
- Combinar múltiplas políticas efetivamente

### **2. Configuração e Integração**
- Configurar Polly com HttpClientFactory
- Usar Dependency Injection para gerenciar políticas
- Configurar políticas via appsettings.json

### **3. Monitoramento e Observabilidade**
- Implementar logging detalhado para políticas
- Medir performance e latência
- Monitorar métricas de resiliência

## 🏗️ Estrutura do Projeto

```
Dica43-Polly/
├── Configuration/
│   └── Settings.cs              # Configurações tipadas para políticas
├── Models/
│   └── Models.cs                # DTOs e modelos de resultado
├── Services/
│   ├── ExternalApiService.cs    # Demonstra retry, circuit breaker, timeout
│   ├── PaymentService.cs        # Demonstra bulkhead e políticas avançadas
│   └── PollyDemoHostedService.cs # Orquestra todas as demonstrações
├── Program.cs                   # Setup DI e HttpClientFactory
├── appsettings.json             # Configurações das políticas
└── README.md                    # Documentação completa
```

## ⚡ Principais Características

### **🔄 Retry Policy com Exponential Backoff**
```csharp
var retryPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .Or<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            logger.LogWarning("Retry {Count} em {Delay}s", retryCount, timespan.TotalSeconds);
        });
```

### **⚡ Circuit Breaker Pattern**
```csharp
var circuitBreakerPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3, // Falhas consecutivas
        durationOfBreak: TimeSpan.FromSeconds(30), // Tempo aberto
        onBreak: (exception, timespan) => logger.LogError("Circuit Breaker ABERTO"),
        onReset: () => logger.LogInformation("Circuit Breaker FECHADO"));
```

### **🚧 Bulkhead Policy (Controle de Paralelismo)**
```csharp
var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 10,        // Máximo 10 operações simultâneas
    maxQueuingActions: 20,         // Máximo 20 na fila
    onBulkheadRejectedAsync: (context) =>
    {
        logger.LogWarning("Bulkhead: Requisição rejeitada");
        return Task.CompletedTask;
    });
```

### **🔄 Fallback Policy**
```csharp
var fallbackPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .FallbackAsync(
        fallbackAction: async (context, cancellationToken) =>
        {
            logger.LogWarning("Executando fallback");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"fallback\": true}")
            };
        });
```

## 🔧 Configurações

### **appsettings.json**
```json
{
  "ExternalApi": {
    "BaseUrl": "https://jsonplaceholder.typicode.com",
    "Timeout": "00:00:10",
    "MaxRetries": 3
  },
  "CircuitBreaker": {
    "HandledEventsAllowedBeforeBreaking": 3,
    "DurationOfBreak": "00:00:30",
    "SamplingDuration": "00:01:00",
    "MinimumThroughput": 5,
    "FailureThreshold": 0.5
  },
  "Bulkhead": {
    "MaxParallelization": 10,
    "MaxQueuingActions": 20
  }
}
```

### **HttpClientFactory Integration**
```csharp
services.AddHttpClient<ExternalApiService>("ExternalApi", client =>
{
    client.BaseAddress = new Uri("https://api.exemplo.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(GetTimeoutPolicy());
```

## 📊 Funcionalidades Demonstradas

### **1. Retry com Exponential Backoff e Jitter**
- Tentativas automáticas para falhas transitórias
- Backoff exponencial para evitar sobrecarga
- Jitter para evitar thundering herd effect

### **2. Circuit Breaker Inteligente**
- Monitoramento de health do serviço
- Abertura automática em falhas consecutivas
- Teste de recuperação (half-open state)

### **3. Timeout Configurável**
- Timeouts diferentes por tipo de operação
- Cancellation automático de operações longas
- Estratégias pessimistic e optimistic

### **4. Bulkhead para Isolamento**
- Controle de recursos compartilhados
- Prevenção de resource starvation
- Isolamento entre diferentes operações

### **5. Fallback Graceful**
- Respostas alternativas em falhas
- Manutenção de funcionalidade degradada
- Cache ou dados padrão como fallback

### **6. Medição de Performance**
- Latência média das operações
- Taxa de sucesso/falha
- Throughput e métricas de resiliência

## 🎮 Como Executar

```bash
# Clone ou navegue até o diretório
cd Dicas/Dica43-Polly

# Restaure as dependências
dotnet restore

# Execute a aplicação
dotnet run
```

## 📋 Saída Esperada

A aplicação demonstra:

1. **Retry Policy**: Tentativas automáticas com backoff exponencial
2. **Circuit Breaker**: Simulação de falhas para abrir/fechar o circuit
3. **Política Combinada**: Timeout + Circuit Breaker + Retry
4. **Fallback**: Resposta alternativa quando tudo falha
5. **Bulkhead**: Controle de paralelismo em operações simultâneas
6. **Performance**: Métricas de latência e throughput

## 🌟 Padrões e Boas Práticas

### **📌 Quando Usar Cada Política**

| Política | Cenário | Exemplo |
|----------|---------|---------|
| **Retry** | Falhas transitórias | Network timeouts, HTTP 5xx |
| **Circuit Breaker** | Serviços instáveis | APIs externas, databases |
| **Timeout** | Operações longas | HTTP requests, queries |
| **Bulkhead** | Isolamento de recursos | Thread pools, connections |
| **Fallback** | Degradação graceful | Cache, dados padrão |

### **🛡️ Configurações Recomendadas**

```csharp
// Para APIs externas críticas
var comprehensivePolicy = Policy.WrapAsync(
    Policy.FallbackAsync<HttpResponseMessage>(/* fallback */),
    Policy.HandleTransientHttpError()
          .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)),
    Policy.HandleTransientHttpError()
          .WaitAndRetryAsync(3, retryAttempt => 
              TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
    Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10))
);
```

### **⚠️ Cuidados Importantes**

- ❌ **Não use retry para erros 4xx** (client errors)
- ❌ **Não configure timeouts muito baixos** (cause false failures)
- ❌ **Não abuse de circuit breakers** (podem mascarar problemas)
- ❌ **Não esqueça de monitorar** (logs e métricas)
- ❌ **Não teste apenas cenários de sucesso** (teste falhas também)

### **✅ Melhores Práticas**

1. **Configure políticas via DI e Configuration**
2. **Use HttpClientFactory com Polly**
3. **Implemente logging detalhado**
4. **Monitore métricas de resiliência**
5. **Teste cenários de falha regularmente**
6. **Documente comportamento esperado**
7. **Use jitter em retry policies**
8. **Combine políticas apropriadamente**

## 📚 Conceitos Avançados

### **Policy Wrapping Order**
A ordem das políticas importa:
```csharp
// Ordem correta: outer → inner
Policy.WrapAsync(
    fallbackPolicy,     // Mais externa
    circuitBreakerPolicy,
    retryPolicy,        // Mais interna
    timeoutPolicy
);
```

### **Context Sharing**
```csharp
var context = new Context("operation-key");
context["userId"] = "123";
context["operationType"] = "payment";

await policy.ExecuteAsync(async (ctx) =>
{
    var userId = ctx["userId"];
    // Use context data
}, context);
```

### **Custom Policies**
```csharp
var customPolicy = Policy
    .Handle<CustomException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            // Custom retry logic
        });
```

## 🔗 Recursos Adicionais

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Microsoft.Extensions.Http.Polly](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#polly-based-handlers)
- [Polly Best Practices](https://github.com/App-vNext/Polly/wiki)
- [Circuit Breaker Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)

---

💡 **Esta dica demonstra como construir aplicações resilientes que gracefully degradam em cenários de falha, mantendo a melhor experiência possível para o usuário final!**
