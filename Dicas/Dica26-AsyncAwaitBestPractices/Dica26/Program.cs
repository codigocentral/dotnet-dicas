using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dica26;

/// <summary>
/// Dica 26: Async/Await Best Practices
/// 
/// Esta dica demonstra as melhores práticas ao trabalhar com async/await em C#,
/// incluindo ConfigureAwait, tratamento de exceções, evitar deadlocks e otimizações de performance.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddScoped<AsyncBestPracticesService>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var service = host.Services.GetRequiredService<AsyncBestPracticesService>();

        logger.LogInformation("=== Dica 26: Async/Await Best Practices ===\n");

        // 1. Melhores Práticas ConfigureAwait
        logger.LogInformation("1. Melhores Práticas ConfigureAwait:");
        await service.DemonstrarConfigureAwaitAsync();

        // 2. Tratamento de Exceções em Métodos Async
        logger.LogInformation("\n2. Tratamento de Exceções em Métodos Async:");
        await service.DemonstrarTratamentoExcecoesAsync();

        // 3. Evitando Async Void
        logger.LogInformation("\n3. Evitando Async Void:");
        await service.DemonstrarProblemasAsyncVoidAsync();

        // 4. Task.WhenAll vs Múltiplos Awaits
        logger.LogInformation("\n4. Task.WhenAll vs Múltiplos Awaits:");
        await service.DemonstrarTaskWhenAllAsync();

        // 5. Melhores Práticas CancellationToken
        logger.LogInformation("\n5. Melhores Práticas CancellationToken:");
        await service.DemonstrarCancellationTokenAsync();

        // 6. ValueTask para Hot Paths
        logger.LogInformation("\n6. ValueTask para Hot Paths:");
        await service.DemonstrarValueTaskAsync();

        // 7. Padrões Fire-and-Forget
        logger.LogInformation("\n7. Padrões Fire-and-Forget:");
        await service.DemonstrarFireAndForgetAsync();

        // 8. Timeout HTTP com CancellationToken
        logger.LogInformation("\n8. Timeout HTTP com CancellationToken:");
        await service.DemonstrarTimeoutHttpAsync();

        logger.LogInformation("\n=== Async/Await Best Practices Demo Completed ===");
    }
}

public class AsyncBestPracticesService
{
    private readonly ILogger<AsyncBestPracticesService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _cache;

    public AsyncBestPracticesService(ILogger<AsyncBestPracticesService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _cache = new Dictionary<string, string>();
    }

    /// <summary>
    /// Demonstra o uso correto de ConfigureAwait
    /// </summary>
    public async Task DemonstrarConfigureAwaitAsync()
    {
        _logger.LogInformation("   📚 ConfigureAwait: Library vs Application code");

        // ❌ RUIM: Em código de biblioteca, não usar ConfigureAwait(false)
        // pode causar deadlocks em aplicações SynchronizationContext
        await MetodoBibliotecaRuimAsync();

        // ✅ BOM: Em código de biblioteca, sempre usar ConfigureAwait(false)
        await MetodoBibliotecaBomAsync();

        // 📝 NOTA: Em aplicações (não bibliotecas), ConfigureAwait(false) pode ser opcional
        // mas ainda é uma boa prática para evitar overhead desnecessário
        await MetodoAplicacaoAsync();
    }

    /// <summary>
    /// Método de biblioteca SEM ConfigureAwait - pode causar deadlock
    /// </summary>
    private async Task MetodoBibliotecaRuimAsync()
    {
        _logger.LogInformation("      ❌ Método biblioteca SEM ConfigureAwait");

        // Simula operação I/O - PERIGOSO em biblioteca
        await Task.Delay(100); // Sem ConfigureAwait(false)

        _logger.LogInformation("      ❌ Completo - risco de deadlock");
    }

    /// <summary>
    /// Método de biblioteca COM ConfigureAwait - seguro
    /// </summary>
    private async Task MetodoBibliotecaBomAsync()
    {
        _logger.LogInformation("      ✅ Método biblioteca COM ConfigureAwait(false)");

        // Simula operação I/O - SEGURO em biblioteca
        await Task.Delay(100).ConfigureAwait(false);

        _logger.LogInformation("      ✅ Completo - sem risco de deadlock");
    }

    /// <summary>
    /// Método de aplicação - ConfigureAwait opcional mas recomendado
    /// </summary>
    private async Task MetodoAplicacaoAsync()
    {
        _logger.LogInformation("      📱 Método de aplicação - ConfigureAwait opcional");

        // Em aplicações, ConfigureAwait(false) é opcional mas recomendado para performance
        await Task.Delay(50).ConfigureAwait(false);

        _logger.LogInformation("      📱 Método de aplicação completo");
    }

    /// <summary>
    /// Demonstra tratamento correto de exceções em métodos async
    /// </summary>
    public async Task DemonstrarTratamentoExcecoesAsync()
    {
        _logger.LogInformation("   🔥 Padrões de Tratamento de Exceções");

        // 1. Tratamento de exceções em métodos async
        await TratarExcecoesCorretamenteAsync();

        // 2. AggregateException com Task.WhenAll
        await TratarExcecoesAgregadasAsync();

        // 3. Propagação de exceções
        await DemonstrarPropagacaoExcecaoAsync();
    }

    private async Task TratarExcecoesCorretamenteAsync()
    {
        try
        {
            _logger.LogInformation("      🎯 Chamando método async que lança exceção...");
            await MetodoAsyncQueLancaExcecaoAsync();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation($"      ✅ Exceção esperada capturada: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"      ❌ Exceção inesperada: {ex.Message}");
        }
    }

    private async Task TratarExcecoesAgregadasAsync()
    {
        try
        {
            _logger.LogInformation("      🎯 Usando Task.WhenAll com múltiplas tasks que falham...");

            var tasks = new[]
            {
                MetodoAsyncQueLancaExcecaoAsync(),
                MetodoAsyncQueLancaExcecaoAsync(),
                Task.Delay(100) // Uma task que não falha
            };

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"      ⚠️  Primeira exceção capturada: {ex.Message}");
            _logger.LogInformation("      📝 Nota: Task.WhenAll lança apenas a primeira exceção");
        }
    }

    private async Task DemonstrarPropagacaoExcecaoAsync()
    {
        _logger.LogInformation("      🔄 Propagação de exceção pela cadeia async");

        try
        {
            await MetodoAsync1();
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"      ✅ Exceção propagada corretamente: {ex.Message}");
        }
    }

    private async Task MetodoAsync1()
    {
        await MetodoAsync2();
    }

    private async Task MetodoAsync2()
    {
        await MetodoAsync3();
    }

    private async Task MetodoAsync3()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("Exceção de chamada async profunda");
    }

    private async Task MetodoAsyncQueLancaExcecaoAsync()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("Exceção async simulada");
    }

    /// <summary>
    /// Demonstra por que evitar async void
    /// </summary>
    public async Task DemonstrarProblemasAsyncVoidAsync()
    {
        _logger.LogInformation("   ⚠️  Problemas com Async Void");

        // ✅ BOM: Async Task para métodos que podem ser awaited
        await MetodoAsyncBomAsync();

        // ❌ RUIM: Async void - não pode ser awaited, exceções não podem ser capturadas
        _logger.LogInformation("      ❌ Chamando método async void (fire and forget)");
        MetodoAsyncVoidRuim(); // Não podemos await isso!

        // Espera um pouco para ver se a async void completa
        await Task.Delay(200);

        _logger.LogInformation("      📝 Async void completou (talvez), mas não pudemos esperar");
    }

    private async Task MetodoAsyncBomAsync()
    {
        _logger.LogInformation("      ✅ Método async Task bom");
        await Task.Delay(100);
        _logger.LogInformation("      ✅ Método async bom completou");
    }

    private async void MetodoAsyncVoidRuim()
    {
        _logger.LogInformation("      ❌ Método async void ruim iniciado");
        await Task.Delay(150);
        _logger.LogInformation("      ❌ Método async void ruim completou");

        // Se essa linha lançasse uma exceção, seria muito difícil de capturar!
        // throw new InvalidOperationException("Exceção não tratada em async void!");
    }

    /// <summary>
    /// Demonstra Task.WhenAll vs multiple awaits sequenciais
    /// </summary>
    public async Task DemonstrarTaskWhenAllAsync()
    {
        _logger.LogInformation("   🚀 Task.WhenAll vs Awaits Sequenciais");

        // ❌ RUIM: Awaits sequenciais - muito lento
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await AwaitsSequenciaisAsync();
        stopwatch.Stop();
        _logger.LogInformation($"      ❌ Awaits sequenciais levaram: {stopwatch.ElapsedMilliseconds}ms");

        // ✅ BOM: Task.WhenAll - execução paralela
        stopwatch.Restart();
        await ParaleloComWhenAllAsync();
        stopwatch.Stop();
        _logger.LogInformation($"      ✅ Task.WhenAll levou: {stopwatch.ElapsedMilliseconds}ms");
    }

    private async Task AwaitsSequenciaisAsync()
    {
        _logger.LogInformation("      🐌 Iniciando operações sequenciais...");

        await SimularChamadaApiAsync("Serviço A", 100);
        await SimularChamadaApiAsync("Serviço B", 100);
        await SimularChamadaApiAsync("Serviço C", 100);

        _logger.LogInformation("      🐌 Operações sequenciais completadas");
    }

    private async Task ParaleloComWhenAllAsync()
    {
        _logger.LogInformation("      🚀 Iniciando operações paralelas...");

        var tasks = new[]
        {
            SimularChamadaApiAsync("Serviço A", 100),
            SimularChamadaApiAsync("Serviço B", 100),
            SimularChamadaApiAsync("Serviço C", 100)
        };

        await Task.WhenAll(tasks);

        _logger.LogInformation("      🚀 Operações paralelas completadas");
    }

    private async Task SimularChamadaApiAsync(string nomeServico, int delayMs)
    {
        _logger.LogInformation($"         📡 Chamando {nomeServico}...");
        await Task.Delay(delayMs);
        _logger.LogInformation($"         ✅ {nomeServico} respondeu");
    }

    /// <summary>
    /// Demonstra uso correto de CancellationToken
    /// </summary>
    public async Task DemonstrarCancellationTokenAsync()
    {
        _logger.LogInformation("   🛑 Melhores Práticas CancellationToken");

        using var cts = new CancellationTokenSource();

        // Cancela após 500ms
        cts.CancelAfter(500);

        try
        {
            await OperacaoDemoradaAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("      ✅ Operação foi cancelada com sucesso");
        }

        // Demonstra propagação de CancellationToken
        await DemonstrarPropagacaoCancellationAsync();
    }

    private async Task OperacaoDemoradaAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("      🔄 Iniciando operação demorada...");

        for (int i = 0; i < 10; i++)
        {
            // ✅ BOM: Verificar cancellation token regularmente
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation($"         Passo {i + 1}/10");

            // ✅ BOM: Passar token para operações async
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation("      ✅ Operação demorada completada");
    }

    private async Task DemonstrarPropagacaoCancellationAsync()
    {
        _logger.LogInformation("      🔗 Propagação de CancellationToken");

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200);

        try
        {
            await MetodoQuePropagaTokenAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("      ✅ Cancelamento propagado pela cadeia de chamadas");
        }
    }

    private async Task MetodoQuePropagaTokenAsync(CancellationToken cancellationToken)
    {
        // ✅ BOM: Sempre propagar CancellationToken
        await OutroMetodoAsync(cancellationToken);
    }

    private async Task OutroMetodoAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken); // Vai ser cancelado
    }

    /// <summary>
    /// Demonstra quando usar ValueTask vs Task
    /// </summary>
    public async Task DemonstrarValueTaskAsync()
    {
        _logger.LogInformation("   ⚡ ValueTask para Hot Paths");

        // Primeira chamada - cache miss
        _logger.LogInformation("      🔍 Primeira chamada (cache miss):");
        var result1 = await ObterDadosCacheAsync("chave1");
        _logger.LogInformation($"         Resultado: {result1}");

        // Segunda chamada - cache hit (ValueTask optimization)
        _logger.LogInformation("      🎯 Segunda chamada (cache hit):");
        var result2 = await ObterDadosCacheAsync("chave1");
        _logger.LogInformation($"         Resultado: {result2}");

        // Demonstra diferença de alocação
        await CompararAlocacoesTaskVsValueTaskAsync();
    }

    /// <summary>
    /// Método que retorna ValueTask para otimizar casos síncronos (cache hit)
    /// </summary>
    private ValueTask<string> ObterDadosCacheAsync(string chave)
    {
        // ✅ Cache hit - retorna ValueTask síncrono (sem alocação de Task)
        if (_cache.TryGetValue(chave, out var valorCache))
        {
            _logger.LogInformation("         💨 Cache HIT - retorno síncrono ValueTask");
            return ValueTask.FromResult(valorCache);
        }

        // Cache miss - precisa fazer operação async
        _logger.LogInformation("         🐌 Cache MISS - operação async necessária");
        return ObterDadosInternoAsync(chave);
    }

    private async ValueTask<string> ObterDadosInternoAsync(string chave)
    {
        // Simula operação I/O
        await Task.Delay(100);

        var dados = $"Dados para {chave} - {DateTime.Now:HH:mm:ss}";
        _cache[chave] = dados;

        return dados;
    }

    private async Task CompararAlocacoesTaskVsValueTaskAsync()
    {
        _logger.LogInformation("      📊 Comparação de alocação Task vs ValueTask:");

        // Task sempre aloca, mesmo para retornos síncronos
        var taskResult = await ObterDadosComTaskAsync("em_cache");
        _logger.LogInformation($"         Resultado Task: {taskResult}");

        // ValueTask não aloca para retornos síncronos
        var valueTaskResult = await ObterDadosComValueTaskAsync("em_cache");
        _logger.LogInformation($"         Resultado ValueTask: {valueTaskResult}");

        _logger.LogInformation("      📝 ValueTask evita alocação para retornos síncronos");

        // Benchmark real: 10.000 cache hits
        await BenchmarkCacheHitsAsync();
    }

    private async Task BenchmarkCacheHitsAsync()
    {
        _logger.LogInformation("\n      🔬 BENCHMARK: 10.000 cache hits (ValueTask vs Task)");

        // Preparar cache
        _cache["chave_benchmark"] = "valor_cache";

        // Benchmark Task
        var taskStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            await ObterDadosComTaskAsync("chave_benchmark");
        }
        taskStopwatch.Stop();
        _logger.LogInformation($"         Task (10K hits): {taskStopwatch.ElapsedMilliseconds}ms");

        // Benchmark ValueTask
        var valueTaskStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            await ObterDadosComValueTaskAsync("chave_benchmark");
        }
        valueTaskStopwatch.Stop();
        _logger.LogInformation($"         ValueTask (10K hits): {valueTaskStopwatch.ElapsedMilliseconds}ms");

        var improvement = ((double)(taskStopwatch.ElapsedMilliseconds - valueTaskStopwatch.ElapsedMilliseconds) / taskStopwatch.ElapsedMilliseconds) * 100;
        _logger.LogInformation($"         💡 ValueTask foi ~{improvement:F1}% mais rápido (menos alocações)");
    }

    private Task<string> ObterDadosComTaskAsync(string chave)
    {
        // ❌ Task sempre aloca, mesmo para retorno imediato
        if (_cache.TryGetValue(chave, out var valor))
        {
            return Task.FromResult(valor); // Ainda aloca um Task
        }

        return Task.FromResult($"Novos dados para {chave}");
    }

    private ValueTask<string> ObterDadosComValueTaskAsync(string chave)
    {
        // ✅ ValueTask não aloca para retorno imediato
        if (_cache.TryGetValue(chave, out var valor))
        {
            return ValueTask.FromResult(valor); // Sem alocação!
        }

        return ValueTask.FromResult($"Novos dados para {chave}");
    }

    /// <summary>
    /// Demonstra padrões seguros de fire-and-forget
    /// </summary>
    public async Task DemonstrarFireAndForgetAsync()
    {
        _logger.LogInformation("   🔥 Padrões Fire-and-Forget");

        // ❌ RUIM: Fire-and-forget sem supervisão
        _logger.LogInformation("\n      ❌ RUIM: Fire-and-forget sem supervisão");
        _logger.LogInformation("      // _ = TarefaBackgroundAsync(); // Exceções serão engolidas!");

        // ✅ BOM: Fire-and-forget supervisionado
        _logger.LogInformation("\n      ✅ BOM: Fire-and-forget supervisionado com Task.Run");
        _ = Task.Run(async () =>
        {
            try
            {
                await TarefaBackgroundSupervisionadaAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tarefa background falhou");
            }
        });

        // ✅ MELHOR: Usar BackgroundService ou IHostedService
        _logger.LogInformation("      💡 MELHOR: Use BackgroundService/IHostedService em produção");
        _logger.LogInformation("      // public class MeuBackgroundService : BackgroundService { ... }");

        // Aguarda um pouco para ver o resultado da tarefa em background
        await Task.Delay(300);
    }

    private async Task TarefaBackgroundSupervisionadaAsync()
    {
        _logger.LogInformation("      🔄 Tarefa background iniciada (supervisionada)");
        await Task.Delay(200);
        _logger.LogInformation("      ✅ Tarefa background completada com sucesso");

        // Se descomentar, a exceção será capturada no try/catch
        // throw new InvalidOperationException("Erro simulado de background");
    }

    /// <summary>
    /// Demonstra timeout HTTP com CancellationToken
    /// </summary>
    public async Task DemonstrarTimeoutHttpAsync()
    {
        _logger.LogInformation("   ⏱️  Timeout HTTP com CancellationToken");

        // Simula requisição HTTP lenta
        _logger.LogInformation("\n      📡 Chamando API lenta com timeout de 2s...");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        try
        {
            var result = await SimularChamadaApiLentaAsync(cts.Token);
            _logger.LogInformation($"      ✅ API respondeu: {result}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("      ⏰ Requisição TIMEOUT após 2 segundos");
        }

        // Exemplo com HttpClient real (comentado para não fazer chamadas externas)
        _logger.LogInformation("\n      💡 Com HttpClient real:");
        _logger.LogInformation("      // using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));");
        _logger.LogInformation("      // var response = await httpClient.GetAsync(url, cts.Token);");

        // Demonstra múltiplos timeouts
        await DemonstrarMultiplosTimeoutsAsync();
    }

    private async Task<string> SimularChamadaApiLentaAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("      🐌 Simulando API lenta (3 segundos de atraso)...");

        // API demora 3 segundos, mas timeout é 2 segundos
        await Task.Delay(3000, cancellationToken);

        return "Resposta da API (não alcançada devido ao timeout)";
    }

    private async Task DemonstrarMultiplosTimeoutsAsync()
    {
        _logger.LogInformation("\n      🔢 Testando múltiplos endpoints com timeouts individuais:");

        var tasks = new[]
        {
            ChamarApiComTimeoutAsync("API A", delayMs: 500, timeoutSegundos: 1),
            ChamarApiComTimeoutAsync("API B", delayMs: 1500, timeoutSegundos: 1),
            ChamarApiComTimeoutAsync("API C", delayMs: 800, timeoutSegundos: 2)
        };

        var results = await Task.WhenAll(tasks);

        _logger.LogInformation("\n      📊 Resultados:");
        foreach (var result in results)
        {
            _logger.LogInformation($"         {result}");
        }
    }

    private async Task<string> ChamarApiComTimeoutAsync(string nomeApi, int delayMs, int timeoutSegundos)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSegundos));

        try
        {
            await Task.Delay(delayMs, cts.Token);
            return $"{nomeApi}: ✅ Sucesso ({delayMs}ms < {timeoutSegundos}s)";
        }
        catch (OperationCanceledException)
        {
            return $"{nomeApi}: ⏰ Timeout ({delayMs}ms > {timeoutSegundos}s)";
        }
    }
}
