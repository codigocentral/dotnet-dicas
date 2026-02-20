using System.Diagnostics;
using System.Runtime;

Console.WriteLine("🗑️ Dica 87: Garbage Collection Tuning (.NET 9)");
Console.WriteLine("===============================================");

// 1. Informações Básicas do GC
Console.WriteLine("\n1. 📊 Informações Básicas do GC:");
Console.WriteLine("─────────────────────────────────");

ExibirInformacoesGC();

// 2. Demonstração de Coleta Forçada
Console.WriteLine("\n2. 🔄 Demonstração de Coleta Forçada:");
Console.WriteLine("─────────────────────────────────────");

await DemonstrarColetaForcada();

// 3. Monitoramento de Memória
Console.WriteLine("\n3. 📈 Monitoramento de Memória:");
Console.WriteLine("───────────────────────────────");

await MonitorarUsoMemoria();

// 4. Configurações de GC (.NET 9)
Console.WriteLine("\n4. ⚙️ Configurações de GC:");
Console.WriteLine("──────────────────────────");

ExibirConfiguracoes();

// 5. Análise das Gerações
Console.WriteLine("\n5. 🎯 Análise das Gerações:");
Console.WriteLine("──────────────────────────");

AnalisarGeracoes();

// 6. Large Object Heap (LOH)
Console.WriteLine("\n6. 📦 Large Object Heap:");
Console.WriteLine("────────────────────────");

DemonstrarLOH();

// 7. Performance Impact
Console.WriteLine("\n7. ⚡ Performance Impact:");
Console.WriteLine("────────────────────────");

await MedirImpactoPerformance();

// 8. GC Notifications (.NET 9)
Console.WriteLine("\n8. 🔔 GC Notifications:");
Console.WriteLine("──────────────────────");

DemonstrarGCNotifications();

Console.WriteLine("\n✅ Demonstração completa de GC Tuning!");

static void ExibirInformacoesGC()
{
    Console.WriteLine($"📊 Max Generations: {GC.MaxGeneration}");
    Console.WriteLine($"🖥️  Server GC: {GCSettings.IsServerGC}");
    Console.WriteLine($"⚡ Latency Mode: {GCSettings.LatencyMode}");
    Console.WriteLine($"💾 Total Memory: {GC.GetTotalMemory(false):N0} bytes");
    Console.WriteLine($"🔢 GC Version: {Environment.Version}");
    
    // Informações específicas do .NET 9
    var gcInfo = GC.GetGCMemoryInfo();
    Console.WriteLine($"🎯 Heap Size: {gcInfo.HeapSizeBytes:N0} bytes");
    Console.WriteLine($"📊 Memory Load: {gcInfo.MemoryLoadBytes:N0} bytes");
    Console.WriteLine($"⬆️  High Memory Threshold: {gcInfo.HighMemoryLoadThresholdBytes:N0} bytes");
}

static async Task DemonstrarColetaForcada()
{
    var memoryBefore = GC.GetTotalMemory(false);
    Console.WriteLine($"📊 Memória antes: {memoryBefore:N0} bytes");
    
    // Criar objetos temporários para demonstração
    var tempObjects = new List<LargeObject>();
    
    Console.WriteLine("🏗️  Criando 1000 objetos temporários...");
    for (int i = 0; i < 1000; i++)
    {
        tempObjects.Add(new LargeObject(Random.Shared.Next(1000, 5000)));
    }
    
    var memoryAfter = GC.GetTotalMemory(false);
    Console.WriteLine($"📊 Memória após criação: {memoryAfter:N0} bytes");
    Console.WriteLine($"📈 Aumento: {(memoryAfter - memoryBefore):N0} bytes");
    
    // Limpar referências
    tempObjects.Clear();
    tempObjects = null;
    
    // Forçar coleta com timing
    var sw = Stopwatch.StartNew();
    
    // Estratégia de coleta otimizada
    await Task.Run(() =>
    {
        GC.Collect(0, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();
        GC.Collect(1, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();
        GC.Collect();
    });
    
    sw.Stop();
    
    var memoryFinal = GC.GetTotalMemory(true);
    Console.WriteLine($"📊 Memória após GC: {memoryFinal:N0} bytes");
    Console.WriteLine($"⏱️  Tempo de coleta: {sw.ElapsedMilliseconds}ms");
    Console.WriteLine($"♻️  Memória liberada: {(memoryAfter - memoryFinal):N0} bytes");
    Console.WriteLine($"📉 Eficiência: {((double)(memoryAfter - memoryFinal) / (memoryAfter - memoryBefore) * 100):F1}%");
}

static async Task MonitorarUsoMemoria()
{
    var processo = Process.GetCurrentProcess();
    
    Console.WriteLine($"🖥️  Working Set: {processo.WorkingSet64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"💾 Private Memory: {processo.PrivateMemorySize64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"🌐 Virtual Memory: {processo.VirtualMemorySize64 / 1024 / 1024:F2}MB");
    Console.WriteLine($"♻️  GC Total Memory: {GC.GetTotalMemory(false) / 1024 / 1024:F2}MB");
    
    // Estatísticas de GC por geração
    Console.WriteLine("\n📊 Estatísticas por Geração:");
    for (int gen = 0; gen <= GC.MaxGeneration; gen++)
    {
        Console.WriteLine($"   Gen {gen}: {GC.CollectionCount(gen)} coletas");
    }
    
    // Monitorar através de múltiplas alocações
    Console.WriteLine("\n🔍 Monitoramento dinâmico:");
    var initialMemory = GC.GetTotalMemory(false);
    
    for (int i = 0; i < 3; i++)
    {
        var obj = new MediumObject();
        var currentMemory = GC.GetTotalMemory(false);
        var generation = GC.GetGeneration(obj);
        
        Console.WriteLine($"   Iteração {i + 1}: {currentMemory / 1024:N0}KB (Gen {generation})");
        
        // Simular uso do objeto
        await Task.Delay(50);
        obj = null;
    }
}

static void ExibirConfiguracoes()
{
    Console.WriteLine($"🔧 Server GC Enabled: {GCSettings.IsServerGC}");
    Console.WriteLine($"🔧 Latency Mode: {GCSettings.LatencyMode}");
    
    // Demonstrar mudanças de configuração
    var originalMode = GCSettings.LatencyMode;
    
    try
    {
        // Tentar diferentes modos de latência
        var modes = new[] { 
            GCLatencyMode.LowLatency, 
            GCLatencyMode.Interactive, 
            GCLatencyMode.Batch 
        };
        
        foreach (var mode in modes)
        {
            if (mode != originalMode)
            {
                try
                {
                    GCSettings.LatencyMode = mode;
                    Console.WriteLine($"✅ Modo alterado para: {GCSettings.LatencyMode}");
                    
                    // Teste rápido com o novo modo
                    var testMemory = GC.GetTotalMemory(false);
                    Console.WriteLine($"   💾 Memória no modo {mode}: {testMemory / 1024:N0}KB");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao alterar para {mode}: {ex.Message}");
                }
            }
        }
    }
    finally
    {
        // Restaurar modo original
        GCSettings.LatencyMode = originalMode;
        Console.WriteLine($"🔄 Modo restaurado para: {GCSettings.LatencyMode}");
    }
}

static void AnalisarGeracoes()
{
    Console.WriteLine("📊 Contadores de coleta por geração:");
    for (int gen = 0; gen <= GC.MaxGeneration; gen++)
    {
        var collectionCount = GC.CollectionCount(gen);
        Console.WriteLine($"   🎯 Geração {gen}: {collectionCount} coletas");
    }
    
    // Criar objetos e rastrear suas gerações
    Console.WriteLine("\n🔍 Rastreamento de objetos:");
    var objects = new List<object>();
    
    for (int i = 0; i < 5; i++)
    {
        var obj = new { Id = i, Data = new byte[1024] };
        objects.Add(obj);
        
        var generation = GC.GetGeneration(obj);
        Console.WriteLine($"   📦 Objeto {i}: Geração {generation}");
    }
    
    // Forçar uma coleta de Gen 0 e verificar promoções
    Console.WriteLine("\n🔄 Após coleta de Gen 0:");
    GC.Collect(0);
    
    for (int i = 0; i < objects.Count; i++)
    {
        var generation = GC.GetGeneration(objects[i]);
        Console.WriteLine($"   📦 Objeto {i}: Geração {generation}");
    }
}

static void DemonstrarLOH()
{
    Console.WriteLine("📦 Demonstrando Large Object Heap...");
    
    // Objetos pequenos (heap normal)
    var smallObject = new byte[1000]; // 1KB
    var smallGeneration = GC.GetGeneration(smallObject);
    Console.WriteLine($"🔸 Objeto pequeno (1KB): Geração {smallGeneration}");
    
    // Objeto grande (LOH) - threshold é ~85KB
    var largeObject = new byte[100_000]; // 100KB
    var largeGeneration = GC.GetGeneration(largeObject);
    Console.WriteLine($"🔹 Objeto grande (100KB): Geração {largeGeneration}");
    
    // Objeto muito grande
    var veryLargeObject = new byte[1_000_000]; // 1MB
    var veryLargeGeneration = GC.GetGeneration(veryLargeObject);
    Console.WriteLine($"🔹 Objeto muito grande (1MB): Geração {veryLargeGeneration}");
    
    if (largeGeneration == 2)
    {
        Console.WriteLine("✅ Confirmado: objetos grandes vão para LOH (Geração 2)");
    }
    
    // Informações de memória após LOH
    var gcInfo = GC.GetGCMemoryInfo();
    Console.WriteLine($"📊 Heap total após LOH: {gcInfo.HeapSizeBytes / 1024 / 1024:F2}MB");
}

static async Task MedirImpactoPerformance()
{
    Console.WriteLine("⚡ Medindo impacto do GC na performance...");
    
    var scenarios = new Dictionary<string, Func<Task>>
    {
        ["Muitas alocações pequenas"] = () => CriarMuitasAlocacoesPequenas(),
        ["Poucas alocações grandes"] = () => CriarPoucasAlocacoesGrandes(),
        ["Objetos de vida longa"] = () => CriarObjetosVidaLonga(),
        ["Objetos temporários"] = () => CriarObjetosTemporarios()
    };
    
    foreach (var scenario in scenarios)
    {
        var gcBefore = new int[GC.MaxGeneration + 1];
        for (int i = 0; i <= GC.MaxGeneration; i++)
        {
            gcBefore[i] = GC.CollectionCount(i);
        }
        
        var sw = Stopwatch.StartNew();
        await scenario.Value();
        sw.Stop();
        
        var gcAfter = new int[GC.MaxGeneration + 1];
        for (int i = 0; i <= GC.MaxGeneration; i++)
        {
            gcAfter[i] = GC.CollectionCount(i);
        }
        
        Console.WriteLine($"🎯 {scenario.Key}:");
        Console.WriteLine($"   ⏱️  Tempo: {sw.ElapsedMilliseconds}ms");
        
        for (int i = 0; i <= GC.MaxGeneration; i++)
        {
            var collections = gcAfter[i] - gcBefore[i];
            if (collections > 0)
            {
                Console.WriteLine($"   ♻️  Gen {i}: +{collections} coletas");
            }
        }
    }
}

static async Task CriarMuitasAlocacoesPequenas()
{
    var objects = new List<SmallObject>();
    for (int i = 0; i < 10_000; i++)
    {
        objects.Add(new SmallObject { Id = i });
        if (i % 1000 == 0) await Task.Yield();
    }
}

static async Task CriarPoucasAlocacoesGrandes()
{
    var objects = new List<LargeObject>();
    for (int i = 0; i < 10; i++)
    {
        objects.Add(new LargeObject(100_000));
        await Task.Delay(10);
    }
}

static async Task CriarObjetosVidaLonga()
{
    var objects = new List<object>();
    for (int i = 0; i < 1000; i++)
    {
        objects.Add(new { Id = i, Data = new byte[1024], Created = DateTime.Now });
        if (i % 100 == 0) await Task.Yield();
    }
    
    // Manter referências (simula vida longa)
    await Task.Delay(100);
}

static async Task CriarObjetosTemporarios()
{
    for (int i = 0; i < 1000; i++)
    {
        var temp = new { Id = i, Data = new byte[1024] };
        // Não manter referência (objeto temporário)
        if (i % 100 == 0) await Task.Yield();
    }
}

static void DemonstrarGCNotifications()
{
    Console.WriteLine("🔔 Configurando notificações de GC...");
    
    try
    {
        // Registrar para notificações de GC (se disponível)
        GC.RegisterForFullGCNotification(10, 10);
        Console.WriteLine("✅ Notificações de GC registradas");
        
        // Simular carga que pode disparar GC
        var objects = new List<object>();
        for (int i = 0; i < 1000; i++)
        {
            objects.Add(new byte[10_000]);
        }
        
        Console.WriteLine("📊 Verificando status de GC...");
        var status = GC.WaitForFullGCApproach(100);
        Console.WriteLine($"   Status: {status}");
        
        // Cleanup
        GC.CancelFullGCNotification();
        Console.WriteLine("🧹 Notificações canceladas");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro com notificações: {ex.Message}");
    }
}

// Classes auxiliares para demonstração
public class SmallObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "Small";
    public DateTime Created { get; set; } = DateTime.Now;
}

public class MediumObject
{
    public int[] Data { get; set; } = new int[1000]; // ~4KB
    public string Description { get; set; } = "Medium object for GC testing";
    public DateTime Created { get; set; } = DateTime.Now;
    public Guid UniqueId { get; set; } = Guid.NewGuid();
}

public class LargeObject
{
    public byte[] Data { get; private set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public int Size => Data?.Length ?? 0;
    
    public LargeObject(int size = 10_000)
    {
        Data = new byte[size];
        
        // Preencher com dados para simular uso real
        Random.Shared.NextBytes(Data);
    }
}

public class DisposableResource : IDisposable
{
    private bool _disposed = false;
    private readonly byte[] _resource = new byte[1024];
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Console.WriteLine("🧹 Recurso gerenciado liberado via Dispose");
            }
            _disposed = true;
        }
    }
    
    ~DisposableResource()
    {
        Dispose(false);
        Console.WriteLine("🔔 Finalizador executado pelo GC");
    }
}
