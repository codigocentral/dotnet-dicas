using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

Console.WriteLine("⚡ Dica 88: Performance Profiling (.NET 9)");
Console.WriteLine("==========================================");

// 1. Stopwatch para Medições Básicas
Console.WriteLine("\n1. ⏱️ Stopwatch para Medições Básicas:");
Console.WriteLine("──────────────────────────────────────");

await MedirPerformanceBasica();

// 2. Performance Counters do Sistema
Console.WriteLine("\n2. 📊 Performance Counters:");
Console.WriteLine("───────────────────────────");

MonitorarPerformanceCounters();

// 3. Profiling de Memória
Console.WriteLine("\n3. 💾 Profiling de Memória:");
Console.WriteLine("───────────────────────────");

await ProfilearUsoMemoria();

// 4. CPU-Intensive Operations
Console.WriteLine("\n4. 🔥 Operações CPU-Intensive:");
Console.WriteLine("──────────────────────────────");

await MedirOperacoesCPU();

// 5. Análise de Allocation
Console.WriteLine("\n5. 📈 Análise de Allocation:");
Console.WriteLine("────────────────────────────");

await AnalisarAllocations();

// 6. Hot Path Detection (.NET 9)
Console.WriteLine("\n6. 🌡️ Hot Path Detection:");
Console.WriteLine("────────────────────────");

await DetectarHotPaths();

// 7. Method Profiling Avançado
Console.WriteLine("\n7. 🔍 Method Profiling Avançado:");
Console.WriteLine("───────────────────────────────");

await ProfilearMetodos();

// 8. Concurrent Performance
Console.WriteLine("\n8. 🔄 Concurrent Performance:");
Console.WriteLine("────────────────────────────");

await MedirPerformanceConcorrente();

Console.WriteLine("\n✅ Demonstração completa de Performance Profiling!");

static async Task MedirPerformanceBasica()
{
    var operacoes = new Dictionary<string, Func<Task>>
    {
        ["Lista simples"] = () => CriarListaSimples(),
        ["LINQ complexo"] = () => ExecutarLINQComplexo(),
        ["StringBuilder"] = () => UsarStringBuilder(),
        ["Operações matemáticas"] = () => OperacoesMatematicas()
    };
    
    foreach (var operacao in operacoes)
    {
        var sw = Stopwatch.StartNew();
        await operacao.Value();
        sw.Stop();
        
        Console.WriteLine($"⚡ {operacao.Key}:");
        Console.WriteLine($"   ⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   🔢 Ticks: {sw.ElapsedTicks:N0}");
        Console.WriteLine($"   🎯 Precisão: {sw.Elapsed.TotalMicroseconds:F2}μs");
    }
}

static void MonitorarPerformanceCounters()
{
    var processo = Process.GetCurrentProcess();
    
    Console.WriteLine($"🖥️  CPU Total Time: {processo.TotalProcessorTime.TotalMilliseconds:F2}ms");
    Console.WriteLine($"💾 Working Set: {processo.WorkingSet64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"📄 Paged Memory: {processo.PagedMemorySize64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"🌐 Virtual Memory: {processo.VirtualMemorySize64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"🧵 Threads: {processo.Threads.Count}");
    Console.WriteLine($"🔧 Handles: {processo.HandleCount}");
    
    // GC Statistics específicas do .NET 9
    Console.WriteLine($"♻️  GC Gen 0: {GC.CollectionCount(0)}");
    Console.WriteLine($"♻️  GC Gen 1: {GC.CollectionCount(1)}");
    Console.WriteLine($"♻️  GC Gen 2: {GC.CollectionCount(2)}");
    Console.WriteLine($"📊 GC Memory: {GC.GetTotalMemory(false) / 1024 / 1024:F2}MB");
    
    // Informações de sistema (.NET 9)
    Console.WriteLine($"🔢 Processor Count: {Environment.ProcessorCount}");
    Console.WriteLine($"📱 OS Version: {Environment.OSVersion}");
    Console.WriteLine($"🏗️  Runtime: {Environment.Version}");
}

static async Task ProfilearUsoMemoria()
{
    var memoryBefore = GC.GetTotalMemory(false);
    Console.WriteLine($"📊 Memória inicial: {memoryBefore / 1024:N0}KB");
    
    // Cenários diferentes de uso de memória
    var cenarios = new Dictionary<string, Func<Task>>
    {
        ["Pequenos objetos"] = () => CriarObjetosPequenos(),
        ["Objetos médios"] = () => CriarObjetosMedios(),
        ["Arrays grandes"] = () => CriarArraysGrandes(),
        ["Strings concatenadas"] = () => ConcatenarStrings()
    };
    
    foreach (var cenario in cenarios)
    {
        var memBefore = GC.GetTotalMemory(false);
        var sw = Stopwatch.StartNew();
        
        await cenario.Value();
        
        sw.Stop();
        var memAfter = GC.GetTotalMemory(false);
        var allocatedMemory = memAfter - memBefore;
        
        Console.WriteLine($"🎯 {cenario.Key}:");
        Console.WriteLine($"   ⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   💾 Memória alocada: {allocatedMemory / 1024:N0}KB");
        Console.WriteLine($"   📈 Taxa de alocação: {allocatedMemory / sw.Elapsed.TotalSeconds / 1024 / 1024:F2}MB/s");
        
        // Forçar limpeza para próximo teste
        GC.Collect();
        await Task.Delay(10);
    }
}

static async Task MedirOperacoesCPU()
{
    var operacoes = new Dictionary<string, (Func<Task> action, int iterations)>
    {
        ["Operações aritméticas"] = (() => OperacoesAritmeticas(), 1_000_000),
        ["String manipulations"] = (() => ManipularStrings(), 10_000),
        ["LINQ operations"] = (() => OperacoesLINQ(), 1_000),
        ["Reflection calls"] = (() => ChamadasReflection(), 10_000)
    };
    
    foreach (var operacao in operacoes)
    {
        var sw = Stopwatch.StartNew();
        var tasks = new List<Task>();
        
        // Executar em paralelo para teste de CPU
        for (int i = 0; i < operacao.Value.iterations; i++)
        {
            if (i % 1000 == 0)
            {
                tasks.Add(operacao.Value.action());
            }
            else
            {
                await operacao.Value.action();
            }
        }
        
        await Task.WhenAll(tasks);
        sw.Stop();
        
        var opsPerSecond = operacao.Value.iterations / sw.Elapsed.TotalSeconds;
        
        Console.WriteLine($"🔥 {operacao.Key}:");
        Console.WriteLine($"   ⏱️  Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   🔢 Iterações: {operacao.Value.iterations:N0}");
        Console.WriteLine($"   🚀 Ops/segundo: {opsPerSecond:N0}");
        Console.WriteLine($"   ⚡ Tempo médio: {sw.Elapsed.TotalMicroseconds / operacao.Value.iterations:F3}μs");
    }
}

static async Task AnalisarAllocations()
{
    Console.WriteLine("🔍 Analisando padrões de alocação...");
    
    var scenarios = new Dictionary<string, Func<Task>>
    {
        ["Value Types"] = () => UsarValueTypes(),
        ["Reference Types"] = () => UsarReferenceTypes(),
        ["Structs vs Classes"] = () => CompararStructsClasses(),
        ["Span<T> vs Arrays"] = () => CompararSpanArrays()
    };
    
    foreach (var scenario in scenarios)
    {
        // Baseline measurements
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);
        var memoryBefore = GC.GetTotalMemory(false);
        
        var sw = Stopwatch.StartNew();
        await scenario.Value();
        sw.Stop();
        
        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);
        var memoryAfter = GC.GetTotalMemory(false);
        
        Console.WriteLine($"📊 {scenario.Key}:");
        Console.WriteLine($"   ⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   💾 Memória delta: {(memoryAfter - memoryBefore) / 1024:N0}KB");
        Console.WriteLine($"   ♻️  GC Gen0: +{gen0After - gen0Before}");
        Console.WriteLine($"   ♻️  GC Gen1: +{gen1After - gen1Before}");
        Console.WriteLine($"   ♻️  GC Gen2: +{gen2After - gen2Before}");
    }
}

static async Task DetectarHotPaths()
{
    Console.WriteLine("🌡️  Detectando hot paths com profiling...");
    
    var methods = new Dictionary<string, (Func<Task> method, int calls)>
    {
        ["Fast Path"] = (() => FastPath(), 100_000),
        ["Medium Path"] = (() => MediumPath(), 10_000),
        ["Slow Path"] = (() => SlowPath(), 1_000),
        ["Very Slow Path"] = (() => VerySlowPath(), 100)
    };
    
    var results = new List<(string name, double avgTime, double totalTime, int calls)>();
    
    foreach (var method in methods)
    {
        var sw = Stopwatch.StartNew();
        
        // Warm-up
        for (int i = 0; i < 100; i++)
        {
            await method.Value.method();
        }
        
        sw.Restart();
        
        // Actual measurement
        for (int i = 0; i < method.Value.calls; i++)
        {
            await method.Value.method();
        }
        
        sw.Stop();
        
        var avgTime = sw.Elapsed.TotalMicroseconds / method.Value.calls;
        results.Add((method.Key, avgTime, sw.Elapsed.TotalMilliseconds, method.Value.calls));
    }
    
    // Ordenar por tempo médio (hot paths primeiro)
    results = results.OrderByDescending(r => r.avgTime).ToList();
    
    Console.WriteLine("\n🔥 Hot Paths (ordenados por tempo médio):");
    foreach (var result in results)
    {
        var percentage = (result.totalTime / results.Sum(r => r.totalTime)) * 100;
        Console.WriteLine($"   🌡️  {result.name}:");
        Console.WriteLine($"      ⏱️  Tempo médio: {result.avgTime:F3}μs");
        Console.WriteLine($"      📊 % do tempo total: {percentage:F1}%");
        Console.WriteLine($"      🔢 Chamadas: {result.calls:N0}");
    }
}

static async Task ProfilearMetodos()
{
    Console.WriteLine("🔍 Profiling detalhado de métodos...");
    
    // Usar attributes para profiling (simulado)
    await ProfiledMethod("DatabaseQuery", () => SimularQueryDatabase());
    await ProfiledMethod("ApiCall", () => SimularChamadaAPI());
    await ProfiledMethod("FileOperation", () => SimularOperacaoArquivo());
    await ProfiledMethod("Calculation", () => SimularCalculoComplexo());
}

[MethodImpl(MethodImplOptions.NoInlining)]
static async Task ProfiledMethod(string methodName, Func<Task> action)
{
    var sw = Stopwatch.StartNew();
    var memoryBefore = GC.GetTotalMemory(false);
    
    try
    {
        await action();
        sw.Stop();
        
        var memoryAfter = GC.GetTotalMemory(false);
        var allocatedMemory = memoryAfter - memoryBefore;
        
        Console.WriteLine($"🎯 {methodName}:");
        Console.WriteLine($"   ✅ Status: Success");
        Console.WriteLine($"   ⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   💾 Memória alocada: {allocatedMemory / 1024:F2}KB");
        Console.WriteLine($"   🔄 Thread: {Thread.CurrentThread.ManagedThreadId}");
    }
    catch (Exception ex)
    {
        sw.Stop();
        Console.WriteLine($"🎯 {methodName}:");
        Console.WriteLine($"   ❌ Status: Error - {ex.GetType().Name}");
        Console.WriteLine($"   ⏱️  Tempo até erro: {sw.ElapsedMilliseconds}ms");
    }
}

static async Task MedirPerformanceConcorrente()
{
    Console.WriteLine("🔄 Medindo performance de operações concorrentes...");
    
    var scenarios = new Dictionary<string, Func<Task>>
    {
        ["Sequential"] = () => ExecutarSequencial(),
        ["Parallel.ForEach"] = () => ExecutarParallelForEach(),
        ["Task.WhenAll"] = () => ExecutarTaskWhenAll(),
        ["Channels"] = () => ExecutarComChannels()
    };
    
    foreach (var scenario in scenarios)
    {
        var sw = Stopwatch.StartNew();
        await scenario.Value();
        sw.Stop();
        
        Console.WriteLine($"🔄 {scenario.Key}: {sw.ElapsedMilliseconds}ms");
    }
}

// Métodos auxiliares para demonstração
static async Task CriarListaSimples()
{
    var lista = new List<int>();
    for (int i = 0; i < 10_000; i++)
    {
        lista.Add(i * i);
    }
    await Task.CompletedTask;
}

static async Task ExecutarLINQComplexo()
{
    var numbers = Enumerable.Range(1, 1_000); // Reduzir para evitar overflow
    var result = numbers
        .Where(x => x % 2 == 0)
        .Select(x => new { Number = x, Square = (long)x * x }) // Usar long para evitar overflow
        .GroupBy(x => x.Number % 10)
        .Select(g => new { Key = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Square) })
        .ToList();
    await Task.CompletedTask;
}

static async Task UsarStringBuilder()
{
    var sb = new StringBuilder();
    for (int i = 0; i < 1_000; i++)
    {
        sb.Append($"Item {i} with some additional text ");
    }
    var result = sb.ToString();
    await Task.CompletedTask;
}

static async Task OperacoesMatematicas()
{
    double result = 0;
    for (int i = 0; i < 100_000; i++)
    {
        result += Math.Sqrt(i) * Math.Sin(i) / Math.Cos(i + 1);
    }
    await Task.CompletedTask;
}

static async Task CriarObjetosPequenos()
{
    var objects = new List<SmallObject>();
    for (int i = 0; i < 10_000; i++)
    {
        objects.Add(new SmallObject { Id = i, Name = $"Object_{i}" });
    }
    await Task.CompletedTask;
}

static async Task CriarObjetosMedios()
{
    var objects = new List<MediumObject>();
    for (int i = 0; i < 1_000; i++)
    {
        objects.Add(new MediumObject(i));
    }
    await Task.CompletedTask;
}

static async Task CriarArraysGrandes()
{
    var arrays = new List<byte[]>();
    for (int i = 0; i < 100; i++)
    {
        arrays.Add(new byte[10_000]);
    }
    await Task.CompletedTask;
}

static async Task ConcatenarStrings()
{
    var result = "";
    for (int i = 0; i < 1_000; i++)
    {
        result += $"String number {i} with additional content ";
    }
    await Task.CompletedTask;
}

static async Task OperacoesAritmeticas()
{
    var x = Random.Shared.Next(1, 100);
    var y = Random.Shared.Next(1, 100);
    var result = (x + y) * (x - y) / (x + 1);
    await Task.CompletedTask;
}

static async Task ManipularStrings()
{
    var text = "Hello World Performance Test";
    var result = text.ToUpper().ToLower().Replace("o", "0").Substring(0, 10);
    await Task.CompletedTask;
}

static async Task OperacoesLINQ()
{
    var numbers = Enumerable.Range(1, 1000);
    var result = numbers.Where(x => x % 2 == 0).Sum();
    await Task.CompletedTask;
}

static async Task ChamadasReflection()
{
    var type = typeof(string);
    var methods = type.GetMethods().Take(5).ToList();
    await Task.CompletedTask;
}

static async Task UsarValueTypes()
{
    var points = new List<Point>();
    for (int i = 0; i < 10_000; i++)
    {
        points.Add(new Point(i, i * 2));
    }
    await Task.CompletedTask;
}

static async Task UsarReferenceTypes()
{
    var objects = new List<RefObject>();
    for (int i = 0; i < 10_000; i++)
    {
        objects.Add(new RefObject(i, i * 2));
    }
    await Task.CompletedTask;
}

static async Task CompararStructsClasses()
{
    // Structs
    var structs = new PointStruct[10_000];
    for (int i = 0; i < structs.Length; i++)
    {
        structs[i] = new PointStruct(i, i * 2);
    }
    
    // Classes
    var classes = new PointClass[10_000];
    for (int i = 0; i < classes.Length; i++)
    {
        classes[i] = new PointClass(i, i * 2);
    }
    
    await Task.CompletedTask;
}

static async Task CompararSpanArrays()
{
    // Array tradicional
    var array = new int[10_000];
    for (int i = 0; i < array.Length; i++)
    {
        array[i] = i * i;
    }
    
    // Span<T>
    Span<int> span = stackalloc int[1000];
    for (int i = 0; i < span.Length; i++)
    {
        span[i] = i * i;
    }
    
    await Task.CompletedTask;
}

static async Task FastPath()
{
    var x = 42;
    var y = x + 1;
    await Task.CompletedTask;
}

static async Task MediumPath()
{
    var list = new List<int> { 1, 2, 3, 4, 5 };
    var sum = list.Sum();
    await Task.CompletedTask;
}

static async Task SlowPath()
{
    var text = "Performance test string";
    for (int i = 0; i < 100; i++)
    {
        text = text.ToUpper().ToLower();
    }
    await Task.CompletedTask;
}

static async Task VerySlowPath()
{
    await Task.Delay(1); // Simular operação lenta
    var result = Enumerable.Range(1, 1000).Select(x => x * x).Sum();
}

static async Task SimularQueryDatabase()
{
    await Task.Delay(Random.Shared.Next(10, 50));
}

static async Task SimularChamadaAPI()
{
    await Task.Delay(Random.Shared.Next(50, 200));
}

static async Task SimularOperacaoArquivo()
{
    await Task.Delay(Random.Shared.Next(5, 30));
}

static async Task SimularCalculoComplexo()
{
    await Task.Delay(Random.Shared.Next(100, 300));
}

static async Task ExecutarSequencial()
{
    for (int i = 0; i < 100; i++)
    {
        await Task.Delay(1);
    }
}

static async Task ExecutarParallelForEach()
{
    var items = Enumerable.Range(0, 100);
    await Task.Run(() => Parallel.ForEach(items, async item =>
    {
        await Task.Delay(1);
    }));
}

static async Task ExecutarTaskWhenAll()
{
    var tasks = Enumerable.Range(0, 100)
        .Select(async i => await Task.Delay(1));
    
    await Task.WhenAll(tasks);
}

static async Task ExecutarComChannels()
{
    var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();
    var writer = channel.Writer;
    var reader = channel.Reader;
    
    // Producer
    var producerTask = Task.Run(async () =>
    {
        for (int i = 0; i < 100; i++)
        {
            await writer.WriteAsync(i);
            await Task.Delay(1);
        }
        writer.Complete();
    });
    
    // Consumer
    var consumerTask = Task.Run(async () =>
    {
        await foreach (var item in reader.ReadAllAsync())
        {
            await Task.Delay(1);
        }
    });
    
    await Task.WhenAll(producerTask, consumerTask);
}

// Classes e structs auxiliares
public class SmallObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class MediumObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public byte[] Data { get; set; }
    
    public MediumObject(int id)
    {
        Id = id;
        Name = $"Object_{id}";
        Created = DateTime.Now;
        Data = new byte[1024]; // 1KB
    }
}

public readonly record struct Point(int X, int Y);

public class RefObject
{
    public int X { get; }
    public int Y { get; }
    
    public RefObject(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public readonly struct PointStruct
{
    public int X { get; }
    public int Y { get; }
    
    public PointStruct(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class PointClass
{
    public int X { get; }
    public int Y { get; }
    
    public PointClass(int x, int y)
    {
        X = x;
        Y = y;
    }
}
