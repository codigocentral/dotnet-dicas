Console.WriteLine("=== Dica 73: ValueTask vs Task ===\n");

var demo = new ValueTaskDemo();

// Demonstrações básicas
Console.WriteLine("1. Diferenças fundamentais:");
await demo.BasicDifferences();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Cenários de uso
Console.WriteLine("2. Quando usar ValueTask:");
await demo.WhenToUseValueTask();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Cenários práticos
Console.WriteLine("3. Exemplos práticos:");
await demo.PracticalExamples();

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Cache scenarios
Console.WriteLine("4. Cenários com cache:");
await demo.CacheScenarios();

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("Resumo das práticas recomendadas:");
Console.WriteLine("✅ Use ValueTask quando há alta probabilidade de completar sincronamente");
Console.WriteLine("✅ Use ValueTask para métodos chamados frequentemente");
Console.WriteLine("✅ Use ValueTask com cache ou valores pré-computados");
Console.WriteLine("✅ Use Task para operações genuinamente assíncronas");
Console.WriteLine("✅ Use Task para APIs públicas (maior compatibilidade)");
Console.WriteLine("⚠️  ValueTask não pode ser aguardado múltiplas vezes");
Console.WriteLine("⚠️  ValueTask é mais complexo de usar corretamente");

public class ValueTaskDemo
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly Random _random = Random.Shared;

    public async Task BasicDifferences()
    {
        Console.WriteLine("Comparando Task vs ValueTask:");

        // Task - sempre aloca no heap
        var taskResult = await GetDataWithTask("exemplo");
        Console.WriteLine($"Task result: {taskResult}");

        // ValueTask - pode evitar alocação se completar sincronamente
        var valueTaskResult = await GetDataWithValueTask("exemplo");
        Console.WriteLine($"ValueTask result: {valueTaskResult}");

        // Demonstra diferença na alocação
        Console.WriteLine("\nComparando alocações:");
        
        // Múltiplas chamadas Task
        for (int i = 0; i < 3; i++)
        {
            await GetDataWithTask($"item-{i}");
        }
        
        // Múltiplas chamadas ValueTask
        for (int i = 0; i < 3; i++)
        {
            await GetDataWithValueTask($"item-{i}");
        }
    }

    public async Task WhenToUseValueTask()
    {
        Console.WriteLine("Cenários ideais para ValueTask:");

        // 1. Operações que frequentemente completam sincronamente
        Console.WriteLine("\n1. Cache hits frequentes:");
        await PopulateCache();
        
        for (int i = 0; i < 5; i++)
        {
            var result = await GetCachedDataValueTask("user-1");
            Console.WriteLine($"   Cache hit: {result}");
        }

        // 2. Validações rápidas
        Console.WriteLine("\n2. Validações que podem ser síncronas:");
        var validInputs = new[] { "valid@email.com", "", "another@test.com", "invalid" };
        
        foreach (var input in validInputs)
        {
            var isValid = await ValidateEmailValueTask(input);
            Console.WriteLine($"   {input}: {(isValid ? "✅ Válido" : "❌ Inválido")}");
        }

        // 3. Operações condicionais
        Console.WriteLine("\n3. Operações condicionais:");
        var conditions = new[] { true, false, true, false };
        
        foreach (var condition in conditions)
        {
            var result = await ConditionalOperationValueTask(condition);
            Console.WriteLine($"   Condição {condition}: {result}");
        }
    }

    public async Task PracticalExamples()
    {
        Console.WriteLine("Exemplos práticos de uso:");

        // Repository pattern com cache
        var userRepo = new UserRepository();
        
        Console.WriteLine("\n1. Repository com cache:");
        var user1 = await userRepo.GetUserByIdValueTask(1); // Cache miss
        var user2 = await userRepo.GetUserByIdValueTask(1); // Cache hit
        var user3 = await userRepo.GetUserByIdValueTask(2); // Cache miss
        
        Console.WriteLine($"   User 1 (miss): {user1?.Name}");
        Console.WriteLine($"   User 1 (hit): {user2?.Name}");
        Console.WriteLine($"   User 2 (miss): {user3?.Name}");

        // Configuration service
        var configService = new ConfigurationService();
        
        Console.WriteLine("\n2. Configuration service:");
        var config1 = await configService.GetConfigValueTask("database-url");
        var config2 = await configService.GetConfigValueTask("api-key");
        var config3 = await configService.GetConfigValueTask("database-url"); // Cached
        
        Console.WriteLine($"   Config 1: {config1}");
        Console.WriteLine($"   Config 2: {config2}");
        Console.WriteLine($"   Config 1 (cached): {config3}");
    }

    public async Task CacheScenarios()
    {
        Console.WriteLine("Demonstrando cenários com cache:");

        var calculator = new MemoizedCalculator();
        
        // Primeiras chamadas (cache miss)
        var start = DateTime.Now;
        var result1 = await calculator.ExpensiveCalculationValueTask(100);
        var time1 = DateTime.Now - start;
        
        start = DateTime.Now;
        var result2 = await calculator.ExpensiveCalculationValueTask(200);
        var time2 = DateTime.Now - start;
        
        // Chamadas repetidas (cache hit)
        start = DateTime.Now;
        var result3 = await calculator.ExpensiveCalculationValueTask(100);
        var time3 = DateTime.Now - start;
        
        start = DateTime.Now;
        var result4 = await calculator.ExpensiveCalculationValueTask(200);
        var time4 = DateTime.Now - start;

        Console.WriteLine($"   Cálculo 100 (miss): {result1} ({time1.TotalMilliseconds:F1}ms)");
        Console.WriteLine($"   Cálculo 200 (miss): {result2} ({time2.TotalMilliseconds:F1}ms)");
        Console.WriteLine($"   Cálculo 100 (hit):  {result3} ({time3.TotalMilliseconds:F1}ms)");
        Console.WriteLine($"   Cálculo 200 (hit):  {result4} ({time4.TotalMilliseconds:F1}ms)");
    }

    // Métodos de demonstração
    private async Task<string> GetDataWithTask(string key)
    {
        // Task sempre aloca, mesmo para operações síncronas
        await Task.Delay(1); // Simula operação assíncrona mínima
        return $"Task data for {key}";
    }

    private ValueTask<string> GetDataWithValueTask(string key)
    {
        // ValueTask pode evitar alocação se completar sincronamente
        if (_cache.TryGetValue(key, out var cached))
        {
            return ValueTask.FromResult(cached); // Sem alocação!
        }

        return GetDataAsyncValueTask(key);
    }

    private async ValueTask<string> GetDataAsyncValueTask(string key)
    {
        await Task.Delay(1);
        var result = $"ValueTask data for {key}";
        _cache[key] = result;
        return result;
    }

    private async Task PopulateCache()
    {
        _cache["user-1"] = "John Doe";
        _cache["user-2"] = "Jane Smith";
        await Task.CompletedTask;
    }

    private ValueTask<string> GetCachedDataValueTask(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return ValueTask.FromResult(value); // Sincrono, sem alocação
        }

        return LoadDataAsyncValueTask(key);
    }

    private async ValueTask<string> LoadDataAsyncValueTask(string key)
    {
        await Task.Delay(50); // Simula I/O
        var value = $"Loaded data for {key}";
        _cache[key] = value;
        return value;
    }

    private ValueTask<bool> ValidateEmailValueTask(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValueTask.FromResult(false); // Validação síncrona
        }

        if (email.Contains('@') && email.Contains('.'))
        {
            return ValueTask.FromResult(true); // Validação síncrona
        }

        // Validação complexa assíncrona (simulada)
        return ComplexValidationAsyncValueTask(email);
    }

    private async ValueTask<bool> ComplexValidationAsyncValueTask(string email)
    {
        await Task.Delay(10); // Simula validação complexa
        return email.Length > 5;
    }

    private ValueTask<string> ConditionalOperationValueTask(bool condition)
    {
        if (condition)
        {
            return ValueTask.FromResult("Immediate result"); // Sincrono
        }

        return DelayedOperationAsyncValueTask();
    }

    private async ValueTask<string> DelayedOperationAsyncValueTask()
    {
        await Task.Delay(25);
        return "Delayed result";
    }
}

// Classes auxiliares para demonstração
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserRepository
{
    private readonly Dictionary<int, User> _cache = new();
    private readonly Random _random = Random.Shared;

    public ValueTask<User?> GetUserByIdValueTask(int id)
    {
        if (_cache.TryGetValue(id, out var user))
        {
            return ValueTask.FromResult<User?>(user); // Cache hit - sincrono
        }

        return LoadUserAsyncValueTask(id);
    }

    private async ValueTask<User?> LoadUserAsyncValueTask(int id)
    {
        await Task.Delay(50); // Simula database call
        
        var user = new User { Id = id, Name = $"User {id}" };
        _cache[id] = user;
        return user;
    }
}

public class ConfigurationService
{
    private readonly Dictionary<string, string> _cache = new();

    public ValueTask<string> GetConfigValueTask(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return ValueTask.FromResult(value); // Cache hit
        }

        return LoadConfigAsyncValueTask(key);
    }

    private async ValueTask<string> LoadConfigAsyncValueTask(string key)
    {
        await Task.Delay(30); // Simula file/remote read
        
        var value = key switch
        {
            "database-url" => "Server=localhost;Database=MyApp",
            "api-key" => "abc123def456",
            _ => "default-value"
        };
        
        _cache[key] = value;
        return value;
    }
}

public class MemoizedCalculator
{
    private readonly Dictionary<int, double> _cache = new();

    public ValueTask<double> ExpensiveCalculationValueTask(int input)
    {
        if (_cache.TryGetValue(input, out var cached))
        {
            return ValueTask.FromResult(cached); // Cache hit - instant
        }

        return CalculateAsyncValueTask(input);
    }

    private async ValueTask<double> CalculateAsyncValueTask(int input)
    {
        await Task.Delay(100); // Simula cálculo pesado
        
        var result = Math.Sqrt(input) * Math.PI;
        _cache[input] = result;
        return result;
    }
}
