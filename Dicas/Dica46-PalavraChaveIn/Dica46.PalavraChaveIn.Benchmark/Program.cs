using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.CompilerServices;

Console.WriteLine("=== Benchmark: Palavra-chave 'in' vs Passagem por Valor ===");
Console.WriteLine("Executando benchmarks de performance...\n");

BenchmarkRunner.Run<InParameterBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class InParameterBenchmark
{
    private readonly LargeStruct _largeStruct;
    private readonly HugeStruct _hugeStruct;
    private readonly ReadonlyLargeStruct _readonlyStruct;
    private readonly MutableStruct _mutableStruct;
    
    public InParameterBenchmark()
    {
        _largeStruct = new LargeStruct();
        _largeStruct.Initialize();
        
        _hugeStruct = new HugeStruct();
        _hugeStruct.Initialize();
        
        _readonlyStruct = new ReadonlyLargeStruct(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        _mutableStruct = new MutableStruct { Value = 42, Factor = 2.5 };
    }
    
    [Benchmark(Baseline = true)]
    public double ProcessLargeStructByValue()
    {
        return ProcessByValue(_largeStruct);
    }
    
    [Benchmark]
    public double ProcessLargeStructByIn()
    {
        return ProcessByIn(in _largeStruct);
    }
    
    [Benchmark]
    public double ProcessHugeStructByValue()
    {
        return ProcessHugeByValue(_hugeStruct);
    }
    
    [Benchmark]
    public double ProcessHugeStructByIn()
    {
        return ProcessHugeByIn(in _hugeStruct);
    }
    
    [Benchmark]
    public double ProcessReadonlyStructByValue()
    {
        return ProcessReadonlyByValue(_readonlyStruct);
    }
    
    [Benchmark]
    public double ProcessReadonlyStructByIn()
    {
        return ProcessReadonlyByIn(in _readonlyStruct);
    }
    
    [Benchmark]
    public double ProcessMutableStructByIn()
    {
        // Este pode gerar cópias defensivas
        return ProcessMutableByIn(in _mutableStruct);
    }
    
    [Benchmark]
    public double VectorCalculationsByValue()
    {
        var vector1 = new Vector3D(1, 2, 3);
        var vector2 = new Vector3D(4, 5, 6);
        
        return CalculateDotProductByValue(vector1, vector2) +
               CalculateDistanceByValue(vector1, vector2);
    }
    
    [Benchmark]
    public double VectorCalculationsByIn()
    {
        var vector1 = new Vector3D(1, 2, 3);
        var vector2 = new Vector3D(4, 5, 6);
        
        return CalculateDotProductByIn(in vector1, in vector2) +
               CalculateDistanceByIn(in vector1, in vector2);
    }
    
    [Benchmark]
    public double MatrixOperationsByValue()
    {
        var matrix = new Matrix4x4();
        matrix.InitializeWithValues(2.0f);
        
        double sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            sum += CalculateMatrixSumByValue(matrix);
        }
        return sum;
    }
    
    [Benchmark]
    public double MatrixOperationsByIn()
    {
        var matrix = new Matrix4x4();
        matrix.InitializeWithValues(2.0f);
        
        double sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            sum += CalculateMatrixSumByIn(in matrix);
        }
        return sum;
    }
    
    // ===== MÉTODOS DE PROCESSAMENTO =====
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessByValue(LargeStruct data)
    {
        return data.Field1 + data.Field10 + data.Field20 + data.Field30;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessByIn(in LargeStruct data)
    {
        return data.Field1 + data.Field10 + data.Field20 + data.Field30;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessHugeByValue(HugeStruct data)
    {
        double sum = 0;
        for (int i = 0; i < 64; i++)
        {
            sum += data.Values[i];
        }
        return sum;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessHugeByIn(in HugeStruct data)
    {
        double sum = 0;
        for (int i = 0; i < 64; i++)
        {
            sum += data.Values[i];
        }
        return sum;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessReadonlyByValue(ReadonlyLargeStruct data)
    {
        return data.Field1 + data.Field5 + data.Field10;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessReadonlyByIn(in ReadonlyLargeStruct data)
    {
        return data.Field1 + data.Field5 + data.Field10;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessMutableByIn(in MutableStruct data)
    {
        // Pode gerar cópias defensivas ao acessar propriedades
        return data.Value * data.Factor + data.CalculateSquare();
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double CalculateDotProductByValue(Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double CalculateDotProductByIn(in Vector3D a, in Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double CalculateDistanceByValue(Vector3D a, Vector3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double CalculateDistanceByIn(in Vector3D a, in Vector3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float CalculateMatrixSumByValue(Matrix4x4 matrix)
    {
        return matrix.M11 + matrix.M12 + matrix.M13 + matrix.M14 +
               matrix.M21 + matrix.M22 + matrix.M23 + matrix.M24 +
               matrix.M31 + matrix.M32 + matrix.M33 + matrix.M34 +
               matrix.M41 + matrix.M42 + matrix.M43 + matrix.M44;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float CalculateMatrixSumByIn(in Matrix4x4 matrix)
    {
        return matrix.M11 + matrix.M12 + matrix.M13 + matrix.M14 +
               matrix.M21 + matrix.M22 + matrix.M23 + matrix.M24 +
               matrix.M31 + matrix.M32 + matrix.M33 + matrix.M34 +
               matrix.M41 + matrix.M42 + matrix.M43 + matrix.M44;
    }
}

// ===== DEFINIÇÃO DAS STRUCTS PARA BENCHMARK =====

public struct LargeStruct
{
    public double Field1, Field2, Field3, Field4, Field5;
    public double Field6, Field7, Field8, Field9, Field10;
    public double Field11, Field12, Field13, Field14, Field15;
    public double Field16, Field17, Field18, Field19, Field20;
    public double Field21, Field22, Field23, Field24, Field25;
    public double Field26, Field27, Field28, Field29, Field30;
    
    public void Initialize()
    {
        Field1 = Field2 = Field3 = Field4 = Field5 = 1.0;
        Field6 = Field7 = Field8 = Field9 = Field10 = 2.0;
        Field11 = Field12 = Field13 = Field14 = Field15 = 3.0;
        Field16 = Field17 = Field18 = Field19 = Field20 = 4.0;
        Field21 = Field22 = Field23 = Field24 = Field25 = 5.0;
        Field26 = Field27 = Field28 = Field29 = Field30 = 6.0;
    }
}

public struct HugeStruct
{
    public readonly double[] Values;
    
    public HugeStruct()
    {
        Values = new double[64]; // 512 bytes + array overhead
    }
    
    public void Initialize()
    {
        for (int i = 0; i < Values.Length; i++)
        {
            Values[i] = i * 1.5;
        }
    }
}

public readonly struct ReadonlyLargeStruct
{
    public double Field1 { get; }
    public double Field2 { get; }
    public double Field3 { get; }
    public double Field4 { get; }
    public double Field5 { get; }
    public double Field6 { get; }
    public double Field7 { get; }
    public double Field8 { get; }
    public double Field9 { get; }
    public double Field10 { get; }
    
    public ReadonlyLargeStruct(double f1, double f2, double f3, double f4, double f5,
                              double f6, double f7, double f8, double f9, double f10)
    {
        Field1 = f1; Field2 = f2; Field3 = f3; Field4 = f4; Field5 = f5;
        Field6 = f6; Field7 = f7; Field8 = f8; Field9 = f9; Field10 = f10;
    }
}

public struct MutableStruct
{
    public int Value { get; set; }
    public double Factor { get; set; }
    
    public readonly double CalculateSquare()
    {
        return Value * Value;
    }
}

public readonly struct Vector3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public struct Matrix4x4
{
    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;
    public float M41, M42, M43, M44;
    
    public void InitializeWithValues(float value)
    {
        M11 = M12 = M13 = M14 = value;
        M21 = M22 = M23 = M24 = value;
        M31 = M32 = M33 = M34 = value;
        M41 = M42 = M43 = M44 = value;
    }
}
