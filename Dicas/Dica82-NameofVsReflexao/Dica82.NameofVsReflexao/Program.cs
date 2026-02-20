using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

Console.WriteLine("=== Dica 82: nameof Não é Reflexão (É Mais Rápido) ===");
Console.WriteLine("nameof() NÃO é reflexão. Ele retorna o nome de uma string em tempo de compilação.");
Console.WriteLine("Isso significa ZERO custo em tempo de execução, sem alocações e sem penalidade de desempenho.");
Console.WriteLine("Compare isso com GetType().Name ou APIs de reflexão que são executadas em tempo de execução.");
Console.WriteLine();

// Demonstração prática das diferenças
var produto = new Produto { Nome = "Laptop", Preco = 2500.99m, Categoria = "Eletrônicos" };
var logger = new PerformanceLogger();

// 1. Demonstração básica de nameof vs Reflection
Console.WriteLine("=== 1. Comparação Básica: nameof vs Reflection ===");

// ✅ CORRETO: nameof (compile-time)
var nomePropriedadeNameof = nameof(produto.Nome);
Console.WriteLine($"nameof(produto.Nome): '{nomePropriedadeNameof}'");

// ❌ CUSTOSO: Reflection (runtime)
var nomePropriedadeReflection = typeof(Produto).GetProperty("Nome")?.Name ?? "";
Console.WriteLine($"Reflection GetProperty: '{nomePropriedadeReflection}'");

Console.WriteLine();

// 2. Teste de performance com muitas iterações
Console.WriteLine("=== 2. Teste de Performance (1.000.000 iterações) ===");

const int iterations = 1_000_000;

// Teste nameof
var sw = Stopwatch.StartNew();
string nameofResult = "";
for (int i = 0; i < iterations; i++)
{
    nameofResult = nameof(produto.Nome); // Compile-time constant
}
sw.Stop();
Console.WriteLine($"nameof: {sw.ElapsedMilliseconds}ms ({sw.ElapsedTicks} ticks)");

// Teste reflection
sw.Restart();
string reflectionResult = "";
for (int i = 0; i < iterations; i++)
{
    reflectionResult = typeof(Produto).GetProperty(nameof(Produto.Nome))?.Name ?? "";
}
sw.Stop();
Console.WriteLine($"Reflection: {sw.ElapsedMilliseconds}ms ({sw.ElapsedTicks} ticks)");

Console.WriteLine();

// 3. Cenários práticos de uso
Console.WriteLine("=== 3. Cenários Práticos de Uso ===");

// Logging com nameof
logger.LogProperty(nameof(produto.Nome), produto.Nome);
logger.LogProperty(nameof(produto.Preco), produto.Preco);

// Validação com nameof
var validador = new ValidadorComNameof();
var erros = validador.ValidarProduto(produto);
foreach (var erro in erros)
{
    Console.WriteLine($"Validação: {erro}");
}

// Exception handling com nameof
try
{
    produto.SetPreco(-100);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Exceção capturada: {ex.Message}");
}

Console.WriteLine();

// 4. Demonstração de AOT compatibility
Console.WriteLine("=== 4. Compatibilidade com AOT (Ahead-of-Time) ===");
DemonstrarCompatibilidadeAOT();

Console.WriteLine();

// 5. Comparação com diferentes cenários de reflection
Console.WriteLine("=== 5. Diferentes Custos de Reflection ===");
CompararCustosReflection();

Console.WriteLine();

// 6. Demonstração de Memory Allocation
Console.WriteLine("=== 6. Demonstração de Alocação de Memória ===");
DemonstrarAlocacaoMemoria();

Console.WriteLine();

// 7. Expression trees demonstration
Console.WriteLine("=== 7. Comparison with Expression Trees ===");
ExpressionDemo.DemonstrarExpressions();

static void DemonstrarCompatibilidadeAOT()
{
    // ✅ nameof funciona perfeitamente com AOT
    Console.WriteLine($"nameof com AOT: {nameof(Produto)} - SEM problemas");
    
    // ❌ Reflection pode ter problemas com AOT trimming
    try
    {
        var type = typeof(Produto);
        Console.WriteLine($"Reflection com AOT: {type.Name} - Pode ser problemático");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro de AOT: {ex.Message}");
    }
}

static void CompararCustosReflection()
{
    var sw = Stopwatch.StartNew();
    
    // nameof - compile time
    var name1 = nameof(Produto.Nome);
    var name2 = nameof(Produto.Preco);
    var name3 = nameof(Produto.Categoria);
    sw.Stop();
    Console.WriteLine($"nameof (3 propriedades): {sw.ElapsedTicks} ticks");
    
    // GetType().Name - runtime
    sw.Restart();
    var produto = new Produto();
    var typeName = produto.GetType().Name;
    sw.Stop();
    Console.WriteLine($"GetType().Name: {sw.ElapsedTicks} ticks");
    
    // GetProperty - mais custoso
    sw.Restart();
    var prop = typeof(Produto).GetProperty("Nome");
    sw.Stop();
    Console.WriteLine($"GetProperty: {sw.ElapsedTicks} ticks");
    
    // GetProperties - muito custoso
    sw.Restart();
    var props = typeof(Produto).GetProperties();
    sw.Stop();
    Console.WriteLine($"GetProperties: {sw.ElapsedTicks} ticks");
}

static void DemonstrarAlocacaoMemoria()
{
    // Medir memória antes
    var memoryBefore = GC.GetTotalMemory(false);
    
    // Usar nameof - zero allocations
    const int iterations = 10000;
    for (int i = 0; i < iterations; i++)
    {
        var name = nameof(Produto.Nome); // Compile-time constant, zero allocation
    }
    
    var memoryAfterNameof = GC.GetTotalMemory(false);
    
    // Usar reflection - aloca objetos
    for (int i = 0; i < iterations; i++)
    {
        var prop = typeof(Produto).GetProperty("Nome"); // Allocates objects
    }
    
    var memoryAfterReflection = GC.GetTotalMemory(false);
    
    Console.WriteLine($"Memória inicial: {memoryBefore:N0} bytes");
    Console.WriteLine($"Após nameof: {memoryAfterNameof:N0} bytes (diferença: {memoryAfterNameof - memoryBefore:N0})");
    Console.WriteLine($"Após reflection: {memoryAfterReflection:N0} bytes (diferença: {memoryAfterReflection - memoryAfterNameof:N0})");
}

public class Produto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;

    public void SetPreco(decimal novoPreco)
    {
        if (novoPreco < 0)
        {
            // ✅ CORRETO: nameof - zero performance cost
            throw new ArgumentException($"Preço não pode ser negativo", nameof(novoPreco));
            
            // ❌ CUSTOSO: String hard-coded ou reflection
            // throw new ArgumentException("Preço não pode ser negativo", "novoPreco");
        }
        
        Preco = novoPreco;
    }
}

public class PerformanceLogger
{
    public void LogProperty<T>(string propertyName, T value, [CallerMemberName] string methodName = "")
    {
        // ✅ nameof garante type safety e zero runtime cost
        Console.WriteLine($"[{methodName}] {propertyName} = {value}");
    }

    public void LogWithReflection<T>(T obj, string propertyName)
    {
        // ❌ Reflection tem custo de runtime
        var property = typeof(T).GetProperty(propertyName);
        var value = property?.GetValue(obj);
        Console.WriteLine($"[Reflection] {propertyName} = {value}");
    }
}

public class ValidadorComNameof
{
    public List<string> ValidarProduto(Produto produto)
    {
        var erros = new List<string>();

        // ✅ CORRETO: nameof para propriedades
        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            erros.Add($"Campo {nameof(produto.Nome)} é obrigatório");
        }

        if (produto.Preco <= 0)
        {
            erros.Add($"Campo {nameof(produto.Preco)} deve ser maior que zero");
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria))
        {
            erros.Add($"Campo {nameof(produto.Categoria)} é obrigatório");
        }

        return erros;
    }
}

// Demonstração de expression trees com nameof
public static class ExpressionHelper
{
    public static string GetPropertyName<T, TProp>(System.Linq.Expressions.Expression<Func<T, TProp>> expression)
    {
        // ✅ Type-safe property access
        if (expression.Body is System.Linq.Expressions.MemberExpression member)
        {
            return member.Member.Name;
        }
        
        throw new ArgumentException("Expression deve ser uma propriedade");
    }
}

// Exemplo de usage com expression trees (mais type-safe que strings)
public static class ExpressionDemo
{
    public static void DemonstrarExpressions()
    {
        // ✅ Type-safe com expressions
        var propName = ExpressionHelper.GetPropertyName<Produto, string>(p => p.Nome);
        Console.WriteLine($"Expression property name: {propName}");
        
        // ✅ Mais simples e rápido com nameof
        var nameofProp = nameof(Produto.Nome);
        Console.WriteLine($"nameof property name: {nameofProp}");
    }
}
