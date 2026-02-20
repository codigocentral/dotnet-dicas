using System.Collections.Concurrent;

namespace Dica28.DotnetRetest;

/// <summary>
/// Serviço que simula operações que podem ser flaky
/// </summary>
public class DataProcessor
{
    private readonly Random _random = new();
    private readonly ConcurrentQueue<string> _processedItems = new();
    private bool _isCompleted;

    public bool IsCompleted => _isCompleted;
    public int ProcessedCount => _processedItems.Count;

    /// <summary>
    /// Processa dados de forma assíncrona - pode ser flaky dependendo do timing
    /// </summary>
    public async Task ProcessAsync(int itemCount = 5)
    {
        _isCompleted = false;
        
        var tasks = Enumerable.Range(1, itemCount)
            .Select(async i =>
            {
                // Simula trabalho com timing variável
                await Task.Delay(_random.Next(10, 100));
                _processedItems.Enqueue($"Item {i}");
            });

        await Task.WhenAll(tasks);
        
        // Simula delay adicional que pode causar race condition
        await Task.Delay(_random.Next(1, 50));
        _isCompleted = true;
    }

    /// <summary>
    /// Operação que falha intermitentemente para simular teste flaky
    /// </summary>
    public async Task<bool> FlakyOperationAsync()
    {
        await Task.Delay(_random.Next(1, 30));
        
        // 30% de chance de falha para simular comportamento flaky
        return _random.NextDouble() > 0.3;
    }

    /// <summary>
    /// Operação de rede simulada que pode falhar
    /// </summary>
    public async Task<string> SimulateNetworkCallAsync()
    {
        await Task.Delay(_random.Next(50, 200));
        
        // Simula falha de rede ocasional
        if (_random.NextDouble() < 0.2) // 20% de chance de falha
        {
            throw new HttpRequestException("Simulated network failure");
        }
        
        return "Network response data";
    }

    /// <summary>
    /// Limpa o estado do processador
    /// </summary>
    public void Reset()
    {
        _isCompleted = false;
        while (_processedItems.TryDequeue(out _)) { }
    }
}
