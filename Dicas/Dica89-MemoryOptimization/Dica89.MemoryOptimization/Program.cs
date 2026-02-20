using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

Console.WriteLine("💾 Dica 89: Memory Optimization (.NET 9)");
Console.WriteLine("=========================================");

// 1. Span<T> e Memory<T> (.NET 9)
Console.WriteLine("\n1. 🎯 Span<T> e Memory<T>:");
Console.WriteLine("──────────────────────────");

await DemonstrarSpanMemory();

// 2. ArrayPool para Reutilização
Console.WriteLine("\n2. ♻️ ArrayPool para Reutilização:");
Console.WriteLine("──────────────────────────────────");

await DemonstrarArrayPool();

// 3. Stackalloc e Stack Allocation
Console.WriteLine("\n3. 📚 Stackalloc e Stack Allocation:");
Console.WriteLine("───────────────────────────────────");

DemonstrarStackalloc();

// 4. Object Pooling Patterns
Console.WriteLine("\n4. 🏊 Object Pooling Patterns:");
Console.WriteLine("──────────────────────────────");

await DemonstrarObjectPooling();

// 5. Memory Pressure Management
Console.WriteLine("\n5. � Memory Pressure Management:");
Console.WriteLine("─────────────────────────────────");

await DemonstrarMemoryPressure();

// 6. Unsafe Code e Pointers
Console.WriteLine("\n6. ⚡ Unsafe Code e Pointers:");
Console.WriteLine("────────────────────────────");

DemonstrarUnsafeCode();

Console.WriteLine("\n✅ Demonstração completa de Memory Optimization!");

static async Task DemonstrarSpanMemory()
{
    Console.WriteLine("🎯 Comparando Array vs Span vs Memory:");
    
    // Array tradicional
    var array = new int[1000];
    for (int i = 0; i < array.Length; i++)
    {
        array[i] = i * i;
    }
    
    // Span<T> - sem alocação adicional (processado antes do await)
    var spanSum = 0;
    {
        Span<int> span = array.AsSpan(100, 500);
        foreach (var item in span)
        {
            spanSum += item;
        }
    }
    
    // Memory<T> - para cenários assíncronos
    Memory<int> memory = array.AsMemory(200, 300);
    var memorySum = await ProcessMemoryAsync(memory);
    
    Console.WriteLine($"✅ Array original: {array.Length} elementos");
    Console.WriteLine($"✅ Span slice: 500 elementos, soma: {spanSum:N0}");
    Console.WriteLine($"✅ Memory slice: {memory.Length} elementos, soma: {memorySum:N0}");
    
    // ReadOnlySpan para operações de leitura (também processado separadamente)
    var average = CalculateAverage(array.AsSpan());
    Console.WriteLine($"✅ Média (ReadOnlySpan): {average:F2}");
    
    // Demonstrar string slicing sem alocação
    var text = "Hello, Memory Optimization World!";
    ReadOnlySpan<char> slice = text.AsSpan(7, 6); // "Memory"
    Console.WriteLine($"✅ String slice: '{slice.ToString()}'");
}

static async Task<int> ProcessMemoryAsync(Memory<int> memory)
{
    await Task.Delay(1); // Simular processamento assíncrono
    var span = memory.Span;
    var sum = 0;
    for (int i = 0; i < span.Length; i++)
    {
        sum += span[i];
    }
    return sum;
}

static double CalculateAverage(ReadOnlySpan<int> data)
{
    if (data.Length == 0) return 0;
    
    long sum = 0;
    foreach (var item in data)
    {
        sum += item;
    }
    return (double)sum / data.Length;
}

static async Task DemonstrarArrayPool()
{
    var pool = ArrayPool<byte>.Shared;
    
    Console.WriteLine("♻️  Demonstrando ArrayPool vs alocação tradicional:");
    
    // Cenário 1: Sem pool (muitas alocações)
    var sw = Stopwatch.StartNew();
    var memoryBefore = GC.GetTotalMemory(false);
    
    for (int i = 0; i < 1000; i++)
    {
        var array = new byte[1024]; // Nova alocação a cada iteração
        ProcessData(array);
    }
    
    sw.Stop();
    var memoryAfter = GC.GetTotalMemory(false);
    var timeWithoutPool = sw.ElapsedMilliseconds;
    var memoryWithoutPool = memoryAfter - memoryBefore;
    
    // Forçar limpeza
    GC.Collect();
    await Task.Delay(10);
    
    // Cenário 2: Com ArrayPool (reutilização)
    sw.Restart();
    memoryBefore = GC.GetTotalMemory(false);
    
    for (int i = 0; i < 1000; i++)
    {
        var array = pool.Rent(1024); // Reutilizar array do pool
        try
        {
            ProcessData(array.AsSpan(0, 1024));
        }
        finally
        {
            pool.Return(array); // Devolver para o pool
        }
    }
    
    sw.Stop();
    memoryAfter = GC.GetTotalMemory(false);
    var timeWithPool = sw.ElapsedMilliseconds;
    var memoryWithPool = memoryAfter - memoryBefore;
    
    Console.WriteLine($"📊 Sem Pool: {timeWithoutPool}ms, {memoryWithoutPool / 1024:N0}KB alocados");
    Console.WriteLine($"📊 Com Pool: {timeWithPool}ms, {memoryWithPool / 1024:N0}KB alocados");
    Console.WriteLine($"⚡ Melhoria: {((double)Math.Max(1, timeWithoutPool - timeWithPool) / Math.Max(1, timeWithoutPool) * 100):F1}% mais rápido");
    Console.WriteLine($"💾 Economia: {((double)Math.Max(1, memoryWithoutPool - memoryWithPool) / Math.Max(1, memoryWithoutPool) * 100):F1}% menos memória");
}

static void ProcessData(Span<byte> data)
{
    // Simular processamento de dados
    for (int i = 0; i < data.Length; i++)
    {
        data[i] = (byte)(i % 256);
    }
}

static void DemonstrarStackalloc()
{
    Console.WriteLine("📚 Demonstrando stackalloc vs heap allocation:");
    
    // Stack allocation - muito mais rápido para arrays pequenos
    Span<int> stackNumbers = stackalloc int[100];
    for (int i = 0; i < stackNumbers.Length; i++)
    {
        stackNumbers[i] = i * i;
    }
    
    // Heap allocation equivalente
    var heapNumbers = new int[100];
    for (int i = 0; i < heapNumbers.Length; i++)
    {
        heapNumbers[i] = i * i;
    }
    
    Console.WriteLine($"✅ Stack array: {stackNumbers.Length} elementos (sem alocação heap)");
    Console.WriteLine($"✅ Heap array: {heapNumbers.Length} elementos (alocação heap)");
    
    // Demonstrar com strings/chars
    Span<char> buffer = stackalloc char[50];
    var text = "Memory Optimization Demo";
    text.AsSpan().CopyTo(buffer);
    
    Console.WriteLine($"✅ Buffer stack: '{buffer.Slice(0, text.Length).ToString()}'");
    
    // Usar para building strings sem StringBuilder
    BuildStringWithStackalloc();
}

static void BuildStringWithStackalloc()
{
    Span<char> buffer = stackalloc char[256];
    var position = 0;
    
    // Construir string manualmente
    var parts = new[] { "Hello", " ", "Memory", " ", "World", "!" };
    foreach (var part in parts)
    {
        part.AsSpan().CopyTo(buffer.Slice(position));
        position += part.Length;
    }
    
    var result = buffer.Slice(0, position).ToString();
    Console.WriteLine($"✅ String construída com stackalloc: '{result}'");
}

static async Task DemonstrarObjectPooling()
{
    Console.WriteLine("🏊 Demonstrando Object Pooling personalizado:");
    
    var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), sb => sb.Clear());
    
    // Teste sem pooling
    var sw = Stopwatch.StartNew();
    var memoryBefore = GC.GetTotalMemory(false);
    
    var results1 = new List<string>();
    for (int i = 0; i < 1000; i++)
    {
        var sb = new StringBuilder(); // Nova instância
        sb.Append("Item ").Append(i).Append(" - ").Append(DateTime.Now.Ticks);
        results1.Add(sb.ToString());
    }
    
    sw.Stop();
    var memoryAfter = GC.GetTotalMemory(false);
    var timeWithoutPool = sw.ElapsedMilliseconds;
    var memoryWithoutPool = memoryAfter - memoryBefore;
    
    // Forçar cleanup
    GC.Collect();
    await Task.Delay(10);
    
    // Teste com pooling
    sw.Restart();
    memoryBefore = GC.GetTotalMemory(false);
    
    var results2 = new List<string>();
    for (int i = 0; i < 1000; i++)
    {
        var sb = pool.Rent(); // Reutilizar do pool
        try
        {
            sb.Append("Item ").Append(i).Append(" - ").Append(DateTime.Now.Ticks);
            results2.Add(sb.ToString());
        }
        finally
        {
            pool.Return(sb); // Retornar ao pool
        }
    }
    
    sw.Stop();
    memoryAfter = GC.GetTotalMemory(false);
    var timeWithPool = sw.ElapsedMilliseconds;
    var memoryWithPool = memoryAfter - memoryBefore;
    
    Console.WriteLine($"📊 Sem Pool: {timeWithoutPool}ms, {memoryWithoutPool / 1024:N0}KB");
    Console.WriteLine($"📊 Com Pool: {timeWithPool}ms, {memoryWithPool / 1024:N0}KB");
    Console.WriteLine($"⚡ Melhoria: {((double)Math.Max(1, timeWithoutPool - timeWithPool) / Math.Max(1, timeWithoutPool) * 100):F1}%");
}

static async Task DemonstrarMemoryPressure()
{
    Console.WriteLine("📊 Demonstrando Memory Pressure Management:");
    
    var initialMemory = GC.GetTotalMemory(false);
    Console.WriteLine($"📊 Memória inicial: {initialMemory / 1024 / 1024:F2}MB");
    
    // Simular diferentes cenários de pressão de memória
    await SimularBaixaPressao();
    await SimularAltaPressao();
    await SimularPressaoControlada();
    
    // Forçar limpeza final
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    
    var finalMemory = GC.GetTotalMemory(true);
    Console.WriteLine($"📊 Memória final: {finalMemory / 1024 / 1024:F2}MB");
    Console.WriteLine($"📈 Delta: {(finalMemory - initialMemory) / 1024 / 1024:F2}MB");
}

static async Task SimularBaixaPressao()
{
    Console.WriteLine("� Cenário: Baixa pressão de memória");
    
    var smallObjects = new List<SmallObject>();
    for (int i = 0; i < 1000; i++)
    {
        smallObjects.Add(new SmallObject(i));
        if (i % 100 == 0) await Task.Yield();
    }
    
    var memory = GC.GetTotalMemory(false);
    Console.WriteLine($"   Memória após 1k objetos pequenos: {memory / 1024:N0}KB");
}

static async Task SimularAltaPressao()
{
    Console.WriteLine("🔴 Cenário: Alta pressão de memória");
    
    var largeObjects = new List<byte[]>();
    for (int i = 0; i < 100; i++)
    {
        largeObjects.Add(new byte[100_000]); // 100KB cada
        if (i % 10 == 0) await Task.Yield();
    }
    
    var memory = GC.GetTotalMemory(false);
    Console.WriteLine($"   Memória após objetos grandes: {memory / 1024 / 1024:F2}MB");
    
    // Simular liberação gradual
    for (int i = largeObjects.Count - 1; i >= 0; i -= 2)
    {
        largeObjects.RemoveAt(i);
        if (i % 20 == 0)
        {
            GC.Collect(0);
            await Task.Yield();
        }
    }
    
    var memoryAfterCleanup = GC.GetTotalMemory(false);
    Console.WriteLine($"   Memória após cleanup: {memoryAfterCleanup / 1024 / 1024:F2}MB");
}

static async Task SimularPressaoControlada()
{
    Console.WriteLine("🟡 Cenário: Pressão controlada com pooling");
    
    var pool = ArrayPool<byte>.Shared;
    var rentedArrays = new List<byte[]>();
    
    // Alugar arrays do pool
    for (int i = 0; i < 50; i++)
    {
        var array = pool.Rent(50_000); // 50KB
        rentedArrays.Add(array);
        if (i % 10 == 0) await Task.Yield();
    }
    
    var memory = GC.GetTotalMemory(false);
    Console.WriteLine($"   Memória com arrays pooled: {memory / 1024 / 1024:F2}MB");
    
    // Devolver arrays ao pool
    foreach (var array in rentedArrays)
    {
        pool.Return(array);
    }
    
    var memoryAfterReturn = GC.GetTotalMemory(false);
    Console.WriteLine($"   Memória após retorno ao pool: {memoryAfterReturn / 1024 / 1024:F2}MB");
}

static unsafe void DemonstrarUnsafeCode()
{
    Console.WriteLine("⚡ Demonstrando unsafe code para performance crítica:");
    
    var data = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    
    // Método seguro
    var safeSum = SumSafe(data);
    
    // Método unsafe
    var unsafeSum = SumUnsafe(data);
    
    Console.WriteLine($"✅ Soma segura: {safeSum}");
    Console.WriteLine($"✅ Soma unsafe: {unsafeSum}");
    
    // Demonstrar operações com pointers
    fixed (int* ptr = data)
    {
        var sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += *(ptr + i); // Aritmética de ponteiros
        }
        Console.WriteLine($"✅ Soma com ponteiros: {sum}");
    }
    
    // Demonstrar struct marshalling
    DemonstrarStructMarshalling();
}

static int SumSafe(int[] data)
{
    var sum = 0;
    for (int i = 0; i < data.Length; i++)
    {
        sum += data[i];
    }
    return sum;
}

static unsafe int SumUnsafe(int[] data)
{
    fixed (int* ptr = data)
    {
        var sum = 0;
        var p = ptr;
        var end = ptr + data.Length;
        
        while (p < end)
        {
            sum += *p++;
        }
        return sum;
    }
}

static void DemonstrarStructMarshalling()
{
    var packed = new PackedStruct { Flag = 1, Value = 42, Code = 100 };
    var size = Marshal.SizeOf<PackedStruct>();
    
    Console.WriteLine($"✅ Struct empacotado: {size} bytes (Flag:{packed.Flag}, Value:{packed.Value}, Code:{packed.Code})");
}

// Classes e structs auxiliares
public class SimpleObjectPool<T> where T : class
{
    private readonly Func<T> _factory;
    private readonly Action<T> _reset;
    private readonly Queue<T> _objects = new();
    private readonly object _lock = new();
    
    public SimpleObjectPool(Func<T> factory, Action<T> reset)
    {
        _factory = factory;
        _reset = reset;
    }
    
    public T Rent()
    {
        lock (_lock)
        {
            if (_objects.TryDequeue(out var obj))
            {
                return obj;
            }
        }
        return _factory();
    }
    
    public void Return(T obj)
    {
        if (obj == null) return;
        
        _reset(obj);
        
        lock (_lock)
        {
            if (_objects.Count < 100) // Limitar tamanho do pool
            {
                _objects.Enqueue(obj);
            }
        }
    }
}

public class SmallObject
{
    public int Id { get; }
    public string Name { get; }
    
    public SmallObject(int id)
    {
        Id = id;
        Name = $"Object_{id}";
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PackedStruct
{
    public byte Flag;
    public int Value;
    public short Code;
}
