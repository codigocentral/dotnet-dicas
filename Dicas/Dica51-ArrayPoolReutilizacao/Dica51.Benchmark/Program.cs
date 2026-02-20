using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Buffers;

BenchmarkRunner.Run<ArrayPoolBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class ArrayPoolBenchmark
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Shared;
    private readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;

    [Params(1000, 10000, 100000)]
    public int ArraySize { get; set; }

    [Benchmark(Baseline = true)]
    public long SumWithNewArray()
    {
        var array = new int[ArraySize];
        
        // Inicializa
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = i;
        }

        // Calcula soma
        long sum = 0;
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }

        return sum;
    }

    [Benchmark]
    public long SumWithArrayPool()
    {
        var array = _pool.Rent(ArraySize);
        
        try
        {
            // Inicializa
            for (int i = 0; i < ArraySize; i++)
            {
                array[i] = i;
            }

            // Calcula soma
            long sum = 0;
            for (int i = 0; i < ArraySize; i++)
            {
                sum += array[i];
            }

            return sum;
        }
        finally
        {
            _pool.Return(array);
        }
    }

    [Benchmark]
    public int ProcessMultipleArraysNew()
    {
        int result = 0;
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            var array = new int[1000];
            
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i * iteration;
            }

            for (int i = 0; i < array.Length; i++)
            {
                result += array[i] % 1000;
            }
        }

        return result;
    }

    [Benchmark]
    public int ProcessMultipleArraysPool()
    {
        int result = 0;
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            var array = _pool.Rent(1000);
            
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    array[i] = i * iteration;
                }

                for (int i = 0; i < 1000; i++)
                {
                    result += array[i] % 1000;
                }
            }
            finally
            {
                _pool.Return(array);
            }
        }

        return result;
    }

    [Benchmark]
    public byte[] ProcessTextNew()
    {
        var input = "Este é um texto de exemplo que será processado múltiplas vezes para demonstrar o uso do ArrayPool vs alocação normal";
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        
        var buffer = new byte[inputBytes.Length * 2]; // Buffer maior
        
        // Copia e duplica dados
        Array.Copy(inputBytes, 0, buffer, 0, inputBytes.Length);
        Array.Copy(inputBytes, 0, buffer, inputBytes.Length, inputBytes.Length);
        
        return buffer;
    }

    [Benchmark]
    public byte[] ProcessTextPool()
    {
        var input = "Este é um texto de exemplo que será processado múltiplas vezes para demonstrar o uso do ArrayPool vs alocação normal";
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        
        var buffer = _bytePool.Rent(inputBytes.Length * 2);
        
        try
        {
            // Copia e duplica dados
            Array.Copy(inputBytes, 0, buffer, 0, inputBytes.Length);
            Array.Copy(inputBytes, 0, buffer, inputBytes.Length, inputBytes.Length);
            
            // Cria array resultado
            var result = new byte[inputBytes.Length * 2];
            Array.Copy(buffer, result, result.Length);
            return result;
        }
        finally
        {
            _bytePool.Return(buffer);
        }
    }

    [Benchmark]
    public void BatchProcessingNew()
    {
        const int batchCount = 50;
        const int batchSize = 2000;

        for (int batch = 0; batch < batchCount; batch++)
        {
            var data = new int[batchSize];
            
            // Preenche dados
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = batch * batchSize + i;
            }

            // Processa (simula ordenação simples)
            Array.Sort(data);
        }
    }

    [Benchmark]
    public void BatchProcessingPool()
    {
        const int batchCount = 50;
        const int batchSize = 2000;

        for (int batch = 0; batch < batchCount; batch++)
        {
            var data = _pool.Rent(batchSize);
            
            try
            {
                // Preenche dados
                for (int i = 0; i < batchSize; i++)
                {
                    data[i] = batch * batchSize + i;
                }

                // Processa (simula ordenação simples)
                Array.Sort(data, 0, batchSize);
            }
            finally
            {
                _pool.Return(data);
            }
        }
    }

    [Benchmark]
    public string ConcatenateStringsNew()
    {
        var strings = new[] { "Hello", "World", "ArrayPool", "Performance", "Test" };
        var totalLength = strings.Sum(s => s.Length);
        
        var buffer = new char[totalLength];
        int position = 0;
        
        foreach (var str in strings)
        {
            str.CopyTo(0, buffer, position, str.Length);
            position += str.Length;
        }

        return new string(buffer);
    }

    [Benchmark]
    public string ConcatenateStringsPool()
    {
        var strings = new[] { "Hello", "World", "ArrayPool", "Performance", "Test" };
        var totalLength = strings.Sum(s => s.Length);
        
        var charPool = ArrayPool<char>.Shared;
        var buffer = charPool.Rent(totalLength);
        
        try
        {
            int position = 0;
            
            foreach (var str in strings)
            {
                str.CopyTo(0, buffer, position, str.Length);
                position += str.Length;
            }

            return new string(buffer, 0, totalLength);
        }
        finally
        {
            charPool.Return(buffer);
        }
    }
}
