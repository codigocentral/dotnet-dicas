using System.Dynamic;
using System.Text.Json;

Console.WriteLine("=== Dica 75: Evitando a Palavra-chave 'dynamic' ===\n");

Console.WriteLine("⚠️  A palavra-chave 'dynamic' deve ser EVITADA na maioria dos casos!");
Console.WriteLine("Vamos demonstrar por que e as alternativas melhores...\n");

// Demonstração 1: Problemas com dynamic
Console.WriteLine("1. Problemas fundamentais com 'dynamic':");
DemonstrarProblemasDynamic();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 2: Performance comparativa
Console.WriteLine("2. Impacto na performance:");
DemonstrarImpactoPerformance();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 3: Alternativas para JSON
Console.WriteLine("3. Alternativas para processamento de JSON:");
DemonstrarAlternativasJSON();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 4: Alternativas para APIs flexíveis
Console.WriteLine("4. Alternativas para APIs flexíveis:");
DemonstrarAlternativasAPIs();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 5: Casos onde dynamic é aceitável
Console.WriteLine("5. Casos onde 'dynamic' é aceitável (mas limitados):");
DemonstrarCasosAceitaveis();

Console.WriteLine("\n=== Resumo: Por que Evitar 'dynamic' ===");
Console.WriteLine("❌ Perda de type safety (erros apenas em runtime)");
Console.WriteLine("❌ Performance significativamente menor (DLR overhead)");
Console.WriteLine("❌ Sem IntelliSense ou autocompletar");
Console.WriteLine("❌ Dificulta refatoração e manutenção");
Console.WriteLine("❌ Sem análise estática de código");
Console.WriteLine("❌ Debugging mais complexo");
Console.WriteLine("\n✅ Use alternativas type-safe sempre que possível!");

static void DemonstrarProblemasDynamic()
{
    Console.WriteLine("  ❌ Problema 1: Sem type safety - erros apenas em runtime");
    
    try
    {
        dynamic obj = new { Name = "João", Age = 30 };
        
        // Isso compila mas falha em runtime!
        // string invalidProperty = obj.NonExistentProperty;
        
        Console.WriteLine($"     Objeto: {obj.Name}, {obj.Age} anos");
        Console.WriteLine("     ⚠️  Propriedade inválida comentada para evitar crash");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"     💥 Erro em runtime: {ex.Message}");
    }
    
    Console.WriteLine("\n  ❌ Problema 2: Sem IntelliSense");
    Console.WriteLine("     • Não há autocompletar para propriedades");
    Console.WriteLine("     • Impossível saber quais membros estão disponíveis");
    Console.WriteLine("     • Typos só são detectados em runtime");
    
    Console.WriteLine("\n  ❌ Problema 3: Performance degradada");
    Console.WriteLine("     • Toda operação passa pelo Dynamic Language Runtime (DLR)");
    Console.WriteLine("     • Resoluções de método custosas em runtime");
    Console.WriteLine("     • Overhead significativo comparado a tipagem forte");
}

static void DemonstrarImpactoPerformance()
{
    Console.WriteLine("  📊 Comparação de performance:");
    
    const int iterations = 1_000_000;
    
    // Teste com tipo estático
    var staticObj = new Person("Maria", 25);
    var sw1 = System.Diagnostics.Stopwatch.StartNew();
    
    string name1 = "";
    for (int i = 0; i < iterations; i++)
    {
        name1 = staticObj.GetDisplayName();
    }
    sw1.Stop();
    
    // Teste com dynamic
    dynamic dynamicObj = new Person("Maria", 25);
    var sw2 = System.Diagnostics.Stopwatch.StartNew();
    
    string name2 = "";
    for (int i = 0; i < iterations; i++)
    {
        name2 = dynamicObj.GetDisplayName();
    }
    sw2.Stop();
    
    Console.WriteLine($"     Iterações: {iterations:N0}");
    Console.WriteLine($"     Tipagem estática: {sw1.ElapsedMilliseconds} ms");
    Console.WriteLine($"     Dynamic: {sw2.ElapsedMilliseconds} ms");
    Console.WriteLine($"     ⚡ Dynamic é {(double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds:F1}x mais lento!");
}

static void DemonstrarAlternativasJSON()
{
    Console.WriteLine("  🔄 Processamento de JSON: dynamic vs alternativas");
    
    var jsonString = """
        {
            "id": 123,
            "name": "Produto Exemplo",
            "price": 99.99,
            "category": "Eletrônicos",
            "inStock": true
        }
        """;
    
    Console.WriteLine("\n  ❌ Abordagem com dynamic (NÃO recomendada):");
    try
    {
        dynamic dynamicProduct = JsonSerializer.Deserialize<dynamic>(jsonString);
        // Problema: JsonSerializer não suporta dynamic diretamente
        Console.WriteLine("     💥 JsonSerializer não funciona bem com dynamic!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"     💥 Erro: {ex.Message}");
    }
    
    Console.WriteLine("\n  ✅ Alternativa 1: Classes tipadas (RECOMENDADO):");
    var typedProduct = JsonSerializer.Deserialize<Product>(jsonString);
    Console.WriteLine($"     Produto: {typedProduct?.Name}");
    Console.WriteLine($"     Preço: {typedProduct?.Price:C}");
    Console.WriteLine($"     Em estoque: {typedProduct?.InStock}");
    
    Console.WriteLine("\n  ✅ Alternativa 2: JsonElement (flexível e type-safe):");
    using var document = JsonDocument.Parse(jsonString);
    var root = document.RootElement;
    
    if (root.TryGetProperty("name", out var nameElement))
    {
        Console.WriteLine($"     Nome via JsonElement: {nameElement.GetString()}");
    }
    
    if (root.TryGetProperty("price", out var priceElement))
    {
        Console.WriteLine($"     Preço via JsonElement: {priceElement.GetDecimal():C}");
    }
}

static void DemonstrarAlternativasAPIs()
{
    Console.WriteLine("  🔌 APIs flexíveis: dynamic vs alternativas");
    
    Console.WriteLine("\n  ❌ API com dynamic (problemática):");
    var dynamicProcessor = new DynamicApiProcessor();
    // dynamicProcessor.ProcessData(someData); // Sem type safety
    
    Console.WriteLine("     • Sem contract claro");
    Console.WriteLine("     • Impossível documentar adequadamente");
    Console.WriteLine("     • Testes mais difíceis");
    
    Console.WriteLine("\n  ✅ Alternativa 1: Interfaces (RECOMENDADO):");
    IDataProcessor interfaceProcessor = new InterfaceApiProcessor();
    var data = new ProcessingData { Value = 42, Name = "Test" };
    var result1 = interfaceProcessor.ProcessData(data);
    Console.WriteLine($"     Resultado via interface: {result1}");
    
    Console.WriteLine("\n  ✅ Alternativa 2: Generics (type-safe e flexível):");
    var genericProcessor = new GenericApiProcessor();
    var result2 = genericProcessor.ProcessData(data);
    Console.WriteLine($"     Resultado via generics: {result2}");
    
    Console.WriteLine("\n  ✅ Alternativa 3: Union types com records:");
    var unionProcessor = new UnionTypeProcessor();
    var textData = new TextData("Hello, World!");
    var result3 = unionProcessor.ProcessData(textData);
    Console.WriteLine($"     Resultado via union types: {result3}");
}

static void DemonstrarCasosAceitaveis()
{
    Console.WriteLine("  ✅ Casos LIMITADOS onde dynamic pode ser aceitável:");
    
    Console.WriteLine("\n  1. Interoperabilidade COM (legado):");
    Console.WriteLine("     • Trabalhando com Office Interop");
    Console.WriteLine("     • APIs COM antigas que não têm tipagem");
    
    Console.WriteLine("\n  2. ExpandoObject para dados ad-hoc:");
    dynamic expando = new ExpandoObject();
    expando.RuntimeProperty = "Valor dinâmico";
    expando.AnotherProperty = 42;
    
    Console.WriteLine($"     ExpandoObject: {expando.RuntimeProperty}, {expando.AnotherProperty}");
    Console.WriteLine("     ⚠️  Apenas para dados temporários/experimentais");
    
    Console.WriteLine("\n  3. Bridging com engines de scripting:");
    Console.WriteLine("     • Integração com Python/JavaScript engines");
    Console.WriteLine("     • Código 'cola' entre sistemas");
    Console.WriteLine("     ⚠️  Isole em camadas específicas");
    
    Console.WriteLine("\n  📝 Regra geral: dynamic NUNCA na lógica de negócio principal!");
}

// ===== CLASSES DE DEMONSTRAÇÃO =====

public class Person
{
    public string Name { get; }
    public int Age { get; }
    
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
    
    public string GetDisplayName()
    {
        return $"{Name} ({Age} anos)";
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
    public bool InStock { get; set; }
}

public class DynamicApiProcessor
{
    public dynamic ProcessData(dynamic data)
    {
        // ❌ Problemático: sem contract, sem type safety
        return data;
    }
}

public interface IDataProcessor
{
    string ProcessData(ProcessingData data);
}

public class InterfaceApiProcessor : IDataProcessor
{
    public string ProcessData(ProcessingData data)
    {
        return $"Processed: {data.Name} = {data.Value}";
    }
}

public class GenericApiProcessor
{
    public string ProcessData<T>(T data) where T : class
    {
        return $"Generic processing: {data.GetType().Name}";
    }
}

public class ProcessingData
{
    public int Value { get; set; }
    public string Name { get; set; } = "";
}

// Union types com records (pattern moderno)
public abstract record DataType;
public record TextData(string Content) : DataType;
public record NumberData(double Value) : DataType;
public record BooleanData(bool Flag) : DataType;

public class UnionTypeProcessor
{
    public string ProcessData(DataType data)
    {
        return data switch
        {
            TextData text => $"Text: {text.Content}",
            NumberData number => $"Number: {number.Value}",
            BooleanData boolean => $"Boolean: {boolean.Flag}",
            _ => "Unknown data type"
        };
    }
}
