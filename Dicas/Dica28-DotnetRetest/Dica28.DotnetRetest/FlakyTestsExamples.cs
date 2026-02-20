using FluentAssertions;

namespace Dica28.DotnetRetest;

/// <summary>
/// Testes que demonstram problemas flaky e suas correções
/// Para testar com dotnet retest, execute:
/// dotnet retest --retry-count 5 --verbose
/// </summary>
public class FlakyTestsExamples
{
    /// <summary>
    /// ❌ Teste flaky - race condition
    /// Este teste pode falhar porque verifica IsCompleted antes da operação terminar
    /// </summary>
    [Fact]
    [Trait("Category", "Flaky")]
    public async Task ProcessData_FlakyTest_MayFail()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        var task = processor.ProcessAsync(3);
        
        // Wait a bit but not enough to guarantee completion
        await Task.Delay(50);

        // Assert - pode falhar devido a race condition
        processor.IsCompleted.Should().BeTrue("o processamento deveria estar completo");
        
        // Aguarda para não deixar tarefas pendentes
        await task;
    }

    /// <summary>
    /// ✅ Teste corrigido - aguarda a conclusão adequadamente
    /// </summary>
    [Fact]
    [Trait("Category", "Stable")]
    public async Task ProcessData_FixedTest_ShouldPass()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        await processor.ProcessAsync(3);

        // Assert
        processor.IsCompleted.Should().BeTrue();
        processor.ProcessedCount.Should().Be(3);
    }

    /// <summary>
    /// ❌ Teste flaky - operação que falha intermitentemente
    /// </summary>
    [Fact]
    [Trait("Category", "Flaky")]
    public async Task FlakyOperation_MayFail()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        var result = await processor.FlakyOperationAsync();

        // Assert - vai falhar ~30% das vezes
        result.Should().BeTrue("a operação deveria ter sucesso");
    }

    /// <summary>
    /// ✅ Teste com retry manual - mais resiliente
    /// </summary>
    [Fact]
    [Trait("Category", "Resilient")]
    public async Task FlakyOperation_WithRetry_ShouldEventuallyPass()
    {
        // Arrange
        var processor = new DataProcessor();
        var maxAttempts = 5;
        var success = false;

        // Act & Assert
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                success = await processor.FlakyOperationAsync();
                if (success) break;
            }
            catch
            {
                if (attempt == maxAttempts) throw;
                await Task.Delay(100); // Wait before retry
            }
        }

        success.Should().BeTrue("a operação deveria ter sucesso após várias tentativas");
    }

    /// <summary>
    /// ❌ Teste flaky - falha de rede simulada
    /// </summary>
    [Fact]
    [Trait("Category", "Flaky")]
    public async Task NetworkCall_FlakyTest_MayFail()
    {
        // Arrange
        var processor = new DataProcessor();

        // Act
        var result = await processor.SimulateNetworkCallAsync();

        // Assert - pode falhar com HttpRequestException
        result.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// ✅ Teste com timeout e tratamento de erro
    /// </summary>
    [Fact]
    [Trait("Category", "Stable")]
    public async Task NetworkCall_WithTimeout_ShouldHandleFailures()
    {
        // Arrange
        var processor = new DataProcessor();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        try
        {
            var result = await processor.SimulateNetworkCallAsync();
            result.Should().NotBeNullOrEmpty();
        }
        catch (HttpRequestException ex)
        {
            // Expected failure - test passes
            ex.Message.Should().Contain("Simulated network failure");
        }
    }

    /// <summary>
    /// ❌ Teste com estado compartilhado - pode falhar se executado em paralelo
    /// </summary>
    private static int _sharedCounter = 0;

    [Fact]
    [Trait("Category", "Flaky")]
    public async Task SharedState_FlakyTest_MayFail()
    {
        // Act
        var initialValue = _sharedCounter;
        await Task.Delay(10); // Simula algum trabalho
        _sharedCounter++;

        // Assert - pode falhar se outros testes modificarem _sharedCounter
        _sharedCounter.Should().Be(initialValue + 1);
    }

    /// <summary>
    /// ✅ Teste sem estado compartilhado - isolado
    /// </summary>
    [Fact]
    [Trait("Category", "Stable")]
    public async Task IsolatedState_StableTest_ShouldPass()
    {
        // Arrange - estado local, não compartilhado
        var localCounter = 0;

        // Act
        await Task.Delay(10);
        localCounter++;

        // Assert
        localCounter.Should().Be(1);
    }
}
