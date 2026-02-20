# 🎯 Dica 57: Message Queues - RabbitMQ e Azure Service Bus

## 📋 Visão Geral

Message Queues são fundamentais para arquiteturas distribuídas, fornecendo comunicação assíncrona confiável entre serviços. Esta demonstração explora RabbitMQ, Azure Service Bus e padrões de messaging.

## 🎯 Conceitos Fundamentais

### 🔄 **Padrões de Messaging**
- **Fire-and-Forget**: Envia mensagem sem aguardar resposta
- **Request-Reply**: Comunicação síncrona sobre infraestrutura assíncrona
- **Publish-Subscribe**: Um para muitos (broadcast)
- **Event Sourcing**: Armazenamento de eventos para reconstrução de estado

### 📨 **Tipos de Mensagem**
- **Commands**: Ações a serem executadas
- **Events**: Notificações do que aconteceu
- **Queries**: Solicitações de informação
- **Documents**: Transferência de dados

## 🐰 RabbitMQ Patterns

### 1. **Work Queues (Task Distribution)**
```csharp
// Múltiplos workers processam tarefas de uma fila
_fallbackBroker.Subscribe<OrderMessage>("work.orders", async order =>
{
    var workerId = GetNextWorkerId();
    Console.WriteLine($"Worker {workerId}: Processing order {order.OrderId}");
    await ProcessOrder(order);
});
```

### 2. **Publish/Subscribe (Fanout Exchange)**
```csharp
// Mensagem vai para todos os subscribers
_broker.Subscribe<UserEvent>("user.events", EmailService.SendWelcome);
_broker.Subscribe<UserEvent>("user.events", AnalyticsService.TrackUser);
_broker.Subscribe<UserEvent>("user.events", NotificationService.CreateProfile);
```

### 3. **Routing (Direct Exchange)**
```csharp
// Roteamento baseado em routing key
var topic = notification.Priority switch
{
    Priority.Critical => "notifications.high",
    Priority.Normal => "notifications.normal",
    Priority.Low => "notifications.low"
};
await _broker.PublishAsync(topic, notification);
```

### 4. **Topics (Topic Exchange)**
```csharp
// Patterns com wildcards (* = uma palavra, # = zero ou mais)
_broker.Subscribe("metrics.cpu.*", CpuMetricsHandler);
_broker.Subscribe("metrics.*.critical", CriticalAlertsHandler);
_broker.Subscribe("metrics.#", AllMetricsCollector);
```

### 5. **RPC (Request/Reply)**
```csharp
var request = new OrderCalculation
{
    OrderId = 123,
    CorrelationId = Guid.NewGuid().ToString(),
    ReplyTo = $"calculations.reply.{correlationId}"
};

await _broker.PublishAsync("calculations.request", request);
// Handler responde no tópico ReplyTo com mesmo CorrelationId
```

## ☁️ Azure Service Bus Patterns

### 1. **Queues (FIFO)**
```csharp
// Processamento sequencial garantido
await serviceBusClient.CreateSender("orders")
    .SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(order)));
```

### 2. **Topics and Subscriptions**
```csharp
// Topic com múltiplas subscriptions filtradas
await sender.SendMessageAsync(new ServiceBusMessage(data)
{
    Subject = "order.created",
    ApplicationProperties = { ["orderValue"] = order.TotalAmount }
});

// Subscription com filtro SQL: orderValue > 1000
```

### 3. **Dead Letter Queue**
```csharp
// Mensagens que falharam após tentativas vão para DLQ
var options = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 1,
    AutoCompleteMessages = false,
    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
};

processor.ProcessMessageAsync += async args =>
{
    try
    {
        await ProcessMessage(args.Message);
        await args.CompleteMessageAsync(args.Message);
    }
    catch (Exception)
    {
        await args.AbandonMessageAsync(args.Message);
        // Após MaxDeliveryCount, vai para Dead Letter
    }
};
```

### 4. **Scheduled Messages**
```csharp
// Agendamento de mensagens para entrega futura
var message = new ServiceBusMessage(data)
{
    ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddHours(1)
};
await sender.SendMessageAsync(message);
```

### 5. **Message Sessions**
```csharp
// Processamento sequencial por sessão (SessionId)
var message = new ServiceBusMessage(data)
{
    SessionId = customerId // Garante ordem por cliente
};

var sessionProcessor = client.CreateSessionProcessor("orders");
sessionProcessor.ProcessSessionMessageAsync += async args =>
{
    // Mensagens da mesma sessão processadas em ordem
    await ProcessInOrder(args.Message);
};
```

## 💾 In-Memory Message Broker

### **Implementação Básica**
```csharp
public class InMemoryMessageBroker
{
    private readonly ConcurrentDictionary<string, List<Func<BaseMessage, Task>>> _subscribers = new();
    private readonly ConcurrentQueue<(string Topic, BaseMessage Message)> _messageQueue = new();

    public async Task PublishAsync<T>(string topic, T message) where T : BaseMessage
    {
        _messageQueue.Enqueue((topic, message));
        // Background processor pega da fila e distribui
    }

    public void Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMessage
    {
        _subscribers.AddOrUpdate(topic, [handler], (key, existing) => 
        {
            existing.Add(handler);
            return existing;
        });
    }
}
```

## 🎯 Padrões de Implementação

### 1. **Message Models**
```csharp
public class BaseMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
    public string ReplyTo { get; set; } = string.Empty;
}

public class OrderMessage : BaseMessage
{
    public int OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Created";
}
```

### 2. **Error Handling e Retry**
```csharp
public async Task<MessageProcessingResult> ProcessWithRetry<T>(T message, Func<T, Task> handler)
{
    var attempts = 0;
    var maxAttempts = 3;
    
    while (attempts < maxAttempts)
    {
        try
        {
            await handler(message);
            return new MessageProcessingResult { Success = true };
        }
        catch (Exception ex)
        {
            attempts++;
            if (attempts >= maxAttempts)
            {
                // Move to Dead Letter Queue
                await SendToDeadLetter(message, ex);
                return new MessageProcessingResult { Success = false, Error = ex.Message };
            }
            
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts))); // Exponential backoff
        }
    }
}
```

### 3. **Correlation e Request/Reply**
```csharp
public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string requestTopic, TRequest request)
    where TRequest : BaseMessage
    where TResponse : BaseMessage
{
    var correlationId = Guid.NewGuid().ToString();
    var replyTopic = $"{requestTopic}.reply.{correlationId}";
    var responseReceived = false;
    TResponse response = null;

    // Subscribe to reply
    _broker.Subscribe<TResponse>(replyTopic, async reply =>
    {
        if (reply.CorrelationId == correlationId)
        {
            response = reply;
            responseReceived = true;
        }
    });

    // Send request
    request.CorrelationId = correlationId;
    request.ReplyTo = replyTopic;
    await _broker.PublishAsync(requestTopic, request);

    // Wait for response with timeout
    var timeout = DateTime.UtcNow.AddSeconds(30);
    while (!responseReceived && DateTime.UtcNow < timeout)
    {
        await Task.Delay(100);
    }

    return response ?? throw new TimeoutException("Request timeout");
}
```

## 📊 Componentes da Demonstração

### **Services**
- `InMemoryMessageBroker`: Message broker em memória para demonstrações
- `RabbitMQService`: Simulação de padrões RabbitMQ
- `AzureServiceBusService`: Simulação de padrões Service Bus
- `MessageQueueDemoService`: Orquestrador das demonstrações

### **Models**
- `BaseMessage`: Classe base com metadados comuns
- `OrderMessage`: Mensagens de pedidos
- `NotificationMessage`: Mensagens de notificação
- `UserEventMessage`: Eventos de usuário
- `PaymentResponseMessage`: Respostas de pagamento
- `SystemMetricMessage`: Métricas de sistema

## 🔧 Configurações de Produção

### **RabbitMQ**
```csharp
var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    RequestedHeartbeat = TimeSpan.FromSeconds(60),
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};
```

### **Azure Service Bus**
```csharp
var client = new ServiceBusClient(connectionString);
var processor = client.CreateProcessor("orders", new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 10,
    AutoCompleteMessages = false,
    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
    ReceiveMode = ServiceBusReceiveMode.PeekLock
});
```

## 💡 Best Practices

### ✅ **Design**
- Use mensagens idempotentes
- Implemente Dead Letter Queues
- Inclua metadados (correlation ID, timestamps)
- Design para eventual consistency
- Valide e sanitize message content

### ⚡ **Performance**
- Faça batch de mensagens quando possível
- Use tamanhos apropriados de mensagem
- Configure timeouts adequados
- Monitore profundidade das filas
- Implemente circuit breakers

### 🔒 **Segurança**
- Criptografe dados sensíveis
- Use autenticação e autorização
- Implemente controles de acesso
- Faça auditoria dos fluxos de mensagem
- Monitore por atividades suspeitas

### 🔄 **Confiabilidade**
- Use filas duráveis para mensagens críticas
- Implemente políticas de retry com exponential backoff
- Trate mensagens duplicadas graciosamente
- Monitore e alerte sobre saúde das filas
- Planeje para disaster recovery

## 📈 Comparação de Performance

| Message Broker | Throughput | Latency | Features | Melhor Para |
|---------------|------------|---------|----------|-------------|
| **RabbitMQ** | 10K-100K+ msg/s | 1-5ms | Exchanges, routing, clustering | High-throughput, routing complexo |
| **Azure Service Bus** | 1K-10K msg/s | 10-50ms | Topics, sessions, DLQ, scheduling | Enterprise integration, .NET |
| **Apache Kafka** | 100K-1M+ msg/s | Sub-ms | Streaming, partitioning, replay | Event streaming, big data |
| **Redis/Memory** | 1M+ msg/s | μs | Fast, simple | Caching, real-time |

## 🎯 Quando Usar Cada Padrão

### 📦 **Simple Queuing**
- **Tool**: RabbitMQ, Azure Service Bus
- **Use Case**: Task distribution, background jobs
- **Example**: Order processing, email sending

### 🏢 **Enterprise Integration**
- **Tool**: Azure Service Bus, IBM MQ
- **Use Case**: Complex routing, transactions
- **Example**: ERP integration, financial systems

### 📊 **Event Streaming**
- **Tool**: Apache Kafka, Azure Event Hubs
- **Use Case**: Real-time analytics, event sourcing
- **Example**: User behavior tracking, IoT data

### ⚡ **Ultra-Low Latency**
- **Tool**: Redis Streams, In-memory
- **Use Case**: Real-time gaming, financial trading
- **Example**: Live chat, price updates

### 🔄 **Request/Reply**
- **Tool**: Any with correlation IDs
- **Use Case**: Synchronous-like over async infrastructure
- **Example**: API gateways, microservice communication

### 📡 **Publish/Subscribe**
- **Tool**: RabbitMQ, Service Bus, Kafka
- **Use Case**: Event notifications, broadcasting
- **Example**: User registration events, system alerts

---

**Esta demonstração ilustra os principais padrões de Message Queues usando implementações simuladas que podem ser facilmente adaptadas para usar brokers reais em produção.**
