using System.Buffers;

Console.WriteLine("=== Dica 51: Reutilização de Arrays com ArrayPool ===\n");

var demo = new ArrayPoolDemo();

// Demonstrações básicas
Console.WriteLine("1. Uso básico do ArrayPool:");
demo.BasicArrayPoolDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstrações avançadas
Console.WriteLine("2. Cenários práticos:");
await demo.PracticalScenarios();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstração de pool customizado
Console.WriteLine("3. Pool customizado:");
demo.CustomPoolDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstração de processamento de dados
Console.WriteLine("4. Processamento de dados com pool:");
await demo.DataProcessingDemo();

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Resumo das práticas recomendadas:");
Console.WriteLine("✅ Use ArrayPool.Shared para pools globais");
Console.WriteLine("✅ SEMPRE devolva arrays com Return()");
Console.WriteLine("✅ Use clearArray: true para dados sensíveis");
Console.WriteLine("✅ Considere pool customizado para cenários específicos");
Console.WriteLine("✅ Ideal para arrays temporários e grandes volumes");
Console.WriteLine("⚠️  Não use arrays após devolvê-los ao pool");
Console.WriteLine("⚠️  Pool pode retornar arrays maiores que solicitado");

public class ArrayPoolDemo
{
    public void BasicArrayPoolDemo()
    {
        Console.WriteLine("Exemplo básico de ArrayPool:");

        // Pool compartilhado - thread-safe e global
        var pool = ArrayPool<int>.Shared;

        // Aluga um array (pode ser maior que solicitado)
        int[] array = pool.Rent(1000);
        
        try
        {
            Console.WriteLine($"Array alugado com tamanho: {array.Length}");
            
            // Usa o array normalmente
            for (int i = 0; i < Math.Min(10, array.Length); i++)
            {
                array[i] = i * i;
            }

            Console.WriteLine("Primeiros 10 valores:");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"  [{i}] = {array[i]}");
            }
        }
        finally
        {
            // SEMPRE devolve o array ao pool
            pool.Return(array, clearArray: true);
            Console.WriteLine("Array devolvido ao pool com limpeza");
        }

        // Exemplo com using para garantir devolução
        using var rental = new ArrayRental<byte>(pool: ArrayPool<byte>.Shared, minimumLength: 4096);
        var buffer = rental.Array;
        
        Console.WriteLine($"\nBuffer alugado com using: {buffer.Length} bytes");
        // Array é automaticamente devolvido quando sai do escopo
    }

    public async Task PracticalScenarios()
    {
        Console.WriteLine("Cenários práticos de uso do ArrayPool:");

        // 1. Processamento de texto
        await TextProcessingExample();

        // 2. Buffer para I/O
        await IoBufferExample();

        // 3. Processamento de imagens (simulado)
        ImageProcessingExample();
    }

    private async Task TextProcessingExample()
    {
        Console.WriteLine("\n1. Processamento de texto:");

        var pool = ArrayPool<char>.Shared;
        var text = "Este é um exemplo de processamento de texto que precisa de buffer temporário para operações como conversão de case, remoção de caracteres especiais, etc.";

        char[] buffer = pool.Rent(text.Length * 2); // Buffer maior para margem de segurança
        
        try
        {
            // Copia texto para buffer
            text.AsSpan().CopyTo(buffer);
            
            // Processa texto (converte para uppercase)
            for (int i = 0; i < text.Length; i++)
            {
                buffer[i] = char.ToUpper(buffer[i]);
            }

            var result = new string(buffer, 0, text.Length);
            Console.WriteLine($"   Texto processado: {result[..50]}...");
            
            await Task.Delay(10); // Simula processamento assíncrono
        }
        finally
        {
            pool.Return(buffer, clearArray: true);
        }
    }

    private async Task IoBufferExample()
    {
        Console.WriteLine("\n2. Buffer para operações de I/O:");

        var pool = ArrayPool<byte>.Shared;
        const int bufferSize = 8192; // 8KB buffer típico

        byte[] buffer = pool.Rent(bufferSize);
        
        try
        {
            // Simula leitura de dados
            var data = "Dados importantes do arquivo ou rede"u8;
            data.CopyTo(buffer);

            Console.WriteLine($"   Buffer I/O: {buffer.Length} bytes alocados");
            Console.WriteLine($"   Dados lidos: {System.Text.Encoding.UTF8.GetString(buffer, 0, data.Length)}");
            
            // Simula operação assíncrona de I/O
            await Task.Delay(50);
        }
        finally
        {
            pool.Return(buffer, clearArray: true);
        }
    }

    private void ImageProcessingExample()
    {
        Console.WriteLine("\n3. Processamento de imagem (simulado):");

        var pool = ArrayPool<int>.Shared;
        const int imageSize = 1920 * 1080; // Full HD

        int[] pixels = pool.Rent(imageSize);
        
        try
        {
            // Simula processamento de imagem
            for (int i = 0; i < Math.Min(1000, pixels.Length); i++)
            {
                pixels[i] = Random.Shared.Next(0, 255) << 16 | 
                           Random.Shared.Next(0, 255) << 8 | 
                           Random.Shared.Next(0, 255);
            }

            Console.WriteLine($"   Imagem processada: {pixels.Length} pixels alocados");
            Console.WriteLine($"   Primeiros pixels: {pixels[0]:X}, {pixels[1]:X}, {pixels[2]:X}");
        }
        finally
        {
            pool.Return(pixels, clearArray: true);
        }
    }

    public void CustomPoolDemo()
    {
        Console.WriteLine("Pool customizado para cenários específicos:");

        // Pool customizado com configurações específicas
        var customPool = ArrayPool<double>.Create(
            maxArrayLength: 1024 * 1024, // 1MB máximo
            maxArraysPerBucket: 50        // Até 50 arrays por bucket
        );

        double[] array = customPool.Rent(10000);
        
        try
        {
            // Simula cálculos científicos
            for (int i = 0; i < Math.Min(1000, array.Length); i++)
            {
                array[i] = Math.Sin(i * Math.PI / 180.0);
            }

            Console.WriteLine($"Pool customizado: {array.Length} elementos");
            Console.WriteLine($"Primeiros valores: {array[0]:F3}, {array[1]:F3}, {array[2]:F3}");
        }
        finally
        {
            customPool.Return(array, clearArray: true);
        }
    }

    public async Task DataProcessingDemo()
    {
        Console.WriteLine("Processamento eficiente de dados em lotes:");

        var pool = ArrayPool<int>.Shared;
        const int batchSize = 10000;
        const int totalBatches = 5;

        for (int batch = 0; batch < totalBatches; batch++)
        {
            int[] data = pool.Rent(batchSize);
            
            try
            {
                // Preenche dados do lote
                for (int i = 0; i < batchSize; i++)
                {
                    data[i] = batch * batchSize + i;
                }

                // Processa lote
                long sum = 0;
                for (int i = 0; i < batchSize; i++)
                {
                    sum += data[i];
                }

                Console.WriteLine($"   Lote {batch + 1}: soma = {sum:N0}");
                
                // Simula processamento assíncrono
                await Task.Delay(10);
            }
            finally
            {
                pool.Return(data, clearArray: true);
            }
        }
    }
}

// Classe helper para facilitar uso com using
public readonly struct ArrayRental<T> : IDisposable
{
    private readonly ArrayPool<T> _pool;
    private readonly bool _clearArray;

    public T[] Array { get; }

    public ArrayRental(ArrayPool<T> pool, int minimumLength, bool clearArray = true)
    {
        _pool = pool;
        _clearArray = clearArray;
        Array = pool.Rent(minimumLength);
    }

    public void Dispose()
    {
        _pool.Return(Array, _clearArray);
    }
}
