using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Dica36.ULIDs.Benchmark;

/// <summary>
/// Benchmarks comparando ULIDs com GUIDs em diferentes cenários
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Benchmark: ULIDs vs GUIDs ===\n");
        
        var summary = BenchmarkRunner.Run<UlidVsGuidBenchmarks>();
        
        Console.WriteLine("\nBenchmark concluído. Resultados salvos em BenchmarkDotNet.Artifacts/");
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class UlidVsGuidBenchmarks
{
    private readonly List<Ulid> _ulids = new();
    private readonly List<Guid> _guids = new();
    private const int CollectionSize = 10000;

    [GlobalSetup]
    public void Setup()
    {
        // Preencher coleções para testes de ordenação
        for (int i = 0; i < CollectionSize; i++)
        {
            _ulids.Add(Ulid.NewUlid());
            _guids.Add(Guid.NewGuid());
            
            // Pequeno delay para garantir ordem temporal nos ULIDs
            if (i % 1000 == 0)
                Thread.Sleep(1);
        }
    }

    [Benchmark(Description = "Geração de GUID")]
    public Guid CreateGuid()
    {
        return Guid.NewGuid();
    }

    [Benchmark(Description = "Geração de ULID")]
    public Ulid CreateUlid()
    {
        return Ulid.NewUlid();
    }

    [Benchmark(Description = "Conversão GUID para string")]
    public string GuidToString()
    {
        return Guid.NewGuid().ToString();
    }

    [Benchmark(Description = "Conversão ULID para string")]
    public string UlidToString()
    {
        return Ulid.NewUlid().ToString();
    }

    [Benchmark(Description = "Parse string para GUID")]
    public Guid ParseGuid()
    {
        return Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
    }

    [Benchmark(Description = "Parse string para ULID")]
    public Ulid ParseUlid()
    {
        return Ulid.Parse("01HHQK9V3X8N9J5KQQ8H8ZJQK9");
    }

    [Benchmark(Description = "Ordenação de GUIDs")]
    public List<Guid> SortGuids()
    {
        return _guids.OrderBy(g => g).ToList();
    }

    [Benchmark(Description = "Ordenação de ULIDs")]
    public List<Ulid> SortUlids()
    {
        return _ulids.OrderBy(u => u).ToList();
    }

    [Benchmark(Description = "Busca binária em GUIDs")]
    public bool BinarySearchGuid()
    {
        var sorted = _guids.OrderBy(g => g).ToArray();
        var target = sorted[sorted.Length / 2];
        return Array.BinarySearch(sorted, target) >= 0;
    }

    [Benchmark(Description = "Busca binária em ULIDs")]
    public bool BinarySearchUlid()
    {
        var sorted = _ulids.OrderBy(u => u).ToArray();
        var target = sorted[sorted.Length / 2];
        return Array.BinarySearch(sorted, target) >= 0;
    }

    [Benchmark(Description = "Conversão ULID para GUID")]
    public Guid UlidToGuid()
    {
        return Ulid.NewUlid().ToGuid();
    }

    [Benchmark(Description = "Criação de ULID de GUID")]
    public Ulid GuidToUlid()
    {
        return new Ulid(Guid.NewGuid());
    }

    [Benchmark(Description = "HashCode de GUID")]
    public int GuidHashCode()
    {
        return Guid.NewGuid().GetHashCode();
    }

    [Benchmark(Description = "HashCode de ULID")]
    public int UlidHashCode()
    {
        return Ulid.NewUlid().GetHashCode();
    }

    [Benchmark(Description = "Comparação de GUIDs")]
    public bool CompareGuids()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        return guid1.CompareTo(guid2) < 0;
    }

    [Benchmark(Description = "Comparação de ULIDs")]
    public bool CompareUlids()
    {
        var ulid1 = Ulid.NewUlid();
        var ulid2 = Ulid.NewUlid();
        return ulid1.CompareTo(ulid2) < 0;
    }
}

/// <summary>
/// Benchmark específico para cenários de banco de dados
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class DatabaseScenarioBenchmarks
{
    private readonly List<EntityWithGuid> _entitiesWithGuid = new();
    private readonly List<EntityWithUlid> _entitiesWithUlid = new();

    [Params(1000, 10000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _entitiesWithGuid.Clear();
        _entitiesWithUlid.Clear();

        for (int i = 0; i < EntityCount; i++)
        {
            _entitiesWithGuid.Add(new EntityWithGuid($"Entity {i}"));
            _entitiesWithUlid.Add(new EntityWithUlid($"Entity {i}"));
        }
    }

    [Benchmark(Description = "Ordenação de entidades por GUID")]
    public List<EntityWithGuid> SortEntitiesByGuid()
    {
        return _entitiesWithGuid.OrderBy(e => e.Id).ToList();
    }

    [Benchmark(Description = "Ordenação de entidades por ULID")]
    public List<EntityWithUlid> SortEntitiesByUlid()
    {
        return _entitiesWithUlid.OrderBy(e => e.Id).ToList();
    }

    [Benchmark(Description = "Busca de entidade por GUID")]
    public EntityWithGuid? FindEntityByGuid()
    {
        var targetId = _entitiesWithGuid[EntityCount / 2].Id;
        return _entitiesWithGuid.FirstOrDefault(e => e.Id == targetId);
    }

    [Benchmark(Description = "Busca de entidade por ULID")]
    public EntityWithUlid? FindEntityByUlid()
    {
        var targetId = _entitiesWithUlid[EntityCount / 2].Id;
        return _entitiesWithUlid.FirstOrDefault(e => e.Id == targetId);
    }

    [Benchmark(Description = "Agrupamento por prefixo de GUID")]
    public Dictionary<string, List<EntityWithGuid>> GroupGuidsByPrefix()
    {
        return _entitiesWithGuid
            .GroupBy(e => e.Id.ToString()[..8])
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    [Benchmark(Description = "Agrupamento por prefixo de ULID")]
    public Dictionary<string, List<EntityWithUlid>> GroupUlidsByPrefix()
    {
        return _entitiesWithUlid
            .GroupBy(e => e.Id.ToString()[..8])
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}

/// <summary>
/// Entidade exemplo usando GUID como chave primária
/// </summary>
public class EntityWithGuid
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public EntityWithGuid(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Entidade exemplo usando ULID como chave primária
/// </summary>
public class EntityWithUlid
{
    public Ulid Id { get; } = Ulid.NewUlid();
    public string Name { get; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public EntityWithUlid(string name)
    {
        Name = name;
    }
}
