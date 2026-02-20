using System.Text;

Console.WriteLine("🚀 Dica 29: Params com Tipos Enumerable (C# 13)");
Console.WriteLine("================================================");
Console.WriteLine();

Console.WriteLine("⚠️  NOTA: C# 13 ainda está em desenvolvimento.");
Console.WriteLine("📋 Esta dica mostra como será no futuro + alternativas atuais.");
Console.WriteLine();

// Demonstração 1: Comparação Array vs ReadOnlySpan (simulado)
Console.WriteLine("📊 1. Comparação: Array vs ReadOnlySpan (Conceitual)");
Console.WriteLine("----------------------------------------------------");

// ❌ Forma atual - sempre aloca no heap
Console.WriteLine("❌ Array params (heap allocation):");
ProcessArray("Item1", "Item2", "Item3");

// ✅ Forma que será possível em C# 13
Console.WriteLine("✅ ReadOnlySpan params (futuro C# 13):");
Console.WriteLine("   // public static void Method(params ReadOnlySpan<string> items)");
ProcessReadOnlySpanSimulated("Item1", "Item2", "Item3");

Console.WriteLine();

// Demonstração 2: Diferentes tipos que serão suportados
Console.WriteLine("📋 2. Tipos que Serão Suportados em C# 13");
Console.WriteLine("-----------------------------------------");

Console.WriteLine("🔧 Span<T> (modificável):");
Console.WriteLine("   // public static void Method(params Span<int> numbers)");
ProcessSpanSimulated(1, 2, 3, 4, 5);

Console.WriteLine("🔒 ReadOnlySpan<T> (imutável):");
Console.WriteLine("   // public static void Method(params ReadOnlySpan<int> numbers)");
ProcessReadOnlySpanIntSimulated(10, 20, 30, 40, 50);

Console.WriteLine("🔄 IEnumerable<T> (flexível):");
Console.WriteLine("   // public static void Method(params IEnumerable<int> numbers)");
ProcessIEnumerableSimulated(100, 200, 300);

Console.WriteLine("📝 List<T> (familiar):");
Console.WriteLine("   // public static void Method(params List<string> items)");
ProcessListSimulated("A", "B", "C");

Console.WriteLine();

// Demonstração 3: Alternativas atuais
Console.WriteLine("🔧 3. Alternativas Disponíveis Hoje");
Console.WriteLine("-----------------------------------");

Console.WriteLine("� Usando ReadOnlySpan com métodos normais:");
ProcessCurrentReadOnlySpan(new ReadOnlySpan<int>([1, 2, 3, 4, 5]));

Console.WriteLine("⚡ Usando stackalloc:");
ProcessCurrentSpan(stackalloc int[] { 10, 20, 30 });

Console.WriteLine("🎯 Usando array como parâmetro:");
ProcessCurrentArray([100, 200, 300]);

Console.WriteLine();

// Demonstração 4: Casos de uso práticos (simulados)
Console.WriteLine("🎯 4. Casos de Uso Futuros");
Console.WriteLine("--------------------------");

Console.WriteLine("📝 Logging de Alta Performance (futuro):");
Console.WriteLine("   // HighPerformanceLogger.LogValues('User:', 12345, 'Action:', 'Login')");
HighPerformanceLoggerCurrent.LogValues("User: 12345 Action: Login Success: true");

Console.WriteLine("🧮 Cálculos Matemáticos (futuro):");
Console.WriteLine($"   // Média: {MathUtilsCurrent.Average([1.5, 2.5, 3.5, 4.5])}");

Console.WriteLine("✅ Validação de Dados (futuro):");
Console.WriteLine($"   // Dados válidos: {ValidatorCurrent.AllValid(["João", "joao@email.com", "(11) 99999-9999"])}");

Console.WriteLine();

Console.WriteLine("✅ Demonstração concluída!");
Console.WriteLine("💡 C# 13 params trará muito mais flexibilidade e performance!");
Console.WriteLine("🔮 Por enquanto, use as alternativas mostradas acima.");

// ================================================
// MÉTODOS ATUAIS (C# 8-12)
// ================================================

// ❌ Forma atual - Array params
static void ProcessArray(params string[] items)
{
    Console.WriteLine($"   Processando {items.Length} items (heap allocation)");
    foreach (var item in items)
        Console.WriteLine($"   - {item}");
}

// Simulação de como será o ReadOnlySpan params
static void ProcessReadOnlySpanSimulated(params string[] items)
{
    var span = new ReadOnlySpan<string>(items);
    Console.WriteLine($"   Processando {span.Length} items (zero allocation - futuro)");
    foreach (var item in span)
        Console.WriteLine($"   - {item}");
}

static void ProcessSpanSimulated(params int[] numbers)
{
    var span = new Span<int>(numbers);
    Console.WriteLine($"   Processando {span.Length} números (modificável - futuro):");
    
    // Simula modificação
    for (int i = 0; i < span.Length; i++)
        span[i] *= 2;
    
    foreach (var num in span)
        Console.WriteLine($"   - {num}");
}

static void ProcessReadOnlySpanIntSimulated(params int[] numbers)
{
    var span = new ReadOnlySpan<int>(numbers);
    Console.WriteLine($"   Processando {span.Length} números (imutável - futuro):");
    foreach (var num in span)
        Console.WriteLine($"   - {num}");
}

static void ProcessIEnumerableSimulated(params int[] numbers)
{
    IEnumerable<int> enumerable = numbers;
    Console.WriteLine($"   Processando números (flexível - futuro):");
    Console.WriteLine($"   - Soma: {enumerable.Sum()}");
    Console.WriteLine($"   - Média: {enumerable.Average():F2}");
}

static void ProcessListSimulated(params string[] items)
{
    var list = new List<string>(items) { "Extra Item" };
    Console.WriteLine($"   Processando {list.Count} items (list - futuro):");
    
    foreach (var item in list)
        Console.WriteLine($"   - {item}");
}

// ================================================
// MÉTODOS ALTERNATIVOS ATUAIS
// ================================================

static void ProcessCurrentReadOnlySpan(ReadOnlySpan<int> numbers)
{
    Console.WriteLine($"   Processando {numbers.Length} números (ReadOnlySpan atual):");
    foreach (var num in numbers)
        Console.WriteLine($"   - {num}");
}

static void ProcessCurrentSpan(Span<int> numbers)
{
    Console.WriteLine($"   Processando {numbers.Length} números (Span atual):");
    foreach (var num in numbers)
        Console.WriteLine($"   - {num}");
}

static void ProcessCurrentArray(int[] numbers)
{
    Console.WriteLine($"   Processando {numbers.Length} números (array atual):");
    foreach (var num in numbers)
        Console.WriteLine($"   - {num}");
}

// ================================================
// CLASSES UTILITÁRIAS ATUAIS
// ================================================

public static class HighPerformanceLoggerCurrent
{
    public static void LogValues(string message)
    {
        Console.WriteLine($"   LOG: {message}");
    }
}

public static class MathUtilsCurrent
{
    public static double Average(double[] numbers)
    {
        if (numbers.Length == 0) return 0;
        
        double sum = 0;
        foreach (var num in numbers)
            sum += num;
            
        return sum / numbers.Length;
    }
}

public static class ValidatorCurrent
{
    public static bool AllValid(string[] inputs)
    {
        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
        }
        return true;
    }
}
