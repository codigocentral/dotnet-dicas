using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

Console.WriteLine("⚡ Dica 96: Performance Optimization & Profiling (.NET 9)");
Console.WriteLine("=========================================================");

// 1. Memory Profiling
Console.WriteLine("\n1. 📊 Memory Profiling:");
Console.WriteLine("──────────────────────");

MedirUsoMemoria();

// 2. CPU Profiling com Stopwatch
Console.WriteLine("\n2. ⏱️ CPU Profiling:");
Console.WriteLine("──────────────────────");

await ProfileCpuUsage();

// 3. ArrayPool vs New Array
Console.WriteLine("\n3. 🔄 ArrayPool vs New Array:");
Console.WriteLine("─────────────────────────────");

CompararArrayPool();

// 4. String Optimizations
Console.WriteLine("\n4. 📝 String Optimizations:");
Console.WriteLine("───────────────────────────");

CompararStringOperations();

// 5. Span vs Array Performance
Console.WriteLine("\n5. 🎯 Span vs Array Performance:");
Console.WriteLine("────────────────────────────────");

CompararSpanArray();

// 6. Parallel Processing
Console.WriteLine("\n6. 🚀 Parallel Processing:");
Console.WriteLine("─────────────────────────");

await CompararParallelismo();

// 7. GC Pressure Analysis
Console.WriteLine("\n7. 🗑️ GC Pressure Analysis:");
Console.WriteLine("───────────────────────────");

AnalisarPressaoGC();

// 8. Hot Path Optimization
Console.WriteLine("\n8. 🔥 Hot Path Optimization:");
Console.WriteLine("────────────────────────────");

DemonstrarHotPathOptimizations();

Console.WriteLine("\n✅ Demonstração completa de Performance Optimization!");

static void MedirUsoMemoria()
{
    var initialMemory = GC.GetTotalMemory(false);
    Console.WriteLine($"💾 Memória inicial: {initialMemory:N0} bytes");
    
    // Simular alocações
    var largeArrays = new List<byte[]>();
    for (int i = 0; i < 100; i++)
    {
        largeArrays.Add(new byte[1024 * 10]); // 10KB cada
    }
    
    var afterAllocation = GC.GetTotalMemory(false);
    Console.WriteLine($"💾 Após alocações: {afterAllocation:N0} bytes");
    Console.WriteLine($"📈 Incremento: {afterAllocation - initialMemory:N0} bytes");
    
    // Forçar coleta
    GC.Collect();
    GC.WaitForPendingFinalizers();
    
    var afterGC = GC.GetTotalMemory(true);
    Console.WriteLine($"🧹 Após GC: {afterGC:N0} bytes");
    Console.WriteLine($"♻️ Coletado: {afterAllocation - afterGC:N0} bytes");
    
    // Informações do GC
    Console.WriteLine($"🗂️ Geração 0: {GC.CollectionCount(0)} coletas");
    Console.WriteLine($"🗂️ Geração 1: {GC.CollectionCount(1)} coletas");
    Console.WriteLine($"🗂️ Geração 2: {GC.CollectionCount(2)} coletas");
}

static async Task ProfileCpuUsage()
{
    var stopwatch = Stopwatch.StartNew();
    
    // Operação CPU intensiva
    var result = await Task.Run(() =>
    {
        long sum = 0;
        for (int i = 0; i < 10_000_000; i++)
        {
            sum += i * i;
        }
        return sum;
    });
    
    stopwatch.Stop();
    
    Console.WriteLine($"🧮 Resultado: {result:N0}");
    Console.WriteLine($"⏱️ Tempo execução: {stopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine($"⚡ Performance: {10_000_000.0 / stopwatch.ElapsedMilliseconds:F2} ops/ms");
    
    // Process metrics
    using var process = Process.GetCurrentProcess();
    Console.WriteLine($"🔧 CPU Time: {process.TotalProcessorTime.TotalMilliseconds:F0}ms");
    Console.WriteLine($"🧠 Working Set: {process.WorkingSet64 / 1024 / 1024:F1}MB");
    Console.WriteLine($"📊 Peak Working Set: {process.PeakWorkingSet64 / 1024 / 1024:F1}MB");
}

static void CompararArrayPool()
{
    const int iterations = 10000;
    const int arraySize = 1024;
    
    var pool = ArrayPool<byte>.Shared;
    
    // Teste com new array
    var sw1 = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        var array = new byte[arraySize];
        // Simular uso
        array[0] = 1;
    }
    sw1.Stop();
    
    // Teste com ArrayPool
    var sw2 = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        var array = pool.Rent(arraySize);
        try
        {
            // Simular uso
            array[0] = 1;
        }
        finally
        {
            pool.Return(array);
        }
    }
    sw2.Stop();
    
    Console.WriteLine($"🆕 New Array: {sw1.ElapsedMilliseconds}ms");
    Console.WriteLine($"🔄 ArrayPool: {sw2.ElapsedMilliseconds}ms");
    Console.WriteLine($"🚀 Melhoria: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
    
    var improvement = ((double)(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds) / sw1.ElapsedMilliseconds) * 100;
    Console.WriteLine($"📊 Economia: {improvement:F1}%");
}

static void CompararStringOperations()
{
    const int iterations = 10000;
    
    // StringBuilder vs String concatenation
    var sw1 = Stopwatch.StartNew();
    string result1 = "";
    for (int i = 0; i < 1000; i++)
    {
        result1 += $"Item {i}, ";
    }
    sw1.Stop();
    
    var sw2 = Stopwatch.StartNew();
    var sb = new StringBuilder(20000);
    for (int i = 0; i < 1000; i++)
    {
        sb.Append($"Item {i}, ");
    }
    var result2 = sb.ToString();
    sw2.Stop();
    
    // String interpolation vs Concat
    var sw3 = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        var str = $"Value: {i}, Status: Active";
    }
    sw3.Stop();
    
    var sw4 = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        var str = string.Concat("Value: ", i.ToString(), ", Status: Active");
    }
    sw4.Stop();
    
    Console.WriteLine($"➕ String Concatenation: {sw1.ElapsedMilliseconds}ms");
    Console.WriteLine($"📝 StringBuilder: {sw2.ElapsedMilliseconds}ms");
    Console.WriteLine($"💡 String Interpolation: {sw3.ElapsedMilliseconds}ms");
    Console.WriteLine($"🔗 String.Concat: {sw4.ElapsedMilliseconds}ms");
    
    Console.WriteLine($"🚀 StringBuilder é {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F1}x mais rápido");
}

static void CompararSpanArray()
{
    const int size = 1000000;
    var array = new int[size];
    
    // Preencher array
    for (int i = 0; i < size; i++)
    {
        array[i] = i;
    }
    
    // Teste com Array
    var sw1 = Stopwatch.StartNew();
    long sum1 = 0;
    for (int i = 0; i < size; i++)
    {
        sum1 += array[i];
    }
    sw1.Stop();
    
    // Teste com Span
    var sw2 = Stopwatch.StartNew();
    long sum2 = 0;
    var span = array.AsSpan();
    for (int i = 0; i < span.Length; i++)
    {
        sum2 += span[i];
    }
    sw2.Stop();
    
    // Teste com Span foreach
    var sw3 = Stopwatch.StartNew();
    long sum3 = 0;
    foreach (var item in span)
    {
        sum3 += item;
    }
    sw3.Stop();
    
    Console.WriteLine($"🔢 Array indexing: {sw1.ElapsedMilliseconds}ms (sum: {sum1:N0})");
    Console.WriteLine($"⚡ Span indexing: {sw2.ElapsedMilliseconds}ms (sum: {sum2:N0})");
    Console.WriteLine($"🔄 Span foreach: {sw3.ElapsedMilliseconds}ms (sum: {sum3:N0})");
    
    if (sw2.ElapsedMilliseconds > 0)
    {
        Console.WriteLine($"🚀 Span melhoria: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
    }
}

static async Task CompararParallelismo()
{
    const int size = 1_000_000; // Reduzido para evitar overflow
    var numbers = Enumerable.Range(1, size).ToArray();
    
    // Processamento sequencial
    var sw1 = Stopwatch.StartNew();
    long sum1 = 0;
    foreach (var num in numbers)
    {
        sum1 += num; // Removido o quadrado para evitar overflow
    }
    sw1.Stop();
    
    // Processamento paralelo (PLINQ)
    var sw2 = Stopwatch.StartNew();
    var sum2 = numbers.AsParallel().Select(x => (long)x).Sum();
    sw2.Stop();
    
    // Processamento paralelo (Parallel.For)
    var sw3 = Stopwatch.StartNew();
    long sum3 = 0;
    var lockObj = new object();
    
    Parallel.For(0, numbers.Length, i =>
    {
        var localSum = (long)numbers[i];
        lock (lockObj)
        {
            sum3 += localSum;
        }
    });
    sw3.Stop();
    
    // Parallel.For com particionamento
    var sw4 = Stopwatch.StartNew();
    var sum4 = await Task.Run(() =>
    {
        return numbers.AsParallel().Aggregate(0L, (acc, x) => acc + (long)x);
    });
    sw4.Stop();
    
    Console.WriteLine($"🐌 Sequencial: {sw1.ElapsedMilliseconds}ms (sum: {sum1:N0})");
    Console.WriteLine($"⚡ PLINQ: {sw2.ElapsedMilliseconds}ms (sum: {sum2:N0})");
    Console.WriteLine($"🔄 Parallel.For: {sw3.ElapsedMilliseconds}ms (sum: {sum3:N0})");
    Console.WriteLine($"🚀 PLINQ Aggregate: {sw4.ElapsedMilliseconds}ms (sum: {sum4:N0})");
    
    if (sw2.ElapsedMilliseconds > 0)
    {
        Console.WriteLine($"📊 Speedup PLINQ: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
    }
    
    Console.WriteLine($"💻 Processadores disponíveis: {Environment.ProcessorCount}");
}

static void AnalisarPressaoGC()
{
    Console.WriteLine("🔬 Análise de pressão no GC:");
    
    var gen0Before = GC.CollectionCount(0);
    var gen1Before = GC.CollectionCount(1);
    var gen2Before = GC.CollectionCount(2);
    
    // Operação que gera pressão no GC
    var lists = new List<List<object>>();
    for (int i = 0; i < 1000; i++)
    {
        var list = new List<object>();
        for (int j = 0; j < 100; j++)
        {
            list.Add(new { Id = j, Name = $"Object_{j}", Data = new byte[128] });
        }
        lists.Add(list);
        
        if (i % 200 == 0)
        {
            lists.Clear(); // Simular liberação periódica
        }
    }
    
    var gen0After = GC.CollectionCount(0);
    var gen1After = GC.CollectionCount(1);
    var gen2After = GC.CollectionCount(2);
    
    Console.WriteLine($"🗂️ Gen 0 coletas: {gen0After - gen0Before}");
    Console.WriteLine($"🗂️ Gen 1 coletas: {gen1After - gen1Before}");
    Console.WriteLine($"🗂️ Gen 2 coletas: {gen2After - gen2Before}");
    
    var totalAllocated = GC.GetTotalAllocatedBytes();
    Console.WriteLine($"💾 Total alocado: {totalAllocated:N0} bytes");
    
    // Verificar se está rodando em servidor
    var isServerGC = GCSettings.IsServerGC;
    Console.WriteLine($"🖥️ Server GC: {(isServerGC ? "Sim" : "Não")}");
    Console.WriteLine($"⚙️ GC Mode: {GCSettings.LatencyMode}");
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static int CalcularQuadradoInline(int value) => value * value;

static int CalcularQuadradoNormal(int value) => value * value;

static void DemonstrarHotPathOptimizations()
{
    const int iterations = 10_000_000;
    
    // Teste método normal
    var sw1 = Stopwatch.StartNew();
    long sum1 = 0;
    for (int i = 0; i < iterations; i++)
    {
        sum1 += CalcularQuadradoNormal(i % 1000);
    }
    sw1.Stop();
    
    // Teste método inline
    var sw2 = Stopwatch.StartNew();
    long sum2 = 0;
    for (int i = 0; i < iterations; i++)
    {
        sum2 += CalcularQuadradoInline(i % 1000);
    }
    sw2.Stop();
    
    // Teste operação direta
    var sw3 = Stopwatch.StartNew();
    long sum3 = 0;
    for (int i = 0; i < iterations; i++)
    {
        var val = i % 1000;
        sum3 += val * val;
    }
    sw3.Stop();
    
    Console.WriteLine($"🔧 Método normal: {sw1.ElapsedMilliseconds}ms (sum: {sum1:N0})");
    Console.WriteLine($"⚡ Método inline: {sw2.ElapsedMilliseconds}ms (sum: {sum2:N0})");
    Console.WriteLine($"🚀 Operação direta: {sw3.ElapsedMilliseconds}ms (sum: {sum3:N0})");
    
    // Demonstrar estruturas otimizadas
    Console.WriteLine("\n🏗️ Estruturas otimizadas:");
    
    // Dictionary vs ConcurrentDictionary para single-thread
    var dict = new Dictionary<int, string>();
    var concurrentDict = new ConcurrentDictionary<int, string>();
    
    var sw4 = Stopwatch.StartNew();
    for (int i = 0; i < 100000; i++)
    {
        dict[i] = $"Value_{i}";
    }
    sw4.Stop();
    
    var sw5 = Stopwatch.StartNew();
    for (int i = 0; i < 100000; i++)
    {
        concurrentDict[i] = $"Value_{i}";
    }
    sw5.Stop();
    
    Console.WriteLine($"📖 Dictionary: {sw4.ElapsedMilliseconds}ms");
    Console.WriteLine($"🔒 ConcurrentDictionary: {sw5.ElapsedMilliseconds}ms");
    Console.WriteLine($"📊 Overhead concurrent: {(double)sw5.ElapsedMilliseconds / sw4.ElapsedMilliseconds:F2}x");
}
