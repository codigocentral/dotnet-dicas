using System.Diagnostics;

Console.WriteLine("==== Dica 68: Value Tuples vs Tuple - Performance e Usabilidade ====\n");

Console.WriteLine("✅ Value Tuples (C# 7+) são SUPERIORES aos Tuple tradicionais!");
Console.WriteLine("Vamos demonstrar as diferenças de performance e usabilidade...\n");

// ===== DEMONSTRAÇÃO 1: DIFERENÇA DE SINTAXE =====
Console.WriteLine("1. Diferença de Sintaxe e Usabilidade");
Console.WriteLine("-------------------------------------");

// Tuple tradicional (reference type)
Tuple<string, int, decimal> oldTuple = new Tuple<string, int, decimal>("João", 30, 5000.50m);
Console.WriteLine("❌ Tuple tradicional (reference type):");
Console.WriteLine($"   • Nome: {oldTuple.Item1}");
Console.WriteLine($"   • Idade: {oldTuple.Item2}");
Console.WriteLine($"   • Salário: {oldTuple.Item3:C}");
Console.WriteLine($"   • Tipo: {oldTuple.GetType().Name}");
Console.WriteLine($"   • ⚠️  Item1, Item2, Item3 - sem nomes semânticos!");

// Value Tuple (value type) - sintaxe moderna
(string nome, int idade, decimal salario) valueTuple = ("Maria", 28, 6000.75m);
Console.WriteLine("\n✅ Value Tuple (value type):");
Console.WriteLine($"   • Nome: {valueTuple.nome}");
Console.WriteLine($"   • Idade: {valueTuple.idade}");
Console.WriteLine($"   • Salário: {valueTuple.salario:C}");
Console.WriteLine($"   • Tipo: {valueTuple.GetType().Name}");
Console.WriteLine($"   • ✅ Nomes semânticos e sintaxe limpa!");

// Sintaxe ainda mais limpa
var pessoa = (nome: "Ana", idade: 25, salario: 4500.00m);
Console.WriteLine($"\n✨ Sintaxe var: {pessoa.nome}, {pessoa.idade} anos, {pessoa.salario:C}\n");

// ===== DEMONSTRAÇÃO 2: PERFORMANCE COMPARISON =====
Console.WriteLine("2. Comparação de Performance");
Console.WriteLine("-----------------------------");

const int iterations = 1_000_000;
var sw = Stopwatch.StartNew();

// Teste com Tuple tradicional
sw.Restart();
long tupleSum = 0;
for (int i = 0; i < iterations; i++)
{
    var tuple = CreateTuple(i);
    tupleSum += tuple.Item2;
}
sw.Stop();
var tupleTime = sw.ElapsedMilliseconds;
Console.WriteLine($"❌ Tuple tradicional: {tupleTime} ms");
Console.WriteLine($"   • {iterations:N0} criações e acessos");
Console.WriteLine($"   • Soma: {tupleSum:N0}");

// Teste com Value Tuple
sw.Restart();
long valueTupleSum = 0;
for (int i = 0; i < iterations; i++)
{
    var vTuple = CreateValueTuple(i);
    valueTupleSum += vTuple.value;
}
sw.Stop();
var valueTupleTime = sw.ElapsedMilliseconds;
Console.WriteLine($"\n✅ Value Tuple: {valueTupleTime} ms");
Console.WriteLine($"   • {iterations:N0} criações e acessos");
Console.WriteLine($"   • Soma: {valueTupleSum:N0}");

var improvement = tupleTime > 0 ? (double)tupleTime / valueTupleTime : 1;
Console.WriteLine($"\n⚡ Value Tuple é {improvement:F1}x mais rápido!\n");

// ===== DEMONSTRAÇÃO 3: MEMORY ALLOCATION =====
Console.WriteLine("3. Impacto na Alocação de Memória");
Console.WriteLine("----------------------------------");

// Força coleta de lixo antes dos testes
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

var initialMemory = GC.GetTotalMemory(false);

// Teste de alocação com Tuple
for (int i = 0; i < 100_000; i++)
{
    var tuple = CreateTuple(i);
    // Simula uso
    _ = tuple.Item1 + tuple.Item2;
}

var afterTupleMemory = GC.GetTotalMemory(false);
var tupleMemoryUsed = afterTupleMemory - initialMemory;

Console.WriteLine($"❌ Tuple tradicional alocou: {tupleMemoryUsed:N0} bytes");

// Força coleta antes do próximo teste
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

initialMemory = GC.GetTotalMemory(false);

// Teste de alocação com Value Tuple
for (int i = 0; i < 100_000; i++)
{
    var vTuple = CreateValueTuple(i);
    // Simula uso
    _ = vTuple.name + vTuple.value;
}

var afterValueTupleMemory = GC.GetTotalMemory(false);
var valueTupleMemoryUsed = afterValueTupleMemory - initialMemory;

Console.WriteLine($"✅ Value Tuple alocou: {valueTupleMemoryUsed:N0} bytes");
Console.WriteLine($"💾 Redução de memória: {tupleMemoryUsed - valueTupleMemoryUsed:N0} bytes\n");

// ===== DEMONSTRAÇÃO 4: RETORNO DE MÚLTIPLOS VALORES =====
Console.WriteLine("4. Retorno de Múltiplos Valores de Métodos");
Console.WriteLine("------------------------------------------");

// Método com Tuple tradicional
var oldResult = CalculateStatsOld(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
Console.WriteLine("❌ Método com Tuple tradicional:");
Console.WriteLine($"   • Soma: {oldResult.Item1}");
Console.WriteLine($"   • Média: {oldResult.Item2:F2}");
Console.WriteLine($"   • Máximo: {oldResult.Item3}");

// Método com Value Tuple
var newResult = CalculateStatsNew(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
Console.WriteLine("\n✅ Método com Value Tuple:");
Console.WriteLine($"   • Soma: {newResult.sum}");
Console.WriteLine($"   • Média: {newResult.average:F2}");
Console.WriteLine($"   • Máximo: {newResult.max}");

// Destructuring - recurso poderoso dos Value Tuples
var (soma, media, maximo) = CalculateStatsNew(new[] { 5, 10, 15, 20, 25 });
Console.WriteLine("\n✨ Com destructuring:");
Console.WriteLine($"   • Soma: {soma}");
Console.WriteLine($"   • Média: {media:F2}");
Console.WriteLine($"   • Máximo: {maximo}\n");

// ===== DEMONSTRAÇÃO 5: VALUE TUPLES EM ESTRUTURAS DE DADOS =====
Console.WriteLine("5. Value Tuples em Estruturas de Dados");
Console.WriteLine("--------------------------------------");

// Dictionary com chaves compostas
var coordinates = new Dictionary<(int x, int y), string>
{
    { (0, 0), "Origem" },
    { (1, 1), "Diagonal" },
    { (5, 3), "Ponto A" },
    { (-2, 4), "Ponto B" }
};

Console.WriteLine("✅ Dictionary com chaves Value Tuple:");
foreach (var ((x, y), description) in coordinates)
{
    Console.WriteLine($"   • ({x}, {y}) -> {description}");
}

// Lista de resultados
var results = new List<(string operation, TimeSpan duration, bool success)>
{
    ("Login", TimeSpan.FromMilliseconds(150), true),
    ("Database Query", TimeSpan.FromMilliseconds(350), true),
    ("File Upload", TimeSpan.FromMilliseconds(2000), false),
    ("Email Send", TimeSpan.FromMilliseconds(800), true)
};

Console.WriteLine("\n✅ Lista de operações com Value Tuples:");
foreach (var (operation, duration, success) in results)
{
    var status = success ? "✅" : "❌";
    Console.WriteLine($"   • {status} {operation}: {duration.TotalMilliseconds}ms");
}

// ===== DEMONSTRAÇÃO 6: LINQ COM VALUE TUPLES =====
Console.WriteLine("\n6. LINQ com Value Tuples");
Console.WriteLine("-------------------------");

var products = new[]
{
    (name: "Notebook", category: "Electronics", price: 2500.00m),
    (name: "Mouse", category: "Electronics", price: 50.00m),
    (name: "Chair", category: "Furniture", price: 300.00m),
    (name: "Desk", category: "Furniture", price: 800.00m),
    (name: "Headphones", category: "Electronics", price: 200.00m)
};

var summary = products
    .GroupBy(p => p.category)
    .Select(g => (
        category: g.Key,
        count: g.Count(),
        avgPrice: g.Average(p => p.price),
        totalValue: g.Sum(p => p.price)
    ))
    .OrderByDescending(s => s.totalValue);

Console.WriteLine("✅ Resumo por categoria (usando LINQ + Value Tuples):");
foreach (var (category, count, avgPrice, totalValue) in summary)
{
    Console.WriteLine($"   • {category}: {count} items, Média: {avgPrice:C}, Total: {totalValue:C}");
}

Console.WriteLine("\n=== RESUMO: Por que usar Value Tuples ===");
Console.WriteLine("✅ VANTAGENS dos Value Tuples:");
Console.WriteLine("   • Performance superior (value type, stack allocation)");
Console.WriteLine("   • Menor uso de memória (sem alocação no heap)");
Console.WriteLine("   • Sintaxe limpa e legível");
Console.WriteLine("   • Nomes semânticos para campos");
Console.WriteLine("   • Destructuring support");
Console.WriteLine("   • Excelente integração com LINQ");
Console.WriteLine("   • Imutabilidade por padrão");
Console.WriteLine();
Console.WriteLine("❌ EVITE Tuple tradicional:");
Console.WriteLine("   • Reference type (heap allocation)");
Console.WriteLine("   • Item1, Item2, Item3 (sem semântica)");
Console.WriteLine("   • Performance inferior");
Console.WriteLine("   • Maior pressão no GC");
Console.WriteLine();
Console.WriteLine("🔑 REGRA: SEMPRE prefira Value Tuples para múltiplos retornos!");

// ===== MÉTODOS DE APOIO =====

static Tuple<string, int> CreateTuple(int value)
{
    return new Tuple<string, int>($"Item{value}", value);
}

static (string name, int value) CreateValueTuple(int value)
{
    return ($"Item{value}", value);
}

static Tuple<int, double, int> CalculateStatsOld(int[] numbers)
{
    var sum = numbers.Sum();
    var average = numbers.Average();
    var max = numbers.Max();
    return new Tuple<int, double, int>(sum, average, max);
}

static (int sum, double average, int max) CalculateStatsNew(int[] numbers)
{
    return (numbers.Sum(), numbers.Average(), numbers.Max());
}
