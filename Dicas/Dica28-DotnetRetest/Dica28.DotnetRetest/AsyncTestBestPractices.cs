namespace Dica28.DotnetRetest;

/// <summary>
/// Demonstra o uso correto de testes assíncronos
/// Evitando async void que é uma armadilha comum
/// </summary>
public class AsyncTestBestPractices
{
    /// <summary>
    /// ❌ NUNCA use async void em testes - o test runner não pode rastreá-los
    /// </summary>
    /*
    [Fact]
    public async void AsyncVoid_BadPractice_DontDoThis()
    {
        var processor = new DataProcessor();
        await processor.ProcessAsync();
        // Test runner pode não aguardar este teste terminar
    }
    */

    /// <summary>
    /// ✅ Sempre retorne Task em testes assíncronos
    /// </summary>
    [Fact]
    public async Task AsyncTask_GoodPractice_AlwaysDoThis()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        await processor.ProcessAsync();

        // Assert
        Assert.True(processor.IsCompleted);
    }

    /// <summary>
    /// ✅ Teste com timeout usando CancellationToken
    /// </summary>
    [Fact]
    public async Task AsyncWithTimeout_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var processor = new DataProcessor();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act & Assert
        var task = processor.ProcessAsync();
        await task.WaitAsync(cts.Token);
        
        Assert.True(processor.IsCompleted);
    }

    /// <summary>
    /// ✅ Teste de operações paralelas
    /// </summary>
    [Fact]
    public async Task ParallelOperations_ShouldAllComplete()
    {
        // Arrange
        var processors = Enumerable.Range(1, 3)
            .Select(_ => new DataProcessor())
            .ToArray();

        // Act
        var tasks = processors.Select(p => p.ProcessAsync(2));
        await Task.WhenAll(tasks);

        // Assert
        foreach (var processor in processors)
        {
            Assert.True(processor.IsCompleted);
            Assert.Equal(2, processor.ProcessedCount);
        }
    }

    /// <summary>
    /// ✅ Teste simples sem ConfigureAwait (recomendado para testes)
    /// Note: Em testes, geralmente não precisamos de ConfigureAwait(false)
    /// </summary>
    [Fact]
    public async Task SimpleAsync_ShouldComplete()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        await processor.ProcessAsync();

        // Assert
        Assert.True(processor.IsCompleted);
    }

    /// <summary>
    /// ✅ Teste com múltiplas operações assíncronas em sequência
    /// </summary>
    [Fact]
    public async Task SequentialAsyncOperations_ShouldMaintainOrder()
    {
        // Arrange
        var processor = new DataProcessor();
        var results = new List<bool>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            processor.Reset();
            await processor.ProcessAsync(1);
            results.Add(processor.IsCompleted);
        }

        // Assert
        Assert.All(results, result => Assert.True(result));
        Assert.Equal(3, results.Count);
    }
}
