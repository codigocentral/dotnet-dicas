using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Dynamic;
using System.Text.Json;

Console.WriteLine("=== Benchmark: Dynamic vs Tipagem Estática ===");
Console.WriteLine("Demonstrando o impacto na performance...\n");

BenchmarkRunner.Run<DynamicVsStaticBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class DynamicVsStaticBenchmark
{
    private readonly Person _staticPerson;
    private readonly dynamic _dynamicPerson;
    private readonly Employee _employee;
    private readonly dynamic _expandoObject;
    private readonly string _jsonString;
    
    public DynamicVsStaticBenchmark()
    {
        _staticPerson = new Person("João Silva", 30, "Desenvolvedor");
        _dynamicPerson = new Person("João Silva", 30, "Desenvolvedor");
        _employee = new Employee { Name = "Maria", Age = 25, Department = "TI", Salary = 5000 };
        
        _expandoObject = new ExpandoObject();
        _expandoObject.Name = "Carlos";
        _expandoObject.Age = 35;
        _expandoObject.Position = "Gerente";
        
        _jsonString = """
            {
                "name": "Ana",
                "age": 28,
                "department": "Marketing",
                "salary": 4500,
                "isActive": true
            }
            """;
    }
    
    // ===== PROPERTY ACCESS BENCHMARKS =====
    
    [Benchmark(Baseline = true)]
    public string StaticPropertyAccess()
    {
        return $"{_staticPerson.Name} - {_staticPerson.Age} - {_staticPerson.Position}";
    }
    
    [Benchmark]
    public string DynamicPropertyAccess()
    {
        return $"{_dynamicPerson.Name} - {_dynamicPerson.Age} - {_dynamicPerson.Position}";
    }
    
    [Benchmark]
    public string ExpandoObjectAccess()
    {
        return $"{_expandoObject.Name} - {_expandoObject.Age} - {_expandoObject.Position}";
    }
    
    // ===== METHOD INVOCATION BENCHMARKS =====
    
    [Benchmark]
    public string StaticMethodInvocation()
    {
        return _staticPerson.GetDisplayName();
    }
    
    [Benchmark]
    public string DynamicMethodInvocation()
    {
        return _dynamicPerson.GetDisplayName();
    }
    
    // ===== CALCULATION BENCHMARKS =====
    
    [Benchmark]
    public double StaticCalculations()
    {
        double result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += _staticPerson.CalculateYearlyBonus(i);
        }
        return result;
    }
    
    [Benchmark]
    public double DynamicCalculations()
    {
        double result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += _dynamicPerson.CalculateYearlyBonus(i);
        }
        return result;
    }
    
    // ===== JSON PROCESSING BENCHMARKS =====
    
    [Benchmark]
    public string JsonWithStrongTypes()
    {
        var employee = JsonSerializer.Deserialize<Employee>(_jsonString);
        return $"{employee?.Name} earns {employee?.Salary:C}";
    }
    
    [Benchmark]
    public string JsonWithJsonElement()
    {
        using var document = JsonDocument.Parse(_jsonString);
        var root = document.RootElement;
        
        var name = root.GetProperty("name").GetString();
        var salary = root.GetProperty("salary").GetDecimal();
        
        return $"{name} earns {salary:C}";
    }
    
    // ===== COLLECTION OPERATIONS BENCHMARKS =====
    
    [Benchmark]
    public int StaticCollectionOperations()
    {
        var list = new List<Person>();
        for (int i = 0; i < 100; i++)
        {
            list.Add(new Person($"Person{i}", 20 + i % 50, "Role" + i % 5));
        }
        
        return list.Count(p => p.Age > 30);
    }
    
    [Benchmark]
    public int DynamicCollectionOperations()
    {
        var list = new List<dynamic>();
        for (int i = 0; i < 100; i++)
        {
            list.Add(new Person($"Person{i}", 20 + i % 50, "Role" + i % 5));
        }
        
        return list.Count(p => p.Age > 30);
    }
    
    // ===== INTERFACE VS DYNAMIC BENCHMARKS =====
    
    [Benchmark]
    public string InterfacePolymorphism()
    {
        IProcessor processor = new ConcreteProcessor();
        return processor.Process("test data");
    }
    
    [Benchmark]
    public string DynamicPolymorphism()
    {
        dynamic processor = new ConcreteProcessor();
        return processor.Process("test data");
    }
    
    // ===== ARITHMETIC OPERATIONS =====
    
    [Benchmark]
    public double StaticArithmetic()
    {
        double result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += _staticPerson.Age * 1.5 + _staticPerson.Name.Length;
        }
        return result;
    }
    
    [Benchmark]
    public double DynamicArithmetic()
    {
        double result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += _dynamicPerson.Age * 1.5 + _dynamicPerson.Name.Length;
        }
        return result;
    }
}

// ===== SUPPORTING TYPES =====

public class Person
{
    public string Name { get; }
    public int Age { get; }
    public string Position { get; }
    
    public Person(string name, int age, string position)
    {
        Name = name;
        Age = age;
        Position = position;
    }
    
    public string GetDisplayName()
    {
        return $"{Name} ({Age}) - {Position}";
    }
    
    public double CalculateYearlyBonus(int performanceScore)
    {
        return (Age * 100) + (performanceScore * 10) + Position.Length * 50;
    }
}

public class Employee
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Department { get; set; } = "";
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
}

public interface IProcessor
{
    string Process(string data);
}

public class ConcreteProcessor : IProcessor
{
    public string Process(string data)
    {
        return $"Processed: {data.ToUpperInvariant()}";
    }
}
