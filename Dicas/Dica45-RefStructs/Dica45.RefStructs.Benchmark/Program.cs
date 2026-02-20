using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;

Console.WriteLine("=== Benchmark: Ref Structs vs Structs Normais ===");
Console.WriteLine("Executando benchmarks de performance...\n");

BenchmarkRunner.Run<RefStructBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class RefStructBenchmark
{
    private const int DataSize = 10000;
    private readonly int[] _sourceData;
    
    public RefStructBenchmark()
    {
        _sourceData = new int[DataSize];
        for (int i = 0; i < DataSize; i++)
        {
            _sourceData[i] = Random.Shared.Next(1, 1000);
        }
    }
    
    [Benchmark(Baseline = true)]
    public ProcessingResult ProcessWithNormalStruct()
    {
        var processor = new NormalStructProcessor(_sourceData);
        return processor.ProcessData();
    }
    
    [Benchmark]
    public ProcessingResult ProcessWithRefStruct()
    {
        var span = _sourceData.AsSpan();
        var processor = new RefStructProcessor(span);
        return processor.ProcessData();
    }
    
    [Benchmark]
    public long SumWithStackAlloc()
    {
        // Simulando processamento com dados na stack
        Span<int> stackData = stackalloc int[1000];
        
        // Copiando uma parte dos dados para a stack
        _sourceData.AsSpan(0, 1000).CopyTo(stackData);
        
        var processor = new RefStructProcessor(stackData);
        var result = processor.ProcessData();
        return result.Sum;
    }
    
    [Benchmark]
    public string ParseNumbersNormal()
    {
        var text = string.Join(",", _sourceData.Take(100));
        var parser = new NormalStringParser(text);
        return parser.ParseAndFormat();
    }
    
    [Benchmark]
    public string ParseNumbersRefStruct()
    {
        var text = string.Join(",", _sourceData.Take(100));
        var span = text.AsSpan();
        var parser = new RefStructStringParser(span);
        return parser.ParseAndFormat();
    }
    
    [Benchmark]
    public void MemoryOperationsNormal()
    {
        var buffer = new byte[1024];
        var manipulator = new NormalMemoryManipulator(buffer);
        
        for (int i = 0; i < 100; i++)
        {
            manipulator.WriteValue(i, i * 4);
            manipulator.ReadValue(i * 4);
        }
    }
    
    [Benchmark]
    public void MemoryOperationsRefStruct()
    {
        Span<byte> buffer = stackalloc byte[1024];
        var manipulator = new RefStructMemoryManipulator(buffer);
        
        for (int i = 0; i < 100; i++)
        {
            manipulator.WriteValue(i, i * 4);
            manipulator.ReadValue(i * 4);
        }
    }
}

// ===== IMPLEMENTAÇÕES NORMAIS (com alocações) =====

public class NormalStructProcessor
{
    private readonly int[] _data;
    
    public NormalStructProcessor(int[] data)
    {
        _data = data;
    }
    
    public ProcessingResult ProcessData()
    {
        long sum = 0;
        int min = int.MaxValue;
        int max = int.MinValue;
        
        foreach (var value in _data)
        {
            sum += value;
            if (value < min) min = value;
            if (value > max) max = value;
        }
        
        return new ProcessingResult(sum, min, max, _data.Length);
    }
}

public class NormalStringParser
{
    private readonly string _text;
    
    public NormalStringParser(string text)
    {
        _text = text;
    }
    
    public string ParseAndFormat()
    {
        var numbers = _text.Split(',')
            .Select(s => int.Parse(s.Trim()))
            .Take(10)
            .ToList();
        
        return string.Join(" | ", numbers.Select(n => $"{n:N0}"));
    }
}

public class NormalMemoryManipulator
{
    private readonly byte[] _buffer;
    
    public NormalMemoryManipulator(byte[] buffer)
    {
        _buffer = buffer;
    }
    
    public void WriteValue(int value, int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, _buffer, offset, sizeof(int));
        }
    }
    
    public int ReadValue(int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            return BitConverter.ToInt32(_buffer, offset);
        }
        return 0;
    }
}

// ===== IMPLEMENTAÇÕES COM REF STRUCT (zero alocações) =====

public ref struct RefStructProcessor
{
    private readonly ReadOnlySpan<int> _data;
    
    public RefStructProcessor(ReadOnlySpan<int> data)
    {
        _data = data;
    }
    
    public ProcessingResult ProcessData()
    {
        if (_data.IsEmpty)
            return new ProcessingResult(0, 0, 0, 0);
        
        long sum = 0;
        int min = int.MaxValue;
        int max = int.MinValue;
        
        foreach (var value in _data)
        {
            sum += value;
            if (value < min) min = value;
            if (value > max) max = value;
        }
        
        return new ProcessingResult(sum, min, max, _data.Length);
    }
}

public ref struct RefStructStringParser
{
    private readonly ReadOnlySpan<char> _text;
    
    public RefStructStringParser(ReadOnlySpan<char> text)
    {
        _text = text;
    }
    
    public string ParseAndFormat()
    {
        Span<int> numbers = stackalloc int[10];
        int count = 0;
        
        var remaining = _text;
        while (count < 10 && !remaining.IsEmpty)
        {
            var commaIndex = remaining.IndexOf(',');
            var numberSpan = commaIndex >= 0 ? remaining.Slice(0, commaIndex) : remaining;
            
            if (int.TryParse(numberSpan.Trim(), out var number))
            {
                numbers[count++] = number;
            }
            
            if (commaIndex < 0) break;
            remaining = remaining.Slice(commaIndex + 1);
        }
        
        // Formatting sem alocações desnecessárias
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0) result.Append(" | ");
            result.Append($"{numbers[i]:N0}");
        }
        
        return result.ToString();
    }
}

public ref struct RefStructMemoryManipulator
{
    private readonly Span<byte> _buffer;
    
    public RefStructMemoryManipulator(Span<byte> buffer)
    {
        _buffer = buffer;
    }
    
    public void WriteValue(int value, int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            MemoryMarshal.Write(_buffer.Slice(offset), in value);
        }
    }
    
    public int ReadValue(int offset)
    {
        if (offset + sizeof(int) <= _buffer.Length)
        {
            return MemoryMarshal.Read<int>(_buffer.Slice(offset));
        }
        return 0;
    }
}

// ===== TIPOS COMPARTILHADOS =====

public readonly struct ProcessingResult
{
    public long Sum { get; }
    public int Min { get; }
    public int Max { get; }
    public int Count { get; }
    
    public ProcessingResult(long sum, int min, int max, int count)
    {
        Sum = sum;
        Min = min;
        Max = max;
        Count = count;
    }
}
