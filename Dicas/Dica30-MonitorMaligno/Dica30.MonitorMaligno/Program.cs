using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Dica30.MonitorMaligno;

/// <summary>
/// Demonstração educativa: O que NÃO fazer!
/// ⚠️ NUNCA USE EM PRODUÇÃO! ⚠️
/// </summary>
internal class Program
{
    private static int _counter = 0;
    private static readonly object _lock = new object();

    static async Task Main(string[] args)
    {
        Console.WriteLine("🎭 Dica 30: O 'Monitor' Maligno - Demonstração Educativa");
        Console.WriteLine("⚠️  AVISO: Esta é uma demonstração do que NÃO fazer!");
        Console.WriteLine();

        // Demonstração 1: Comportamento correto (sem Monitor maligno)
        await DemonstrarComportamentoCorreto();
        
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        
        // Demonstração 2: Alternativas seguras e modernas
        await DemonstrarAlternativasSeguras();
        
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        
        // Demonstração 3: Padrões recomendados
        DemonstrarPadroesRecomendados();
    }

    private static async Task DemonstrarComportamentoCorreto()
    {
        Console.WriteLine("🔐 1. Comportamento CORRETO usando lock:");
        
        var tasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => IncrementarComLock(taskId));
        }
        
        await Task.WhenAll(tasks);
        Console.WriteLine($"   Resultado final: {_counter}");
        Console.WriteLine("   ✅ Sincronização correta!");
    }

    private static void IncrementarComLock(int taskId)
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (_lock)
            {
                _counter++;
            }
        }
        Console.WriteLine($"   Task {taskId} completada");
    }

    private static async Task DemonstrarAlternativasSeguras()
    {
        Console.WriteLine("🛡️  2. Alternativas SEGURAS e MODERNAS:");
        
        // Reset counter
        _counter = 0;
        
        // 2.1 SemaphoreSlim para controle assíncrono
        var semaphore = new SemaphoreSlim(1, 1);
        var semaphoreTasks = new Task[3];
        
        for (int i = 0; i < 3; i++)
        {
            int taskId = i;
            semaphoreTasks[i] = IncrementarComSemaphore(taskId, semaphore);
        }
        
        await Task.WhenAll(semaphoreTasks);
        Console.WriteLine($"   Resultado com SemaphoreSlim: {_counter}");
        
        // 2.2 Interlocked para operações atômicas
        Console.WriteLine("\n   🔬 Usando Interlocked (mais performático):");
        _counter = 0;
        
        var interlockedTasks = new Task[3];
        for (int i = 0; i < 3; i++)
        {
            int taskId = i;
            interlockedTasks[i] = Task.Run(() => IncrementarComInterlocked(taskId));
        }
        
        await Task.WhenAll(interlockedTasks);
        Console.WriteLine($"   Resultado com Interlocked: {_counter}");
        
        semaphore.Dispose();
    }

    private static async Task IncrementarComSemaphore(int taskId, SemaphoreSlim semaphore)
    {
        for (int i = 0; i < 1000; i++)
        {
            await semaphore.WaitAsync();
            try
            {
                _counter++;
            }
            finally
            {
                semaphore.Release();
            }
        }
        Console.WriteLine($"   Task {taskId} (SemaphoreSlim) completada");
    }

    private static void IncrementarComInterlocked(int taskId)
    {
        for (int i = 0; i < 1000; i++)
        {
            Interlocked.Increment(ref _counter);
        }
        Console.WriteLine($"   Task {taskId} (Interlocked) completada");
    }

    private static void DemonstrarPadroesRecomendados()
    {
        Console.WriteLine("📋 3. PADRÕES RECOMENDADOS:");
        Console.WriteLine();
        
        Console.WriteLine("   ✅ DO (Faça):");
        Console.WriteLine("   • Use 'lock' para sincronização simples");
        Console.WriteLine("   • Use SemaphoreSlim para cenários assíncronos");
        Console.WriteLine("   • Use Interlocked para operações atômicas simples");
        Console.WriteLine("   • Use ConcurrentCollections quando apropriado");
        Console.WriteLine("   • Use ReaderWriterLockSlim para leitura/escrita intensiva");
        Console.WriteLine();
        
        Console.WriteLine("   ❌ DON'T (Não faça):");
        Console.WriteLine("   • NUNCA crie classes chamadas 'Monitor'");
        Console.WriteLine("   • NUNCA sobrescreva classes do namespace System");
        Console.WriteLine("   • NUNCA use using para renomear tipos core do .NET");
        Console.WriteLine("   • Evite Monitor.Enter/Exit manual (prefira 'lock')");
        Console.WriteLine();
        
        // Demonstração de ConcurrentCollection
        Console.WriteLine("   🔗 Exemplo com ConcurrentBag:");
        var bag = new ConcurrentBag<int>();
        
        Parallel.For(0, 10, i =>
        {
            bag.Add(i * 10);
        });
        
        Console.WriteLine($"   Itens na ConcurrentBag: {bag.Count}");
        Console.WriteLine("   ✅ Thread-safe sem locks explícitos!");
        
        Console.WriteLine();
        Console.WriteLine("🎓 LIÇÃO IMPORTANTE:");
        Console.WriteLine("   Respeite os namespaces do .NET Framework!");
        Console.WriteLine("   Use as ferramentas certas para cada cenário!");
        Console.WriteLine("   Mantenha seu código limpo e legível!");
    }
}
