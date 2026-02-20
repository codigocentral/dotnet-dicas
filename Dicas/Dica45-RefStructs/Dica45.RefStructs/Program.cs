using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

Console.WriteLine("=== Dica 45: Ref Structs (Alto Desempenho e Segurança de Memória) ===\n");

// Demonstração 1: Comparando ref struct vs struct normal
Console.WriteLine("1. Comparando ref struct vs struct normal:");
DemonstrarDiferencasRefStruct();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 2: Processamento de dados com zero allocations
Console.WriteLine("2. Processamento de dados com zero allocations:");
DemonstrarProcessamentoZeroAllocation();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 3: Parsing de alta performance
Console.WriteLine("3. Parsing de alta performance:");
DemonstrarParsingPerformance();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 4: Manipulação de memória segura
Console.WriteLine("4. Manipulação de memória segura:");
DemonstrarManipulacaoMemoriaSegura();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 5: Restrições e limitações
Console.WriteLine("5. Restrições e limitações dos ref structs:");
DemonstrarRestricoes();

Console.WriteLine("\n=== Resumo das Vantagens dos Ref Structs ===");
Console.WriteLine("✅ Sempre alocados na stack (zero heap allocation)");
Console.WriteLine("✅ Performance extremamente alta");
Console.WriteLine("✅ Segurança de memória garantida pelo compilador");
Console.WriteLine("✅ Ideais para hot paths e processamento de dados");
Console.WriteLine("✅ Sem overhead de garbage collection");
Console.WriteLine("✅ Perfeitos para parsing e manipulação de buffers");

static void DemonstrarDiferencasRefStruct()
{
    Console.WriteLine("  📊 Diferenças fundamentais:");
    
    // Struct normal - pode ser alocada no heap
    var normalStruct = new NormalStruct(42, "Normal");
    Console.WriteLine($"     Normal struct: {normalStruct.Value} - {normalStruct.Name}");
    
    // Ref struct - SEMPRE na stack
    Span<char> nameBuffer = stackalloc char[6] { 'R', 'e', 'f', ' ', '!', '\0' };
    var refStruct = new RefStruct(42, nameBuffer);
    Console.WriteLine($"     Ref struct: {refStruct.Value} - {refStruct.GetName()}");
    
    Console.WriteLine("\n  🔒 Restrições do ref struct:");
    Console.WriteLine("     • Não pode ser boxed para object");
    Console.WriteLine("     • Não pode ser usado em métodos async");
    Console.WriteLine("     • Não pode ser armazenado em campos de classe");
    Console.WriteLine("     • Não pode implementar interfaces");
    
    // Demonstrar que ref struct força stack allocation
    Span<int> span = stackalloc int[100];
    var processor = new StackOnlyProcessor(span);
    Console.WriteLine($"     Stack processor criado com {processor.Length} elementos");
}

static void DemonstrarProcessamentoZeroAllocation()
{
    Console.WriteLine("  ⚡ Processamento sem alocações no heap:");
    
    // Dados de exemplo na stack
    Span<int> dados = stackalloc int[1000];
    for (int i = 0; i < dados.Length; i++)
    {
        dados[i] = Random.Shared.Next(1, 1000);
    }
    
    // Processar usando ref struct
    var processor = new HighPerformanceProcessor(dados);
    
    var sw = Stopwatch.StartNew();
    var resultado = processor.ProcessarDados();
    sw.Stop();
    
    Console.WriteLine($"     Processados {dados.Length} elementos");
    Console.WriteLine($"     Soma: {resultado.Soma:N0}");
    Console.WriteLine($"     Média: {resultado.Media:F2}");
    Console.WriteLine($"     Máximo: {resultado.Maximo:N0}");
    Console.WriteLine($"     Tempo: {sw.Elapsed.TotalMicroseconds:F1} μs");
    Console.WriteLine($"     ✅ Zero alocações no heap!");
}

static void DemonstrarParsingPerformance()
{
    Console.WriteLine("  📝 Parsing de strings de alta performance:");
    
    var texto = "123,456,789,012,345";
    var span = texto.AsSpan();
    
    // Parsing direto sem ref struct aninhado para evitar problemas de escopo
    Span<long> numeros = stackalloc long[10];
    int count = 0;
    int inicio = 0;
    
    for (int i = 0; i <= span.Length && count < numeros.Length; i++)
    {
        if (i == span.Length || span[i] == ',')
        {
            var numeroSpan = span.Slice(inicio, i - inicio);
            if (long.TryParse(numeroSpan, out var numero))
            {
                numeros[count++] = numero;
            }
            inicio = i + 1;
        }
    }
    
    Console.WriteLine($"     Texto original: '{texto}'");
    Console.WriteLine($"     Números encontrados: {count}");
    
    var resultado = numeros.Slice(0, count);
    for (int i = 0; i < resultado.Length; i++)
    {
        Console.WriteLine($"       [{i}]: {resultado[i]:N0}");
    }
    
    Console.WriteLine("     ✅ Parsing realizado sem alocações!");
}

static void DemonstrarManipulacaoMemoriaSegura()
{
    Console.WriteLine("  🛡️ Manipulação segura de memória:");
    
    // Buffer na stack
    Span<byte> buffer = stackalloc byte[256];
    
    // Preenchimento seguro
    var manipulator = new SafeMemoryManipulator(buffer);
    manipulator.PreencherComPadrao();
    
    Console.WriteLine($"     Buffer de {buffer.Length} bytes criado na stack");
    
    // Operações seguras
    manipulator.EscreverString("Hello, Ref Structs!", 0);
    manipulator.EscreverNumero(42, 50);
    manipulator.EscreverNumero(12345, 60);
    
    // Leitura segura
    var texto = manipulator.LerString(0, 19);
    var numero1 = manipulator.LerNumero(50);
    var numero2 = manipulator.LerNumero(60);
    
    Console.WriteLine($"     Texto lido: '{texto}'");
    Console.WriteLine($"     Número 1: {numero1}");
    Console.WriteLine($"     Número 2: {numero2}");
    Console.WriteLine("     ✅ Todas as operações foram type-safe!");
}

static void DemonstrarRestricoes()
{
    Console.WriteLine("  ⚠️  Principais restrições dos ref structs:");
    
    Console.WriteLine("     1. Não podem ser usados com async/await:");
    Console.WriteLine("        // ❌ Isso não compila:");
    Console.WriteLine("        // async Task ProcessAsync(RefStruct data) { ... }");
    
    Console.WriteLine("\n     2. Não podem ser armazenados em campos de classe:");
    Console.WriteLine("        // ❌ Isso não compila:");
    Console.WriteLine("        // class Container { RefStruct field; }");
    
    Console.WriteLine("\n     3. Não podem ser boxed:");
    Console.WriteLine("        // ❌ Isso não compila:");
    Console.WriteLine("        // object obj = refStruct;");
    
    Console.WriteLine("\n     4. Não podem implementar interfaces:");
    Console.WriteLine("        // ❌ Isso não compila:");
    Console.WriteLine("        // ref struct RefStruct : IDisposable { ... }");
    
    Console.WriteLine("\n     ✅ Em troca, você obtém:");
    Console.WriteLine("        • Performance máxima (stack-only)");
    Console.WriteLine("        • Zero GC pressure");
    Console.WriteLine("        • Segurança de memória garantida");
    Console.WriteLine("        • Ideal para hot paths e processamento crítico");
}

// Struct normal para comparação
public struct NormalStruct
{
    public int Value { get; }
    public string Name { get; }
    
    public NormalStruct(int value, string name)
    {
        Value = value;
        Name = name;
    }
}

// Ref struct - sempre na stack
public ref struct RefStruct
{
    public int Value { get; }
    private readonly Span<char> _name;
    
    public RefStruct(int value, Span<char> name)
    {
        Value = value;
        _name = name;
    }
    
    public string GetName()
    {
        return new string(_name.TrimEnd('\0'));
    }
}

// Ref struct para processamento stack-only
public ref struct StackOnlyProcessor
{
    private readonly Span<int> _data;
    
    public int Length => _data.Length;
    
    public StackOnlyProcessor(Span<int> data)
    {
        _data = data;
    }
    
    public int CalcularSoma()
    {
        int soma = 0;
        foreach (var item in _data)
        {
            soma += item;
        }
        return soma;
    }
}

// Ref struct para processamento de alta performance
public ref struct HighPerformanceProcessor
{
    private readonly Span<int> _dados;
    
    public HighPerformanceProcessor(Span<int> dados)
    {
        _dados = dados;
    }
    
    public ResultadoProcessamento ProcessarDados()
    {
        if (_dados.IsEmpty)
            return new ResultadoProcessamento(0, 0, 0);
        
        long soma = 0;
        int maximo = int.MinValue;
        
        foreach (var valor in _dados)
        {
            soma += valor;
            if (valor > maximo)
                maximo = valor;
        }
        
        double media = (double)soma / _dados.Length;
        
        return new ResultadoProcessamento(soma, media, maximo);
    }
}

// Struct normal para resultado (não precisa ser ref struct)
public readonly struct ResultadoProcessamento
{
    public long Soma { get; }
    public double Media { get; }
    public int Maximo { get; }
    
    public ResultadoProcessamento(long soma, double media, int maximo)
    {
        Soma = soma;
        Media = media;
        Maximo = maximo;
    }
}

// Ref struct para parsing de números (mantido para referência)
public ref struct NumberParser
{
    private readonly ReadOnlySpan<char> _texto;
    
    public NumberParser(ReadOnlySpan<char> texto)
    {
        _texto = texto;
    }
    
    public int ParseNumbers(Span<long> buffer)
    {
        int count = 0;
        int inicio = 0;
        
        for (int i = 0; i <= _texto.Length && count < buffer.Length; i++)
        {
            if (i == _texto.Length || _texto[i] == ',')
            {
                var numeroSpan = _texto.Slice(inicio, i - inicio);
                if (long.TryParse(numeroSpan, out var numero))
                {
                    buffer[count++] = numero;
                }
                inicio = i + 1;
            }
        }
        
        return count;
    }
}

// Ref struct para manipulação segura de memória
public ref struct SafeMemoryManipulator
{
    private readonly Span<byte> _buffer;
    
    public SafeMemoryManipulator(Span<byte> buffer)
    {
        _buffer = buffer;
    }
    
    public void PreencherComPadrao()
    {
        _buffer.Fill(0);
    }
    
    public void EscreverString(string texto, int offset)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(texto);
        var destino = _buffer.Slice(offset, Math.Min(bytes.Length, _buffer.Length - offset));
        bytes.AsSpan(0, destino.Length).CopyTo(destino);
    }
    
    public void EscreverNumero(int numero, int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            MemoryMarshal.Write(_buffer.Slice(offset), in numero);
        }
    }
    
    public string LerString(int offset, int length)
    {
        var slice = _buffer.Slice(offset, Math.Min(length, _buffer.Length - offset));
        
        // Encontrar o fim da string (primeiro byte zero)
        int fim = slice.IndexOf((byte)0);
        if (fim >= 0)
            slice = slice.Slice(0, fim);
        
        return System.Text.Encoding.UTF8.GetString(slice);
    }
    
    public int LerNumero(int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            return MemoryMarshal.Read<int>(_buffer.Slice(offset));
        }
        return 0;
    }
}
