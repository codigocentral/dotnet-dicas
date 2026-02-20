namespace Dica77.BlazorPerformance;

public class ListItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class DataService
{
    private readonly List<ListItem> _cachedItems = new();
    private readonly Random _random = new();

    public DataService()
    {
        // Pre-gera alguns itens para cache
        GenerateItems(1000);
    }

    public ListItem GetItem(int index)
    {
        if (index < _cachedItems.Count)
        {
            return _cachedItems[index];
        }

        // Gera item dinamicamente se não estiver no cache
        return new ListItem
        {
            Id = index,
            Description = $"Dynamic Item {index} - {GetRandomText()}",
            Timestamp = DateTime.Now.AddMinutes(-_random.Next(0, 1440)),
            Category = GetRandomCategory(),
            IsActive = _random.NextDouble() > 0.3
        };
    }

    public IEnumerable<ListItem> GetVirtualizedItems(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            yield return new ListItem
            {
                Id = i,
                Description = $"Virtualized Item {i} - {GetRandomText()}",
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(0, 1440)),
                Category = GetRandomCategory(),
                IsActive = _random.NextDouble() > 0.3
            };
        }
    }

    public async Task<IEnumerable<ListItem>> GetItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        // Simula operação assíncrona
        await Task.Delay(100, cancellationToken);
        
        return Enumerable.Range(0, count)
            .Select(i => GetItem(i))
            .ToList();
    }

    private void GenerateItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _cachedItems.Add(new ListItem
            {
                Id = i,
                Description = $"Cached Item {i} - {GetRandomText()}",
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(0, 1440)),
                Category = GetRandomCategory(),
                IsActive = _random.NextDouble() > 0.3
            });
        }
    }

    private string GetRandomText()
    {
        var texts = new[]
        {
            "High Performance", "Optimized Rendering", "Blazor WebAssembly",
            "Virtual Scrolling", "Component Caching", "Memory Efficient",
            "Fast Loading", "Responsive UI", "Modern Web", "Cross Platform"
        };
        return texts[_random.Next(texts.Length)];
    }

    private string GetRandomCategory()
    {
        var categories = new[] { "Performance", "UI", "Data", "Network", "Cache" };
        return categories[_random.Next(categories.Length)];
    }
}
