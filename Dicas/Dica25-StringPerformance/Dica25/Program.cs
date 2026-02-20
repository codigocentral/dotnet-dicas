using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Dica25.StringPerformance;

/// <summary>
/// Dica 25: String Performance - Interpolação vs StringBuilder vs Concat
/// 
/// Esta dica demonstra as diferentes técnicas de concatenação de strings
/// e seus impactos de performance e alocação de memória.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddScoped<StringPerformanceService>();
            })
            .Build();

        var service = host.Services.GetRequiredService<StringPerformanceService>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== Dica 25: String Performance Comparison ===");

        // Demonstrar diferentes cenários
        await service.DemonstrarConcatenacaoSimples();
        await service.DemonstrarConcatenacaoLoop();
        await service.DemonstrarFormatacaoCompleta();
        await service.DemonstrarBestPractices();
    }
}

public class StringPerformanceService
{
    private readonly ILogger<StringPerformanceService> _logger;

    public StringPerformanceService(ILogger<StringPerformanceService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cenário 1: Concatenação simples (poucos elementos)
    /// </summary>
    public async Task DemonstrarConcatenacaoSimples()
    {
        _logger.LogInformation("\n--- Cenário 1: Concatenação Simples ---");

        var nome = "João";
        var idade = 30;
        var cidade = "São Paulo";

        // ✅ RECOMENDADO: String Interpolation para casos simples
        var mensagem1 = $"Olá, {nome}! Você tem {idade} anos e mora em {cidade}.";
        _logger.LogInformation("String Interpolation: {Mensagem}", mensagem1);

        // ✅ ALTERNATIVA: String.Concat para performance máxima
        var mensagem2 = string.Concat("Olá, ", nome, "! Você tem ", idade.ToString(), " anos e mora em ", cidade, ".");
        _logger.LogInformation("String.Concat: {Mensagem}", mensagem2);

        // ❌ EVITAR: Concatenação com +
        var mensagem3 = "Olá, " + nome + "! Você tem " + idade + " anos e mora em " + cidade + ".";
        _logger.LogInformation("Concatenação com +: {Mensagem}", mensagem3);

        await Task.Delay(100);
    }

    /// <summary>
    /// Cenário 2: Concatenação em loop (muitos elementos)
    /// </summary>
    public async Task DemonstrarConcatenacaoLoop()
    {
        _logger.LogInformation("\n--- Cenário 2: Concatenação em Loop ---");

        var items = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

        // ✅ RECOMENDADO: StringBuilder para loops
        var sb = new StringBuilder();
        sb.AppendLine("Lista de items:");
        foreach (var item in items)
        {
            sb.AppendLine($"- {item}");
        }
        var resultado1 = sb.ToString();
        _logger.LogInformation("StringBuilder:\n{Resultado}", resultado1.TrimEnd());

        // ✅ ALTERNATIVA: String.Join para casos específicos
        var resultado2 = "Lista de items:\n" + string.Join("\n", items.Select(item => $"- {item}"));
        _logger.LogInformation("String.Join:\n{Resultado}", resultado2);

        // ❌ EVITAR: Concatenação com + em loop
        var resultado3 = "Lista de items:\n";
        foreach (var item in items)
        {
            resultado3 += $"- {item}\n";
        }
        _logger.LogInformation("Concatenação em loop:\n{Resultado}", resultado3.TrimEnd());

        await Task.Delay(100);
    }

    /// <summary>
    /// Cenário 3: Formatação complexa
    /// </summary>
    public async Task DemonstrarFormatacaoCompleta()
    {
        _logger.LogInformation("\n--- Cenário 3: Formatação Complexa ---");

        var produto = new Produto
        {
            Id = 1001,
            Nome = "Notebook Gamer",
            Preco = 2499.99m,
            DataCriacao = DateTime.Now,
            Categoria = "Eletrônicos"
        };

        // ✅ RECOMENDADO: String Interpolation com formatação
        var relatorio1 = $"""
            === RELATÓRIO DO PRODUTO ===
            ID: {produto.Id:D6}
            Nome: {produto.Nome}
            Preço: {produto.Preco:C}
            Data: {produto.DataCriacao:dd/MM/yyyy HH:mm}
            Categoria: {produto.Categoria}
            Status: {(produto.Preco > 1000 ? "Premium" : "Standard")}
            """;
        _logger.LogInformation("String Interpolation com formatação:\n{Relatorio}", relatorio1);

        // ✅ ALTERNATIVA: StringBuilder para casos complexos
        var sb = new StringBuilder();
        sb.AppendLine("=== RELATÓRIO DO PRODUTO ===");
        sb.AppendLine($"ID: {produto.Id:D6}");
        sb.AppendLine($"Nome: {produto.Nome}");
        sb.AppendLine($"Preço: {produto.Preco:C}");
        sb.AppendLine($"Data: {produto.DataCriacao:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Categoria: {produto.Categoria}");
        sb.AppendLine($"Status: {(produto.Preco > 1000 ? "Premium" : "Standard")}");
        var relatorio2 = sb.ToString();
        _logger.LogInformation("StringBuilder complexo:\n{Relatorio}", relatorio2.TrimEnd());

        await Task.Delay(100);
    }

    /// <summary>
    /// Cenário 4: Best practices por contexto
    /// </summary>
    public async Task DemonstrarBestPractices()
    {
        _logger.LogInformation("\n--- Cenário 4: Best Practices ---");

        var dados = Enumerable.Range(1, 1000).ToArray();

        // ✅ Para poucos elementos (< 5): String Interpolation
        var resumo = $"Processados {dados.Length} elementos entre {dados.Min()} e {dados.Max()}";
        _logger.LogInformation("Resumo: {Resumo}", resumo);

        // ✅ Para collections: String.Join
        var primeiros10 = string.Join(", ", dados.Take(10));
        _logger.LogInformation("Primeiros 10: {Primeiros}", primeiros10);

        // ✅ Para construção incremental: StringBuilder
        var sb = new StringBuilder(dados.Length * 10); // Pre-allocate capacity
        sb.AppendLine("Números pares:");
        
        var pares = dados.Where(x => x % 2 == 0).Take(20);
        foreach (var numero in pares)
        {
            sb.AppendLine($"  {numero}");
        }
        
        var resultado = sb.ToString();
        _logger.LogInformation("Números pares (StringBuilder):\n{Resultado}", resultado.TrimEnd());

        // ✅ Para templates: Raw String Literals (C# 11+)
        var template = $$"""
            {
              "summary": "{{resumo}}",
              "total": {{dados.Length}},
              "sample": [{{primeiros10}}]
            }
            """;
        _logger.LogInformation("JSON Template:\n{Template}", template);

        await Task.Delay(100);
    }
}

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Categoria { get; set; } = string.Empty;
}
