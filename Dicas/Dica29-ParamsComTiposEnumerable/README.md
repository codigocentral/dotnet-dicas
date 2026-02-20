# Dica 29: Params com Tipos Enumerable (C# 13)

## 📋 Problema

A palavra-chave `params` tradicionalmente era limitada a tipos `array`, o que causava:

- **Alocações desnecessárias no heap** para cada chamada
- **Limitações de tipos** - apenas arrays eram suportados  
- **Performance subótima** em cenários de alta frequência
- **Menos flexibilidade** para diferentes tipos de coleção

```csharp
// ❌ C# 12 e anteriores - apenas arrays
public void ProcessItems(params string[] items) // Sempre aloca no heap
{
    foreach (var item in items)
        Console.WriteLine(item);
}
```

## ✅ Solução

C# 13 introduziu uma **atualização massiva** ao `params`, permitindo:

- **Múltiplos tipos `IEnumerable`** (List, IEnumerable, Span, ReadOnlySpan)
- **Performance significativamente melhor**
- **Alocações reduzidas ou zero** dependendo do tipo
- **Mais opções para o consumidor**

```csharp
// ✅ C# 13 - múltiplos tipos suportados
public void ProcessItems(params ReadOnlySpan<string> items) // Zero alocações!
{
    foreach (var item in items)
        Console.WriteLine(item);
}
```

## 💡 Tipos Suportados em C# 13

### 1. Span<T> - Zero Alocações
```csharp
public static void PrintNumbers(params Span<int> numbers)
{
    foreach (var num in numbers)
        Console.WriteLine(num);
}

// Uso:
PrintNumbers(1, 2, 3, 4); // Alocado na stack
```

### 2. ReadOnlySpan<T> - Zero Alocações + Imutabilidade
```csharp
public static void PrintItems(params ReadOnlySpan<string> items)
{
    foreach (var item in items)
        Console.WriteLine($"Item: {item}");
}

// Uso:
PrintItems("A", "B", "C"); // Stack allocation
```

### 3. IEnumerable<T> - Máxima Flexibilidade
```csharp
public static void ProcessCollection(params IEnumerable<int> numbers)
{
    Console.WriteLine($"Sum: {numbers.Sum()}");
}

// Uso:
ProcessCollection(1, 2, 3); // Pode usar qualquer IEnumerable
```

### 4. List<T> - Familiar e Eficiente
```csharp
public static void ManageList(params List<string> items)
{
    items.Add("Extra Item"); // Pode modificar
    Console.WriteLine($"Total: {items.Count}");
}
```

## 🚀 Comparação de Performance

### Benchmark: Array vs Span vs ReadOnlySpan

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ParamsBenchmark
{
    [Benchmark(Baseline = true)]
    public void ArrayParams()
    {
        ProcessArray(1, 2, 3, 4, 5);
    }
    
    [Benchmark]
    public void SpanParams()
    {
        ProcessSpan(1, 2, 3, 4, 5);
    }
    
    [Benchmark]
    public void ReadOnlySpanParams()
    {
        ProcessReadOnlySpan(1, 2, 3, 4, 5);
    }
    
    private static void ProcessArray(params int[] numbers) { }
    private static void ProcessSpan(params Span<int> numbers) { }
    private static void ProcessReadOnlySpan(params ReadOnlySpan<int> numbers) { }
}
```

### Resultados Esperados:
```
|              Method |      Mean |   Error | Allocated |
|-------------------- |----------:|--------:|----------:|
|          ArrayParams| 10.00 ns  | 0.1 ns  |     40 B  |
|           SpanParams|  2.50 ns  | 0.1 ns  |      0 B  |
| ReadOnlySpanParams  |  2.30 ns  | 0.1 ns  |      0 B  |
```

## 🎯 Casos de Uso Práticos

### 1. Logging de Alta Performance
```csharp
public static class HighPerformanceLogger
{
    public static void LogValues(params ReadOnlySpan<object> values)
    {
        if (!IsEnabled) return;
        
        foreach (var value in values)
            WriteToLog(value?.ToString() ?? "null");
    }
}

// Uso sem alocações:
HighPerformanceLogger.LogValues("User:", userId, "Action:", action);
```

### 2. Cálculos Matemáticos
```csharp
public static class MathUtils
{
    public static double Average(params ReadOnlySpan<double> numbers)
    {
        if (numbers.IsEmpty) return 0;
        
        double sum = 0;
        foreach (var num in numbers)
            sum += num;
            
        return sum / numbers.Length;
    }
}

// Uso:
var avg = MathUtils.Average(1.5, 2.5, 3.5, 4.5); // Zero heap allocations
```

### 3. Validação de Dados
```csharp
public static class Validator
{
    public static bool AllValid(params ReadOnlySpan<string> inputs)
    {
        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
        }
        return true;
    }
}

// Uso:
bool isValid = Validator.AllValid(name, email, phone);
```

### 4. Construção de Strings Eficiente
```csharp
public static class StringBuilderExtensions
{
    public static StringBuilder AppendAll(this StringBuilder sb, 
        params ReadOnlySpan<string> values)
    {
        foreach (var value in values)
            sb.Append(value);
        return sb;
    }
}

// Uso:
var result = new StringBuilder()
    .AppendAll("Hello", " ", "World", "!");
```

## 🔄 Migração do Código Existente

### Antes (C# 12):
```csharp
public class OldWay
{
    // ❌ Apenas arrays, sempre aloca
    public static void Process(params string[] items)
    {
        foreach (var item in items)
            DoWork(item);
    }
    
    public static int Sum(params int[] numbers)
    {
        return numbers.Sum(); // Heap allocation
    }
}
```

### Depois (C# 13):
```csharp
public class NewWay
{
    // ✅ ReadOnlySpan - zero alocações
    public static void Process(params ReadOnlySpan<string> items)
    {
        foreach (var item in items)
            DoWork(item);
    }
    
    // ✅ Span para operações que podem modificar
    public static int Sum(params ReadOnlySpan<int> numbers)
    {
        int total = 0;
        foreach (var num in numbers) // No LINQ to avoid allocations
            total += num;
        return total;
    }
}
```

## ⚠️ Considerações Importantes

### 1. Compatibilidade
```csharp
// C# 13 é backward compatible
public void Method(params ReadOnlySpan<int> numbers) { }

// Todas essas chamadas funcionam:
Method(1, 2, 3);                    // Implicit conversion
Method(new[] { 1, 2, 3 });          // Array
Method(new List<int> { 1, 2, 3 });  // List (via IEnumerable)
Method(stackalloc int[] { 1, 2, 3 }); // Stackalloc
```

### 2. Escolha do Tipo Correto
```csharp
// Use ReadOnlySpan<T> quando:
// - Performance é crítica
// - Dados são apenas leitura
// - Quer evitar alocações

// Use Span<T> quando:
// - Precisa modificar os dados
// - Performance é crítica

// Use IEnumerable<T> quando:
// - Flexibilidade é mais importante que performance
// - API pode receber diferentes tipos de coleção

// Use List<T> quando:
// - Precisa adicionar/remover itens
// - Familiaridade da API é importante
```

### 3. Limitações do Span
```csharp
// ❌ Span não pode ser usado em métodos async
public async Task<int> ProcessAsync(params Span<int> numbers) // Compile error!
{
    await Task.Delay(100);
    return numbers.Length;
}

// ✅ Use IEnumerable para métodos async
public async Task<int> ProcessAsync(params IEnumerable<int> numbers)
{
    await Task.Delay(100);
    return numbers.Count();
}
```

## 🏆 Melhores Práticas

### 1. Priorize ReadOnlySpan para Performance
```csharp
// ✅ Melhor para hot paths
public static bool Contains(params ReadOnlySpan<int> values, int target)
{
    foreach (var value in values)
        if (value == target) return true;
    return false;
}
```

### 2. Use IEnumerable para APIs Públicas Flexíveis
```csharp
// ✅ Máxima compatibilidade
public static void LogItems(params IEnumerable<object> items)
{
    foreach (var item in items)
        Logger.Log(item?.ToString());
}
```

### 3. Documente a Escolha
```csharp
/// <summary>
/// Calcula a média dos números fornecidos.
/// Usa ReadOnlySpan para zero alocações em hot paths.
/// </summary>
/// <param name="numbers">Números para calcular a média</param>
/// <returns>Média dos números</returns>
public static double Average(params ReadOnlySpan<double> numbers)
{
    // implementação
}
```

## 🔗 Recursos Adicionais

- [C# 13 Params Collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)
- [Span<T> Performance Guide](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)
- [Memory and Span Usage Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/)
