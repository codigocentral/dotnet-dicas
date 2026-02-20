using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Reflection;

BenchmarkRunner.Run<NameofVsReflectionBenchmarks>();

[MemoryDiagnoser]
[SimpleJob]
public class NameofVsReflectionBenchmarks
{
    private readonly TestClass _testInstance = new();
    private readonly Type _testType = typeof(TestClass);
    private PropertyInfo? _cachedProperty;

    [GlobalSetup]
    public void Setup()
    {
        // Cache property info para benchmark mais justo
        _cachedProperty = _testType.GetProperty(nameof(TestClass.TestProperty));
    }

    [Benchmark(Baseline = true)]
    public string UsingNameof()
    {
        // ✅ CORRETO: nameof - compile-time constant
        return nameof(TestClass.TestProperty);
    }

    [Benchmark]
    public string UsingReflection_GetProperty()
    {
        // ❌ LENTO: Reflection sem cache
        return _testType.GetProperty("TestProperty")?.Name ?? "";
    }

    [Benchmark]
    public string UsingReflection_GetProperty_WithNameof()
    {
        // ❌ LENTO: Reflection com nameof para string (ainda usa reflection)
        return _testType.GetProperty(nameof(TestClass.TestProperty))?.Name ?? "";
    }

    [Benchmark]
    public string UsingReflection_Cached()
    {
        // ❌ MELHOR que reflection sem cache, mas ainda lento
        return _cachedProperty?.Name ?? "";
    }

    [Benchmark]
    public string UsingGetTypeName()
    {
        // ❌ LENTO: GetType().Name tem custo de runtime
        return _testInstance.GetType().Name;
    }

    [Benchmark]
    public string UsingTypeofName()
    {
        // ✅ MELHOR: typeof().Name é resolvido em compile-time
        return typeof(TestClass).Name;
    }

    [Benchmark]
    public PropertyInfo[] UsingGetProperties()
    {
        // ❌ MUITO LENTO: GetProperties aloca array e faz muito trabalho
        return _testType.GetProperties();
    }

    [Benchmark]
    public PropertyInfo[] UsingGetPropertiesWithBinding()
    {
        // ❌ AINDA MAIS LENTO: GetProperties com binding flags
        return _testType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class PropertyAccessBenchmarks
{
    private readonly TestClass _instance = new() { TestProperty = "Test Value", Number = 42 };
    private readonly PropertyInfo _propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty))!;

    [Benchmark(Baseline = true)]
    public string DirectPropertyAccess()
    {
        // ✅ MAIS RÁPIDO: Acesso direto
        return _instance.TestProperty;
    }

    [Benchmark]
    public object? ReflectionPropertyAccess()
    {
        // ❌ LENTO: Acesso via reflection
        return _propertyInfo.GetValue(_instance);
    }

    [Benchmark]
    public object? ReflectionPropertyAccess_Uncached()
    {
        // ❌ MUITO LENTO: Reflection sem cache
        var prop = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));
        return prop?.GetValue(_instance);
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class LoggingBenchmarks
{
    private readonly TestClass _instance = new() { TestProperty = "Test", Number = 123 };

    [Benchmark(Baseline = true)]
    public string LogWithNameof()
    {
        // ✅ EFICIENTE: nameof em logging
        return $"Property {nameof(_instance.TestProperty)} = {_instance.TestProperty}";
    }

    [Benchmark]
    public string LogWithHardcodedString()
    {
        // ❌ PERIGOSO: String hard-coded (pode quebrar em refatoração)
        return $"Property TestProperty = {_instance.TestProperty}";
    }

    [Benchmark]
    public string LogWithReflection()
    {
        // ❌ LENTO: Usando reflection para obter nome
        var propName = typeof(TestClass).GetProperty("TestProperty")?.Name ?? "";
        return $"Property {propName} = {_instance.TestProperty}";
    }

    [Benchmark]
    public string LogWithExpressionTree()
    {
        // ❌ MAIS LENTO: Expression trees têm overhead
        var propName = ExpressionHelper.GetPropertyName<TestClass, string>(x => x.TestProperty);
        return $"Property {propName} = {_instance.TestProperty}";
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ValidationBenchmarks
{
    private readonly TestClass _instance = new() { TestProperty = "", Number = -1 };

    [Benchmark(Baseline = true)]
    public List<string> ValidateWithNameof()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(_instance.TestProperty))
        {
            // ✅ EFICIENTE: nameof
            errors.Add($"{nameof(_instance.TestProperty)} is required");
        }
        
        if (_instance.Number < 0)
        {
            errors.Add($"{nameof(_instance.Number)} must be positive");
        }
        
        return errors;
    }

    [Benchmark]
    public List<string> ValidateWithReflection()
    {
        var errors = new List<string>();
        var type = typeof(TestClass);
        
        if (string.IsNullOrEmpty(_instance.TestProperty))
        {
            // ❌ LENTO: reflection para obter nome
            var propName = type.GetProperty("TestProperty")?.Name ?? "";
            errors.Add($"{propName} is required");
        }
        
        if (_instance.Number < 0)
        {
            var propName = type.GetProperty("Number")?.Name ?? "";
            errors.Add($"{propName} must be positive");
        }
        
        return errors;
    }

    [Benchmark]
    public List<string> ValidateWithHardcodedStrings()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(_instance.TestProperty))
        {
            // ❌ PERIGOSO: hard-coded
            errors.Add("TestProperty is required");
        }
        
        if (_instance.Number < 0)
        {
            errors.Add("Number must be positive");
        }
        
        return errors;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ExceptionBenchmarks
{
    [Benchmark(Baseline = true)]
    public Exception CreateExceptionWithNameof()
    {
        // ✅ EFICIENTE: nameof
        return new ArgumentException("Invalid value", nameof(TestClass.TestProperty));
    }

    [Benchmark]
    public Exception CreateExceptionWithReflection()
    {
        // ❌ LENTO: reflection
        var propName = typeof(TestClass).GetProperty("TestProperty")?.Name ?? "";
        return new ArgumentException("Invalid value", propName);
    }

    [Benchmark]
    public Exception CreateExceptionWithHardcoded()
    {
        // ❌ PERIGOSO: hard-coded
        return new ArgumentException("Invalid value", "TestProperty");
    }
}

// Helper classes
public class TestClass
{
    public string TestProperty { get; set; } = string.Empty;
    public int Number { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}

public static class ExpressionHelper
{
    public static string GetPropertyName<T, TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is System.Linq.Expressions.MemberExpression member)
        {
            return member.Member.Name;
        }
        
        throw new ArgumentException("Expression must be a property");
    }
}
