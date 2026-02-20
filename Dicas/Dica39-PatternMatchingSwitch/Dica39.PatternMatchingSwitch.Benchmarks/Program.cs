using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<PatternMatchingBenchmarks>();

[MemoryDiagnoser]
[SimpleJob]
public class PatternMatchingBenchmarks
{
    private readonly int[] _testData;
    private readonly object[] _objectData;
    private readonly DayOfWeek[] _dayData;
    private readonly IShape[] _shapeData;

    public PatternMatchingBenchmarks()
    {
        _testData = Enumerable.Range(1, 1000).ToArray();
        _objectData = new object[] { 42, "Hello", 3.14, true, new Person("João", 30) };
        _dayData = Enum.GetValues<DayOfWeek>();
        _shapeData = new IShape[]
        {
            new Rectangle(5, 3),
            new Circle(4),
            new Triangle(6, 4),
            new Square(5)
        };
    }

    [Benchmark(Baseline = true)]
    public string CategoryWithTraditionalSwitch()
    {
        string result = "";
        foreach (var value in _testData.Take(100))
        {
            result = GetCategoryTraditionalSwitch(value % 10 + 1);
        }
        return result;
    }

    [Benchmark]
    public string CategoryWithSwitchExpression()
    {
        string result = "";
        foreach (var value in _testData.Take(100))
        {
            result = GetCategorySwitchExpression(value % 10 + 1);
        }
        return result;
    }

    [Benchmark]
    public string CategoryWithIfElse()
    {
        string result = "";
        foreach (var value in _testData.Take(100))
        {
            result = GetCategoryIfElse(value % 10 + 1);
        }
        return result;
    }

    [Benchmark]
    public string TypePatternMatching()
    {
        string result = "";
        foreach (var obj in _objectData)
        {
            for (int i = 0; i < 20; i++)
            {
                result = GetTypeDescription(obj);
            }
        }
        return result;
    }

    [Benchmark]
    public string TypeWithTraditionalApproach()
    {
        string result = "";
        foreach (var obj in _objectData)
        {
            for (int i = 0; i < 20; i++)
            {
                result = GetTypeDescriptionTraditional(obj);
            }
        }
        return result;
    }

    [Benchmark]
    public string DayTypeWithSwitchExpression()
    {
        string result = "";
        foreach (var day in _dayData)
        {
            for (int i = 0; i < 10; i++)
            {
                result = GetDayTypeSwitchExpression(day);
            }
        }
        return result;
    }

    [Benchmark]
    public string DayTypeWithTraditionalSwitch()
    {
        string result = "";
        foreach (var day in _dayData)
        {
            for (int i = 0; i < 10; i++)
            {
                result = GetDayTypeTraditionalSwitch(day);
            }
        }
        return result;
    }

    [Benchmark]
    public double ShapeAreaWithPatternMatching()
    {
        double total = 0;
        foreach (var shape in _shapeData)
        {
            for (int i = 0; i < 25; i++)
            {
                total += CalculateAreaPatternMatching(shape);
            }
        }
        return total;
    }

    [Benchmark]
    public double ShapeAreaWithPolymorphism()
    {
        double total = 0;
        foreach (var shape in _shapeData)
        {
            for (int i = 0; i < 25; i++)
            {
                total += shape.CalculateArea();
            }
        }
        return total;
    }

    [Benchmark]
    public double ShapeAreaWithTypeChecking()
    {
        double total = 0;
        foreach (var shape in _shapeData)
        {
            for (int i = 0; i < 25; i++)
            {
                total += CalculateAreaTypeChecking(shape);
            }
        }
        return total;
    }

    [Benchmark]
    public string PropertyPatternMatching()
    {
        string result = "";
        var person = new Person("Test", 25);
        for (int i = 0; i < 1000; i++)
        {
            result = ClassifyPersonWithPatterns(person);
        }
        return result;
    }

    [Benchmark]
    public string PropertyWithTraditionalCode()
    {
        string result = "";
        var person = new Person("Test", 25);
        for (int i = 0; i < 1000; i++)
        {
            result = ClassifyPersonTraditional(person);
        }
        return result;
    }

    // Implementation methods
    private string GetCategoryTraditionalSwitch(int value)
    {
        switch (value)
        {
            case 1:
            case 2:
            case 3:
                return "Baixo";
            case 4:
            case 5:
            case 6:
                return "Médio";
            case 7:
            case 8:
            case 9:
                return "Alto";
            case 10:
                return "Máximo";
            default:
                return "Inválido";
        }
    }

    private string GetCategorySwitchExpression(int value) => value switch
    {
        >= 1 and <= 3 => "Baixo",
        >= 4 and <= 6 => "Médio",
        >= 7 and <= 9 => "Alto",
        10 => "Máximo",
        _ => "Inválido"
    };

    private string GetCategoryIfElse(int value)
    {
        if (value >= 1 && value <= 3) return "Baixo";
        if (value >= 4 && value <= 6) return "Médio";
        if (value >= 7 && value <= 9) return "Alto";
        if (value == 10) return "Máximo";
        return "Inválido";
    }

    private string GetTypeDescription(object obj) => obj switch
    {
        int i => $"Integer: {i}",
        string s => $"String: {s}",
        double d => $"Double: {d}",
        bool b => $"Boolean: {b}",
        Person p => $"Person: {p.Name}",
        null => "Null",
        _ => "Unknown"
    };

    private string GetTypeDescriptionTraditional(object obj)
    {
        if (obj is int i) return $"Integer: {i}";
        if (obj is string s) return $"String: {s}";
        if (obj is double d) return $"Double: {d}";
        if (obj is bool b) return $"Boolean: {b}";
        if (obj is Person p) return $"Person: {p.Name}";
        if (obj == null) return "Null";
        return "Unknown";
    }

    private string GetDayTypeSwitchExpression(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday or DayOfWeek.Friday => "Weekday",
        DayOfWeek.Saturday or DayOfWeek.Sunday => "Weekend",
        _ => "Invalid"
    };

    private string GetDayTypeTraditionalSwitch(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
            case DayOfWeek.Friday:
                return "Weekday";
            case DayOfWeek.Saturday:
            case DayOfWeek.Sunday:
                return "Weekend";
            default:
                return "Invalid";
        }
    }

    private double CalculateAreaPatternMatching(IShape shape) => shape switch
    {
        Rectangle { Width: var w, Height: var h } => w * h,
        Circle { Radius: var r } => Math.PI * r * r,
        Triangle { Base: var b, Height: var h } => 0.5 * b * h,
        Square { Side: var s } => s * s,
        _ => 0
    };

    private double CalculateAreaTypeChecking(IShape shape)
    {
        if (shape is Rectangle rect)
            return rect.Width * rect.Height;
        if (shape is Circle circle)
            return Math.PI * circle.Radius * circle.Radius;
        if (shape is Triangle triangle)
            return 0.5 * triangle.Base * triangle.Height;
        if (shape is Square square)
            return square.Side * square.Side;
        return 0;
    }

    private string ClassifyPersonWithPatterns(Person person) => person switch
    {
        { Age: < 18 } => "Minor",
        { Age: >= 18 and < 60 } => "Adult",
        { Age: >= 60 } => "Senior",
        _ => "Unknown"
    };

    private string ClassifyPersonTraditional(Person person)
    {
        if (person.Age < 18) return "Minor";
        if (person.Age >= 18 && person.Age < 60) return "Adult";
        if (person.Age >= 60) return "Senior";
        return "Unknown";
    }
}

// Supporting types
public record Person(string Name, int Age);

public interface IShape
{
    double CalculateArea();
}

public record Rectangle(double Width, double Height) : IShape
{
    public double CalculateArea() => Width * Height;
}

public record Circle(double Radius) : IShape
{
    public double CalculateArea() => Math.PI * Radius * Radius;
}

public record Triangle(double Base, double Height) : IShape
{
    public double CalculateArea() => 0.5 * Base * Height;
}

public record Square(double Side) : IShape
{
    public double CalculateArea() => Side * Side;
}
