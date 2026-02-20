using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;

BenchmarkRunner.Run<StackallocBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class StackallocBenchmark
{
    private readonly string _testData = "The quick brown fox jumps over the lazy dog. This is a longer text for conversion testing.";
    private readonly int[] _sourceNumbers = Enumerable.Range(1, 1000).ToArray();

    [Benchmark(Baseline = true)]
    public byte[] ConvertToUtf8WithHeapArray()
    {
        return Encoding.UTF8.GetBytes(_testData);
    }

    [Benchmark]
    public byte[] ConvertToUtf8WithStackalloc()
    {
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(_testData.Length);
        if (maxByteCount <= 1024) // Limite seguro para stack
        {
            Span<byte> buffer = stackalloc byte[maxByteCount];
            int bytesWritten = Encoding.UTF8.GetBytes(_testData, buffer);
            return buffer[..bytesWritten].ToArray();
        }
        return Encoding.UTF8.GetBytes(_testData); // Fallback para heap
    }

    [Benchmark]
    public int SumWithHeapArray()
    {
        var temp = new int[_sourceNumbers.Length];
        Array.Copy(_sourceNumbers, temp, _sourceNumbers.Length);
        
        int sum = 0;
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] *= 2;
            sum += temp[i];
        }
        return sum;
    }

    [Benchmark]
    public int SumWithStackallocSmall()
    {
        // Processa em lotes pequenos usando stackalloc
        const int batchSize = 256; // Seguro para stack
        int totalSum = 0;

        for (int offset = 0; offset < _sourceNumbers.Length; offset += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, _sourceNumbers.Length - offset);
            Span<int> batch = stackalloc int[currentBatchSize];
            
            // Copia lote
            _sourceNumbers.AsSpan(offset, currentBatchSize).CopyTo(batch);
            
            // Processa lote
            for (int i = 0; i < batch.Length; i++)
            {
                batch[i] *= 2;
                totalSum += batch[i];
            }
        }
        return totalSum;
    }

    [Benchmark]
    public int[] ProcessArrayWithHeap()
    {
        var result = new int[100];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = i * i + i;
        }
        return result;
    }

    [Benchmark]
    public int[] ProcessArrayWithStackalloc()
    {
        Span<int> temp = stackalloc int[100];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = i * i + i;
        }
        return temp.ToArray();
    }

    [Benchmark]
    public string ParseNumbersWithHeap()
    {
        var input = "1,22,333,4444,55555";
        var parts = input.Split(',');
        var numbers = new List<int>();
        
        foreach (var part in parts)
        {
            if (int.TryParse(part, out int number))
                numbers.Add(number);
        }
        
        return string.Join("-", numbers);
    }

    [Benchmark]
    public string ParseNumbersWithStackalloc()
    {
        var input = "1,22,333,4444,55555".AsSpan();
        Span<int> numbers = stackalloc int[10]; // Máximo esperado
        int count = 0;
        
        int start = 0;
        for (int i = 0; i <= input.Length; i++)
        {
            if (i == input.Length || input[i] == ',')
            {
                if (i > start && count < numbers.Length)
                {
                    if (int.TryParse(input[start..i], out int number))
                    {
                        numbers[count++] = number;
                    }
                }
                start = i + 1;
            }
        }
        
        var result = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0) result.Append('-');
            result.Append(numbers[i]);
        }
        return result.ToString();
    }

    [Benchmark]
    public char[] ToUpperWithHeap()
    {
        var chars = _testData.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = char.ToUpper(chars[i]);
        }
        return chars;
    }

    [Benchmark]
    public char[] ToUpperWithStackalloc()
    {
        if (_testData.Length <= 512) // Limite seguro
        {
            Span<char> chars = stackalloc char[_testData.Length];
            _testData.AsSpan().CopyTo(chars);
            
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = char.ToUpper(chars[i]);
            }
            
            return chars.ToArray();
        }
        
        // Fallback para heap se muito grande
        return _testData.ToUpper().ToCharArray();
    }
}
