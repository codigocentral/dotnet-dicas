// Demonstração do C# 12 "Alias para Qualquer Tipo"
// Esta feature permite usar 'using' para criar aliases para qualquer tipo

// 1. Simplificar nomes para tipos longos ou complicados
using LongTypeName = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Tuple<int, string, decimal>>>;
using ConfigType = System.Collections.Generic.Dictionary<string, object>;

// 2. Desambiguar tipos e resolver conflitos de nomes
using SystemTimer = System.Timers.Timer;
using ThreadingTimer = System.Threading.Timer;

// 3. Definir tipos value tuple compartilháveis
using PersonInfo = (string Name, int Age, string Email);
using Coordinates = (double Latitude, double Longitude);
using ProductDetails = (string Name, decimal Price, int Stock, string Category);

// 4. Adicionar clareza ao código usando nomes mais descritivos
using UserId = int;
using ProductId = long;
using Temperature = double;
using Percentage = decimal;

// Exemplo de alias para delegates
using EventHandler = System.Action<string, System.DateTime>;

namespace Dica22.AliasParaQualquerTipo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==== Dica 22: Alias para Qualquer Tipo (C# 12) ====");
        Console.WriteLine("C# 12 permite usar 'using' para criar aliases para qualquer tipo,");
        Console.WriteLine("resolvendo problemas de clareza, simplicidade e desambiguação.\n");

        // 1. Demonstração de simplificação de nomes
        Console.WriteLine("1. Simplificação de tipos longos:");
        
        LongTypeName complexData = new()
        {
            ["users"] = new List<Tuple<int, string, decimal>>
            {
                new(1, "João", 1500.00m),
                new(2, "Maria", 2300.50m),
                new(3, "Pedro", 1800.75m)
            },
            ["products"] = new List<Tuple<int, string, decimal>>
            {
                new(101, "Notebook", 2500.00m),
                new(102, "Mouse", 50.00m)
            }
        };

        Console.WriteLine($"   ✅ Usando alias 'LongTypeName' em vez de:");
        Console.WriteLine($"      Dictionary<string, List<Tuple<int, string, decimal>>>");
        Console.WriteLine($"   📊 Dados carregados: {complexData.Count} categorias");
        
        ConfigType appConfig = new()
        {
            ["database_host"] = "localhost",
            ["database_port"] = 5432,
            ["cache_enabled"] = true,
            ["max_connections"] = 100
        };
        
        Console.WriteLine($"   ⚙️  Configuração: {appConfig.Count} parâmetros definidos");
        Console.WriteLine();

        // 2. Demonstração de desambiguação
        Console.WriteLine("2. Desambiguação de tipos:");
        
        // Agora fica claro qual Timer estamos usando
        SystemTimer systemTimer = new(1000);
        systemTimer.Elapsed += (sender, e) => { /* handler */ };
        
        ThreadingTimer threadingTimer = new(_ => { /* callback */ }, null, 1000, 1000);
        
        Console.WriteLine($"   ✅ SystemTimer: {systemTimer.Interval}ms");
        Console.WriteLine($"   ✅ ThreadingTimer: Período de 1000ms");
        Console.WriteLine($"   💡 Sem alias seria: System.Timers.Timer vs System.Threading.Timer");
        Console.WriteLine();

        // 3. Demonstração de value tuples compartilháveis
        Console.WriteLine("3. Value tuples compartilháveis:");
        
        PersonInfo[] people = 
        {
            ("Ana Silva", 28, "ana@email.com"),
            ("Carlos Santos", 35, "carlos@email.com"),
            ("Beatriz Lima", 42, "beatriz@email.com")
        };

        Console.WriteLine($"   👥 Pessoas cadastradas:");
        foreach (var person in people)
        {
            Console.WriteLine($"      {person.Name}, {person.Age} anos - {person.Email}");
        }

        Coordinates[] locations =
        {
            (-23.5505, -46.6333), // São Paulo
            (-22.9068, -43.1729), // Rio de Janeiro
            (-19.9191, -43.9386)  // Belo Horizonte
        };

        Console.WriteLine($"   🗺️  Coordenadas:");
        string[] cities = { "São Paulo", "Rio de Janeiro", "Belo Horizonte" };
        for (int i = 0; i < locations.Length; i++)
        {
            Console.WriteLine($"      {cities[i]}: {locations[i].Latitude:F4}, {locations[i].Longitude:F4}");
        }

        ProductDetails[] products =
        {
            ("Smartphone", 899.99m, 50, "Eletrônicos"),
            ("Livro C#", 79.90m, 200, "Livros"),
            ("Cadeira Gamer", 599.00m, 15, "Móveis")
        };

        Console.WriteLine($"   🛍️  Produtos:");
        foreach (var product in products)
        {
            Console.WriteLine($"      {product.Name}: R$ {product.Price:F2} (Estoque: {product.Stock}) - {product.Category}");
        }
        Console.WriteLine();

        // 4. Demonstração de clareza com tipos primitivos
        Console.WriteLine("4. Clareza com tipos primitivos:");
        
        UserId currentUser = 12345;
        ProductId selectedProduct = 67890;
        Temperature roomTemp = 23.5;
        Percentage discount = 0.15m;
        
        Console.WriteLine($"   🆔 User ID: {currentUser}");
        Console.WriteLine($"   📦 Product ID: {selectedProduct}");
        Console.WriteLine($"   🌡️  Temperatura: {roomTemp:F1}°C");
        Console.WriteLine($"   💰 Desconto: {discount:P0}");
        
        // Métodos usando os aliases
        var finalPrice = CalculatePrice(selectedProduct, discount);
        var isComfortable = IsComfortableTemperature(roomTemp);
        
        Console.WriteLine($"   💵 Preço final: R$ {finalPrice:F2}");
        Console.WriteLine($"   😊 Temperatura confortável: {(isComfortable ? "Sim" : "Não")}");
        Console.WriteLine();

        // 5. Comparação Before/After
        Console.WriteLine("5. Comparação Before/After:");
        
        Console.WriteLine("   ❌ Sem aliases:");
        Console.WriteLine("      Dictionary<string, List<Tuple<int, string, decimal>>> data;");
        Console.WriteLine("      System.Timers.Timer timer1;");
        Console.WriteLine("      System.Threading.Timer timer2;");
        Console.WriteLine("      (string, int, string) person;");
        Console.WriteLine("      int userId; // Qual tipo de ID?");
        
        Console.WriteLine("\n   ✅ Com aliases C# 12:");
        Console.WriteLine("      LongTypeName data;");
        Console.WriteLine("      SystemTimer timer1;");
        Console.WriteLine("      ThreadingTimer timer2;");
        Console.WriteLine("      PersonInfo person;");
        Console.WriteLine("      UserId userId; // Claramente um ID de usuário");

        Console.WriteLine("\n=== Resumo dos Benefícios ===");
        Console.WriteLine("✅ Simplifica tipos complexos com nomes mais legíveis");
        Console.WriteLine("✅ Resolve conflitos de namespace sem fully qualified names");
        Console.WriteLine("✅ Permite compartilhar definições de value tuples");
        Console.WriteLine("✅ Adiciona significado semântico a tipos primitivos");
        Console.WriteLine("✅ Melhora legibilidade e manutenibilidade do código");
        Console.WriteLine("✅ Funciona com qualquer tipo: classes, structs, interfaces, delegates");
        Console.WriteLine("✅ Aliases são locais ao arquivo (escopo respeitado)");
        Console.WriteLine("✅ Compatível com IntelliSense e refatoração do IDE");
    }

    // Métodos auxiliares demonstrando uso dos aliases
    static decimal CalculatePrice(ProductId productId, Percentage discount)
    {
        // Simular busca de preço por ID
        decimal basePrice = productId switch
        {
            67890 => 899.99m,
            _ => 100.00m
        };
        
        return basePrice * (1 - discount);
    }
    
    static bool IsComfortableTemperature(Temperature temp)
    {
        return temp >= 18.0 && temp <= 26.0;
    }
}

// Exemplo adicional: usando aliases em classes
public class OrderService
{
    // Aliases tornam as assinaturas mais claras
    public decimal ProcessOrder(UserId userId, ProductId productId, Percentage discount)
    {
        Console.WriteLine($"Processando pedido: User {userId}, Product {productId}, Desconto {discount:P}");
        return CalculateOrderTotal(productId, discount);
    }
    
    private static decimal CalculateOrderTotal(ProductId productId, Percentage discount)
    {
        // Lógica de cálculo usando tipos com significado semântico claro
        return productId * (1 - discount);
    }
}

// Exemplo de alias para delegates

public static class EventLogger
{
    private static EventHandler? _logHandler;
    
    public static void Subscribe(EventHandler handler)
    {
        _logHandler += handler;
    }
    
    public static void LogEvent(string message)
    {
        _logHandler?.Invoke(message, DateTime.Now);
    }
}
