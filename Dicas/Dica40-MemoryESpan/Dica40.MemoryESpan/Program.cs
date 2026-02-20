using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Dica40.MemoryESpan;

/// <summary>
/// Demonstra o uso de Memory&lt;T&gt; e Span&lt;T&gt; para manipulação eficiente de memória
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<DemoService>();
            })
            .Build();

        var demo = host.Services.GetRequiredService<DemoService>();
        await demo.ExecutarTodasDemonstracoes();
    }
}

public class DemoService
{
    private readonly ILogger<DemoService> _logger;

    public DemoService(ILogger<DemoService> logger)
    {
        _logger = logger;
    }

    public async Task ExecutarTodasDemonstracoes()
    {
        Console.WriteLine("===== Dica 40: Memory<T> e Span<T> - Manipulação Eficiente de Memória ====\n");

        Console.WriteLine("🧠 1. SPAN<T> BÁSICO - Manipulação Zero-Copy");
        Console.WriteLine("─────────────────────────────────────────────");
        DemonstrarSpanBasico();
        Console.WriteLine("✅ Demonstração de Span<T> básico concluída\n");

        Console.WriteLine("💾 2. MEMORY<T> - Gerenciamento Assíncrono");
        Console.WriteLine("─────────────────────────────────────────────");
        await DemonstrarMemoryAssincrono();
        Console.WriteLine("✅ Demonstração de Memory<T> assíncrono concluída\n");

        Console.WriteLine("🔄 3. SLICING E MANIPULAÇÃO");
        Console.WriteLine("──────────────────────────────");
        DemonstrarSlicing();
        Console.WriteLine("✅ Demonstração de slicing concluída\n");

        Console.WriteLine("⚡ 4. PERFORMANCE COM ARRAYPOOL");
        Console.WriteLine("────────────────────────────────────");
        DemonstrarArrayPool();
        Console.WriteLine("✅ Demonstração de ArrayPool concluída\n");

        Console.WriteLine("🔍 5. UNSAFE E MARSHAL");
        Console.WriteLine("─────────────────────────");
        DemonstrarUnsafeOperations();
        Console.WriteLine("✅ Demonstração de operações unsafe concluída\n");

        Console.WriteLine("📊 6. BENCHMARKS DE PERFORMANCE");
        Console.WriteLine("────────────────────────────────");
        ExecutarBenchmarks();
        Console.WriteLine("✅ Benchmarks concluídos\n");

        Console.WriteLine("📋 7. RESUMO DAS BOAS PRÁTICAS");
        Console.WriteLine("─────────────────────────────────");
        ExibirBoasPraticas();

        Console.WriteLine("\n=== Demonstração concluída ===");
    }

    private void DemonstrarSpanBasico()
    {
        _logger.LogInformation("🧠 Demonstrando Span<T> básico...");

        // Array original
        int[] numeros = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        _logger.LogInformation("📊 Array original: [{numeros}]", string.Join(", ", numeros));

        // Span sobre array completo
        Span<int> spanCompleto = numeros.AsSpan();
        _logger.LogInformation("📊 Span completo: [{span}]", string.Join(", ", spanCompleto.ToArray()));

        // Slice do meio
        Span<int> spanMeio = numeros.AsSpan(3, 4); // índices 3-6
        _logger.LogInformation("📊 Span meio (3-6): [{span}]", string.Join(", ", spanMeio.ToArray()));

        // Modificar através do span
        spanMeio[0] = 999;
        _logger.LogInformation("📊 Após modificar span[0] = 999: [{numeros}]", string.Join(", ", numeros));

        // Operações com span
        spanMeio.Fill(42);
        _logger.LogInformation("📊 Após Fill(42): [{numeros}]", string.Join(", ", numeros));

        // Span em string
        string texto = "Hello, World!";
        ReadOnlySpan<char> spanTexto = texto.AsSpan(7, 5); // "World"
        _logger.LogInformation("📝 Span de string: '{span}'", spanTexto.ToString());

        // Stackalloc span
        Span<int> spanStack = stackalloc int[5];
        for (int i = 0; i < spanStack.Length; i++)
        {
            spanStack[i] = i * i;
        }
        _logger.LogInformation("📊 Span stackalloc: [{span}]", string.Join(", ", spanStack.ToArray()));
    }

    private async Task DemonstrarMemoryAssincrono()
    {
        _logger.LogInformation("💾 Demonstrando Memory<T> assíncrono...");

        var buffer = new byte[1024];
        var memory = buffer.AsMemory();

        // Simular operação assíncrona
        await PreencherBufferAsync(memory.Slice(0, 100));
        _logger.LogInformation("📊 Buffer preenchido: primeiros 20 bytes = [{bytes}]", 
            string.Join(", ", buffer[..20]));

        // Memory permite passar para métodos async
        await ProcessarDadosAsync(memory.Slice(100, 200));
        _logger.LogInformation("📊 Dados processados no intervalo 100-299");

        // Memory<T> é seguro para captura em closures
        var task = Task.Run(async () =>
        {
            await Task.Delay(10);
            var span = memory.Slice(300, 100).Span;
            span.Fill(255);
        });
        await task;

        _logger.LogInformation("📊 Buffer após processamento assíncrono: bytes 300-319 = [{bytes}]", 
            string.Join(", ", buffer[300..320]));
    }

    private async Task PreencherBufferAsync(Memory<byte> memory)
    {
        await Task.Delay(1); // Simular operação async
        var span = memory.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(i % 256);
        }
    }

    private async Task ProcessarDadosAsync(Memory<byte> memory)
    {
        await Task.Delay(1); // Simular operação async
        var span = memory.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(span[i] ^ 0x55); // XOR simples
        }
    }

    private void DemonstrarSlicing()
    {
        _logger.LogInformation("🔄 Demonstrando slicing e manipulação...");

        string csv = "nome,idade,cidade,salario,departamento";
        ReadOnlySpan<char> linha = csv.AsSpan();

        _logger.LogInformation("📝 CSV original: '{csv}'", csv);

        // Dividir por vírgulas usando span
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
        // Último campo
        campos.Add(linha.Slice(inicio).ToString());

        _logger.LogInformation("📊 Campos extraídos: [{campos}]", string.Join(" | ", campos));

        // Operações eficientes com ReadOnlySpan
        var dadosNumericos = "123.45,67.89,90.12,345.67";
        ReadOnlySpan<char> numeros = dadosNumericos.AsSpan();
        
        var valores = new List<double>();
        inicio = 0;
        
        for (int i = 0; i <= numeros.Length; i++)
        {
            if (i == numeros.Length || numeros[i] == ',')
            {
                var numero = numeros.Slice(inicio, i - inicio);
                if (double.TryParse(numero, out double valor))
                {
                    valores.Add(valor);
                }
                inicio = i + 1;
            }
        }

        _logger.LogInformation("📊 Valores parseados: [{valores}]", string.Join(", ", valores));

        // Comparações eficientes
        ReadOnlySpan<char> palavra1 = "programming".AsSpan();
        ReadOnlySpan<char> palavra2 = "PROGRAMMING".AsSpan();
        
        bool iguais = palavra1.Equals(palavra2, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("📝 'programming' == 'PROGRAMMING' (ignore case): {iguais}", iguais);
    }

    private void DemonstrarArrayPool()
    {
        _logger.LogInformation("⚡ Demonstrando ArrayPool...");

        var pool = ArrayPool<byte>.Shared;
        
        // Alugar array do pool
        byte[] buffer = pool.Rent(1024);
        _logger.LogInformation("📊 Array alugado do pool: tamanho = {tamanho}", buffer.Length);

        try
        {
            // Usar como Memory/Span
            var memory = buffer.AsMemory(0, 512); // Usar apenas 512 bytes
            var span = memory.Span;
            
            // Preencher com dados
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (byte)(i % 256);
            }
            
            _logger.LogInformation("📊 Buffer preenchido: primeiros 10 bytes = [{bytes}]", 
                string.Join(", ", span[..10].ToArray()));

            // Simular processamento
            ProcessarComSpan(span);
            
            _logger.LogInformation("📊 Após processamento: primeiros 10 bytes = [{bytes}]", 
                string.Join(", ", span[..10].ToArray()));
        }
        finally
        {
            // SEMPRE devolver o array ao pool
            pool.Return(buffer);
            _logger.LogInformation("♻️ Array devolvido ao pool");
        }

        // Demonstrar reutilização
        byte[] buffer2 = pool.Rent(1024);
        _logger.LogInformation("📊 Novo array do pool (pode ser o mesmo): tamanho = {tamanho}", buffer2.Length);
        
        // Verificar se é o mesmo array (pode ou não ser)
        bool mesmoArray = ReferenceEquals(buffer, buffer2);
        _logger.LogInformation("🔍 Mesmo array reutilizado: {reutilizado}", mesmoArray);
        
        pool.Return(buffer2);
    }

    private void ProcessarComSpan(Span<byte> span)
    {
        // Operação in-place eficiente
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(span[i] * 2);
        }
    }

    private void DemonstrarUnsafeOperations()
    {
        _logger.LogInformation("🔍 Demonstrando operações unsafe...");

        // Alocação na stack
        Span<int> spanStack = stackalloc int[10];
        for (int i = 0; i < spanStack.Length; i++)
        {
            spanStack[i] = i * i;
        }
        
        _logger.LogInformation("📊 Span stackalloc: [{valores}]", 
            string.Join(", ", spanStack.ToArray()));

        // Interoperabilidade com ponteiros
        var array = new int[] { 10, 20, 30, 40, 50 };
        var span = array.AsSpan();
        
        ProcessarComPointerUnsafe(span);
        
        _logger.LogInformation("📊 Array após modificação via pointer: [{array}]", 
            string.Join(", ", array));

        // MemoryMarshal para operações avançadas
        var bytes = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 };
        var spanBytes = bytes.AsSpan();
        var spanInts = MemoryMarshal.Cast<byte, int>(spanBytes);
        
        _logger.LogInformation("📊 Bytes: [{bytes}]", string.Join(", ", bytes));
        _logger.LogInformation("📊 Como ints: [{ints}]", string.Join(", ", spanInts.ToArray()));

        // Span de estruturas
        var pontos = new Ponto[] 
        { 
            new(1, 2), 
            new(3, 4), 
            new(5, 6) 
        };
        
        var spanPontos = pontos.AsSpan();
        foreach (ref var ponto in spanPontos)
        {
            ponto.X *= 2;
            ponto.Y *= 2;
        }
        
        _logger.LogInformation("📊 Pontos após multiplicação: [{pontos}]", 
            string.Join(", ", pontos.Select(p => $"({p.X},{p.Y})")));
    }

    private unsafe void ProcessarComPointerUnsafe(Span<int> span)
    {
        fixed (int* ptr = span)
        {
            _logger.LogInformation("🔗 Pointer fixado: valor em ptr[2] = {valor}", ptr[2]);
            
            // Modificar através do ponteiro
            ptr[2] = 999;
        }
    }

    private void ExecutarBenchmarks()
    {
        _logger.LogInformation("📊 Executando benchmarks de performance...");

        const int iterations = 1_000_000;
        var dados = Enumerable.Range(0, 1000).ToArray();

        // Benchmark: Array tradicional vs Span
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ProcessarArrayTradicional(dados);
        }
        sw.Stop();
        var tempoArray = sw.ElapsedMilliseconds;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ProcessarComSpan(dados.AsSpan());
        }
        sw.Stop();
        var tempoSpan = sw.ElapsedMilliseconds;

        _logger.LogInformation("⏱️ Array tradicional: {tempo}ms", tempoArray);
        _logger.LogInformation("⏱️ Span<T>: {tempo}ms", tempoSpan);
        _logger.LogInformation("🏆 Span é {melhoria:F1}x mais rápido", 
            (double)tempoArray / tempoSpan);

        // Benchmark: String operations
        const string texto = "Esta é uma string de teste para demonstrar performance";
        const int stringIterations = 100_000;

        sw.Restart();
        for (int i = 0; i < stringIterations; i++)
        {
            ProcessarStringTradicional(texto);
        }
        sw.Stop();
        var tempoString = sw.ElapsedMilliseconds;

        sw.Restart();
        for (int i = 0; i < stringIterations; i++)
        {
            ProcessarStringComSpan(texto.AsSpan());
        }
        sw.Stop();
        var tempoStringSpan = sw.ElapsedMilliseconds;

        _logger.LogInformation("⏱️ String tradicional: {tempo}ms", tempoString);
        _logger.LogInformation("⏱️ ReadOnlySpan<char>: {tempo}ms", tempoStringSpan);
        _logger.LogInformation("🏆 Span é {melhoria:F1}x mais rápido", 
            (double)tempoString / tempoStringSpan);
    }

    private int ProcessarArrayTradicional(int[] array)
    {
        int soma = 0;
        for (int i = 0; i < array.Length; i++)
        {
            soma += array[i];
        }
        return soma;
    }

    private int ProcessarComSpan(ReadOnlySpan<int> span)
    {
        int soma = 0;
        for (int i = 0; i < span.Length; i++)
        {
            soma += span[i];
        }
        return soma;
    }

    private int ProcessarStringTradicional(string texto)
    {
        return texto.Split(' ').Length;
    }

    private int ProcessarStringComSpan(ReadOnlySpan<char> span)
    {
        int palavras = 1;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ' ')
                palavras++;
        }
        return palavras;
    }

    private void ExibirBoasPraticas()
    {
        Console.WriteLine("✅ Use Span<T> para manipulação de arrays sem alocação");
        Console.WriteLine("✅ Use Memory<T> para operações assíncronas");
        Console.WriteLine("✅ Use ReadOnlySpan<T> para dados imutáveis");
        Console.WriteLine("✅ Use ArrayPool<T> para buffers temporários");
        Console.WriteLine("✅ Use stackalloc para pequenos arrays temporários");
        Console.WriteLine("✅ Evite ToArray() desnecessário em Span<T>");
        Console.WriteLine("✅ Use slicing para extrair partes sem cópia");
        Console.WriteLine("✅ Combine com unsafe code quando necessário");
        Console.WriteLine();
        Console.WriteLine("🎯 QUANDO USAR MEMORY<T> E SPAN<T>");
        Console.WriteLine("──────────────────────────────────────");
        Console.WriteLine("🔹 Processamento de grandes volumes de dados");
        Console.WriteLine("🔹 Parsing eficiente de strings");
        Console.WriteLine("🔹 Manipulação de buffers de I/O");
        Console.WriteLine("🔹 Operações de rede e serialização");
        Console.WriteLine("🔹 Substituição de substring operations");
    }
}

public record struct Ponto(int X, int Y);
