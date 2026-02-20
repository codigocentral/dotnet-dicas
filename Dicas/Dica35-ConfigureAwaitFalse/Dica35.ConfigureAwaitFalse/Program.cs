using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("==== Dica 35: ConfigureAwait(false) - Evitando Deadlocks ====\n");

// Setup do host com logging
using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<BibliotecaService>();
        services.AddSingleton<WebApiService>();
        services.AddSingleton<ProcessamentoService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var bibliotecaService = host.Services.GetRequiredService<BibliotecaService>();
var webApiService = host.Services.GetRequiredService<WebApiService>();
var processamentoService = host.Services.GetRequiredService<ProcessamentoService>();

// 1. Problema: Biblioteca SEM ConfigureAwait(false)
Console.WriteLine("🚨 1. PROBLEMA - Biblioteca sem ConfigureAwait(false):");
Console.WriteLine("────────────────────────────────────────────────────────");
logger.LogInformation("Iniciando teste sem ConfigureAwait(false)");

try
{
    // Simular contexto de UI (pode causar deadlock)
    await SimularContextoUI(async () =>
    {
        var resultado = await bibliotecaService.OperacaoSemConfigureAwait("dados importantes");
        Console.WriteLine($"  Resultado: {resultado}");
    });
}
catch (Exception ex)
{
    Console.WriteLine($"  ❌ Erro: {ex.Message}");
}

// 2. Solução: Biblioteca COM ConfigureAwait(false)
Console.WriteLine("\n✅ 2. SOLUÇÃO - Biblioteca com ConfigureAwait(false):");
Console.WriteLine("─────────────────────────────────────────────────────");
logger.LogInformation("Iniciando teste com ConfigureAwait(false)");

await SimularContextoUI(async () =>
{
    var resultado = await bibliotecaService.OperacaoComConfigureAwait("dados importantes");
    Console.WriteLine($"  Resultado: {resultado}");
});

// 3. Web API - quando NÃO usar ConfigureAwait(false)
Console.WriteLine("\n🌐 3. WEB API - Quando NÃO usar ConfigureAwait(false):");
Console.WriteLine("──────────────────────────────────────────────────────");
logger.LogInformation("Testando Web API sem ConfigureAwait");

var resultadoApi = await webApiService.ProcessarRequestSemConfigureAwait("requisição web");
Console.WriteLine($"  API Response: {resultadoApi}");

// 4. Processamento batch - usando ConfigureAwait(false)
Console.WriteLine("\n📦 4. PROCESSAMENTO BATCH - Com ConfigureAwait(false):");
Console.WriteLine("─────────────────────────────────────────────────────");
logger.LogInformation("Processando batch de itens");

var itens = Enumerable.Range(1, 5).Select(i => $"item-{i}").ToArray();
var resultados = await processamentoService.ProcessarBatchComConfigureAwait(itens);

foreach (var resultado in resultados)
{
    Console.WriteLine($"  Processado: {resultado}");
}

// 5. Demonstração de Thread Context
Console.WriteLine("\n🧵 5. DEMONSTRAÇÃO DE THREAD CONTEXT:");
Console.WriteLine("────────────────────────────────────────");
await DemonstrarThreadContext();

// 6. Boas práticas resumidas
Console.WriteLine("\n📋 6. RESUMO DAS BOAS PRÁTICAS:");
Console.WriteLine("─────────────────────────────────");
Console.WriteLine("✅ USE ConfigureAwait(false) em bibliotecas");
Console.WriteLine("✅ USE ConfigureAwait(false) em processamento batch");
Console.WriteLine("✅ USE ConfigureAwait(false) quando não precisa do contexto");
Console.WriteLine("❌ NÃO USE em Web APIs (ASP.NET Core não tem SynchronizationContext)");
Console.WriteLine("❌ NÃO USE quando precisar acessar UI após await");
Console.WriteLine("❌ NÃO USE quando precisar do HttpContext após await");

// 7. Teste de performance
Console.WriteLine("\n⚡ 7. TESTE DE PERFORMANCE:");
Console.WriteLine("──────────────────────────────");
await TestarPerformance();

Console.WriteLine("\n=== Demonstração concluída ===");

static async Task SimularContextoUI(Func<Task> operacao)
{
    // Simula um contexto de UI/WinForms que pode causar deadlock
    var originalContext = SynchronizationContext.Current;
    
    try
    {
        // Instalar contexto personalizado que simula UI
        var uiContext = new CustomSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(uiContext);
        
        Console.WriteLine($"  Context antes: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        
        await operacao();
    }
    finally
    {
        SynchronizationContext.SetSynchronizationContext(originalContext);
    }
}

static async Task DemonstrarThreadContext()
{
    Console.WriteLine($"  Thread inicial: {Thread.CurrentThread.ManagedThreadId}");
    
    // Sem ConfigureAwait(false) - tenta manter contexto
    await Task.Delay(100);
    Console.WriteLine($"  Após await sem ConfigureAwait: {Thread.CurrentThread.ManagedThreadId}");
    
    // Com ConfigureAwait(false) - não preserva contexto
    await Task.Delay(100).ConfigureAwait(false);
    Console.WriteLine($"  Após await com ConfigureAwait(false): {Thread.CurrentThread.ManagedThreadId}");
    
    // Voltar para contexto original se necessário
    await Task.Delay(100);
    Console.WriteLine($"  Thread final: {Thread.CurrentThread.ManagedThreadId}");
}

static async Task TestarPerformance()
{
    const int iterations = 1000;
    
    // Teste sem ConfigureAwait(false)
    var startSem = DateTime.Now;
    for (int i = 0; i < iterations; i++)
    {
        await Task.Delay(1);
    }
    var tempoSem = DateTime.Now - startSem;
    
    // Teste com ConfigureAwait(false)
    var startCom = DateTime.Now;
    for (int i = 0; i < iterations; i++)
    {
        await Task.Delay(1).ConfigureAwait(false);
    }
    var tempoCom = DateTime.Now - startCom;
    
    Console.WriteLine($"  Sem ConfigureAwait(false): {tempoSem.TotalMilliseconds:F1}ms");
    Console.WriteLine($"  Com ConfigureAwait(false): {tempoCom.TotalMilliseconds:F1}ms");
    Console.WriteLine($"  Diferença: {(tempoSem.TotalMilliseconds - tempoCom.TotalMilliseconds):F1}ms");
}

// Classes de serviço

public class BibliotecaService
{
    private readonly ILogger<BibliotecaService> _logger;

    public BibliotecaService(ILogger<BibliotecaService> logger)
    {
        _logger = logger;
    }

    // ❌ PROBLEMA: Biblioteca sem ConfigureAwait(false)
    public async Task<string> OperacaoSemConfigureAwait(string input)
    {
        _logger.LogInformation("Iniciando operação SEM ConfigureAwait(false)");
        
        // Simulação de operações I/O sem ConfigureAwait(false)
        await Task.Delay(50); // Pode tentar capturar contexto
        var dados = await CarregarDadosExternos(input); // Pode tentar capturar contexto
        await SalvarResultado(dados); // Pode tentar capturar contexto
        
        return $"Processado: {dados}";
    }

    // ✅ SOLUÇÃO: Biblioteca com ConfigureAwait(false)
    public async Task<string> OperacaoComConfigureAwait(string input)
    {
        _logger.LogInformation("Iniciando operação COM ConfigureAwait(false)");
        
        // Simulação de operações I/O com ConfigureAwait(false)
        await Task.Delay(50).ConfigureAwait(false);
        var dados = await CarregarDadosExternos(input).ConfigureAwait(false);
        await SalvarResultado(dados).ConfigureAwait(false);
        
        return $"Processado: {dados}";
    }

    private async Task<string> CarregarDadosExternos(string input)
    {
        // Simula carregamento de banco/API
        await Task.Delay(30).ConfigureAwait(false);
        return $"dados-{input}";
    }

    private async Task SalvarResultado(string dados)
    {
        // Simula salvamento
        await Task.Delay(20).ConfigureAwait(false);
        _logger.LogInformation("Dados salvos: {dados}", dados);
    }
}

public class WebApiService
{
    private readonly ILogger<WebApiService> _logger;

    public WebApiService(ILogger<WebApiService> logger)
    {
        _logger = logger;
    }

    // ✅ CORRETO: Web API sem ConfigureAwait(false)
    // ASP.NET Core não tem SynchronizationContext, então não há benefício
    public async Task<string> ProcessarRequestSemConfigureAwait(string request)
    {
        _logger.LogInformation("Processando request de Web API");
        
        // Em Web APIs, não precisamos de ConfigureAwait(false)
        await Task.Delay(100);
        var dados = await ConsultarBancoDados(request);
        await EnviarNotificacao(dados);
        
        return $"API Response: {dados}";
    }

    private async Task<string> ConsultarBancoDados(string request)
    {
        await Task.Delay(50);
        return $"db-result-{request}";
    }

    private async Task EnviarNotificacao(string dados)
    {
        await Task.Delay(30);
        _logger.LogInformation("Notificação enviada para: {dados}", dados);
    }
}

public class ProcessamentoService
{
    private readonly ILogger<ProcessamentoService> _logger;

    public ProcessamentoService(ILogger<ProcessamentoService> logger)
    {
        _logger = logger;
    }

    // ✅ CORRETO: Processamento batch com ConfigureAwait(false)
    public async Task<string[]> ProcessarBatchComConfigureAwait(string[] itens)
    {
        _logger.LogInformation("Iniciando processamento batch de {count} itens", itens.Length);
        
        var resultados = new List<string>();
        
        foreach (var item in itens)
        {
            // Use ConfigureAwait(false) em processamento batch
            var resultado = await ProcessarItemIndividual(item).ConfigureAwait(false);
            resultados.Add(resultado);
        }
        
        // Processamento paralelo também se beneficia
        var tarefasParalelas = itens.Select(async item =>
        {
            await Task.Delay(Random.Shared.Next(10, 50)).ConfigureAwait(false);
            return $"parallel-{item}";
        });
        
        var resultadosParalelos = await Task.WhenAll(tarefasParalelas);
        resultados.AddRange(resultadosParalelos);
        
        return resultados.ToArray();
    }

    private async Task<string> ProcessarItemIndividual(string item)
    {
        // Simula processamento pesado
        await Task.Delay(Random.Shared.Next(20, 100)).ConfigureAwait(false);
        return $"processed-{item}";
    }
}

// SynchronizationContext personalizado para simular UI
public class CustomSynchronizationContext : SynchronizationContext
{
    private readonly object _lock = new();
    private readonly Queue<(SendOrPostCallback callback, object? state)> _queue = new();
    private volatile bool _isRunning = true;

    public CustomSynchronizationContext()
    {
        // Simula message pump da UI
        Task.Run(MessagePump);
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_lock)
        {
            _queue.Enqueue((d, state));
            Monitor.Pulse(_lock);
        }
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        // Implementação simples - em UI real seria mais complexo
        d(state);
    }

    private void MessagePump()
    {
        while (_isRunning)
        {
            (SendOrPostCallback callback, object? state) item;
            
            lock (_lock)
            {
                while (_queue.Count == 0 && _isRunning)
                {
                    Monitor.Wait(_lock, 100);
                }
                
                if (!_isRunning || _queue.Count == 0)
                    continue;
                
                item = _queue.Dequeue();
            }
            
            try
            {
                item.callback(item.state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no message pump: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        lock (_lock)
        {
            Monitor.PulseAll(_lock);
        }
    }
}
