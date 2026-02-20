using System.Numerics;

namespace Dica49.StaticAbstractMembers;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("==== Dica 49: Static Abstract Members - Recursos Modernos C# 11+ ====\n");

        Console.WriteLine("🚀 Static Abstract Members permitem definir contratos estáticos em interfaces!");
        Console.WriteLine("Revolucionam Generic Math e Operadores Customizados...\n");

        // ===== DEMONSTRAÇÃO 1: GENERIC MATH COM STATIC ABSTRACT MEMBERS =====
        Console.WriteLine("1. Generic Math - Operações Matemáticas Genéricas");
        Console.WriteLine("---------------------------------------------------");

        // Exemplo com diferentes tipos numéricos
        var intResult = Calculator.Add(10, 20);
        var doubleResult = Calculator.Add(3.14, 2.86);
        var decimalResult = Calculator.Add(100.50m, 50.25m);

        Console.WriteLine($"✅ int: {intResult}");
        Console.WriteLine($"✅ double: {doubleResult:F2}");
        Console.WriteLine($"✅ decimal: {decimalResult}");

        // Operações mais complexas
        var intValues = new[] { 1, 2, 3, 4, 5 };
        var doubleValues = new[] { 1.1, 2.2, 3.3, 4.4, 5.5 };

        Console.WriteLine($"\n📊 Soma array int: {Calculator.Sum(intValues)}");
        Console.WriteLine($"📊 Soma array double: {Calculator.Sum(doubleValues):F1}");
        Console.WriteLine($"📊 Média array int: {Calculator.Average(intValues):F2}");
        Console.WriteLine($"📊 Média array double: {Calculator.Average(doubleValues):F2}\n");

        // ===== DEMONSTRAÇÃO 2: OPERADORES CUSTOMIZADOS =====
        Console.WriteLine("2. Operadores Customizados com Static Abstract");
        Console.WriteLine("----------------------------------------------");

        var vector1 = new Vector3D(1, 2, 3);
        var vector2 = new Vector3D(4, 5, 6);

        Console.WriteLine($"Vector1: {vector1}");
        Console.WriteLine($"Vector2: {vector2}");
        Console.WriteLine($"Soma: {vector1 + vector2}");
        Console.WriteLine($"Produto escalar: {Vector3D.DotProduct(vector1, vector2)}");
        Console.WriteLine($"Magnitude Vector1: {Vector3D.Magnitude(vector1):F2}");

        // Usando operações genéricas de forma mais simples
        var vectorSum = vector1 + vector2;
        var vectorScaled = vector1 * 2.5;

        Console.WriteLine($"✅ Soma simples: {vectorSum}");
        Console.WriteLine($"✅ Escala simples: {vectorScaled}\n");

        // ===== DEMONSTRAÇÃO 3: FACTORY PATTERN COM STATIC ABSTRACT =====
        Console.WriteLine("3. Factory Pattern com Static Abstract Members");
        Console.WriteLine("-----------------------------------------------");

        // Criação de diferentes parsers usando static abstract
        var csvData = "John,25,Engineer\nJane,30,Designer";
        var jsonData = """{"name": "Bob", "age": 35, "role": "Manager"}""";

        var csvPersons = CsvPersonParser.ParseMultipleToPersons(csvData);
        var jsonPerson = JsonPersonParser.ParseToPerson(jsonData);

        Console.WriteLine("📄 Dados CSV:");
        foreach (var person in csvPersons)
        {
            Console.WriteLine($"   • {person}");
        }

        Console.WriteLine($"\n📄 Dados JSON: {jsonPerson}\n");

        // ===== DEMONSTRAÇÃO 4: CUSTOM COLLECTIONS COM STATIC ABSTRACT =====
        Console.WriteLine("4. Custom Collections com Static Abstract");
        Console.WriteLine("------------------------------------------");

        // Demonstrando diferentes estratégias de criação
        var numberList = CollectionFactory.CreateAndFill<NumberList, int>([1, 2, 3, 4, 5]);
        var stringSet = CollectionFactory.CreateAndFill<StringSet, string>(["apple", "banana", "cherry", "apple"]);

        Console.WriteLine($"NumberList: {numberList}");
        Console.WriteLine($"StringSet: {stringSet}");

        // Operações genéricas em collections
        Console.WriteLine($"NumberList contém 3? {numberList.Contains(3)}");
        Console.WriteLine($"StringSet contém 'banana'? {stringSet.Contains("banana")}");
        Console.WriteLine($"StringSet size: {stringSet.Count}\n");

        // ===== DEMONSTRAÇÃO 5: PERFORMANCE COMPARISON =====
        Console.WriteLine("5. Comparação de Performance - Static Abstract vs Alternativas");
        Console.WriteLine("---------------------------------------------------------------");

        const int iterations = 1_000_000;

        // Static Abstract Members (mais eficiente)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long staticSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            staticSum += Calculator.Add(i, i + 1);
        }
        sw.Stop();
        var staticTime = sw.ElapsedMilliseconds;

        // Interface tradicional (menos eficiente)
        sw.Restart();
        ICalculator calc = new TraditionalCalculator();
        long interfaceSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            interfaceSum += calc.Add(i, i + 1);
        }
        sw.Stop();
        var interfaceTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"✅ Static Abstract: {staticTime} ms (soma: {staticSum:N0})");
        Console.WriteLine($"❌ Interface tradicional: {interfaceTime} ms (soma: {interfaceSum:N0})");
        Console.WriteLine($"⚡ Melhoria: {(double)interfaceTime / staticTime:F1}x mais rápido!\n");

        // ===== DEMONSTRAÇÃO 6: CONSTRAINTS COM STATIC ABSTRACT =====
        Console.WriteLine("6. Generic Constraints com Static Abstract Members");
        Console.WriteLine("---------------------------------------------------");

        // Exemplos de algorithms que funcionam com qualquer tipo que implementa os operadores necessários
        var matrix1 = new Matrix2x2<int>(1, 2, 3, 4);
        var matrix2 = new Matrix2x2<int>(5, 6, 7, 8);

        Console.WriteLine($"Matrix1:\n{matrix1}");
        Console.WriteLine($"Matrix2:\n{matrix2}");
        Console.WriteLine($"Soma das matrizes:\n{matrix1 + matrix2}");

        var doubleMatrix1 = new Matrix2x2<double>(1.5, 2.5, 3.5, 4.5);
        var doubleMatrix2 = new Matrix2x2<double>(0.5, 1.0, 1.5, 2.0);

        Console.WriteLine($"Double Matrix Soma:\n{doubleMatrix1 + doubleMatrix2}");

        Console.WriteLine("\n=== RESUMO: Static Abstract Members ===");
        Console.WriteLine("✅ VANTAGENS:");
        Console.WriteLine("   • Performance superior (sem virtual calls)");
        Console.WriteLine("   • Generic Math nativo no .NET");
        Console.WriteLine("   • Operadores customizados type-safe");
        Console.WriteLine("   • Factory patterns mais elegantes");
        Console.WriteLine("   • Constraints mais expressivos");
        Console.WriteLine();
        Console.WriteLine("🎯 CASOS DE USO:");
        Console.WriteLine("   • Bibliotecas matemáticas");
        Console.WriteLine("   • Operadores customizados");
        Console.WriteLine("   • Factory patterns");
        Console.WriteLine("   • Parsers genéricos");
        Console.WriteLine("   • Algorithms numericos");
        Console.WriteLine();
        Console.WriteLine("⚠️  REQUISITOS:");
        Console.WriteLine("   • C# 11+ (.NET 7+)");
        Console.WriteLine("   • Interfaces com static abstract members");
        Console.WriteLine("   • Generic constraints apropriados");
    }
}

// ===== IMPLEMENTAÇÕES DAS CLASSES E INTERFACES =====

// Calculator genérico usando constraints
public static class Calculator
{
    public static T Add<T>(T left, T right) 
        where T : IAdditionOperators<T, T, T>
        => left + right;

    public static T Sum<T>(IEnumerable<T> values) 
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
        => values.Aggregate(T.AdditiveIdentity, (acc, val) => acc + val);

    public static double Average<T>(IEnumerable<T> values) 
        where T : INumber<T>
        => double.CreateChecked(Sum(values)) / values.Count();
}

// Vector 3D com operadores customizados
public readonly record struct Vector3D(double X, double Y, double Z)
{
    public static Vector3D operator +(Vector3D left, Vector3D right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static Vector3D operator *(Vector3D vector, double scalar)
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    public static double DotProduct(Vector3D left, Vector3D right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    public static double Magnitude(Vector3D vector)
        => Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

    public override string ToString() => $"({X:F1}, {Y:F1}, {Z:F1})";
}

// Factory pattern com static abstract
public interface IParseable<TSelf> where TSelf : IParseable<TSelf>
{
    static abstract TSelf Parse(string data);
    static abstract IEnumerable<TSelf> ParseMultiple(string data);
}

public static class DataParser
{
    public static T ParseSingle<T>(string data) where T : IParseable<T>
        => T.Parse(data);

    public static IEnumerable<T> ParseMultiple<T>(string data) where T : IParseable<T>
        => T.ParseMultiple(data);
}

// Implementações concretas dos parsers
public record Person(string Name, int Age, string Role);

public readonly struct CsvPersonParser : IParseable<CsvPersonParser>
{
    public static CsvPersonParser Parse(string data)
    {
        return new CsvPersonParser();
    }

    public static IEnumerable<CsvPersonParser> ParseMultiple(string data)
        => data.Split('\n').Select(Parse);
    
    public static Person ParseToPerson(string data)
    {
        var parts = data.Split(',');
        return new Person(parts[0], int.Parse(parts[1]), parts[2]);
    }
    
    public static IEnumerable<Person> ParseMultipleToPersons(string data)
        => data.Split('\n').Select(ParseToPerson);
}

public readonly struct JsonPersonParser : IParseable<JsonPersonParser>
{
    public static JsonPersonParser Parse(string data)
    {
        return new JsonPersonParser();
    }

    public static IEnumerable<JsonPersonParser> ParseMultiple(string data)
        => [Parse(data)];
    
    public static Person ParseToPerson(string data)
    {
        // Simplified JSON parsing for demo
        var lines = data.Trim('{', '}').Split(',');
        var name = lines[0].Split(':')[1].Trim().Trim('"');
        var age = int.Parse(lines[1].Split(':')[1].Trim());
        var role = lines[2].Split(':')[1].Trim().Trim('"');
        return new Person(name, age, role);
    }
}

// Collections com static abstract
public interface ICreatable<TSelf, T> where TSelf : ICreatable<TSelf, T>
{
    static abstract TSelf Create();
    static abstract TSelf CreateFrom(IEnumerable<T> items);
    bool Contains(T item);
    int Count { get; }
}

public static class CollectionFactory
{
    public static TSelf CreateAndFill<TSelf, T>(IEnumerable<T> items) 
        where TSelf : ICreatable<TSelf, T>
        => TSelf.CreateFrom(items);
}

// Implementações de collections
public class NumberList : ICreatable<NumberList, int>
{
    private readonly List<int> _items = [];

    public static NumberList Create() => new();
    
    public static NumberList CreateFrom(IEnumerable<int> items)
    {
        var list = new NumberList();
        list._items.AddRange(items);
        return list;
    }

    public bool Contains(int item) => _items.Contains(item);
    public int Count => _items.Count;
    public override string ToString() => $"[{string.Join(", ", _items)}]";
}

public class StringSet : ICreatable<StringSet, string>
{
    private readonly HashSet<string> _items = [];

    public static StringSet Create() => new();
    
    public static StringSet CreateFrom(IEnumerable<string> items)
    {
        var set = new StringSet();
        foreach (var item in items) set._items.Add(item);
        return set;
    }

    public bool Contains(string item) => _items.Contains(item);
    public int Count => _items.Count;
    public override string ToString() => $"{{{string.Join(", ", _items)}}}";
}

// Interface tradicional para comparação de performance
public interface ICalculator
{
    int Add(int left, int right);
}

public class TraditionalCalculator : ICalculator
{
    public int Add(int left, int right) => left + right;
}

// Matrix com operadores genéricos
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

    public override string ToString()
        => $"[{A11} {A12}]\n[{A21} {A22}]";
}
