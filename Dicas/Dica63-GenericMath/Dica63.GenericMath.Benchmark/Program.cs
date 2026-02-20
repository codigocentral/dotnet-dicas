using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Numerics;

namespace Dica63.GenericMath.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("🧮 Benchmarks: Generic Math vs Implementações Específicas\n");
        
        BenchmarkRunner.Run<ArithmeticBenchmarks>();
        BenchmarkRunner.Run<MathAlgorithmsBenchmarks>();
        BenchmarkRunner.Run<StatisticsBenchmarks>();
        BenchmarkRunner.Run<GeometryBenchmarks>();
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class ArithmeticBenchmarks
{
    private readonly int[] _intData;
    private readonly double[] _doubleData;
    private readonly decimal[] _decimalData;
    
    public ArithmeticBenchmarks()
    {
        _intData = Enumerable.Range(1, 1000).ToArray();
        _doubleData = _intData.Select(x => (double)x).ToArray();
        _decimalData = _intData.Select(x => (decimal)x).ToArray();
    }

    [Benchmark]
    public int TraditionalIntSum()
    {
        int sum = 0;
        foreach (var value in _intData)
        {
            sum += value;
        }
        return sum;
    }

    [Benchmark]
    public int GenericIntSum()
    {
        int sum = 0;
        foreach (var value in _intData)
        {
            sum = SomaGenerica(sum, value);
        }
        return sum;
    }

    [Benchmark]
    public double TraditionalDoubleSum()
    {
        double sum = 0.0;
        foreach (var value in _doubleData)
        {
            sum += value;
        }
        return sum;
    }

    [Benchmark]
    public double GenericDoubleSum()
    {
        double sum = 0.0;
        foreach (var value in _doubleData)
        {
            sum = SomaGenerica(sum, value);
        }
        return sum;
    }

    [Benchmark]
    public decimal TraditionalDecimalSum()
    {
        decimal sum = 0m;
        foreach (var value in _decimalData)
        {
            sum += value;
        }
        return sum;
    }

    [Benchmark]
    public decimal GenericDecimalSum()
    {
        decimal sum = 0m;
        foreach (var value in _decimalData)
        {
            sum = SomaGenerica(sum, value);
        }
        return sum;
    }

    static T SomaGenerica<T>(T a, T b) where T : INumber<T> => a + b;
}

[MemoryDiagnoser]
[SimpleJob]
public class MathAlgorithmsBenchmarks
{
    [Params(10, 15, 20)]
    public int FatorialInput { get; set; }

    [Params(2, 10)]
    public int PotenciaBase { get; set; }

    [Params(8, 16)]
    public int PotenciaExpoente { get; set; }

    [Benchmark]
    public int TraditionalFactorial()
    {
        return TraditionalFactorialImpl(FatorialInput);
    }

    [Benchmark]
    public int GenericFactorial()
    {
        return FatorialGenerico(FatorialInput);
    }

    [Benchmark]
    public int TraditionalPower()
    {
        return TraditionalPowerImpl(PotenciaBase, PotenciaExpoente);
    }

    [Benchmark]
    public int GenericPower()
    {
        return PotenciaGenerica(PotenciaBase, PotenciaExpoente);
    }

    [Benchmark]
    public BigInteger TraditionalBigIntegerFactorial()
    {
        return TraditionalBigIntegerFactorialImpl(FatorialInput);
    }

    [Benchmark]
    public BigInteger GenericBigIntegerFactorial()
    {
        return FatorialGenerico(new BigInteger(FatorialInput));
    }

    private static int TraditionalFactorialImpl(int n)
    {
        if (n <= 1) return 1;
        int result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    private static int TraditionalPowerImpl(int baseNum, int exponent)
    {
        if (exponent == 0) return 1;
        int result = 1;
        int currentBase = baseNum;
        int exp = exponent;
        
        while (exp > 0)
        {
            if ((exp & 1) == 1)
                result *= currentBase;
            currentBase *= currentBase;
            exp >>= 1;
        }
        return result;
    }

    private static BigInteger TraditionalBigIntegerFactorialImpl(int n)
    {
        if (n <= 1) return 1;
        BigInteger result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    static T FatorialGenerico<T>(T n) where T : INumber<T>
    {
        if (n <= T.One) return T.One;
        
        T resultado = T.One;
        T contador = T.One;
        
        while (contador <= n)
        {
            resultado *= contador;
            contador += T.One;
        }
        
        return resultado;
    }

    static T PotenciaGenerica<T>(T baseNum, int expoente) where T : INumber<T>
    {
        if (expoente == 0) return T.One;
        if (expoente == 1) return baseNum;
        
        T resultado = T.One;
        T baseAtual = baseNum;
        int exp = expoente;
        
        while (exp > 0)
        {
            if ((exp & 1) == 1)
                resultado *= baseAtual;
            baseAtual *= baseAtual;
            exp >>= 1;
        }
        
        return resultado;
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class StatisticsBenchmarks
{
    private readonly int[] _intData;
    private readonly double[] _doubleData;
    private readonly decimal[] _decimalData;

    public StatisticsBenchmarks()
    {
        var random = new Random(42);
        _intData = Enumerable.Range(1, 1000)
            .Select(_ => random.Next(1, 100))
            .ToArray();
        _doubleData = _intData.Select(x => (double)x + random.NextDouble()).ToArray();
        _decimalData = _intData.Select(x => (decimal)x + (decimal)random.NextDouble()).ToArray();
    }

    [Benchmark]
    public double TraditionalIntAverage()
    {
        return _intData.Average();
    }

    [Benchmark]
    public int GenericIntAverage()
    {
        return MediaGenerica(_intData);
    }

    [Benchmark]
    public double TraditionalDoubleAverage()
    {
        return _doubleData.Average();
    }

    [Benchmark]
    public double GenericDoubleAverage()
    {
        return MediaGenerica(_doubleData);
    }

    [Benchmark]
    public double TraditionalIntVariance()
    {
        double mean = _intData.Average();
        return _intData.Select(x => Math.Pow(x - mean, 2)).Average();
    }

    [Benchmark]
    public int GenericIntVariance()
    {
        return VarianciaGenerica(_intData);
    }

    [Benchmark]
    public double TraditionalDoubleVariance()
    {
        double mean = _doubleData.Average();
        return _doubleData.Select(x => Math.Pow(x - mean, 2)).Average();
    }

    [Benchmark]
    public double GenericDoubleVariance()
    {
        return VarianciaGenerica(_doubleData);
    }

    static T MediaGenerica<T>(T[] valores) where T : INumber<T>
    {
        if (valores.Length == 0) return T.Zero;
        
        T soma = T.Zero;
        foreach (var valor in valores)
            soma += valor;
            
        return soma / T.CreateChecked(valores.Length);
    }

    static T VarianciaGenerica<T>(T[] valores) where T : INumber<T>
    {
        if (valores.Length <= 1) return T.Zero;
        
        T media = MediaGenerica(valores);
        T somaQuadrados = T.Zero;
        
        foreach (var valor in valores)
        {
            T diferenca = valor - media;
            somaQuadrados += diferenca * diferenca;
        }
        
        return somaQuadrados / T.CreateChecked(valores.Length - 1);
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class GeometryBenchmarks
{
    private readonly (int X, int Y)[] _intPoints;
    private readonly (double X, double Y)[] _doublePoints;
    private readonly (float X, float Y)[] _floatPoints;

    public GeometryBenchmarks()
    {
        var random = new Random(42);
        _intPoints = Enumerable.Range(0, 1000)
            .Select(_ => (random.Next(-100, 100), random.Next(-100, 100)))
            .ToArray();
        _doublePoints = _intPoints
            .Select(p => ((double)p.X + random.NextDouble(), (double)p.Y + random.NextDouble()))
            .ToArray();
        _floatPoints = _intPoints
            .Select(p => ((float)p.X + (float)random.NextDouble(), (float)p.Y + (float)random.NextDouble()))
            .ToArray();
    }

    [Benchmark]
    public double TraditionalIntDistance()
    {
        double totalDistance = 0;
        foreach (var point in _intPoints)
        {
            totalDistance += Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }
        return totalDistance;
    }

    [Benchmark]
    public double GenericFloatDistance()
    {
        double totalDistance = 0;
        foreach (var point in _floatPoints)
        {
            var p = new Ponto2D<float>(point.X, point.Y);
            totalDistance += p.DistanciaOrigem();
        }
        return totalDistance;
    }

    [Benchmark]
    public double TraditionalDoubleDistance()
    {
        double totalDistance = 0;
        foreach (var point in _doublePoints)
        {
            totalDistance += Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }
        return totalDistance;
    }

    [Benchmark]
    public double GenericDoubleDistance()
    {
        double totalDistance = 0;
        foreach (var point in _doublePoints)
        {
            var p = new Ponto2D<double>(point.X, point.Y);
            totalDistance += p.DistanciaOrigem();
        }
        return totalDistance;
    }

    [Benchmark]
    public double TraditionalCircleArea()
    {
        double totalArea = 0;
        for (int i = 1; i <= 100; i++)
        {
            totalArea += Math.PI * i * i;
        }
        return totalArea;
    }

    [Benchmark]
    public int GenericIntCircleArea()
    {
        int totalArea = 0;
        for (int i = 1; i <= 100; i++)
        {
            var circle = new Circulo<int>(i);
            totalArea += circle.Area();
        }
        return totalArea;
    }

    [Benchmark]
    public double GenericDoubleCircleArea()
    {
        double totalArea = 0;
        for (int i = 1; i <= 100; i++)
        {
            var circle = new Circulo<double>(i);
            totalArea += circle.Area();
        }
        return totalArea;
    }
}

// ===== CLASSES AUXILIARES PARA BENCHMARK =====

public struct Ponto2D<T> where T : INumber<T>, IRootFunctions<T>
{
    public T X { get; }
    public T Y { get; }

    public Ponto2D(T x, T y)
    {
        X = x;
        Y = y;
    }

    public T DistanciaOrigem()
    {
        return T.Sqrt(X * X + Y * Y);
    }
}

public struct Circulo<T> where T : INumber<T>
{
    public T Raio { get; }

    public Circulo(T raio)
    {
        Raio = raio;
    }

    public T Area()
    {
        var pi = T.CreateChecked(Math.PI);
        return pi * Raio * Raio;
    }
}
