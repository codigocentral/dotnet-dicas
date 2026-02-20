using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

Console.WriteLine("=== Benchmark: LINQ Deferred vs Immediate Execution ===");
Console.WriteLine("Demonstrando o impacto na performance...\n");

BenchmarkRunner.Run<LINQDeferredExecutionBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class LINQDeferredExecutionBenchmark
{
    private readonly List<int> _sourceData;
    private readonly List<Product> _products;
    
    public LINQDeferredExecutionBenchmark()
    {
        _sourceData = Enumerable.Range(1, 100_000).ToList();
        _products = GenerateProducts(10_000).ToList();
    }
    
    // ===== DEFERRED VS IMMEDIATE EXECUTION =====
    
    [Benchmark(Baseline = true)]
    public List<int> DeferredWithSingleEnumeration()
    {
        var query = _sourceData
            .Where(x => x % 2 == 0)
            .Select(x => x * 2)
            .Take(1000);
        
        // Single enumeration - optimal
        return query.ToList();
    }
    
    [Benchmark]
    public List<int> ImmediateWithToList()
    {
        var query = _sourceData
            .Where(x => x % 2 == 0)
            .Select(x => x * 2)
            .Take(1000)
            .ToList(); // Immediate execution
        
        return query;
    }
    
    // ===== MULTIPLE ENUMERATION PROBLEM =====
    
    [Benchmark]
    public (int count, long sum, double avg) MultipleEnumerationBad()
    {
        var query = _sourceData
            .Where(x => x % 2 == 0)
            .Select(x => x * x);
        
        // BAD: Multiple enumeration - executes query 3 times!
        var count = query.Count();
        var sum = query.Sum();
        var avg = query.Average();
        
        return (count, sum, avg);
    }
    
    [Benchmark]
    public (int count, long sum, double avg) SingleEnumerationGood()
    {
        var query = _sourceData
            .Where(x => x % 2 == 0)
            .Select(x => x * x);
        
        // GOOD: Single enumeration
        var materialized = query.ToList();
        var count = materialized.Count;
        var sum = materialized.Sum();
        var avg = materialized.Average();
        
        return (count, sum, avg);
    }
    
    // ===== TAKE VS FULL PROCESSING =====
    
    [Benchmark]
    public List<int> DeferredTakeOptimal()
    {
        // Deferred - only processes what's needed
        return _sourceData
            .Where(IsExpensive)
            .Select(ExpensiveTransform)
            .Take(100)
            .ToList();
    }
    
    [Benchmark]
    public List<int> ImmediateTakeSuboptimal()
    {
        // Immediate - processes everything, then takes
        return _sourceData
            .Where(IsExpensive)
            .Select(ExpensiveTransform)
            .ToList()
            .Take(100)
            .ToList();
    }
    
    // ===== COMPLEX QUERIES =====
    
    [Benchmark]
    public List<string> ComplexQueryDeferred()
    {
        var query = _products
            .Where(p => p.Price > 50)
            .GroupBy(p => p.Category)
            .Select(g => $"{g.Key}: {g.Count()} items")
            .OrderBy(s => s)
            .Take(10);
        
        return query.ToList();
    }
    
    [Benchmark]
    public List<string> ComplexQueryImmediate()
    {
        var filtered = _products
            .Where(p => p.Price > 50)
            .ToList();
        
        var grouped = filtered
            .GroupBy(p => p.Category)
            .ToList();
        
        var selected = grouped
            .Select(g => $"{g.Key}: {g.Count()} items")
            .ToList();
        
        var ordered = selected
            .OrderBy(s => s)
            .ToList();
        
        return ordered.Take(10).ToList();
    }
    
    // ===== FIRST/SINGLE OPERATIONS =====
    
    [Benchmark]
    public Product FindFirstDeferred()
    {
        // Optimal - stops at first match
        return _products
            .Where(p => p.Price > 100)
            .OrderBy(p => p.Price)
            .First();
    }
    
    [Benchmark]
    public Product FindFirstImmediate()
    {
        // Suboptimal - processes all, then takes first
        return _products
            .Where(p => p.Price > 100)
            .OrderBy(p => p.Price)
            .ToList()
            .First();
    }
    
    // ===== ANY/ALL OPERATIONS =====
    
    [Benchmark]
    public bool AnyOperationDeferred()
    {
        // Optimal - short circuits
        return _products
            .Where(p => p.Name.Length > 10)
            .Any(p => p.Price > 200);
    }
    
    [Benchmark]
    public bool AnyOperationImmediate()
    {
        // Suboptimal - processes all
        return _products
            .Where(p => p.Name.Length > 10)
            .ToList()
            .Any(p => p.Price > 200);
    }
    
    // ===== COUNT WITH FILTER =====
    
    [Benchmark]
    public int CountWithFilterDeferred()
    {
        // Optimal - uses specialized Count with predicate
        return _products.Count(p => p.Price > 75 && p.Category == "Electronics");
    }
    
    [Benchmark]
    public int CountWithFilterImmediate()
    {
        // Suboptimal - materializes then counts
        return _products
            .Where(p => p.Price > 75 && p.Category == "Electronics")
            .ToList()
            .Count;
    }
    
    // ===== PARALLEL LINQ COMPARISON =====
    
    [Benchmark]
    public List<int> ParallelLINQDeferred()
    {
        return _sourceData
            .AsParallel()
            .Where(IsExpensive)
            .Select(ExpensiveTransform)
            .Take(1000)
            .ToList();
    }
    
    [Benchmark]
    public List<int> SequentialLINQDeferred()
    {
        return _sourceData
            .Where(IsExpensive)
            .Select(ExpensiveTransform)
            .Take(1000)
            .ToList();
    }
    
    // ===== HELPER METHODS =====
    
    private static bool IsExpensive(int value)
    {
        // Simulate expensive predicate
        return value % 1000 == 0;
    }
    
    private static int ExpensiveTransform(int value)
    {
        // Simulate expensive transformation
        return value * value + value;
    }
    
    private static IEnumerable<Product> GenerateProducts(int count)
    {
        var categories = new[] { "Electronics", "Clothing", "Books", "Sports", "Home" };
        var random = new Random(42);
        
        for (int i = 1; i <= count; i++)
        {
            yield return new Product
            {
                Id = i,
                Name = $"Product {i}",
                Category = categories[random.Next(categories.Length)],
                Price = random.Next(10, 500)
            };
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
}
