# Dica 26: Async/Await Best Practices

## 📋 Sobre

Esta dica demonstra as **melhores práticas** ao trabalhar com `async/await` em C#, cobrindo tópicos essenciais como `ConfigureAwait`, tratamento de exceções, prevenção de deadlocks, e otimizações de performance com `ValueTask`.

## 🎯 Conceitos Demonstrados

### 1. **ConfigureAwait Best Practices**
- **Library Code**: Sempre usar `ConfigureAwait(false)` para evitar deadlocks
- **Application Code**: `ConfigureAwait(false)` é opcional mas recomendado
- **SynchronizationContext**: Como evitar problemas de contexto

### 2. **Exception Handling em Métodos Async**
- Tratamento correto de exceções em código assíncrono
- `AggregateException` com `Task.WhenAll`
- Propagação de exceções através da cadeia async

### 3. **Avoiding Async Void**
- Por que `async void` é problemático
- Quando usar `async Task` vs `async void`
- Problemas de captura de exceções

### 4. **Task.WhenAll vs Multiple Awaits**
- Execução paralela vs sequencial
- Impacto significativo na performance
- Quando usar cada abordagem

### 5. **CancellationToken Best Practices**
- Propagação correta de `CancellationToken`
- Verificações regulares com `ThrowIfCancellationRequested()`
- Padrões de cancelamento cooperativo

### 6. **ValueTask para Hot Paths**
- Quando usar `ValueTask<T>` vs `Task<T>`
- Otimização de alocações em operações síncronas
- Padrões de cache com `ValueTask`

## 🚀 Como Executar

### Executar o Projeto Principal
```bash
cd "Dicas/Dica26-AsyncAwaitBestPractices/Dica26"
dotnet run
```

### Executar os Benchmarks
```bash
cd "Dicas/Dica26-AsyncAwaitBestPractices/Dica26.Benchmark"
dotnet run -c Release
```

## 📊 Resultados de Performance Esperados

### Sequential vs Parallel (Task.WhenAll)
```
| Method                    | Mean       | Ratio | Allocated |
|-------------------------- |-----------:|------:|----------:|
| SequentialAwaits          | 152.3 ms   | 1.00  | 2.1 KB    |
| ParallelWithTaskWhenAll   | 51.2 ms    | 0.34  | 2.5 KB    |
```

### Task vs ValueTask (Cached Operations)
```
| Method                    | Mean       | Ratio | Allocated |
|-------------------------- |-----------:|------:|----------:|
| TaskBasedCaching          | 125.3 μs   | 1.00  | 14.2 KB   |
| ValueTaskBasedCaching     | 98.7 μs    | 0.79  | 3.8 KB    |
```

### ConfigureAwait Impact
```
| Method                    | Mean       | Ratio | Allocated |
|-------------------------- |-----------:|------:|----------:|
| WithoutConfigureAwait     | 22.1 ms    | 1.00  | 1.8 KB    |
| WithConfigureAwait        | 21.7 ms    | 0.98  | 1.8 KB    |
```

## 🎓 Principais Lições

### ✅ **DO (Faça)**
- **Use `ConfigureAwait(false)`** em código de biblioteca
- **Use `Task.WhenAll`** para operações paralelas independentes
- **Propague `CancellationToken`** através da cadeia de chamadas
- **Use `ValueTask<T>`** para hot paths com cache
- **Sempre use `async Task`** em vez de `async void` (exceto event handlers)

### ❌ **DON'T (Não Faça)**
- **Não use `async void`** exceto para event handlers
- **Não faça `await` sequencial** de operações independentes
- **Não ignore `CancellationToken`** em operações de longa duração
- **Não use `Task.Run`** desnecessariamente em aplicações ASP.NET Core
- **Não misture código síncrono e assíncrono** (.Result, .Wait())

## 🔍 Detalhes Técnicos

### ConfigureAwait(false)
```csharp
// ❌ RUIM: Em biblioteca - pode causar deadlock
await SomeOperationAsync();

// ✅ BOM: Em biblioteca - evita deadlock
await SomeOperationAsync().ConfigureAwait(false);
```

### Task.WhenAll vs Sequential
```csharp
// ❌ RUIM: ~300ms total (100ms × 3)
await CallServiceA(); // 100ms
await CallServiceB(); // 100ms  
await CallServiceC(); // 100ms

// ✅ BOM: ~100ms total (paralelo)
await Task.WhenAll(
    CallServiceA(), // 100ms
    CallServiceB(), // 100ms
    CallServiceC()  // 100ms
);
```

### ValueTask Optimization
```csharp
// ✅ Otimizado: Sem alocação para cache hit
ValueTask<string> GetDataAsync(string key)
{
    if (_cache.TryGetValue(key, out var value))
        return ValueTask.FromResult(value); // Sem alocação!
    
    return new ValueTask<string>(LoadDataAsync(key));
}
```

## 📚 Recursos Adicionais

- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [Understanding the Whys, Whats, and Whens of ValueTask](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/)
- [Async Programming Patterns](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/)

## ⚡ Performance Tips

1. **Use `ValueTask<T>`** para métodos que frequentemente completam sincronamente
2. **Use `Task.WhenAll`** para operações paralelas
3. **Use `ConfigureAwait(false)`** em bibliotecas
4. **Propague `CancellationToken`** para operações canceláveis
5. **Evite `Task.Run`** em ASP.NET Core para trabalho CPU-bound
6. **Use `IAsyncEnumerable<T>`** para streaming de dados

---

> 💡 **Dica Extra**: Em aplicações ASP.NET Core modernas (.NET 6+), o `ConfigureAwait(false)` não é mais crítico devido ao comportamento do SynchronizationContext, mas ainda é uma boa prática para bibliotecas e pode trazer pequenos benefícios de performance.
