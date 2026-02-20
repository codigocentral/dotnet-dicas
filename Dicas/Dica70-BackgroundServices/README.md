# Dica 70 - Background Services

## 📋 Sobre

Esta dica demonstra como implementar e usar **Background Services** em .NET para executar tarefas em segundo plano de forma eficiente e robusta.

## 🎯 Conceitos Demonstrados

### 1. **Tipos de Background Services**
- **TimedBackgroundService**: Executa tarefas em intervalos regulares
- **QueueBackgroundService**: Processa itens de filas
- **BatchProcessingService**: Processa dados em lotes
- **HealthMonitoringService**: Monitora saúde do sistema
- **SingletonService**: Garante execução única

### 2. **Padrões de Implementação**
- **Timer-based**: Usando `Task.Delay()` e `CancellationToken`
- **Queue-based**: Producer/Consumer com `Channel<T>`
- **Priority Queue**: Processamento baseado em prioridades
- **Batch Processing**: Processamento em lotes para performance
- **Health Monitoring**: Verificações automáticas de saúde

### 3. **Funcionalidades Avançadas**
- **Retry Mechanisms**: Tentativas automáticas em caso de falha
- **Graceful Shutdown**: Finalização elegante com `CancellationToken`
- **Job Tracking**: Rastreamento de status e métricas
- **Service Monitoring**: Monitoramento em tempo real

## 🚀 Funcionalidades

### Background Services

1. **Timed Background Service**
   ```csharp
   // Executa limpeza automática a cada 5 minutos
   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
       while (!stoppingToken.IsCancellationRequested)
       {
           await DoWorkAsync(stoppingToken);
           await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
       }
   }
   ```

2. **Email Background Service**
   ```csharp
   // Processa emails com diferentes prioridades
   private async Task ProcessEmailAsync(QueueItem<EmailNotification> queueItem)
   {
       var processingTime = queueItem.Data.Priority switch
       {
           EmailPriority.Critical => 500,
           EmailPriority.High => 1000,
           EmailPriority.Normal => 2000,
           EmailPriority.Low => 3000
       };
   }
   ```

3. **Data Processing Service**
   ```csharp
   // Processa até 3 jobs em paralelo
   var semaphore = new SemaphoreSlim(3, 3);
   var task = ProcessNextJobAsync(semaphore, stoppingToken);
   ```

### Sistemas de Fila

1. **Channel-based Queue** (FIFO)
   ```csharp
   public class ChannelQueueService<T> : IQueueService<T>
   {
       private readonly Channel<QueueItem<T>> _channel;
       
       public async Task EnqueueAsync(QueueItem<T> item)
       {
           await _writer.WriteAsync(item);
       }
   }
   ```

2. **Priority Queue**
   ```csharp
   public class PriorityQueueService<T> : IQueueService<T>
   {
       private readonly PriorityQueue<QueueItem<T>, int> _queue = new();
       
       // Itens de maior prioridade são processados primeiro
   }
   ```

### Monitoramento e Health Checks

1. **Health Monitoring**
   ```csharp
   private async Task PerformHealthChecksAsync()
   {
       var checks = new[]
       {
           CheckDatabaseHealthAsync(),
           CheckExternalApiHealthAsync(),
           CheckMemoryUsageAsync(),
           CheckDiskSpaceAsync()
       };
   }
   ```

2. **Job Tracking**
   ```csharp
   public async Task<string> CreateJobAsync(string name, string description)
   {
       var job = new JobInfo
       {
           Name = name,
           Description = description,
           Status = JobStatus.Pending
       };
   }
   ```

## 📡 API Endpoints

### Jobs
- `GET /api/jobs` - Lista jobs com filtros
- `GET /api/jobs/{id}` - Obtém job específico
- `GET /api/jobs/statistics` - Estatísticas dos jobs

### Queue
- `POST /api/queue/email` - Adiciona email à fila
- `POST /api/queue/data-processing` - Adiciona job de processamento
- `POST /api/queue/email/test-batch` - Adiciona emails de teste
- `GET /api/queue/status` - Status das filas

### Monitoring
- `GET /api/monitoring/services` - Status dos serviços
- `GET /api/monitoring/health` - Health checks recentes
- `GET /api/monitoring/dashboard` - Dashboard completo
- `GET /api/monitoring/singleton/status` - Status do singleton service

## 🛠️ Como Testar

### 1. **Iniciar a Aplicação**
```bash
cd Dica70-BackgroundServices
dotnet run
```

### 2. **Acessar Swagger**
- Abra: `http://localhost:5000`
- Swagger UI com documentação completa

### 3. **Testar Funcionalidades**

**Adicionar emails à fila:**
```bash
curl -X POST "http://localhost:5000/api/queue/email/test-batch?count=10"
```

**Verificar status das filas:**
```bash
curl "http://localhost:5000/api/queue/status"
```

**Ver dashboard:**
```bash
curl "http://localhost:5000/api/monitoring/dashboard"
```

**Health check:**
```bash
curl "http://localhost:5000/health"
```

### 4. **Monitorar Logs**
```bash
# Os logs mostram:
# - Execução dos background services
# - Processamento de filas
# - Health checks
# - Métricas de performance
```

## 🔧 Configuração

### Registro dos Serviços
```csharp
// Filas
builder.Services.AddSingleton<IQueueService<EmailNotification>, ChannelQueueService<EmailNotification>>();
builder.Services.AddSingleton<IQueueService<DataProcessingJob>, PriorityQueueService<DataProcessingJob>>();

// Tracking
builder.Services.AddSingleton<IJobTrackingService, JobTrackingService>();

// Background Services
builder.Services.AddHostedService<TimedBackgroundService>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DataProcessingBackgroundService>();

// Singleton Service (acessível em controllers)
builder.Services.AddSingleton<SingletonBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SingletonBackgroundService>());
```

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());
```

## 📊 Métricas e Monitoramento

### Dashboard Inclui:
- **Status dos Serviços**: Running/Stopped, última atividade
- **Estatísticas de Jobs**: Total, por status, taxa de sucesso
- **Health Checks**: Percentual de saúde, tempo de resposta
- **Filas**: Número de itens, status de processamento

### Logs Estruturados:
- **Info**: Início/fim de processamento
- **Debug**: Detalhes de execução
- **Warning**: Falhas recuperáveis
- **Error**: Erros críticos com stack trace

## 🎯 Casos de Uso

### 1. **Processamento de Email**
- Fila com prioridades (Critical, High, Normal, Low)
- Retry automático para falhas temporárias
- Métricas de taxa de entrega

### 2. **Processamento de Dados**
- Batch processing para performance
- Processamento paralelo controlado
- Tracking de progresso

### 3. **Manutenção Automática**
- Limpeza de dados temporários
- Remoção de logs antigos
- Execução agendada

### 4. **Monitoramento de Sistema**
- Health checks de componentes críticos
- Alertas para problemas
- Histórico de disponibilidade

## 🔍 Detalhes Técnicos

### Cancelamento Graceful
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await ProcessWorkAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Shutdown graceful
            break;
        }
    }
}
```

### Retry Pattern
```csharp
if (queueItem.RetryCount < queueItem.MaxRetries)
{
    var retryItem = queueItem with
    {
        RetryCount = queueItem.RetryCount + 1
    };
    
    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryItem.RetryCount)));
    await _queue.EnqueueAsync(retryItem);
}
```

### Thread Safety
```csharp
private long _processedCount = 0;
Interlocked.Increment(ref _processedCount);
```

## 📈 Performance

### Otimizações Implementadas:
- **Channel<T>**: Para filas thread-safe e performáticas
- **SemaphoreSlim**: Controle de concorrência
- **PriorityQueue**: Processamento otimizado por prioridade
- **Batch Processing**: Reduz overhead de transações
- **Memory Pooling**: Via ObjectPool quando necessário

## 🏆 Benefícios

### 1. **Escalabilidade**
- Processamento assíncrono
- Controle de concorrência
- Balanceamento de carga

### 2. **Confiabilidade**
- Retry automático
- Health monitoring
- Graceful shutdown

### 3. **Observabilidade**
- Logs estruturados
- Métricas em tempo real
- Dashboard de monitoramento

### 4. **Manutenibilidade**
- Código bem estruturado
- Interfaces claras
- Testabilidade alta

---

## 🔗 Conceitos Relacionados
- Hosted Services
- Dependency Injection
- Channels
- Health Checks
- Logging
- Cancellation Tokens
- Task Parallel Library (TPL)

Esta implementação demonstra as melhores práticas para Background Services em aplicações .NET, fornecendo uma base sólida para processamento assíncrono em produção.
