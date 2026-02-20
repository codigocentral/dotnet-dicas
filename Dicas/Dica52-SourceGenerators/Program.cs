using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

Console.WriteLine("===== Dica 52: Source Generators ====");
Console.WriteLine();
Console.WriteLine("🔧 Source Generators permitem gerar código automaticamente em tempo de compilação!");
Console.WriteLine("Código gerado é type-safe, performático e transparente...");
Console.WriteLine();

// 1. Demonstração de ToString() manual vs automático
Console.WriteLine("1. Demonstração ToString() Manual vs Automático");
Console.WriteLine("-----------------------------------------------");

var pessoa = new Pessoa { Nome = "João", Idade = 30, Email = "joao@email.com" };
var produto = new Produto { Id = 1, Nome = "Notebook", Preco = 2500.99m, Categoria = "Eletrônicos" };

Console.WriteLine("✅ ToString() implementado manualmente:");
Console.WriteLine($"   Pessoa: {pessoa}");
Console.WriteLine($"   Produto: {produto}");
Console.WriteLine();

// 2. Demonstração de Factory Pattern manual
Console.WriteLine("2. Factory Pattern Manual");
Console.WriteLine("-------------------------");

var config1 = ConfiguracaoFactory.Create();
var config2 = ConfiguracaoFactory.Create("Production", 30, true);
var usuario1 = UsuarioFactory.Create("admin", "admin@sistema.com");

Console.WriteLine("✅ Factory methods implementados manualmente:");
Console.WriteLine($"   Configuração padrão: {config1}");
Console.WriteLine($"   Configuração customizada: {config2}");
Console.WriteLine($"   Usuário: {usuario1}");
Console.WriteLine();

// 3. Demonstração de como Source Generators funcionariam
Console.WriteLine("3. Como Source Generators Funcionariam");
Console.WriteLine("-------------------------------------");

Console.WriteLine("✅ Com Source Generators, este código seria gerado automaticamente:");
Console.WriteLine(@"
// Gerado automaticamente para classes com [AutoToString]
partial class Pessoa
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(""Pessoa { "");
        sb.Append(""Nome = "");
        sb.Append(Nome?.ToString() ?? ""null"");
        sb.Append("", "");
        sb.Append(""Idade = "");
        sb.Append(Idade.ToString());
        sb.Append("", "");
        sb.Append(""Email = "");
        sb.Append(Email?.ToString() ?? ""null"");
        sb.Append("" }"");
        return sb.ToString();
    }
}

// Gerado automaticamente para classes com [AutoFactory]
public static class PessoaFactory
{
    public static Pessoa Create(string nome, string email)
    {
        return new Pessoa(nome, email);
    }
}");
Console.WriteLine();

// 4. Demonstração prática com DI
Console.WriteLine("4. Integração com Dependency Injection");
Console.WriteLine("--------------------------------------");

var services = new ServiceCollection();
services.AddTransient<INotificationService, EmailNotificationService>();
services.AddSingleton(config2);

var provider = services.BuildServiceProvider();
var notificationService = provider.GetRequiredService<INotificationService>();

Console.WriteLine("✅ Usando objetos com DI:");
await notificationService.SendAsync("Sistema iniciado com sucesso!");
Console.WriteLine();

// 5. Performance e análise
Console.WriteLine("5. Vantagens dos Source Generators");
Console.WriteLine("----------------------------------");

Console.WriteLine("✅ Vantagens técnicas:");
Console.WriteLine("   • Zero overhead em runtime");
Console.WriteLine("   • Código gerado em compile-time");
Console.WriteLine("   • Type-safe e IntelliSense completo");
Console.WriteLine("   • Debugging transparente");
Console.WriteLine("   • Integração perfeita com IDEs");
Console.WriteLine();

// 6. Comparação de performance simulada
Console.WriteLine("6. Análise de Performance Simulada");
Console.WriteLine("----------------------------------");

const int iterations = 1_000_000;

// ToString manual otimizado
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    _ = pessoa.ToString(); // Método manual
}
stopwatch.Stop();
var manualTime = stopwatch.ElapsedMilliseconds;

// Reflection-based ToString (mais lento)
stopwatch.Restart();
for (int i = 0; i < iterations; i++)
{
    _ = GenerateToStringViaReflection(pessoa); // Via reflection
}
stopwatch.Stop();
var reflectionTime = stopwatch.ElapsedMilliseconds;

Console.WriteLine($"✅ ToString() manual: {manualTime} ms ({iterations:N0} iterações)");
Console.WriteLine($"   ToString() reflection: {reflectionTime} ms ({iterations:N0} iterações)");
Console.WriteLine($"   Source Generator seria similar ao manual: ~{manualTime} ms");
Console.WriteLine($"   Speedup vs Reflection: {((double)reflectionTime / manualTime):F1}x");
Console.WriteLine();

// 7. Demonstração de Source Generator para JSON
Console.WriteLine("7. Cenários Avançados de Source Generators");
Console.WriteLine("------------------------------------------");

var jsonPessoa = JsonSerializer.Serialize(pessoa);
var jsonProduto = JsonSerializer.Serialize(produto);

Console.WriteLine("✅ Casos de uso comuns para Source Generators:");
Console.WriteLine("   • ToString() methods automáticos");
Console.WriteLine("   • Factory patterns type-safe");
Console.WriteLine("   • DTO mapping automático");
Console.WriteLine("   • JSON serializers otimizados");
Console.WriteLine("   • Validation code generation");
Console.WriteLine("   • Builder patterns automáticos");
Console.WriteLine("   • Dependency injection registration");
Console.WriteLine("   • API client generation");
Console.WriteLine();

// 8. Reflexão sobre tipos e métodos
Console.WriteLine("8. Análise de Código Gerado Simulado");
Console.WriteLine("------------------------------------");

var assembly = Assembly.GetExecutingAssembly();
var factoryTypes = assembly.GetTypes()
    .Where(t => t.Name.Contains("Factory"))
    .ToList();

Console.WriteLine("✅ Factory classes encontradas:");
foreach (var type in factoryTypes)
{
    Console.WriteLine($"   • {type.Name}");
    
    var createMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name == "Create")
        .ToList();
    
    foreach (var method in createMethods)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"     - Create({parameters})");
    }
}
Console.WriteLine();

Console.WriteLine("=== RESUMO: Source Generators ===");
Console.WriteLine("✅ VANTAGENS:");
Console.WriteLine("   • Código gerado em tempo de compilação");
Console.WriteLine("   • Zero overhead em runtime");
Console.WriteLine("   • Type-safe e IntelliSense completo");
Console.WriteLine("   • Reduz boilerplate code drasticamente");
Console.WriteLine("   • Melhora maintainability");
Console.WriteLine("   • Performance equivalente a código manual");
Console.WriteLine();

Console.WriteLine("🎯 CASOS DE USO:");
Console.WriteLine("   • ToString() methods automáticos");
Console.WriteLine("   • Factory patterns");
Console.WriteLine("   • DTO mapping automático");
Console.WriteLine("   • Serialização otimizada");
Console.WriteLine("   • Validation code generation");
Console.WriteLine("   • Builder patterns");
Console.WriteLine("   • DI container registration");
Console.WriteLine("   • API client generation");
Console.WriteLine();

Console.WriteLine("⚠️  CONSIDERAÇÕES:");
Console.WriteLine("   • Setup inicial mais complexo");
Console.WriteLine("   • Requer conhecimento de Roslyn APIs");
Console.WriteLine("   • Debugging pode ser mais difícil");
Console.WriteLine("   • Compatibilidade com ferramentas de build");

// Método auxiliar para demonstrar reflection
static string GenerateToStringViaReflection(object obj)
{
    var type = obj.GetType();
    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    
    var parts = properties.Select(p => $"{p.Name} = {p.GetValue(obj)?.ToString() ?? "null"}");
    return $"{type.Name} {{ {string.Join(", ", parts)} }}";
}

// Classes para demonstração
public class Pessoa
{
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Email { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Pessoa {{ Nome = {Nome}, Idade = {Idade}, Email = {Email} }}";
    }
}

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Produto {{ Id = {Id}, Nome = {Nome}, Preco = {Preco}, Categoria = {Categoria} }}";
    }
}

public class Configuracao
{
    public string Environment { get; set; } = "Development";
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableLogging { get; set; } = true;

    public Configuracao() { }

    public Configuracao(string environment, int timeoutSeconds, bool enableLogging)
    {
        Environment = environment;
        TimeoutSeconds = timeoutSeconds;
        EnableLogging = enableLogging;
    }

    public override string ToString()
    {
        return $"Configuracao {{ Environment = {Environment}, TimeoutSeconds = {TimeoutSeconds}, EnableLogging = {EnableLogging} }}";
    }
}

public class Usuario
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public Usuario(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }

    public override string ToString()
    {
        return $"Usuario {{ Nome = {Nome}, Email = {Email}, CriadoEm = {CriadoEm} }}";
    }
}

// Factory classes implementadas manualmente (simulando o que seria gerado)
public static class ConfiguracaoFactory
{
    public static Configuracao Create()
    {
        return new Configuracao();
    }

    public static Configuracao Create(string environment, int timeoutSeconds, bool enableLogging)
    {
        return new Configuracao(environment, timeoutSeconds, enableLogging);
    }
}

public static class UsuarioFactory
{
    public static Usuario Create(string nome, string email)
    {
        return new Usuario(nome, email);
    }
}

// Serviços para demonstração com DI
public interface INotificationService
{
    Task SendAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public Task SendAsync(string message)
    {
        Console.WriteLine($"   📧 Email enviado: {message}");
        return Task.CompletedTask;
    }
}
