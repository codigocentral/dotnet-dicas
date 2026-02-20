using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.CompilerServices;

BenchmarkRunner.Run<MethodInliningBenchmarks>();

[MemoryDiagnoser]
[SimpleJob]
public class MethodInliningBenchmarks
{
    private readonly double[] _data = Enumerable.Range(1, 10000).Select(x => (double)x).ToArray();
    
    [Params(1000, 10000, 100000)]
    public int Iterations { get; set; }

    [Benchmark(Baseline = true)]
    public double NormalMethod()
    {
        double sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var index = i % _data.Length;
            sum += AddNormal(_data[index], 1.5);
        }
        return sum;
    }

    [Benchmark]
    public double InlinedMethod()
    {
        double sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var index = i % _data.Length;
            sum += AddInlined(_data[index], 1.5);
        }
        return sum;
    }

    [Benchmark]
    public double NoInlineMethod()
    {
        double sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var index = i % _data.Length;
            sum += AddNoInline(_data[index], 1.5);
        }
        return sum;
    }

    [Benchmark]
    public double DirectCalculation()
    {
        double sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var index = i % _data.Length;
            // Cálculo direto sem chamada de método
            sum += _data[index] + 1.5;
        }
        return sum;
    }

    private double AddNormal(double a, double b) => a + b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double AddInlined(double a, double b) => a + b;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private double AddNoInline(double a, double b) => a + b;
}

[MemoryDiagnoser]
[SimpleJob]
public class ComplexCalculationBenchmarks
{
    private readonly Point[] _points = Enumerable.Range(0, 1000)
        .Select(i => new Point(i * 0.1, (i * 0.1) + 1))
        .ToArray();

    [Benchmark(Baseline = true)]
    public double DistanceCalculationNormal()
    {
        double totalDistance = 0;
        for (int i = 0; i < _points.Length - 1; i++)
        {
            totalDistance += CalculateDistanceNormal(_points[i], _points[i + 1]);
        }
        return totalDistance;
    }

    [Benchmark]
    public double DistanceCalculationInlined()
    {
        double totalDistance = 0;
        for (int i = 0; i < _points.Length - 1; i++)
        {
            totalDistance += CalculateDistanceInlined(_points[i], _points[i + 1]);
        }
        return totalDistance;
    }

    [Benchmark]
    public double DistanceCalculationDirect()
    {
        double totalDistance = 0;
        for (int i = 0; i < _points.Length - 1; i++)
        {
            var p1 = _points[i];
            var p2 = _points[i + 1];
            var deltaX = p2.X - p1.X;
            var deltaY = p2.Y - p1.Y;
            totalDistance += Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        return totalDistance;
    }

    private double CalculateDistanceNormal(Point p1, Point p2)
    {
        var deltaX = p2.X - p1.X;
        var deltaY = p2.Y - p1.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double CalculateDistanceInlined(Point p1, Point p2)
    {
        var deltaX = p2.X - p1.X;
        var deltaY = p2.Y - p1.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ValidationBenchmarks
{
    private readonly string[] _emails = 
    {
        "user@example.com",
        "invalid.email",
        "test@domain.org",
        "bad@",
        "@domain.com",
        "",
        "good.email@company.co.uk",
        "user.name+tag@example.com"
    };

    [Benchmark(Baseline = true)]
    public int ValidateEmailsNormal()
    {
        int validCount = 0;
        foreach (var email in _emails)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (IsValidEmailNormal(email))
                    validCount++;
            }
        }
        return validCount;
    }

    [Benchmark]
    public int ValidateEmailsInlined()
    {
        int validCount = 0;
        foreach (var email in _emails)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (IsValidEmailInlined(email))
                    validCount++;
            }
        }
        return validCount;
    }

    [Benchmark]
    public int ValidateEmailsNoInline()
    {
        int validCount = 0;
        foreach (var email in _emails)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (IsValidEmailNoInline(email))
                    validCount++;
            }
        }
        return validCount;
    }

    private bool IsValidEmailNormal(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.LastIndexOf('@') == email.IndexOf('@');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsValidEmailInlined(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.LastIndexOf('@') == email.IndexOf('@');
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool IsValidEmailNoInline(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.LastIndexOf('@') == email.IndexOf('@');
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class MathOperationsBenchmarks
{
    private readonly int[] _numbers = Enumerable.Range(1, 10000).ToArray();

    [Benchmark(Baseline = true)]
    public int PowerOfTwoCheckNormal()
    {
        int count = 0;
        foreach (var number in _numbers)
        {
            if (IsPowerOfTwoNormal(number))
                count++;
        }
        return count;
    }

    [Benchmark]
    public int PowerOfTwoCheckInlined()
    {
        int count = 0;
        foreach (var number in _numbers)
        {
            if (IsPowerOfTwoInlined(number))
                count++;
        }
        return count;
    }

    [Benchmark]
    public int PowerOfTwoCheckDirect()
    {
        int count = 0;
        foreach (var number in _numbers)
        {
            if (number > 0 && (number & (number - 1)) == 0)
                count++;
        }
        return count;
    }

    private bool IsPowerOfTwoNormal(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPowerOfTwoInlined(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ArrayProcessingBenchmarks
{
    private readonly double[] _sourceArray = Enumerable.Range(1, 10000).Select(x => (double)x).ToArray();
    private readonly double[] _targetArray = new double[10000];

    [Benchmark(Baseline = true)]
    public void ProcessArrayNormal()
    {
        for (int i = 0; i < _sourceArray.Length; i++)
        {
            _targetArray[i] = TransformValueNormal(_sourceArray[i]);
        }
    }

    [Benchmark]
    public void ProcessArrayInlined()
    {
        for (int i = 0; i < _sourceArray.Length; i++)
        {
            _targetArray[i] = TransformValueInlined(_sourceArray[i]);
        }
    }

    [Benchmark]
    public void ProcessArrayDirect()
    {
        for (int i = 0; i < _sourceArray.Length; i++)
        {
            _targetArray[i] = _sourceArray[i] * 2.0 + 1.0;
        }
    }

    private double TransformValueNormal(double value)
    {
        return value * 2.0 + 1.0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double TransformValueInlined(double value)
    {
        return value * 2.0 + 1.0;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class PropertyAccessBenchmarks
{
    private readonly TestObject _obj = new(42, "Test", 3.14);

    [Benchmark(Baseline = true)]
    public double CalculateWithPropertiesNormal()
    {
        double result = 0;
        for (int i = 0; i < 100000; i++)
        {
            result += CalculateNormal(_obj.IntValue, _obj.DoubleValue);
        }
        return result;
    }

    [Benchmark]
    public double CalculateWithPropertiesInlined()
    {
        double result = 0;
        for (int i = 0; i < 100000; i++)
        {
            result += CalculateInlined(_obj.IntValue, _obj.DoubleValue);
        }
        return result;
    }

    [Benchmark]
    public double CalculateWithCachedValues()
    {
        double result = 0;
        var intVal = _obj.IntValue;
        var doubleVal = _obj.DoubleValue;
        
        for (int i = 0; i < 100000; i++)
        {
            result += CalculateInlined(intVal, doubleVal);
        }
        return result;
    }

    private double CalculateNormal(int intVal, double doubleVal)
    {
        return intVal * doubleVal + 0.5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double CalculateInlined(int intVal, double doubleVal)
    {
        return intVal * doubleVal + 0.5;
    }
}

// Helper types
public readonly record struct Point(double X, double Y);

public readonly record struct TestObject(int IntValue, string StringValue, double DoubleValue);
