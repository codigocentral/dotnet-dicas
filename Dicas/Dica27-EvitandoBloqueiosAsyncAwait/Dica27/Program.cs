using System.Collections.Concurrent;

Console.WriteLine("=== Dica 27: Evitando Bloqueios em Código Assíncrono ===\n");

var deadlockDemo = new DeadlockDemo();
var solutionDemo = new SolutionDemo();

// Demonstração dos problemas de deadlock
Console.WriteLine("1. Problemas comuns que causam deadlock:");
await deadlockDemo.DemonstrarProblemas();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstração das soluções
Console.WriteLine("2. Soluções para evitar deadlock:");
await solutionDemo.DemonstrarSolucoes();

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Resumo das práticas recomendadas:");
Console.WriteLine("✅ Use async/await em toda a cadeia (async all the way)");
Console.WriteLine("✅ Use ConfigureAwait(false) em bibliotecas");
Console.WriteLine("✅ Use Task.Run para operações CPU-intensive");
Console.WriteLine("✅ Use Task.WhenAll para paralelismo");
Console.WriteLine("✅ Controle a concorrência com processamento em lotes");
Console.WriteLine("❌ NUNCA use .Result, .Wait() ou GetAwaiter().GetResult()");
Console.WriteLine("❌ NUNCA misture código síncrono e assíncrono sem cuidado");

public class DeadlockDemo
{
    public async Task DemonstrarProblemas()
    {
        Console.WriteLine("⚠️  ATENÇÃO: Os exemplos abaixo podem causar deadlock em ambientes síncronos!");
        Console.WriteLine("Em aplicações reais, evite esses padrões:\n");

        // Exemplo problemático 1: .Result
        Console.WriteLine("❌ Problema 1: Usando .Result em contexto síncrono");
        Console.WriteLine("// NUNCA faça isso:");
        Console.WriteLine("var resultado = FazerTrabalhoAsync().Result; // Pode causar deadlock!");

        // Exemplo problemático 2: .Wait()
        Console.WriteLine("\n❌ Problema 2: Usando .Wait() em contexto síncrono");
        Console.WriteLine("// NUNCA faça isso:");
        Console.WriteLine("FazerTrabalhoAsync().Wait(); // Pode causar deadlock!");

        // Exemplo problemático 3: GetAwaiter().GetResult()
        Console.WriteLine("\n❌ Problema 3: Usando GetAwaiter().GetResult()");
        Console.WriteLine("// NUNCA faça isso:");
        Console.WriteLine("FazerTrabalhoAsync().GetAwaiter().GetResult(); // Pode causar deadlock!");

        // Demonstração CONTROLADA de deadlock (SEGURA para gravação)
        Console.WriteLine("\n🎬 DEMO: Simulando cenário de deadlock (com timeout para não travar):");
        await DemonstrarCenarioDeadlockControladoAsync();

        // Demonstração segura do padrão problemático
        Console.WriteLine("\n✅ Versão segura para demonstração:");
        try
        {
            var resultado = await FazerTrabalhoSeguroAsync();
            Console.WriteLine($"Resultado obtido de forma segura: {resultado}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro capturado: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstração CONTROLADA de cenário de deadlock - SEGURA para gravação!
    /// Usa timeout para evitar travar o programa.
    /// </summary>
    private async Task DemonstrarCenarioDeadlockControladoAsync()
    {
        Console.WriteLine("   🔴 Simulando: código bloqueante esperando async...");

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var tarefaDeadlock = Task.Run(() =>
        {
            try
            {
                Console.WriteLine("   ⏳ Thread tentando fazer .Result (MÁ PRÁTICA)...");
                Console.WriteLine("   ⏳ [Em produção isso travaria aqui indefinidamente]");

                // Simula o bloqueio - espera 2 segundos para mostrar o "travamento"
                Thread.Sleep(2000);

                Console.WriteLine("   ⏳ [Ainda esperando... CPU em baixa, fila crescendo...]");
                Thread.Sleep(500);

                // Em vez de realmente travar, mostramos que foi cancelado
                cts.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   💥 TIMEOUT! Em produção isso ficaria travado para sempre.");
            }
        }, cts.Token);

        try
        {
            await tarefaDeadlock;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("   ✅ Demo concluída - deadlock foi SIMULADO com segurança");
        }

        Console.WriteLine("   📊 Sintomas: CPU baixa, latência alta, threads bloqueadas");
    }

    private async Task<string> FazerTrabalhoSeguroAsync()
    {
        await Task.Delay(100); // Simula operação assíncrona
        return "Operação concluída";
    }

    /*
    ⚠️  CÓDIGO REAL DE DEADLOCK - COMENTADO PARA SEGURANÇA ⚠️

    NÃO execute isso durante a gravação! Vai TRAVAR seu programa.
    Use apenas para mostrar o código fonte na tela.

    private void RealDeadlockExample_NEVER_RUN_THIS()
    {
        // ❌ PERIGO: Isso vai travar em aplicações com SynchronizationContext
        // (WPF, WinForms, ASP.NET antigo)

        var task = GetDataWithContextAsync();
        var result = task.Result; // 💀 DEADLOCK AQUI
    }

    private async Task<string> GetDataWithContextAsync()
    {
        // Task precisa voltar ao contexto original
        await Task.Delay(1000);
        return "Nunca vai chegar aqui...";
    }
    */
}

public class SolutionDemo
{
    public async Task DemonstrarSolucoes()
    {
        Console.WriteLine("✅ Soluções recomendadas:\n");

        // Solução 1: ConfigureAwait(false)
        await DemonstrarConfigureAwait();

        // Solução 2: Async all the way
        await DemonstrarAsyncAteORaiz();

        // Solução 3: Task.Run para CPU-bound
        await DemonstrarTaskRun();

        // Solução 4: Processamento paralelo
        await DemonstrarProcessamentoParalelo();
    }

    private async Task DemonstrarConfigureAwait()
    {
        Console.WriteLine("1. Use ConfigureAwait(false) em bibliotecas:");

        var servico = new ServicoBiblioteca();
        var resultado = await servico.ProcessarDadosAsync("dados importantes");
        Console.WriteLine($"   Resultado: {resultado}");
    }

    private async Task DemonstrarAsyncAteORaiz()
    {
        Console.WriteLine("\n2. Async/await até a raiz (async all the way):");

        var dados = await CarregarDadosAsync();
        var processados = await ProcessarDadosAsync(dados);
        var salvos = await SalvarDadosAsync(processados);

        Console.WriteLine($"   Pipeline assíncrono concluído: {salvos}");
    }

    private async Task DemonstrarTaskRun()
    {
        Console.WriteLine("\n3. Use Task.Run para operações CPU-intensive:");

        var resultado = await Task.Run(() => ComputacaoIntensiva(1000000));
        Console.WriteLine($"   Resultado do cálculo intensivo: {resultado}");
    }

    private async Task DemonstrarProcessamentoParalelo()
    {
        Console.WriteLine("\n4. Processamento paralelo com Task.WhenAll:");

        var tarefas = new[]
        {
            ProcessarItemAsync("Item 1"),
            ProcessarItemAsync("Item 2"),
            ProcessarItemAsync("Item 3"),
            ProcessarItemAsync("Item 4")
        };

        var resultados = await Task.WhenAll(tarefas);
        Console.WriteLine($"   Processados {resultados.Length} itens em paralelo:");
        foreach (var resultado in resultados)
        {
            Console.WriteLine($"     - {resultado}");
        }

        // Demonstração de ThreadPool Starvation
        await DemonstrarEsgotamentoThreadPool();

        // Demonstração com processamento em lotes
        await DemonstrarProcessamentoEmLotes();
    }

    private async Task DemonstrarEsgotamentoThreadPool()
    {
        Console.WriteLine("\n🚨 ThreadPool Starvation: Problema vs Solução:");

        // ❌ PROBLEMA: Over-parallelism sem limite
        Console.WriteLine("\n   ❌ RUIM: 100 tasks sem controle (saturando recursos)");
        var cronometroRuim = System.Diagnostics.Stopwatch.StartNew();

        var tarefasDescontroladas = Enumerable.Range(1, 100)
            .Select(i => Task.Run(async () =>
            {
                await Task.Delay(50); // Simula I/O
                return i;
            }));

        await Task.WhenAll(tarefasDescontroladas);
        cronometroRuim.Stop();
        Console.WriteLine($"   ❌ Tempo: {cronometroRuim.ElapsedMilliseconds}ms (pode causar timeouts/429 em APIs reais)");

        // ✅ SOLUÇÃO: SemaphoreSlim limitando paralelismo
        Console.WriteLine("\n   ✅ BOM: Limitando paralelismo com SemaphoreSlim (max 10 concurrent)");
        var cronometroBom = System.Diagnostics.Stopwatch.StartNew();

        var semaforo = new SemaphoreSlim(10); // Máximo 10 operações simultâneas
        var tarefasControladas = Enumerable.Range(1, 100)
            .Select(async i =>
            {
                await semaforo.WaitAsync(); // Aguarda slot disponível
                try
                {
                    await Task.Delay(50); // Simula I/O
                    return i;
                }
                finally
                {
                    semaforo.Release(); // Libera slot
                }
            });

        var resultadosControlados = await Task.WhenAll(tarefasControladas);
        cronometroBom.Stop();
        Console.WriteLine($"   ✅ Tempo: {cronometroBom.ElapsedMilliseconds}ms (controlado, sem saturação)");
        Console.WriteLine($"   ✅ Processados: {resultadosControlados.Length} items com backpressure");
    }

    private async Task DemonstrarProcessamentoEmLotes()
    {
        Console.WriteLine("\n5. Processamento em lotes para controlar concorrência:");

        var itens = Enumerable.Range(1, 10).Select(i => $"Item {i}").ToArray();
        const int tamanhoLote = 3;

        var resultados = new ConcurrentBag<string>();

        for (int i = 0; i < itens.Length; i += tamanhoLote)
        {
            var lote = itens.Skip(i).Take(tamanhoLote);
            var tarefasLote = lote.Select(async item =>
            {
                var resultado = await ProcessarItemAsync(item);
                resultados.Add(resultado);
                return resultado;
            });

            await Task.WhenAll(tarefasLote);
            Console.WriteLine($"   Lote processado: {string.Join(", ", lote)}");
        }
    }

    private async Task<string> CarregarDadosAsync()
    {
        await Task.Delay(50); // Simula I/O
        return "dados carregados";
    }

    private async Task<string> ProcessarDadosAsync(string dados)
    {
        await Task.Delay(30); // Simula processamento
        return $"{dados} -> processados";
    }

    private async Task<string> SalvarDadosAsync(string dados)
    {
        await Task.Delay(40); // Simula I/O
        return $"{dados} -> salvos";
    }

    private async Task<string> ProcessarItemAsync(string item)
    {
        await Task.Delay(Random.Shared.Next(50, 200)); // Simula tempo variável
        return $"{item} processado";
    }

    private int ComputacaoIntensiva(int iteracoes)
    {
        int resultado = 0;
        for (int i = 0; i < iteracoes; i++)
        {
            resultado += i * i % 1000;
        }
        return resultado;
    }
}

public class ServicoBiblioteca
{
    public async Task<string> ProcessarDadosAsync(string dados)
    {
        // ✅ SEMPRE use ConfigureAwait(false) em bibliotecas
        await Task.Delay(100).ConfigureAwait(false);

        // Simula processamento adicional
        await FazerTrabalhoInternoAsync().ConfigureAwait(false);

        return $"Biblioteca processou: {dados}";
    }

    private async Task FazerTrabalhoInternoAsync()
    {
        // ✅ ConfigureAwait(false) em toda a cadeia
        await Task.Delay(50).ConfigureAwait(false);
    }
}
