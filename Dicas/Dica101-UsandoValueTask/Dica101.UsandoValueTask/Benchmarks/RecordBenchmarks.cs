using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Dica101;

/// <summary>
/// Benchmark comparando Record vs Class tradicional
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class RecordBenchmarks
{
    private readonly Person[] _recordPersons;
    private readonly TraditionalPerson[] _classPersons;
    private readonly Point[] _recordStructs;
    private readonly Money[] _readonlyRecordStructs;

    public RecordBenchmarks()
    {
        // Dados de teste
        var names = new[]
        {
            ("João", "Silva", 25, "joao@test.com"),
            ("Maria", "Santos", 30, "maria@test.com"),
            ("Pedro", "Costa", 35, "pedro@test.com"),
            ("Ana", "Ferreira", 28, "ana@test.com"),
            ("Carlos", "Oliveira", 40, "carlos@test.com")
        };

        // Criar arrays para benchmark
        _recordPersons = names.Select(n => new Person(n.Item1, n.Item2, n.Item3, n.Item4)).ToArray();
        _classPersons = names.Select(n => new TraditionalPerson(n.Item1, n.Item2, n.Item3, n.Item4)).ToArray();
        
        _recordStructs = Enumerable.Range(0, 1000)
            .Select(i => new Point(i, i * 2))
            .ToArray();
            
        _readonlyRecordStructs = Enumerable.Range(0, 1000)
            .Select(i => new Money(i * 100m, "BRL"))
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Creation")]
    public Person[] CreateRecords()
    {
        var results = new Person[1000];
        for (int i = 0; i < 1000; i++)
        {
            var idx = i % 5;
            var source = _recordPersons[idx];
            results[i] = new Person(source.FirstName, source.LastName, source.Age, source.Email);
        }
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public TraditionalPerson[] CreateClasses()
    {
        var results = new TraditionalPerson[1000];
        for (int i = 0; i < 1000; i++)
        {
            var idx = i % 5;
            var source = _classPersons[idx];
            results[i] = new TraditionalPerson(source.FirstName, source.LastName, source.Age, source.Email);
        }
        return results;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Equality")]
    public bool RecordEquality()
    {
        var person1 = new Person("João", "Silva", 25, "joao@test.com");
        var person2 = new Person("João", "Silva", 25, "joao@test.com");
        var person3 = new Person("Maria", "Santos", 30, "maria@test.com");

        return person1 == person2 && person1 != person3;
    }

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool ClassEquality()
    {
        var person1 = new TraditionalPerson("João", "Silva", 25, "joao@test.com");
        var person2 = new TraditionalPerson("João", "Silva", 25, "joao@test.com");
        var person3 = new TraditionalPerson("Maria", "Santos", 30, "maria@test.com");

        return person1.Equals(person2) && !person1.Equals(person3);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Mutation")]
    public Person[] RecordMutation()
    {
        var results = new Person[_recordPersons.Length];
        for (int i = 0; i < _recordPersons.Length; i++)
        {
            // Usando expressão 'with' para criar cópia modificada
            results[i] = _recordPersons[i] with { Age = _recordPersons[i].Age + 1 };
        }
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Mutation")]
    public TraditionalPerson[] ClassMutation()
    {
        var results = new TraditionalPerson[_classPersons.Length];
        for (int i = 0; i < _classPersons.Length; i++)
        {
            // Usando método manual para criar cópia modificada
            results[i] = _classPersons[i].WithAge(_classPersons[i].Age + 1);
        }
        return results;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("RecordStruct")]
    public Point[] StructOperations()
    {
        var results = new Point[_recordStructs.Length];
        for (int i = 0; i < _recordStructs.Length; i++)
        {
            // Operações com record struct
            results[i] = _recordStructs[i].MoveBy(1.0, 1.0);
        }
        return results;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ReadonlyRecordStruct")]
    public Money[] ReadonlyStructOperations()
    {
        var results = new Money[_readonlyRecordStructs.Length];
        for (int i = 0; i < _readonlyRecordStructs.Length; i++)
        {
            // Operações com readonly record struct
            results[i] = _readonlyRecordStructs[i].Multiply(1.1m);
        }
        return results;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HashCode")]
    public int RecordHashCodes()
    {
        int result = 0;
        foreach (var person in _recordPersons)
        {
            result ^= person.GetHashCode();
        }
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("HashCode")]
    public int ClassHashCodes()
    {
        int result = 0;
        foreach (var person in _classPersons)
        {
            result ^= person.GetHashCode();
        }
        return result;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ToString")]
    public string[] RecordToString()
    {
        var results = new string[_recordPersons.Length];
        for (int i = 0; i < _recordPersons.Length; i++)
        {
            results[i] = _recordPersons[i].ToString();
        }
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ToString")]
    public string[] ClassToString()
    {
        var results = new string[_classPersons.Length];
        for (int i = 0; i < _classPersons.Length; i++)
        {
            results[i] = _classPersons[i].ToString();
        }
        return results;
    }
}
