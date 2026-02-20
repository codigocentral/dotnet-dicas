# Dica 38: Usando Channels - Comunicação Producer-Consumer

## 📖 Problema

Implementar comunicação assíncrona eficiente entre producers e consumers em aplicações .NET, especialmente para cenários de processamento em background, filas de trabalho e pipelines de dados.

## ✅ Solução

Use `System.Threading.Channels` para criar pipelines assíncronos type-safe, eficientes e com controle de fluxo entre producers e consumers.

## 🎯 Tipos de Channels

### 1. Unbounded Channel (Sem Limite)

```csharp
// Channel sem limite de itens
var channel = Channel.CreateUnbounded<string>();
var writer = channel.Writer;
var reader = channel.Reader;

// Producer
await writer.WriteAsync("mensagem");
writer.Complete();

// Consumer
await foreach (var item in reader.ReadAllAsync())
{
    Console.WriteLine(item);
}
```

### 2. Bounded Channel (Com Limite)

```csharp
// Channel com limite de 100 itens
var options = new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait, // Aguarda quando cheio
    SingleReader = false,
    SingleWriter = false
};

var channel = Channel.CreateBounded<string>(options);
```

### 3. Configurações de FullMode

```csharp
public enum BoundedChannelFullMode
{
    Wait,        // Bloqueia producer quando cheio
    DropNewest,  // Descarta itens mais novos
    DropOldest   // Descarta itens mais antigos
}
```

## 🔄 Padrões de Uso

### Single Producer, Single Consumer

```csharp
public class ProcessadorPedidos
{
    private readonly Channel<Pedido> _channel;
    private readonly ChannelWriter<Pedido> _writer;
    private readonly ChannelReader<Pedido> _reader;

    public ProcessadorPedidos()
    {
        _channel = Channel.CreateUnbounded<Pedido>();
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public async Task AdicionarPedidoAsync(Pedido pedido)
    {
        await _writer.WriteAsync(pedido);
    }

    public async Task ProcessarPedidosAsync(CancellationToken cancellationToken)
    {
        await foreach (var pedido in _reader.ReadAllAsync(cancellationToken))
        {
            await ProcessarPedido(pedido);
        }
    }

    public void CompletarProcessamento()
    {
        _writer.Complete();
    }
}
```

### Multiple Producers, Multiple Consumers

```csharp
public class ProcessadorImagens
{
    private readonly Channel<ImagemJob> _channel;

    public ProcessadorImagens()
    {
        var options = new BoundedChannelOptions(50)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false, // Múltiplos consumers
            SingleWriter = false  // Múltiplos producers
        };
        
        _channel = Channel.CreateBounded<ImagemJob>(options);
    }

    // Múltiplos workers podem chamar este método
    public async Task ProcessarImagensAsync(string workerId, CancellationToken cancellationToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            await ProcessarImagem(job, workerId);
        }
    }
}
```

## 📊 Comparação: Channel vs Alternativas

| Aspecto | Channel | ConcurrentQueue | BlockingCollection |
|---------|---------|-----------------|-------------------|
| **Async/Await** | ✅ Nativo | ❌ Requer polling | ❌ Bloqueia threads |
| **Type Safety** | ✅ Genérico | ✅ Genérico | ✅ Genérico |
| **Backpressure** | ✅ Bounded modes | ❌ Sem controle | ✅ Com limite |
| **Performance** | ✅ Alta | ✅ Alta | ⚠️ Média |
| **Cancellation** | ✅ CancellationToken | ❌ Manual | ✅ CancellationToken |
| **Memory Usage** | ✅ Controlável | ❌ Pode crescer | ✅ Controlável |

## 🚀 Cenários de Uso

### 1. Background Job Processing

```csharp
public class JobProcessor : BackgroundService
{
    private readonly Channel<JobItem> _jobChannel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _jobChannel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessJob(job);
        }
    }

    public async Task EnqueueJobAsync(JobItem job)
    {
        await _jobChannel.Writer.WriteAsync(job);
    }
}
```

### 2. Event Streaming

```csharp
public class EventStream<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

    public async Task PublishAsync(T eventData)
    {
        await _channel.Writer.WriteAsync(eventData);
    }

    public IAsyncEnumerable<T> SubscribeAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
```

### 3. Pipeline de Dados

```csharp
public class DataPipeline
{
    public async Task ProcessDataAsync(IAsyncEnumerable<RawData> input)
    {
        var stage1 = Channel.CreateBounded<ProcessedData>(100);
        var stage2 = Channel.CreateBounded<FinalData>(50);

        // Stage 1: Transformação
        var transform = ProcessStage1(input, stage1.Writer);
        
        // Stage 2: Validação
        var validate = ProcessStage2(stage1.Reader, stage2.Writer);
        
        // Stage 3: Persistência
        var persist = ProcessStage3(stage2.Reader);

        await Task.WhenAll(transform, validate, persist);
    }
}
```

## ⚡ Performance e Boas Práticas

### 1. Escolha do Tipo Correto

```csharp
// Para alta throughput sem controle de memória
var unbounded = Channel.CreateUnbounded<T>();

// Para controle de memória e backpressure
var bounded = Channel.CreateBounded<T>(capacity: 1000);
```

### 2. Configuração Otimizada

```csharp
var options = new BoundedChannelOptions(capacity)
{
    // Single reader/writer = melhor performance
    SingleReader = true,   // Se só há um consumer
    SingleWriter = true,   // Se só há um producer
    
    // Escolha baseada no cenário
    FullMode = BoundedChannelFullMode.Wait // Mais seguro
};
```

### 3. Tratamento de Erros

```csharp
public async Task ProcessItemsAsync(CancellationToken cancellationToken)
{
    try
    {
        await foreach (var item in _reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await ProcessItem(item);
            }
            catch (Exception ex)
            {
                // Log erro mas continua processando
                _logger.LogError(ex, "Erro ao processar item");
            }
        }
    }
    catch (OperationCanceledException)
    {
        // Esperado quando cancela
    }
}
```

### 4. Finalização Adequada

```csharp
public async Task FinalizeAsync()
{
    // Sinaliza que não há mais itens
    _writer.Complete();
    
    // Aguarda processamento de itens pendentes
    await _consumerTask;
}
```

## 🔍 Monitoramento e Observabilidade

```csharp
public class MonitoredChannel<T>
{
    private readonly Channel<T> _channel;
    private long _itemsProduced;
    private long _itemsConsumed;

    public async Task WriteAsync(T item)
    {
        await _channel.Writer.WriteAsync(item);
        Interlocked.Increment(ref _itemsProduced);
    }

    public async IAsyncEnumerable<T> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            Interlocked.Increment(ref _itemsConsumed);
            yield return item;
        }
    }

    public (long Produced, long Consumed, long Pending) GetStats()
    {
        var produced = Interlocked.Read(ref _itemsProduced);
        var consumed = Interlocked.Read(ref _itemsConsumed);
        return (produced, consumed, produced - consumed);
    }
}
```

## 💡 Vantagens dos Channels

1. **Async-First**: Projetado para async/await desde o início
2. **Type-Safe**: Fortemente tipado
3. **Memory Efficient**: Controle preciso de uso de memória
4. **Backpressure**: Controle de fluxo integrado
5. **Cancellation**: Suporte nativo a CancellationToken
6. **High Performance**: Otimizado para alta throughput

Os Channels são a solução moderna para comunicação producer-consumer em .NET, oferecendo melhor performance e usabilidade que alternativas tradicionais!
