using System.Runtime.InteropServices;

Console.WriteLine("=== Dica 48: Usando stackalloc para Performance ===\n");

var demo = new StackallocDemo();

// Demonstrações básicas
Console.WriteLine("1. Uso básico do stackalloc:");
demo.BasicStackallocDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstrações com Span<T>
Console.WriteLine("2. stackalloc com Span<T>:");
demo.SpanStackallocDemo();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstrações práticas
Console.WriteLine("3. Casos de uso práticos:");
await demo.PracticalExamples();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstração de parsing
Console.WriteLine("4. Parsing eficiente:");
demo.EfficientParsing();

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Resumo das práticas recomendadas:");
Console.WriteLine("✅ Use stackalloc para arrays pequenos e temporários");
Console.WriteLine("✅ Combine com Span<T> para flexibilidade");
Console.WriteLine("✅ Ideal para buffers de tamanho conhecido e limitado");
Console.WriteLine("✅ Evita alocações no heap e pressão no GC");
Console.WriteLine("⚠️  Cuidado com o tamanho da stack (recomendado < 1KB)");
Console.WriteLine("⚠️  Apenas para contextos unsafe ou com Span<T>");

public class StackallocDemo
{
    public void BasicStackallocDemo()
    {
        Console.WriteLine("Exemplo básico de stackalloc:");
        
        // Aloca array na stack em vez do heap
        Span<int> numbers = stackalloc int[10];
        
        // Inicializa os valores
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = (i + 1) * (i + 1); // quadrados
        }

        Console.WriteLine("Quadrados calculados na stack:");
        for (int i = 0; i < numbers.Length; i++)
        {
            Console.WriteLine($"  {i + 1}² = {numbers[i]}");
        }

        // Exemplo com inicializadores
        Span<byte> buffer = stackalloc byte[] { 1, 2, 3, 4, 5 };
        Console.WriteLine($"\nBuffer inicializado: [{string.Join(", ", buffer.ToArray())}]");
    }

    public void SpanStackallocDemo()
    {
        Console.WriteLine("stackalloc com Span<T> oferece mais flexibilidade:");

        // Buffer para processamento de texto
        Span<char> textBuffer = stackalloc char[256];
        var text = "Hello, stackalloc world!";
        text.AsSpan().CopyTo(textBuffer);

        Console.WriteLine($"Texto copiado para buffer na stack: {new string(textBuffer[..text.Length])}");

        // Processamento in-place
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLower(textBuffer[i]))
                textBuffer[i] = char.ToUpper(textBuffer[i]);
        }

        Console.WriteLine($"Texto convertido: {new string(textBuffer[..text.Length])}");

        // Exemplo com slicing
        var slice = textBuffer[..5];
        Console.WriteLine($"Slice dos primeiros 5 caracteres: {new string(slice)}");
    }

    public async Task PracticalExamples()
    {
        Console.WriteLine("Casos práticos de uso do stackalloc:");

        // 1. Buffer para operações de rede/I-O
        await NetworkBufferExample();

        // 2. Processamento de dados numéricos
        NumericalProcessingExample();

        // 3. Conversão de dados
        DataConversionExample();
    }

    private async Task NetworkBufferExample()
    {
        Console.WriteLine("\n1. Buffer para operações de I/O:");
        
        // Simula leitura de dados pequenos
        Span<byte> ioBuffer = stackalloc byte[1024]; // 1KB buffer
        
        // Simula preenchimento do buffer
        var data = "Dados importantes do servidor"u8;
        data.CopyTo(ioBuffer);

        Console.WriteLine($"   Dados lidos no buffer: {System.Text.Encoding.UTF8.GetString(ioBuffer[..data.Length])}");
        
        // Simula processamento assíncrono
        await Task.Delay(10);
        Console.WriteLine("   Processamento assíncrono concluído");
    }

    private void NumericalProcessingExample()
    {
        Console.WriteLine("\n2. Processamento numérico eficiente:");

        Span<double> coefficients = stackalloc double[] { 2.5, -1.3, 0.8, 3.2, -0.5 };
        Span<double> results = stackalloc double[coefficients.Length];

        // Aplica transformação matemática
        for (int i = 0; i < coefficients.Length; i++)
        {
            results[i] = Math.Pow(coefficients[i], 2) + 1;
        }

        Console.WriteLine("   Transformação aplicada:");
        for (int i = 0; i < coefficients.Length; i++)
        {
            Console.WriteLine($"     f({coefficients[i]:F1}) = {results[i]:F2}");
        }
    }

    private void DataConversionExample()
    {
        Console.WriteLine("\n3. Conversão eficiente de dados:");

        // Converte string para bytes sem alocar array no heap
        var text = "Exemplo de conversão";
        Span<byte> utf8Bytes = stackalloc byte[System.Text.Encoding.UTF8.GetByteCount(text)];
        
        int bytesWritten = System.Text.Encoding.UTF8.GetBytes(text, utf8Bytes);
        
        Console.WriteLine($"   Texto original: {text}");
        Console.WriteLine($"   Bytes UTF-8: [{string.Join(", ", utf8Bytes[..bytesWritten].ToArray())}]");
        Console.WriteLine($"   Total de bytes: {bytesWritten}");

        // Converte de volta para string
        var decoded = System.Text.Encoding.UTF8.GetString(utf8Bytes[..bytesWritten]);
        Console.WriteLine($"   Texto decodificado: {decoded}");
    }

    public void EfficientParsing()
    {
        Console.WriteLine("Parsing eficiente com stackalloc:");

        var input = "123,456,789,012,345";
        
        // Buffer para armazenar números parseados
        Span<long> numbers = stackalloc long[10]; // Máximo 10 números
        int count = 0;

        // Parser manual usando Span
        var span = input.AsSpan();
        int start = 0;

        for (int i = 0; i <= span.Length; i++)
        {
            if (i == span.Length || span[i] == ',')
            {
                if (i > start && count < numbers.Length)
                {
                    var numberSpan = span[start..i];
                    if (long.TryParse(numberSpan, out long result))
                    {
                        numbers[count++] = result;
                    }
                }
                start = i + 1;
            }
        }

        Console.WriteLine($"String original: {input}");
        Console.WriteLine("Números parseados:");
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"  [{i}] = {numbers[i]:N0}");
        }

        // Calcula estatísticas usando o buffer na stack
        long sum = 0;
        long max = long.MinValue;
        long min = long.MaxValue;

        for (int i = 0; i < count; i++)
        {
            sum += numbers[i];
            if (numbers[i] > max) max = numbers[i];
            if (numbers[i] < min) min = numbers[i];
        }

        Console.WriteLine($"\nEstatísticas:");
        Console.WriteLine($"  Soma: {sum:N0}");
        Console.WriteLine($"  Média: {sum / count:N0}");
        Console.WriteLine($"  Mínimo: {min:N0}");
        Console.WriteLine($"  Máximo: {max:N0}");
    }
}
