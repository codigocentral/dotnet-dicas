using System.Collections;
using System.Diagnostics;

Console.WriteLine("==== Dica 47: LINQ e Execução Deferida (Deferred Execution) ====\n");

Console.WriteLine("⚠️  LINQ usa EXECUÇÃO DEFERIDA - operações só são executadas quando você ITERA!");
Console.WriteLine("Vamos demonstrar por que isso é importante para performance...\n");

// ===== DEMONSTRAÇÃO 1: DIFERENÇA ENTRE EXECUÇÃO DEFERIDA E IMEDIATA =====
Console.WriteLine("1. Diferença fundamental: Deferred vs Immediate Execution");
Console.WriteLine("----------------------------------------------------------");

var sourceData = Enumerable.Range(1, 1_000_000).ToList();
Console.WriteLine($"📊 Dataset: {sourceData.Count:N0} números");

// LINQ com execução deferida (IEnumerable<T>)
var sw = Stopwatch.StartNew();
var deferredQuery = sourceData
    .Where(x => x % 2 == 0)
    .Select(x => x * 2)
    .Take(100);
sw.Stop();
Console.WriteLine($"✅ Query DEFERIDA criada em: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • Tipo: {deferredQuery.GetType().Name}");
Console.WriteLine($"   • ⚠️  NADA foi processado ainda!");

// Execução imediata com ToList()
sw.Restart();
var immediateQuery = sourceData
    .Where(x => x % 2 == 0)
    .Select(x => x * 2)
    .Take(100)
    .ToList();
sw.Stop();
Console.WriteLine($"❌ Query IMEDIATA executada em: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • Tipo: {immediateQuery.GetType().Name}");
Console.WriteLine($"   • ✅ Dados processados e na memória!\n");

// ===== DEMONSTRAÇÃO 2: MULTIPLE ENUMERATION PROBLEM =====
Console.WriteLine("2. Problema da MÚLTIPLA ENUMERAÇÃO (Performance Killer!)");
Console.WriteLine("--------------------------------------------------------");

// Query deferida que será enumerada múltiplas vezes
var expensiveQuery = GenerateExpensiveData()
    .Where(item => item.IsValid)
    .Select(item => item.Value);

Console.WriteLine("❌ ANTI-PATTERN: Múltipla enumeração da mesma query");
sw.Restart();
var count = expensiveQuery.Count();
var sum = expensiveQuery.Sum();
var avg = expensiveQuery.Average();
sw.Stop();
Console.WriteLine($"   • Count: {count:N0}");
Console.WriteLine($"   • Sum: {sum:N0}");
Console.WriteLine($"   • Average: {avg:F2}");
Console.WriteLine($"   • ⚡ Tempo total: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • 🔥 A query foi executada 3 VEZES!");

Console.WriteLine("\n✅ SOLUÇÃO: Materializar uma vez com ToList/ToArray");
sw.Restart();
var materialized = expensiveQuery.ToList();
var countMat = materialized.Count;
var sumMat = materialized.Sum();
var avgMat = materialized.Average();
sw.Stop();
Console.WriteLine($"   • Count: {countMat:N0}");
Console.WriteLine($"   • Sum: {sumMat:N0}");
Console.WriteLine($"   • Average: {avgMat:F2}");
Console.WriteLine($"   • ⚡ Tempo total: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • ✅ A query foi executada APENAS 1 VEZ!\n");

// ===== DEMONSTRAÇÃO 3: LAZY EVALUATION =====
Console.WriteLine("3. Lazy Evaluation - Queries são construídas, não executadas");
Console.WriteLine("-------------------------------------------------------------");

Console.WriteLine("Criando query complexa...");
var lazyQuery = GetLargeDataset()
    .Where(LogPredicate("Filtro 1"))
    .Select(LogTransform("Transform 1"))
    .Where(LogPredicate("Filtro 2"))
    .Select(LogTransform("Transform 2"));

Console.WriteLine("✅ Query criada - NENHUMA operação foi executada ainda!\n");

Console.WriteLine("Agora vamos ITERAR (enumerar) a query:");
var result = lazyQuery.Take(3).ToList();
Console.WriteLine($"✅ Resultado: [{string.Join(", ", result)}]\n");

// ===== DEMONSTRAÇÃO 4: YIELD vs ToList PERFORMANCE =====
Console.WriteLine("4. Yield vs ToList - Quando usar cada um");
Console.WriteLine("------------------------------------------");

// Cenário: Processamos apenas os primeiros 5 elementos
var largeDataset = Enumerable.Range(1, 10_000_000);

// Com yield (deferred) - processa apenas o necessário
sw.Restart();
var yieldResult = ProcessWithYield(largeDataset).Take(5).ToList();
sw.Stop();
Console.WriteLine($"✅ Yield (deferred): {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • Processou apenas: 5 elementos");
Console.WriteLine($"   • Resultado: [{string.Join(", ", yieldResult)}]");

// Com ToList (immediate) - processa TUDO desnecessariamente
sw.Restart();
var listResult = ProcessWithToList(largeDataset).Take(5).ToList();
sw.Stop();
Console.WriteLine($"❌ ToList (immediate): {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"   • Processou todos: 10,000,000 elementos");
Console.WriteLine($"   • Resultado: [{string.Join(", ", listResult)}]");

// ===== DEMONSTRAÇÃO 5: CUIDADOS COM CLOSURE EM QUERIES =====
Console.WriteLine("\n5. Cuidado com Closures em LINQ Queries");
Console.WriteLine("----------------------------------------");

DemonstrateClosure();

Console.WriteLine("\n=== RESUMO: Boas Práticas com LINQ Deferred Execution ===");
Console.WriteLine("✅ USE execução deferida quando:");
Console.WriteLine("   • Processará apenas parte dos dados (Take, First, etc.)");
Console.WriteLine("   • Quer composição de queries flexível");
Console.WriteLine("   • Pipeline de transformações grandes");
Console.WriteLine();
Console.WriteLine("❌ EVITE execução deferida quando:");
Console.WriteLine("   • Vai enumerar múltiplas vezes");
Console.WriteLine("   • Precisa de todas as operações LINQ (Count, Sum, Average)");
Console.WriteLine("   • Query depende de variáveis que podem mudar");
Console.WriteLine();
Console.WriteLine("🔑 REGRA DE OURO: Materialize (ToList/ToArray) quando for usar múltiplas vezes!");

// ===== MÉTODOS DE APOIO =====

static IEnumerable<DataItem> GenerateExpensiveData()
{
    Console.WriteLine("   🔄 Gerando dados caros...");
    for (int i = 1; i <= 1000; i++)
    {
        // Simula operação custosa
        Thread.Sleep(1);
        yield return new DataItem(i, i % 3 == 0);
    }
}

static Func<int, bool> LogPredicate(string name)
{
    return x =>
    {
        Console.WriteLine($"      {name}: verificando {x}");
        return x % 2 == 0;
    };
}

static Func<int, int> LogTransform(string name)
{
    return x =>
    {
        Console.WriteLine($"      {name}: transformando {x} -> {x * 10}");
        return x * 10;
    };
}

static IEnumerable<int> GetLargeDataset()
{
    return Enumerable.Range(1, 20);
}

static IEnumerable<int> ProcessWithYield(IEnumerable<int> source)
{
    foreach (var item in source)
    {
        // Simula processamento pesado
        var result = item * item;
        yield return result;
    }
}

static List<int> ProcessWithToList(IEnumerable<int> source)
{
    var result = new List<int>();
    foreach (var item in source)
    {
        // Simula processamento pesado
        result.Add(item * item);
    }
    return result;
}

static void DemonstrateClosure()
{
    var multiplier = 2;
    var numbers = Enumerable.Range(1, 5);
    
    // Query que captura 'multiplier' por closure
    var query = numbers.Select(x => x * multiplier);
    
    Console.WriteLine($"Multiplier inicial: {multiplier}");
    Console.WriteLine($"Query resultado: [{string.Join(", ", query)}]");
    
    // PROBLEMA: mudamos a variável APÓS criar a query
    multiplier = 10;
    Console.WriteLine($"\nMultiplier mudou para: {multiplier}");
    Console.WriteLine($"Query resultado: [{string.Join(", ", query)}]");
    Console.WriteLine("⚠️  A query usa o valor ATUAL da variável!");
    
    // SOLUÇÃO: materializar quando necessário
    multiplier = 2;
    var materialized = numbers.Select(x => x * multiplier).ToList();
    multiplier = 10;
    Console.WriteLine($"\nQuery materializada: [{string.Join(", ", materialized)}]");
    Console.WriteLine("✅ Query materializada mantém valores originais!");
}

public record DataItem(int Value, bool IsValid);
