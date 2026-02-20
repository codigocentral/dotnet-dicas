# Dica 40: Memory<T> e Span<T> - Manipulação Eficiente de Memória

## 📖 Problema

Melhorar a performance de aplicações através da manipulação eficiente de memória, reduzindo alocações desnecessárias e garbage collection em operações com arrays, strings e buffers.

## ✅ Solução

Use `Memory<T>` e `Span<T>` para manipulação zero-copy de dados, permitindo operações high-performance sem alocações extras na heap.

## 🧠 O que são Memory<T> e Span<T>?

### Span<T> - Zero-Copy Views
- **Stack-only**: Não pode ser armazenado na heap
- **Zero allocation**: Não aloca memória adicional
- **High performance**: Acesso direto à memória
- **Type-safe**: Fortemente tipado

### Memory<T> - Heap-Safe Views
- **Heap-friendly**: Pode ser armazenado em fields e capturado em closures
- **Async-safe**: Compatível com async/await
- **Sliceable**: Suporte a operações de slice eficientes

## 🔄 Principais Usos

### 1. Manipulação de Arrays

```csharp
int[] numeros = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

// Span sobre array completo
Span<int> spanCompleto = numeros.AsSpan();

// Slice eficiente (sem cópia)
Span<int> meio = numeros.AsSpan(3, 4); // índices 3-6

// Modificações in-place
meio.Fill(42);
meio[0] = 999;
```

### 2. Processamento de Strings

```csharp
string csv = "nome,idade,cidade,salario";
ReadOnlySpan<char> linha = csv.AsSpan();

// Split sem alocações
var campos = new List<string>();
int inicio = 0;

for (int i = 0; i < linha.Length; i++)
{
    if (linha[i] == ',')
    {
        campos.Add(linha.Slice(inicio, i - inicio).ToString());
        inicio = i + 1;
    }
}
```

### 3. Operações Assíncronas

```csharp
public async Task ProcessarDadosAsync(Memory<byte> memory)
{
    // Memory<T> é seguro para async
    await Task.Delay(100);
    
    var span = memory.Span; // Converter para Span quando necessário
    span.Fill(0);
}

// Uso
var buffer = new byte[1024];
await ProcessarDadosAsync(buffer.AsMemory(0, 512));
```

### 4. Stackalloc para Performance

```csharp
// Alocação na stack (muito rápido)
Span<int> buffer = stackalloc int[256];

// Usar normalmente
buffer.Fill(42);
for (int i = 0; i < buffer.Length; i++)
{
    buffer[i] = i * i;
}
```

## ⚡ ArrayPool<T> para Buffers Temporários

```csharp
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(1024);

try
{
    var memory = buffer.AsMemory(0, 512);
    var span = memory.Span;
    
    // Processar dados
    span.Fill(0xFF);
}
finally
{
    pool.Return(buffer); // SEMPRE devolver!
}
```

## 🔧 Interoperabilidade com Unsafe Code

```csharp
unsafe void ProcessarComPointer(Span<int> span)
{
    fixed (int* ptr = span)
    {
        // Trabalhar diretamente com ponteiro
        for (int i = 0; i < span.Length; i++)
        {
            ptr[i] *= 2;
        }
    }
}

// MemoryMarshal para conversões
byte[] bytes = [1, 0, 0, 0, 2, 0, 0, 0];
Span<int> ints = MemoryMarshal.Cast<byte, int>(bytes.AsSpan());
```

## 📊 Comparação de Performance

| Operação | Array Tradicional | Span<T> | Benefício |
|----------|------------------|---------|-----------|
| **Slice** | Array.Copy() | Zero-copy | 10-100x mais rápido |
| **Fill** | Loop manual | span.Fill() | 2-5x mais rápido |
| **Search** | LINQ Where() | span iteração | 3-10x mais rápido |
| **Parse** | string.Split() | span slicing | 5-20x mais rápido |

## 🎯 Padrões de Uso

### 1. Parsing Eficiente

```csharp
public static bool TryParseInt(ReadOnlySpan<char> span, out int value)
{
    value = 0;
    if (span.IsEmpty) return false;
    
    int sign = 1;
    int start = 0;
    
    if (span[0] == '-')
    {
        sign = -1;
        start = 1;
    }
    
    for (int i = start; i < span.Length; i++)
    {
        char c = span[i];
        if (c < '0' || c > '9') return false;
        
        value = value * 10 + (c - '0');
    }
    
    value *= sign;
    return true;
}
```

### 2. Buffer Management

```csharp
public class EfficientStringBuilder
{
    private readonly ArrayPool<char> _pool = ArrayPool<char>.Shared;
    private char[] _buffer;
    private int _length;

    public EfficientStringBuilder(int capacity = 256)
    {
        _buffer = _pool.Rent(capacity);
    }

    public void Append(ReadOnlySpan<char> text)
    {
        EnsureCapacity(_length + text.Length);
        text.CopyTo(_buffer.AsSpan(_length));
        _length += text.Length;
    }

    public override string ToString()
    {
        return new string(_buffer, 0, _length);
    }

    public void Dispose()
    {
        _pool.Return(_buffer);
    }
}
```

### 3. Stream Processing

```csharp
public async Task<int> ProcessStreamAsync(Stream stream, Memory<byte> buffer)
{
    int totalProcessed = 0;
    int bytesRead;
    
    while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
    {
        var chunk = buffer.Slice(0, bytesRead);
        ProcessChunk(chunk.Span);
        totalProcessed += bytesRead;
    }
    
    return totalProcessed;
}

private void ProcessChunk(Span<byte> chunk)
{
    // Processar dados in-place
    for (int i = 0; i < chunk.Length; i++)
    {
        chunk[i] = (byte)(chunk[i] ^ 0x55);
    }
}
```

## ⚠️ Limitações e Cuidados

### Span<T> Limitações
```csharp
// ❌ NÃO FUNCIONA - Span não pode ser field
public class BadExample
{
    private Span<int> _span; // Erro de compilação!
}

// ❌ NÃO FUNCIONA - Span não pode ser capturado
public void BadAsync()
{
    Span<int> span = stackalloc int[10];
    
    Task.Run(() => 
    {
        span[0] = 42; // Erro de compilação!
    });
}
```

### Memory<T> é a Solução
```csharp
// ✅ FUNCIONA - Memory pode ser field
public class GoodExample
{
    private Memory<int> _memory; // OK!
    
    public async Task ProcessAsync()
    {
        await Task.Run(() =>
        {
            var span = _memory.Span; // Converter quando necessário
            span[0] = 42; // OK!
        });
    }
}
```

## 💡 Boas Práticas

1. **Use Span<T>** para operações síncronas locais
2. **Use Memory<T>** para operações assíncronas e armazenamento
3. **Use ReadOnlySpan<T>** para dados imutáveis
4. **Use ArrayPool<T>** para buffers temporários grandes
5. **Use stackalloc** para pequenos arrays temporários
6. **Evite ToArray()** desnecessário em Span<T>
7. **Combine com unsafe** quando necessário para máxima performance

Memory<T> e Span<T> são fundamentais para aplicações high-performance em .NET, oferecendo controle preciso sobre alocações e acesso eficiente à memória!
