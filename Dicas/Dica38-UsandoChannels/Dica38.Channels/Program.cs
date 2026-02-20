using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

Console.WriteLine("==== Dica 38: Usando Channels - Comunicação Producer-Consumer ====\n");

// Configuração do Host com DI
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ProcessadorPedidos>();
        services.AddSingleton<MonitorLogs>();
        services.AddSingleton<ProcessadorImagens>();
        services.AddSingleton<DemoService>();
    })
    .Build();

// Executar demonstrações
var demoService = host.Services.GetRequiredService<DemoService>();
await demoService.ExecutarDemonstracaoAsync();

await host.StopAsync();

// Modelos para demonstração
public record Pedido(int Id, string Cliente, decimal Valor, DateTime Timestamp = default)
{
    public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.Now : Timestamp;
}

public record LogEntry(string Level, string Message, DateTime Timestamp = default)
{
    public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.Now : Timestamp;
}

public record ImagemJob(int Id, string CaminhoArquivo, string TipoProcessamento);

// Serviço que demonstra Channel básico (unbounded)
public class ProcessadorPedidos
{
    private readonly Channel<Pedido> _channel;
    private readonly ChannelWriter<Pedido> _writer;
    private readonly ChannelReader<Pedido> _reader;
    private readonly ILogger<ProcessadorPedidos> _logger;

    public ProcessadorPedidos(ILogger<ProcessadorPedidos> logger)
    {
        _logger = logger;
        
        // Channel unbounded - sem limite de itens
        _channel = Channel.CreateUnbounded<Pedido>();
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public async Task AdicionarPedidoAsync(Pedido pedido)
    {
        await _writer.WriteAsync(pedido);
        _logger.LogInformation("📋 Pedido {Id} adicionado à fila: {Cliente} - R$ {Valor:F2}", 
            pedido.Id, pedido.Cliente, pedido.Valor);
    }

    public async Task ProcessarPedidosAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🚀 Iniciando processamento de pedidos...");
        
        await foreach (var pedido in _reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                // Simula processamento do pedido
                await ProcessarPedidoIndividual(pedido);
                
                _logger.LogInformation("✅ Pedido {Id} processado com sucesso!", pedido.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar pedido {Id}", pedido.Id);
            }
        }
    }

    private async Task ProcessarPedidoIndividual(Pedido pedido)
    {
        // Simula validação
        await Task.Delay(Random.Shared.Next(50, 200));
        
        // Simula processamento de pagamento
        await Task.Delay(Random.Shared.Next(100, 300));
        
        // Simula atualização de estoque
        await Task.Delay(Random.Shared.Next(30, 100));
    }

    public void CompletarProcessamento()
    {
        _writer.Complete();
        _logger.LogInformation("🔚 Sinalizado fim do processamento de pedidos");
    }
}

// Serviço que demonstra Channel bounded (com limite)
public class MonitorLogs
{
    private readonly Channel<LogEntry> _channel;
    private readonly ChannelWriter<LogEntry> _writer;
    private readonly ChannelReader<LogEntry> _reader;
    private readonly ILogger<MonitorLogs> _logger;

    public MonitorLogs(ILogger<MonitorLogs> logger)
    {
        _logger = logger;
        
        // Channel bounded - máximo 1000 itens
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait, // Aguarda quando cheio
            SingleReader = false, // Múltiplos readers permitidos
            SingleWriter = false  // Múltiplos writers permitidos
        };
        
        _channel = Channel.CreateBounded<LogEntry>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public async Task AdicionarLogAsync(LogEntry entry)
    {
        if (await _writer.WaitToWriteAsync())
        {
            await _writer.WriteAsync(entry);
        }
    }

    public async Task MonitorarLogsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 Iniciando monitoramento de logs...");
        
        var contador = 0;
        await foreach (var logEntry in _reader.ReadAllAsync(cancellationToken))
        {
            contador++;
            
            // Processa logs de acordo com o nível
            switch (logEntry.Level.ToUpper())
            {
                case "ERROR":
                    _logger.LogError("🔴 [{Timestamp}] {Message}", 
                        logEntry.Timestamp.ToString("HH:mm:ss.fff"), logEntry.Message);
                    break;
                case "WARNING":
                    _logger.LogWarning("🟡 [{Timestamp}] {Message}", 
                        logEntry.Timestamp.ToString("HH:mm:ss.fff"), logEntry.Message);
                    break;
                default:
                    _logger.LogInformation("🔵 [{Timestamp}] {Message}", 
                        logEntry.Timestamp.ToString("HH:mm:ss.fff"), logEntry.Message);
                    break;
            }
            
            // A cada 10 logs, mostra estatística
            if (contador % 10 == 0)
            {
                _logger.LogInformation("📈 Processados {Count} logs até agora", contador);
            }
        }
        
        _logger.LogInformation("🏁 Monitoramento finalizado. Total processado: {Total} logs", contador);
    }

    public void CompletarMonitoramento()
    {
        _writer.Complete();
        _logger.LogInformation("🔚 Sinalizado fim do monitoramento de logs");
    }
}

// Serviço que demonstra Channel com múltiplos consumers
public class ProcessadorImagens
{
    private readonly Channel<ImagemJob> _channel;
    private readonly ChannelWriter<ImagemJob> _writer;
    private readonly ChannelReader<ImagemJob> _reader;
    private readonly ILogger<ProcessadorImagens> _logger;

    public ProcessadorImagens(ILogger<ProcessadorImagens> logger)
    {
        _logger = logger;
        
        // Channel bounded com drop newest quando cheio
        var options = new BoundedChannelOptions(50)
        {
            FullMode = BoundedChannelFullMode.DropNewest,
            SingleReader = false,
            SingleWriter = true
        };
        
        _channel = Channel.CreateBounded<ImagemJob>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public async Task AdicionarJobAsync(ImagemJob job)
    {
        if (await _writer.WaitToWriteAsync())
        {
            await _writer.WriteAsync(job);
            _logger.LogInformation("🖼️ Job {Id} adicionado: {Arquivo} ({Tipo})", 
                job.Id, job.CaminhoArquivo, job.TipoProcessamento);
        }
    }

    public async Task ProcessarImagensAsync(string nomeWorker, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🤖 Worker {Worker} iniciado para processamento de imagens", nomeWorker);
        
        await foreach (var job in _reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await ProcessarImagemIndividual(job, nomeWorker);
                _logger.LogInformation("✅ [{Worker}] Job {Id} concluído", nomeWorker, job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [{Worker}] Erro ao processar Job {Id}", nomeWorker, job.Id);
            }
        }
        
        _logger.LogInformation("🔚 Worker {Worker} finalizado", nomeWorker);
    }

    private async Task ProcessarImagemIndividual(ImagemJob job, string worker)
    {
        _logger.LogInformation("⚙️ [{Worker}] Processando {Arquivo} - {Tipo}", 
            worker, job.CaminhoArquivo, job.TipoProcessamento);
        
        // Simula tempo de processamento baseado no tipo
        var tempoProcessamento = job.TipoProcessamento switch
        {
            "Redimensionar" => Random.Shared.Next(100, 300),
            "Filtro" => Random.Shared.Next(200, 500),
            "Compressão" => Random.Shared.Next(300, 800),
            _ => Random.Shared.Next(100, 400)
        };
        
        await Task.Delay(tempoProcessamento);
    }

    public void CompletarProcessamento()
    {
        _writer.Complete();
        _logger.LogInformation("🔚 Sinalizado fim do processamento de imagens");
    }
}

// Serviço principal da demonstração
public class DemoService
{
    private readonly ProcessadorPedidos _processadorPedidos;
    private readonly MonitorLogs _monitorLogs;
    private readonly ProcessadorImagens _processadorImagens;
    private readonly ILogger<DemoService> _logger;

    public DemoService(
        ProcessadorPedidos processadorPedidos,
        MonitorLogs monitorLogs,
        ProcessadorImagens processadorImagens,
        ILogger<DemoService> logger)
    {
        _processadorPedidos = processadorPedidos;
        _monitorLogs = monitorLogs;
        _processadorImagens = processadorImagens;
        _logger = logger;
    }

    public async Task ExecutarDemonstracaoAsync()
    {
        await DemonstrarChannelBasico();
        await Task.Delay(2000);
        
        await DemonstrarChannelBounded();
        await Task.Delay(2000);
        
        await DemonstrarMultiplosConsumers();
        await Task.Delay(1000);
        
        await DemonstrarComparacaoComQueue();
        
        MostrarResumoBoasPraticas();
    }

    private async Task DemonstrarChannelBasico()
    {
        Console.WriteLine("🔧 1. CHANNEL BÁSICO (Unbounded) - Processamento de Pedidos");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        
        // Inicia o consumer em background
        var consumerTask = Task.Run(() => _processadorPedidos.ProcessarPedidosAsync(cts.Token));
        
        // Simula chegada de pedidos
        var pedidos = new[]
        {
            new Pedido(1, "João Silva", 150.00m),
            new Pedido(2, "Maria Santos", 89.99m),
            new Pedido(3, "Carlos Oliveira", 300.50m),
            new Pedido(4, "Ana Costa", 45.75m),
            new Pedido(5, "Pedro Alves", 199.99m)
        };

        foreach (var pedido in pedidos)
        {
            await _processadorPedidos.AdicionarPedidoAsync(pedido);
            await Task.Delay(500); // Simula intervalo entre pedidos
        }

        // Aguarda um pouco e finaliza
        await Task.Delay(2000);
        _processadorPedidos.CompletarProcessamento();
        
        try
        {
            await consumerTask;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando cancela por timeout
        }

        Console.WriteLine("✅ Demonstração de Channel básico concluída\n");
    }

    private async Task DemonstrarChannelBounded()
    {
        Console.WriteLine("📊 2. CHANNEL BOUNDED - Monitor de Logs");
        Console.WriteLine("────────────────────────────────────────");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        
        // Inicia o consumer
        var consumerTask = Task.Run(() => _monitorLogs.MonitorarLogsAsync(cts.Token));
        
        // Simula geração de logs
        var tiposLog = new[] { "INFO", "WARNING", "ERROR", "DEBUG" };
        var mensagens = new[]
        {
            "Usuário logado com sucesso",
            "Falha na conexão com banco de dados",
            "Cache invalidado",
            "Operação concluída",
            "Memória baixa detectada",
            "Backup realizado",
            "Erro de validação",
            "Sistema iniciado"
        };

        for (int i = 0; i < 25; i++)
        {
            var tipo = tiposLog[Random.Shared.Next(tiposLog.Length)];
            var mensagem = mensagens[Random.Shared.Next(mensagens.Length)];
            
            await _monitorLogs.AdicionarLogAsync(new LogEntry(tipo, $"{mensagem} (#{i + 1})"));
            await Task.Delay(200);
        }

        await Task.Delay(1000);
        _monitorLogs.CompletarMonitoramento();
        
        try
        {
            await consumerTask;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando cancela por timeout
        }

        Console.WriteLine("✅ Demonstração de Channel bounded concluída\n");
    }

    private async Task DemonstrarMultiplosConsumers()
    {
        Console.WriteLine("🤖 3. MÚLTIPLOS CONSUMERS - Processamento de Imagens");
        Console.WriteLine("──────────────────────────────────────────────────────");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        
        // Inicia múltiplos workers
        var workers = new[]
        {
            Task.Run(() => _processadorImagens.ProcessarImagensAsync("Worker-1", cts.Token)),
            Task.Run(() => _processadorImagens.ProcessarImagensAsync("Worker-2", cts.Token)),
            Task.Run(() => _processadorImagens.ProcessarImagensAsync("Worker-3", cts.Token))
        };

        // Simula jobs de processamento
        var tipos = new[] { "Redimensionar", "Filtro", "Compressão" };
        
        for (int i = 1; i <= 15; i++)
        {
            var tipo = tipos[Random.Shared.Next(tipos.Length)];
            var job = new ImagemJob(i, $"imagem_{i:D3}.jpg", tipo);
            
            await _processadorImagens.AdicionarJobAsync(job);
            await Task.Delay(300);
        }

        await Task.Delay(2000);
        _processadorImagens.CompletarProcessamento();
        
        try
        {
            await Task.WhenAll(workers);
        }
        catch (OperationCanceledException)
        {
            // Esperado quando cancela por timeout
        }

        Console.WriteLine("✅ Demonstração de múltiplos consumers concluída\n");
    }

    private async Task DemonstrarComparacaoComQueue()
    {
        Console.WriteLine("⚡ 4. COMPARAÇÃO: Channel vs ConcurrentQueue");
        Console.WriteLine("──────────────────────────────────────────────");
        
        const int itemCount = 10000;
        
        // Teste com Channel
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var channel = Channel.CreateUnbounded<int>();
        var writer = channel.Writer;
        var reader = channel.Reader;
        
        var producerTask = Task.Run(async () =>
        {
            for (int i = 0; i < itemCount; i++)
            {
                await writer.WriteAsync(i);
            }
            writer.Complete();
        });
        
        var consumerTask = Task.Run(async () =>
        {
            var count = 0;
            await foreach (var item in reader.ReadAllAsync())
            {
                count++;
            }
            return count;
        });
        
        await Task.WhenAll(producerTask, consumerTask);
        sw.Stop();
        var channelTime = sw.ElapsedMilliseconds;
        
        // Teste com ConcurrentQueue
        sw.Restart();
        var queue = new System.Collections.Concurrent.ConcurrentQueue<int>();
        var completed = false;
        
        var queueProducer = Task.Run(() =>
        {
            for (int i = 0; i < itemCount; i++)
            {
                queue.Enqueue(i);
            }
            completed = true;
        });
        
        var queueConsumer = Task.Run(() =>
        {
            var count = 0;
            while (!completed || !queue.IsEmpty)
            {
                if (queue.TryDequeue(out _))
                {
                    count++;
                }
                else
                {
                    Task.Delay(1).Wait();
                }
            }
            return count;
        });
        
        await Task.WhenAll(queueProducer, queueConsumer);
        sw.Stop();
        var queueTime = sw.ElapsedMilliseconds;
        
        Console.WriteLine($"📈 Resultados para {itemCount:N0} itens:");
        Console.WriteLine($"   Channel: {channelTime}ms");
        Console.WriteLine($"   ConcurrentQueue: {queueTime}ms");
        Console.WriteLine($"   Diferença: {Math.Abs(channelTime - queueTime)}ms");
        
        if (channelTime < queueTime)
            Console.WriteLine("🏆 Channel foi mais rápido!");
        else
            Console.WriteLine("🏆 ConcurrentQueue foi mais rápido!");
            
        Console.WriteLine();
    }

    private void MostrarResumoBoasPraticas()
    {
        Console.WriteLine("📋 5. RESUMO DAS BOAS PRÁTICAS");
        Console.WriteLine("─────────────────────────────────");
        Console.WriteLine("✅ Use Channel.CreateUnbounded() para alta throughput");
        Console.WriteLine("✅ Use Channel.CreateBounded() para controle de memória");
        Console.WriteLine("✅ Configure FullMode adequadamente:");
        Console.WriteLine("   • Wait: Bloqueia producer quando cheio");
        Console.WriteLine("   • DropNewest: Descarta itens mais novos");
        Console.WriteLine("   • DropOldest: Descarta itens mais antigos");
        Console.WriteLine("✅ Use multiple consumers para paralelismo");
        Console.WriteLine("✅ Sempre chame Writer.Complete() ao finalizar");
        Console.WriteLine("✅ Use ReadAllAsync() com CancellationToken");
        Console.WriteLine("✅ Channel é melhor que ConcurrentQueue para async/await");

        Console.WriteLine("\n🎯 6. QUANDO USAR CHANNELS");
        Console.WriteLine("──────────────────────────────");
        Console.WriteLine("🔹 Producer-Consumer assíncrono");
        Console.WriteLine("🔹 Background processing/job queues");
        Console.WriteLine("🔹 Event streaming");
        Console.WriteLine("🔹 Pipeline de dados");
        Console.WriteLine("🔹 Substituição async para BlockingCollection");

        Console.WriteLine("\n=== Demonstração concluída ===");
    }
}
