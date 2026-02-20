using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;
using System.Collections.Concurrent;

Console.WriteLine("🚀 Dica 92: Real-time Communications (.NET 9)");
Console.WriteLine("==============================================");

// 1. Channels para Comunicação Assíncrona
Console.WriteLine("\n1. 📡 Channels para Comunicação:");
Console.WriteLine("──────────────────────────────────");

await DemonstrarChannels();

// 2. Producer-Consumer Pattern
Console.WriteLine("\n2. 🏭 Producer-Consumer Pattern:");
Console.WriteLine("────────────────────────────────");

await DemonstrarProducerConsumer();

// 3. Pub/Sub Pattern Simulado
Console.WriteLine("\n3. 📢 Pub/Sub Pattern:");
Console.WriteLine("──────────────────────");

await DemonstrarPubSub();

// 4. SignalR Client Simulation
Console.WriteLine("\n4. 📱 SignalR Client Simulation:");
Console.WriteLine("────────────────────────────────");

await DemonstrarSignalRClient();

// 5. Event-Driven Communication
Console.WriteLine("\n5. ⚡ Event-Driven Communication:");
Console.WriteLine("─────────────────────────────────");

await DemonstrarEventDriven();

// 6. Real-time Data Streaming
Console.WriteLine("\n6. 🌊 Real-time Data Streaming:");
Console.WriteLine("───────────────────────────────");

await DemonstrarDataStreaming();

Console.WriteLine("\n✅ Demonstração completa de Real-time Communications!");

static async Task DemonstrarChannels()
{
    Console.WriteLine("📡 Channel básico - Single Producer/Single Consumer:");
    
    // Channel com capacidade limitada
    var channel = Channel.CreateBounded<string>(10);
    var writer = channel.Writer;
    var reader = channel.Reader;
    
    // Producer task
    var producerTask = Task.Run(async () =>
    {
        for (int i = 1; i <= 5; i++)
        {
            await writer.WriteAsync($"Mensagem {i}");
            Console.WriteLine($"📤 Enviado: Mensagem {i}");
            await Task.Delay(500);
        }
        writer.Complete();
        Console.WriteLine("📤 Producer finalizado");
    });
    
    // Consumer task
    var consumerTask = Task.Run(async () =>
    {
        await foreach (var message in reader.ReadAllAsync())
        {
            Console.WriteLine($"📥 Recebido: {message}");
            await Task.Delay(200);
        }
        Console.WriteLine("📥 Consumer finalizado");
    });
    
    await Task.WhenAll(producerTask, consumerTask);
    
    // Channel com múltiplos tipos
    await DemonstrarChannelMultiTipo();
}

static async Task DemonstrarChannelMultiTipo()
{
    Console.WriteLine("\n📡 Channel com objetos complexos:");
    
    var messageChannel = Channel.CreateUnbounded<Message>();
    var writer = messageChannel.Writer;
    var reader = messageChannel.Reader;
    
    // Producer de diferentes tipos de mensagem
    _ = Task.Run(async () =>
    {
        var messages = new[]
        {
            new Message("INFO", "Sistema iniciado", DateTime.Now),
            new Message("WARNING", "Memória em 80%", DateTime.Now),
            new Message("ERROR", "Falha na conexão", DateTime.Now),
            new Message("SUCCESS", "Backup concluído", DateTime.Now)
        };
        
        foreach (var msg in messages)
        {
            await writer.WriteAsync(msg);
            await Task.Delay(300);
        }
        
        writer.Complete();
    });
    
    // Consumer com processamento baseado no tipo
    await foreach (var message in reader.ReadAllAsync())
    {
        var emoji = message.Level switch
        {
            "INFO" => "ℹ️",
            "WARNING" => "⚠️",
            "ERROR" => "❌",
            "SUCCESS" => "✅",
            _ => "📝"
        };
        
        Console.WriteLine($"{emoji} [{message.Level}] {message.Content} ({message.Timestamp:HH:mm:ss})");
    }
}

static async Task DemonstrarProducerConsumer()
{
    Console.WriteLine("🏭 Producer-Consumer com múltiplos workers:");
    
    var workChannel = Channel.CreateBounded<WorkItem>(100);
    var resultChannel = Channel.CreateUnbounded<WorkResult>();
    
    var cts = new CancellationTokenSource();
    
    // Producer
    var producerTask = Task.Run(async () =>
    {
        for (int i = 1; i <= 20; i++)
        {
            var workItem = new WorkItem(i, $"Task-{i}", Random.Shared.Next(100, 1000));
            await workChannel.Writer.WriteAsync(workItem);
            Console.WriteLine($"🏭 Trabalho criado: {workItem.Name} (complexidade: {workItem.Complexity})");
            await Task.Delay(100);
        }
        workChannel.Writer.Complete();
    });
    
    // Multiple Consumers (Workers)
    var workerTasks = Enumerable.Range(1, 3).Select(workerId =>
        Task.Run(async () =>
        {
            await foreach (var workItem in workChannel.Reader.ReadAllAsync(cts.Token))
            {
                Console.WriteLine($"⚙️ Worker-{workerId} processando: {workItem.Name}");
                
                // Simular processamento baseado na complexidade
                await Task.Delay(workItem.Complexity / 10);
                
                var result = new WorkResult(workItem.Id, workerId, true, $"Processado com sucesso");
                await resultChannel.Writer.WriteAsync(result);
                
                Console.WriteLine($"✅ Worker-{workerId} completou: {workItem.Name}");
            }
        })
    ).ToArray();
    
    // Result Collector
    var collectorTask = Task.Run(async () =>
    {
        var processedCount = 0;
        var successCount = 0;
        
        await foreach (var result in resultChannel.Reader.ReadAllAsync(cts.Token))
        {
            processedCount++;
            if (result.Success) successCount++;
            
            Console.WriteLine($"📊 Resultado: Task-{result.WorkItemId} por Worker-{result.WorkerId} - {(result.Success ? "✅" : "❌")}");
            
            if (processedCount >= 20) // Todos os itens processados
            {
                break;
            }
        }
        
        Console.WriteLine($"📈 Resumo: {successCount}/{processedCount} tarefas concluídas com sucesso");
    });
    
    await producerTask;
    await Task.WhenAll(workerTasks);
    resultChannel.Writer.Complete();
    await collectorTask;
    
    cts.Cancel();
}

static async Task DemonstrarPubSub()
{
    Console.WriteLine("📢 Publish/Subscribe pattern:");
    
    var eventBus = new SimpleEventBus();
    
    // Subscribers
    eventBus.Subscribe<UserLoggedIn>(evt =>
    {
        Console.WriteLine($"🔐 Security: Usuário {evt.UserId} logou de {evt.IpAddress}");
        return Task.CompletedTask;
    });
    
    eventBus.Subscribe<UserLoggedIn>(evt =>
    {
        Console.WriteLine($"📊 Analytics: Login registrado para {evt.UserId}");
        return Task.CompletedTask;
    });
    
    eventBus.Subscribe<OrderCreated>(evt =>
    {
        Console.WriteLine($"📦 Fulfillment: Processar pedido {evt.OrderId} - ${evt.Total:F2}");
        return Task.CompletedTask;
    });
    
    eventBus.Subscribe<OrderCreated>(evt =>
    {
        Console.WriteLine($"💳 Payment: Cobrar ${evt.Total:F2} do cliente {evt.UserId}");
        return Task.CompletedTask;
    });
    
    // Publishers
    await eventBus.PublishAsync(new UserLoggedIn("user-123", "192.168.1.100"));
    await Task.Delay(200);
    
    await eventBus.PublishAsync(new OrderCreated("order-456", "user-123", 299.99m));
    await Task.Delay(200);
    
    await eventBus.PublishAsync(new UserLoggedIn("user-789", "10.0.0.50"));
    
    Console.WriteLine($"📊 Total de eventos publicados: {eventBus.EventCount}");
}

static async Task DemonstrarSignalRClient()
{
    Console.WriteLine("📱 Simulação de cliente SignalR:");
    
    // Simular conexão SignalR (sem servidor real)
    var hubSimulator = new SignalRHubSimulator();
    
    // Simular cliente conectando
    var client1 = new SignalRClientSimulator("Client-1");
    var client2 = new SignalRClientSimulator("Client-2");
    var client3 = new SignalRClientSimulator("Client-3");
    
    await hubSimulator.ConnectClientAsync(client1);
    await hubSimulator.ConnectClientAsync(client2);
    await hubSimulator.ConnectClientAsync(client3);
    
    // Simular envio de mensagens broadcast
    await hubSimulator.BroadcastAsync("Bem-vindos ao chat!");
    await Task.Delay(300);
    
    await hubSimulator.SendToClientAsync(client1.Id, "Mensagem privada para Client-1");
    await Task.Delay(300);
    
    await hubSimulator.SendToGroupAsync("VIP", "Mensagem exclusiva para VIPs");
    await Task.Delay(300);
    
    // Simular clientes enviando mensagens
    await client1.SendMessageAsync("Olá pessoal!");
    await client2.SendMessageAsync("Oi! Como estão?");
    await client3.SendMessageAsync("Tudo bem aqui! 👋");
    
    await Task.Delay(500);
    
    // Desconectar clientes
    await hubSimulator.DisconnectClientAsync(client1);
    await hubSimulator.DisconnectClientAsync(client2);
    await hubSimulator.DisconnectClientAsync(client3);
}

static async Task DemonstrarEventDriven()
{
    Console.WriteLine("⚡ Arquitetura orientada a eventos:");
    
    var eventProcessor = new EventProcessor();
    
    // Registrar handlers para diferentes tipos de eventos
    eventProcessor.RegisterHandler<StockUpdated>(async evt =>
    {
        Console.WriteLine($"📦 Estoque atualizado: {evt.ProductId} - Quantidade: {evt.NewQuantity}");
        
        if (evt.NewQuantity < 10)
        {
            await eventProcessor.TriggerEventAsync(new LowStockAlert(evt.ProductId, evt.NewQuantity));
        }
    });
    
    eventProcessor.RegisterHandler<LowStockAlert>(async evt =>
    {
        Console.WriteLine($"🚨 ALERTA: Estoque baixo para {evt.ProductId} ({evt.Quantity} unidades)");
        
        // Simular criação automática de pedido de reposição
        await eventProcessor.TriggerEventAsync(new RestockOrdered(evt.ProductId, 100));
    });
    
    eventProcessor.RegisterHandler<RestockOrdered>(async evt =>
    {
        Console.WriteLine($"📋 Pedido de reposição criado: {evt.ProductId} - {evt.Quantity} unidades");
        await Task.Delay(100); // Simular processamento
    });
    
    // Simular eventos
    await eventProcessor.TriggerEventAsync(new StockUpdated("PROD-001", 15));
    await Task.Delay(200);
    
    await eventProcessor.TriggerEventAsync(new StockUpdated("PROD-002", 5)); // Vai gerar alerta
    await Task.Delay(200);
    
    await eventProcessor.TriggerEventAsync(new StockUpdated("PROD-003", 3)); // Vai gerar alerta
    await Task.Delay(500);
    
    Console.WriteLine($"📊 Total de eventos processados: {eventProcessor.ProcessedEventCount}");
}

static async Task DemonstrarDataStreaming()
{
    Console.WriteLine("🌊 Streaming de dados em tempo real:");
    
    var dataStream = new RealTimeDataStream();
    
    // Configurar diferentes tipos de dados
    var sensors = new[]
    {
        new SensorConfig("TEMP-01", "Temperature", 18, 25, 0.5),
        new SensorConfig("HUM-01", "Humidity", 40, 60, 2.0),
        new SensorConfig("PRESS-01", "Pressure", 1000, 1050, 1.0)
    };
    
    var cts = new CancellationTokenSource();
    
    // Consumer - processar dados em tempo real
    var consumerTask = Task.Run(async () =>
    {
        await foreach (var reading in dataStream.GetReadingsAsync(cts.Token))
        {
            var status = reading.Value switch
            {
                < 20 when reading.SensorType == "Temperature" => "🔵 Frio",
                > 23 when reading.SensorType == "Temperature" => "🔴 Quente",
                < 45 when reading.SensorType == "Humidity" => "💧 Seco", 
                > 55 when reading.SensorType == "Humidity" => "💦 Úmido",
                < 1010 when reading.SensorType == "Pressure" => "📉 Baixa",
                > 1040 when reading.SensorType == "Pressure" => "📈 Alta",
                _ => "✅ Normal"
            };
            
            Console.WriteLine($"📊 {reading.SensorId} ({reading.SensorType}): {reading.Value:F1} {reading.Unit} - {status}");
        }
    });
    
    // Producer - gerar dados de sensores
    var producerTask = Task.Run(async () =>
    {
        for (int i = 0; i < 15; i++) // 15 leituras
        {
            foreach (var sensor in sensors)
            {
                var reading = GenerateReading(sensor);
                await dataStream.AddReadingAsync(reading);
            }
            
            await Task.Delay(1000); // Intervalo de 1 segundo
        }
        
        dataStream.Complete();
    });
    
    await producerTask;
    await Task.Delay(2000); // Aguardar processamento
    cts.Cancel();
    
    try
    {
        await consumerTask;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("🛑 Stream finalizado");
    }
}

static SensorReading GenerateReading(SensorConfig config)
{
    var random = Random.Shared;
    var value = config.MinValue + (random.NextDouble() * (config.MaxValue - config.MinValue));
    
    // Adicionar um pouco de ruído
    value += (random.NextDouble() - 0.5) * config.NoiseLevel;
    
    var unit = config.Type switch
    {
        "Temperature" => "°C",
        "Humidity" => "%",
        "Pressure" => "hPa",
        _ => ""
    };
    
    return new SensorReading(config.Id, config.Type, value, unit, DateTime.Now);
}

// Classes de suporte
public record Message(string Level, string Content, DateTime Timestamp);

public record WorkItem(int Id, string Name, int Complexity);

public record WorkResult(int WorkItemId, int WorkerId, bool Success, string Message);

// Event classes
public interface IEvent
{
    DateTime Timestamp { get; }
}

public record UserLoggedIn(string UserId, string IpAddress) : IEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public record OrderCreated(string OrderId, string UserId, decimal Total) : IEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public record StockUpdated(string ProductId, int NewQuantity) : IEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public record LowStockAlert(string ProductId, int Quantity) : IEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public record RestockOrdered(string ProductId, int Quantity) : IEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

// Event Bus
public class SimpleEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();
    private int _eventCount = 0;
    
    public int EventCount => _eventCount;
    
    public void Subscribe<T>(Func<T, Task> handler) where T : IEvent
    {
        var eventType = typeof(T);
        var handlers = _handlers.GetOrAdd(eventType, _ => new List<Func<object, Task>>());
        
        lock (handlers)
        {
            handlers.Add(evt => handler((T)evt));
        }
    }
    
    public async Task PublishAsync<T>(T eventObj) where T : IEvent
    {
        Interlocked.Increment(ref _eventCount);
        
        var eventType = typeof(T);
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return;
        
        List<Func<object, Task>> handlersToCall;
        lock (handlers)
        {
            handlersToCall = new List<Func<object, Task>>(handlers);
        }
        
        var tasks = handlersToCall.Select(handler => handler(eventObj));
        await Task.WhenAll(tasks);
    }
}

// SignalR Simulation
public class SignalRClientSimulator
{
    public string Id { get; }
    public string ConnectionId { get; } = Guid.NewGuid().ToString();
    
    public SignalRClientSimulator(string id)
    {
        Id = id;
    }
    
    public async Task SendMessageAsync(string message)
    {
        Console.WriteLine($"📱 {Id}: {message}");
        await Task.Delay(50); // Simular latência
    }
    
    public async Task ReceiveMessageAsync(string message, string from = "Server")
    {
        Console.WriteLine($"📨 {Id} recebeu de {from}: {message}");
        await Task.Delay(10);
    }
}

public class SignalRHubSimulator
{
    private readonly List<SignalRClientSimulator> _clients = new();
    
    public async Task ConnectClientAsync(SignalRClientSimulator client)
    {
        _clients.Add(client);
        Console.WriteLine($"🔗 {client.Id} conectado (ConnectionId: {client.ConnectionId[..8]}...)");
        await Task.Delay(50);
    }
    
    public async Task DisconnectClientAsync(SignalRClientSimulator client)
    {
        _clients.Remove(client);
        Console.WriteLine($"❌ {client.Id} desconectado");
        await Task.Delay(50);
    }
    
    public async Task BroadcastAsync(string message)
    {
        Console.WriteLine($"📢 Broadcast: {message}");
        var tasks = _clients.Select(c => c.ReceiveMessageAsync(message));
        await Task.WhenAll(tasks);
    }
    
    public async Task SendToClientAsync(string clientId, string message)
    {
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client != null)
        {
            Console.WriteLine($"📤 Enviando para {clientId}: {message}");
            await client.ReceiveMessageAsync(message);
        }
    }
    
    public async Task SendToGroupAsync(string groupName, string message)
    {
        Console.WriteLine($"👥 Grupo {groupName}: {message}");
        // Simular que apenas alguns clientes estão no grupo
        var groupClients = _clients.Take(2);
        var tasks = groupClients.Select(c => c.ReceiveMessageAsync(message, $"Grupo {groupName}"));
        await Task.WhenAll(tasks);
    }
}

// Event Processor
public class EventProcessor
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();
    private int _processedEventCount = 0;
    
    public int ProcessedEventCount => _processedEventCount;
    
    public void RegisterHandler<T>(Func<T, Task> handler) where T : IEvent
    {
        var eventType = typeof(T);
        var handlers = _handlers.GetOrAdd(eventType, _ => new List<Func<object, Task>>());
        
        lock (handlers)
        {
            handlers.Add(evt => handler((T)evt));
        }
    }
    
    public async Task TriggerEventAsync<T>(T eventObj) where T : IEvent
    {
        Interlocked.Increment(ref _processedEventCount);
        
        var eventType = typeof(T);
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return;
        
        List<Func<object, Task>> handlersToCall;
        lock (handlers)
        {
            handlersToCall = new List<Func<object, Task>>(handlers);
        }
        
        var tasks = handlersToCall.Select(handler => handler(eventObj));
        await Task.WhenAll(tasks);
    }
}

// Data Streaming
public record SensorConfig(string Id, string Type, double MinValue, double MaxValue, double NoiseLevel);

public record SensorReading(string SensorId, string SensorType, double Value, string Unit, DateTime Timestamp);

public class RealTimeDataStream
{
    private readonly Channel<SensorReading> _channel = Channel.CreateUnbounded<SensorReading>();
    
    public async Task AddReadingAsync(SensorReading reading)
    {
        await _channel.Writer.WriteAsync(reading);
    }
    
    public void Complete()
    {
        _channel.Writer.Complete();
    }
    
    public IAsyncEnumerable<SensorReading> GetReadingsAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
