using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

Console.WriteLine("=== Benchmark: Value Tuples vs Tuple Performance ===");
Console.WriteLine("Demonstrando o impacto na performance...\n");

BenchmarkRunner.Run<ValueTupleVsTupleBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class ValueTupleVsTupleBenchmark
{
    private readonly int[] _testData;
    private readonly string[] _stringData;
    
    public ValueTupleVsTupleBenchmark()
    {
        _testData = Enumerable.Range(1, 10000).ToArray();
        _stringData = Enumerable.Range(1, 10000).Select(i => $"Item{i}").ToArray();
    }
    
    // ===== CREATION BENCHMARKS =====
    
    [Benchmark(Baseline = true)]
    public List<(int, string)> CreateValueTuples()
    {
        var result = new List<(int, string)>();
        for (int i = 0; i < _testData.Length; i++)
        {
            result.Add((_testData[i], _stringData[i]));
        }
        return result;
    }
    
    [Benchmark]
    public List<Tuple<int, string>> CreateTuples()
    {
        var result = new List<Tuple<int, string>>();
        for (int i = 0; i < _testData.Length; i++)
        {
            result.Add(new Tuple<int, string>(_testData[i], _stringData[i]));
        }
        return result;
    }
    
    // ===== ACCESS BENCHMARKS =====
    
    [Benchmark]
    public long ValueTupleAccess()
    {
        long sum = 0;
        for (int i = 0; i < _testData.Length; i++)
        {
            var tuple = (_testData[i], _stringData[i]);
            sum += tuple.Item1 + tuple.Item2.Length;
        }
        return sum;
    }
    
    [Benchmark]
    public long TupleAccess()
    {
        long sum = 0;
        for (int i = 0; i < _testData.Length; i++)
        {
            var tuple = new Tuple<int, string>(_testData[i], _stringData[i]);
            sum += tuple.Item1 + tuple.Item2.Length;
        }
        return sum;
    }
    
    // ===== NAMED FIELDS BENCHMARKS =====
    
    [Benchmark]
    public long ValueTupleWithNames()
    {
        long sum = 0;
        for (int i = 0; i < _testData.Length; i++)
        {
            var data = (id: _testData[i], name: _stringData[i]);
            sum += data.id + data.name.Length;
        }
        return sum;
    }
    
    // ===== METHOD RETURN BENCHMARKS =====
    
    [Benchmark]
    public List<(int sum, double avg, int max)> ValueTupleMethodReturns()
    {
        var result = new List<(int sum, double avg, int max)>();
        for (int i = 0; i < 1000; i++)
        {
            var chunk = _testData.Skip(i * 10).Take(10).ToArray();
            result.Add(CalculateStatsValueTuple(chunk));
        }
        return result;
    }
    
    [Benchmark]
    public List<Tuple<int, double, int>> TupleMethodReturns()
    {
        var result = new List<Tuple<int, double, int>>();
        for (int i = 0; i < 1000; i++)
        {
            var chunk = _testData.Skip(i * 10).Take(10).ToArray();
            result.Add(CalculateStatsTuple(chunk));
        }
        return result;
    }
    
    // ===== COLLECTION OPERATIONS =====
    
    [Benchmark]
    public Dictionary<(int, string), int> ValueTupleAsDictionaryKey()
    {
        var dict = new Dictionary<(int, string), int>();
        for (int i = 0; i < 1000; i++)
        {
            var key = (_testData[i], _stringData[i]);
            dict[key] = i;
        }
        return dict;
    }
    
    [Benchmark]
    public Dictionary<Tuple<int, string>, int> TupleAsDictionaryKey()
    {
        var dict = new Dictionary<Tuple<int, string>, int>();
        for (int i = 0; i < 1000; i++)
        {
            var key = new Tuple<int, string>(_testData[i], _stringData[i]);
            dict[key] = i;
        }
        return dict;
    }
    
    // ===== LINQ OPERATIONS =====
    
    [Benchmark]
    public List<(string category, int count, double average)> ValueTupleWithLINQ()
    {
        var data = _testData.Take(1000)
            .Select((value, index) => (
                value: value,
                category: value % 10 == 0 ? "Even10" : value % 2 == 0 ? "Even" : "Odd",
                index: index
            ));
        
        return data
            .GroupBy(item => item.category)
            .Select(g => (
                category: g.Key,
                count: g.Count(),
                average: g.Average(x => x.value)
            ))
            .ToList();
    }
    
    [Benchmark]
    public List<Tuple<string, int, double>> TupleWithLINQ()
    {
        var data = _testData.Take(1000)
            .Select((value, index) => new Tuple<int, string, int>(
                value,
                value % 10 == 0 ? "Even10" : value % 2 == 0 ? "Even" : "Odd",
                index
            ));
        
        return data
            .GroupBy(item => item.Item2)
            .Select(g => new Tuple<string, int, double>(
                g.Key,
                g.Count(),
                g.Average(x => x.Item1)
            ))
            .ToList();
    }
    
    // ===== COMPLEX NESTED STRUCTURES =====
    
    [Benchmark]
    public List<(int id, (string name, decimal price) product, (DateTime date, string status) order)> ComplexValueTuples()
    {
        var result = new List<(int, (string, decimal), (DateTime, string))>();
        for (int i = 0; i < 1000; i++)
        {
            var order = (
                id: i,
                product: (name: $"Product{i}", price: i * 10.5m),
                order: (date: DateTime.Now.AddDays(-i), status: i % 2 == 0 ? "Shipped" : "Pending")
            );
            result.Add(order);
        }
        return result;
    }
    
    [Benchmark]
    public List<Tuple<int, Tuple<string, decimal>, Tuple<DateTime, string>>> ComplexTuples()
    {
        var result = new List<Tuple<int, Tuple<string, decimal>, Tuple<DateTime, string>>>();
        for (int i = 0; i < 1000; i++)
        {
            var order = new Tuple<int, Tuple<string, decimal>, Tuple<DateTime, string>>(
                i,
                new Tuple<string, decimal>($"Product{i}", i * 10.5m),
                new Tuple<DateTime, string>(DateTime.Now.AddDays(-i), i % 2 == 0 ? "Shipped" : "Pending")
            );
            result.Add(order);
        }
        return result;
    }
    
    // ===== ARRAY OPERATIONS =====
    
    [Benchmark]
    public (int, string)[] ValueTupleArray()
    {
        var result = new (int, string)[1000];
        for (int i = 0; i < 1000; i++)
        {
            result[i] = (i, $"Item{i}");
        }
        return result;
    }
    
    [Benchmark]
    public Tuple<int, string>[] TupleArray()
    {
        var result = new Tuple<int, string>[1000];
        for (int i = 0; i < 1000; i++)
        {
            result[i] = new Tuple<int, string>(i, $"Item{i}");
        }
        return result;
    }
    
    // ===== DECONSTRUCTION BENCHMARKS =====
    
    [Benchmark]
    public long ValueTupleDeconstruction()
    {
        long sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var data = GetValueTupleData(i);
            var (id, name, value) = data;
            sum += id + name.Length + value;
        }
        return sum;
    }
    
    [Benchmark]
    public long TupleExplicitAccess()
    {
        long sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var data = GetTupleData(i);
            sum += data.Item1 + data.Item2.Length + data.Item3;
        }
        return sum;
    }
    
    // ===== EQUALITY COMPARISONS =====
    
    [Benchmark]
    public int ValueTupleEquality()
    {
        int count = 0;
        var reference = (id: 500, name: "Item500");
        
        for (int i = 0; i < 1000; i++)
        {
            var current = (id: i, name: $"Item{i}");
            if (current.Equals(reference))
                count++;
        }
        return count;
    }
    
    [Benchmark]
    public int TupleEquality()
    {
        int count = 0;
        var reference = new Tuple<int, string>(500, "Item500");
        
        for (int i = 0; i < 1000; i++)
        {
            var current = new Tuple<int, string>(i, $"Item{i}");
            if (current.Equals(reference))
                count++;
        }
        return count;
    }
    
    // ===== HELPER METHODS =====
    
    private static (int sum, double avg, int max) CalculateStatsValueTuple(int[] numbers)
    {
        return (numbers.Sum(), numbers.Average(), numbers.Max());
    }
    
    private static Tuple<int, double, int> CalculateStatsTuple(int[] numbers)
    {
        return new Tuple<int, double, int>(numbers.Sum(), numbers.Average(), numbers.Max());
    }
    
    private static (int id, string name, int value) GetValueTupleData(int i)
    {
        return (i, $"Item{i}", i * 2);
    }
    
    private static Tuple<int, string, int> GetTupleData(int i)
    {
        return new Tuple<int, string, int>(i, $"Item{i}", i * 2);
    }
}
