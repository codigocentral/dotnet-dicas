using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Numerics;

namespace Dica49.StaticAbstractMembers.Benchmark;

[MemoryDiagnoser]
[SimpleJob]
public class StaticAbstractMembersBenchmarks
{
    private readonly int[] _intArray = Enumerable.Range(1, 10000).ToArray();
    private readonly double[] _doubleArray = Enumerable.Range(1, 10000).Select(x => (double)x).ToArray();
    private readonly Vector3D[] _vectorArray = Enumerable.Range(1, 1000).Select(i => new Vector3D(i, i * 2, i * 3)).ToArray();
    
    [Benchmark(Baseline = true)]
    public long DirectSum()
    {
        long sum = 0;
        for (int i = 0; i < _intArray.Length; i++)
        {
            sum += _intArray[i];
        }
        return sum;
    }

    [Benchmark]
    public long StaticAbstractSum()
    {
        return Calculator.Sum(_intArray.Select(x => (long)x));
    }

    [Benchmark]
    public long InterfaceSum()
    {
        ICalculator calc = new TraditionalCalculator();
        long sum = 0;
        for (int i = 0; i < _intArray.Length; i++)
        {
            sum += calc.Add(_intArray[i], 0);
        }
        return sum;
    }

    [Benchmark]
    public double DirectDoubleSum()
    {
        double sum = 0;
        for (int i = 0; i < _doubleArray.Length; i++)
        {
            sum += _doubleArray[i];
        }
        return sum;
    }

    [Benchmark]
    public double StaticAbstractDoubleSum()
    {
        return Calculator.Sum(_doubleArray);
    }

    [Benchmark]
    public Vector3D DirectVectorSum()
    {
        var sum = new Vector3D(0, 0, 0);
        for (int i = 0; i < _vectorArray.Length; i++)
        {
            sum = new Vector3D(
                sum.X + _vectorArray[i].X,
                sum.Y + _vectorArray[i].Y,
                sum.Z + _vectorArray[i].Z
            );
        }
        return sum;
    }

    [Benchmark]
    public Vector3D StaticAbstractVectorSum()
    {
        return VectorOperations.Sum(_vectorArray);
    }

    [Benchmark]
    public int MatrixOperationsDirect()
    {
        int sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var matrix1 = new Matrix2x2<int>(i, i + 1, i + 2, i + 3);
            var matrix2 = new Matrix2x2<int>(i + 4, i + 5, i + 6, i + 7);
            var result = matrix1 + matrix2;
            sum += result.A11 + result.A12 + result.A21 + result.A22;
        }
        return sum;
    }

    [Benchmark]
    public NumberList CollectionCreationDirect()
    {
        var items = Enumerable.Range(1, 100);
        return NumberList.CreateFrom(items);
    }

    [Benchmark]
    public NumberList CollectionCreationFactory()
    {
        var items = Enumerable.Range(1, 100);
        return CollectionFactory.CreateAndFill<NumberList, int>(items);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Dica 49: Static Abstract Members - Performance Benchmarks ===\n");
        
        var summary = BenchmarkRunner.Run<StaticAbstractMembersBenchmarks>();
        
        Console.WriteLine("\n=== ANÁLISE DOS RESULTADOS ===");
        Console.WriteLine("✅ Static Abstract Members oferecem:");
        Console.WriteLine("   • Performance comparável ao código direto");
        Console.WriteLine("   • Maior type safety que interfaces tradicionais");
        Console.WriteLine("   • Zero overhead para operações genéricas");
        Console.WriteLine("   • Reutilização de código sem custos de performance");
        Console.WriteLine();
        Console.WriteLine("🎯 RECOMENDAÇÕES:");
        Console.WriteLine("   • Use para generic math e operadores");
        Console.WriteLine("   • Prefira sobre interfaces tradicionais para performance");
        Console.WriteLine("   • Combine com System.Numerics.INumber<T>");
        Console.WriteLine("   • Ideal para bibliotecas matemáticas");
    }
}

// Classes auxiliares para os benchmarks
public static class VectorOperations
{
    public static Vector3D Sum(IEnumerable<Vector3D> vectors)
    {
        var sum = new Vector3D(0, 0, 0);
        foreach (var vector in vectors)
        {
            sum = sum + vector;
        }
        return sum;
    }
}

// Calculator genérico usando constraints
public static class Calculator
{
    public static T Add<T>(T left, T right) 
        where T : IAdditionOperators<T, T, T>
        => left + right;

    public static T Sum<T>(IEnumerable<T> values) 
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
        => values.Aggregate(T.AdditiveIdentity, (acc, val) => acc + val);
}

public readonly record struct Vector3D(double X, double Y, double Z)
{
    public static Vector3D operator +(Vector3D left, Vector3D right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
}

public readonly struct Matrix2x2<T> where T : INumber<T>
{
    public readonly T A11, A12, A21, A22;

    public Matrix2x2(T a11, T a12, T a21, T a22)
    {
        A11 = a11; A12 = a12; A21 = a21; A22 = a22;
    }

    public static Matrix2x2<T> operator +(Matrix2x2<T> left, Matrix2x2<T> right)
        => new(left.A11 + right.A11, left.A12 + right.A12, 
               left.A21 + right.A21, left.A22 + right.A22);
}

// Collections com static abstract
public interface ICreatable<TSelf, T> where TSelf : ICreatable<TSelf, T>
{
    static abstract TSelf CreateFrom(IEnumerable<T> items);
}

public static class CollectionFactory
{
    public static TSelf CreateAndFill<TSelf, T>(IEnumerable<T> items) 
        where TSelf : ICreatable<TSelf, T>
        => TSelf.CreateFrom(items);
}

public class NumberList : ICreatable<NumberList, int>
{
    private readonly List<int> _items = [];

    public static NumberList CreateFrom(IEnumerable<int> items)
    {
        var list = new NumberList();
        list._items.AddRange(items);
        return list;
    }
}

public interface ICalculator
{
    int Add(int left, int right);
}

public class TraditionalCalculator : ICalculator
{
    public int Add(int left, int right) => left + right;
}
