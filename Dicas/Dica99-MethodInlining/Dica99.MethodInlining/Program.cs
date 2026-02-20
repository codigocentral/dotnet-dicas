using System.Runtime.CompilerServices;
using System.Diagnostics;

Console.WriteLine("=== Dica 99: Inlining de Métodos (Method Inlining) ===");
Console.WriteLine("Métodos pequenos são frequentemente 'inlineados' pelo JIT compiler.");
Console.WriteLine("Isso elimina a call stack e a sobrecarga, substituindo a chamada pelo corpo do método.");
Console.WriteLine("Use [MethodImpl(MethodImplOptions.AggressiveInlining)] para forçar inlining quando apropriado.");
Console.WriteLine();

// Demonstração prática de method inlining
var calculator = new MathCalculator();
var optimizedCalculator = new OptimizedMathCalculator();

Console.WriteLine("=== 1. Comparação Básica: Método Normal vs Inlined ===");

// Teste com método normal
var sw = Stopwatch.StartNew();
double result1 = 0;
for (int i = 0; i < 1_000_000; i++)
{
    result1 += calculator.Add(i, i + 1);
}
sw.Stop();
Console.WriteLine($"Método normal: {sw.ElapsedMilliseconds}ms (resultado: {result1})");

// Teste com método inlined
sw.Restart();
double result2 = 0;
for (int i = 0; i < 1_000_000; i++)
{
    result2 += optimizedCalculator.AddInlined(i, i + 1);
}
sw.Stop();
Console.WriteLine($"Método inlined: {sw.ElapsedMilliseconds}ms (resultado: {result2})");

Console.WriteLine();

// Demonstração de cenários práticos
Console.WriteLine("=== 2. Cenários Práticos de Uso ===");

var geometryHelper = new GeometryHelper();
var point1 = new Point(3, 4);
var point2 = new Point(6, 8);

Console.WriteLine($"Distância entre pontos: {geometryHelper.CalculateDistance(point1, point2):F2}");
Console.WriteLine($"Área do círculo (raio 5): {geometryHelper.CircleArea(5):F2}");
Console.WriteLine($"É número par? {geometryHelper.IsEven(42)}");

Console.WriteLine();

// Hot path demonstration
Console.WriteLine("=== 3. Demonstração de Hot Path ===");
HotPathDemo.ExecuteDemo();

Console.WriteLine();

// Exemplo de quando NÃO usar inlining
Console.WriteLine("=== 4. Quando NÃO Usar Inlining ===");
AntiPatternsDemo.DemonstrarAntiPatterns();

Console.WriteLine();

// Performance de validação inline
Console.WriteLine("=== 5. Validação com Inlining ===");
ValidadorDemo.ExecutarDemo();

public class MathCalculator
{
    // Método normal - JIT pode ou não fazer inline automaticamente
    public double Add(double a, double b)
    {
        return a + b;
    }

    public double Multiply(double a, double b)
    {
        return a * b;
    }

    public double Divide(double a, double b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return a / b;
    }
}

public class OptimizedMathCalculator
{
    // ✅ CORRETO: Força inlining para operações simples em hot paths
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double AddInlined(double a, double b)
    {
        return a + b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double MultiplyInlined(double a, double b)
    {
        return a * b;
    }

    // ✅ CORRETO: Inlining para validações simples
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValidNumber(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

public readonly record struct Point(double X, double Y);

public class GeometryHelper
{
    // ✅ CORRETO: Inlining para cálculos matemáticos pequenos
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double CalculateDistance(Point p1, Point p2)
    {
        var deltaX = p2.X - p1.X;
        var deltaY = p2.Y - p1.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double CircleArea(double radius)
    {
        return Math.PI * radius * radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEven(int number)
    {
        return (number & 1) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Square(double value)
    {
        return value * value;
    }
}

public static class HotPathDemo
{
    public static void ExecuteDemo()
    {
        const int iterations = 5_000_000;
        var data = Enumerable.Range(1, 1000).Select(x => (double)x).ToArray();

        // Processamento com métodos inline
        var sw = Stopwatch.StartNew();
        double sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            var index = i % data.Length;
            sum += ProcessValueInlined(data[index]);
        }
        sw.Stop();
        Console.WriteLine($"Hot path com inlining: {sw.ElapsedMilliseconds}ms (sum: {sum:F0})");

        // Processamento sem inline
        sw.Restart();
        sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            var index = i % data.Length;
            sum += ProcessValueNormal(data[index]);
        }
        sw.Stop();
        Console.WriteLine($"Hot path sem inlining: {sw.ElapsedMilliseconds}ms (sum: {sum:F0})");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ProcessValueInlined(double value)
    {
        return value * 1.1 + 0.5;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ProcessValueNormal(double value)
    {
        return value * 1.1 + 0.5;
    }
}

public static class AntiPatternsDemo
{
    public static void DemonstrarAntiPatterns()
    {
        Console.WriteLine("❌ EVITE inlining em métodos grandes:");
        
        // Este método é muito grande para inlining
        var result = ProcessLargeMethod(42);
        Console.WriteLine($"Resultado do método grande: {result}");
        
        // Este método tem muita lógica complexa
        var validation = ComplexValidation("test@email.com");
        Console.WriteLine($"Validação complexa: {validation}");
    }

    // ❌ INCORRETO: Método muito grande para inlining
    // [MethodImpl(MethodImplOptions.AggressiveInlining)] // NÃO faça isso!
    public static int ProcessLargeMethod(int input)
    {
        // Método muito grande - inlining pode prejudicar performance
        int result = input;
        
        for (int i = 0; i < 10; i++)
        {
            result = result * 2;
            if (result > 100)
            {
                result = result % 100;
            }
            result += i * 3;
        }
        
        // Mais lógica complexa...
        if (result > 50)
        {
            result = PerformComplexCalculation(result);
        }
        
        return result;
    }

    // ❌ INCORRETO: Método com muita lógica para inlining
    public static bool ComplexValidation(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        if (!email.Contains('@'))
            return false;
            
        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;
            
        // Mais validações...
        return parts[0].Length > 0 && parts[1].Contains('.');
    }

    private static int PerformComplexCalculation(int value)
    {
        return value * value + value - 10;
    }
}

public static class ValidadorDemo
{
    public static void ExecutarDemo()
    {
        const int iterations = 1_000_000;
        var emails = new[] { "test@email.com", "invalid", "user@domain.org", "", "bad@", "@domain.com" };

        // Validação com inlining
        var sw = Stopwatch.StartNew();
        int validCount = 0;
        for (int i = 0; i < iterations; i++)
        {
            var email = emails[i % emails.Length];
            if (IsValidEmailInlined(email))
                validCount++;
        }
        sw.Stop();
        Console.WriteLine($"Validação inlined: {sw.ElapsedMilliseconds}ms (válidos: {validCount})");

        // Validação sem inlining
        sw.Restart();
        validCount = 0;
        for (int i = 0; i < iterations; i++)
        {
            var email = emails[i % emails.Length];
            if (IsValidEmailNormal(email))
                validCount++;
        }
        sw.Stop();
        Console.WriteLine($"Validação normal: {sw.ElapsedMilliseconds}ms (válidos: {validCount})");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidEmailInlined(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.LastIndexOf('@') == email.IndexOf('@');
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsValidEmailNormal(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.IndexOf('@') > 0 && 
               email.LastIndexOf('@') == email.IndexOf('@');
    }
}

// Exemplo de biblioteca micro-otimizada
public static class MicroOptimizedHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextPowerOfTwo(int value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FastMod(uint value, uint divisor)
    {
        // Otimização para divisores que são potência de 2
        return value & (divisor - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp(double value, double min, double max)
    {
        return value < min ? min : (value > max ? max : value);
    }
}
