using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dica41.InterpolatedStrings;

/// <summary>
/// Demonstra o uso avançado de Interpolated Strings e StringBuilder para performance otimizada
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<DemoService>();
            })
            .Build();

        var demo = host.Services.GetRequiredService<DemoService>();
        demo.ExecutarTodasDemonstracoes();
    }
}

public class DemoService
{
    private readonly ILogger<DemoService> _logger;

    public DemoService(ILogger<DemoService> logger)
    {
        _logger = logger;
    }

    public Task ExecutarTodasDemonstracoes()
    {
        Console.WriteLine("===== Dica 41: Interpolated Strings e StringBuilder - Performance Otimizada ====\n");

        Console.WriteLine("🔤 1. INTERPOLATED STRINGS BÁSICO");
        Console.WriteLine("─────────────────────────────────────");
        DemonstrarInterpolatedStringsBasico();
        Console.WriteLine("✅ Demonstração básica concluída\n");

        Console.WriteLine("⚡ 2. INTERPOLATED STRING HANDLERS");
        Console.WriteLine("──────────────────────────────────────");
        DemonstrarStringHandlers();
        Console.WriteLine("✅ Demonstração de handlers concluída\n");

        Console.WriteLine("🔧 3. FORMATAÇÃO AVANÇADA");
        Console.WriteLine("────────────────────────────");
        DemonstrarFormatacaoAvancada();
        Console.WriteLine("✅ Demonstração de formatação concluída\n");

        Console.WriteLine("🏗️ 4. STRINGBUILDER OTIMIZADO");
        Console.WriteLine("─────────────────────────────────");
        DemonstrarStringBuilderOtimizado();
        Console.WriteLine("✅ Demonstração de StringBuilder concluída\n");

        Console.WriteLine("🎯 5. CUSTOM INTERPOLATED STRING HANDLER");
        Console.WriteLine("──────────────────────────────────────────");
        DemonstrarCustomHandler();
        Console.WriteLine("✅ Demonstração de handler customizado concluída\n");

        Console.WriteLine("📊 6. BENCHMARKS DE PERFORMANCE");
        Console.WriteLine("────────────────────────────────");
        ExecutarBenchmarks();
        Console.WriteLine("✅ Benchmarks concluídos\n");

        Console.WriteLine("📋 7. RESUMO DAS BOAS PRÁTICAS");
        Console.WriteLine("─────────────────────────────────");
        ExibirBoasPraticas();

        Console.WriteLine("\n=== Demonstração concluída ===");
        return Task.CompletedTask;
    }

    private void DemonstrarInterpolatedStringsBasico()
    {
        _logger.LogInformation("🔤 Demonstrando Interpolated Strings básico...");

        var nome = "João Silva";
        var idade = 30;
        var salario = 5500.75m;
        var dataContratacao = new DateTime(2020, 3, 15);

        // Interpolação básica
        var mensagem1 = $"Funcionário: {nome}, Idade: {idade} anos";
        _logger.LogInformation("📝 Básico: {mensagem}", mensagem1);

        // Formatação inline
        var mensagem2 = $"Salário: {salario:C} - Contratado em: {dataContratacao:dd/MM/yyyy}";
        _logger.LogInformation("📝 Formatado: {mensagem}", mensagem2);

        // Expressões dentro da interpolação
        var mensagem3 = $"Em 5 anos terá {idade + 5} anos e ganhará {salario * 1.5m:C}";
        _logger.LogInformation("📝 Expressões: {mensagem}", mensagem3);

        // Interpolação condicional
        var status = idade >= 18 ? "Maior de idade" : "Menor de idade";
        var mensagem4 = $"Status: {(idade >= 18 ? "Adulto" : "Jovem")} - {status}";
        _logger.LogInformation("📝 Condicional: {mensagem}", mensagem4);

        // Verbatim + Interpolated
        var caminho = @$"C:\Users\{nome.Replace(" ", "")}\Documents\arquivo_{DateTime.Now:yyyyMMdd}.txt";
        _logger.LogInformation("📁 Caminho: {caminho}", caminho);

        // Raw string literals + Interpolated (C# 11+)
        var json = $$"""
        {
            "nome": "{{nome}}",
            "idade": {{idade}},
            "salario": {{salario}},
            "contratacao": "{{dataContratacao:yyyy-MM-dd}}"
        }
        """;
        _logger.LogInformation("📄 JSON:\n{json}", json);
    }

    private void DemonstrarStringHandlers()
    {
        _logger.LogInformation("⚡ Demonstrando String Handlers...");

        var produtos = new List<Produto>
        {
            new("Notebook", 2500.00m, "Eletrônicos"),
            new("Mouse", 45.90m, "Acessórios"),
            new("Teclado", 180.50m, "Acessórios")
        };

        // DefaultInterpolatedStringHandler para otimização
        var sb = new DefaultInterpolatedStringHandler();
        sb.AppendLiteral("Produtos disponíveis:");

        foreach (var produto in produtos)
        {
            sb.AppendLiteral("\n- ");
            sb.AppendFormatted(produto.Nome);
            sb.AppendLiteral(" (");
            sb.AppendFormatted(produto.Categoria);
            sb.AppendLiteral("): ");
            sb.AppendFormatted(produto.Preco, "C");
        }

        var resultado = sb.ToStringAndClear();
        _logger.LogInformation("📋 Lista de produtos:\n{resultado}", resultado);

        // Comparação com interpolação tradicional
        var tradicional = $"Total de produtos: {produtos.Count}";
        
        // Usando handler manual
        var handler = new DefaultInterpolatedStringHandler(18, 1);
        handler.AppendLiteral("Total de produtos: ");
        handler.AppendFormatted(produtos.Count);
        var comHandler = handler.ToStringAndClear();

        _logger.LogInformation("📊 Tradicional: {tradicional}", tradicional);
        _logger.LogInformation("📊 Com Handler: {comHandler}", comHandler);
    }

    private void DemonstrarFormatacaoAvancada()
    {
        _logger.LogInformation("🔧 Demonstrando formatação avançada...");

        var numero = 1234567.89;
        var data = DateTime.Now;
        var porcentagem = 0.156789;
        var guid = Guid.NewGuid();

        // Formatação numérica
        var formatosNumericos = $"""
        Número: {numero}
        Moeda: {numero:C}
        Decimal (2 casas): {numero:F2}
        Científica: {numero:E}
        Percentual: {porcentagem:P2}
        Hexadecimal: {(int)numero:X}
        """;
        _logger.LogInformation("📊 Formatos numéricos:\n{formatos}", formatosNumericos);

        // Formatação de data/hora
        var formatosData = $"""
        Padrão: {data}
        Curta: {data:d}
        Longa: {data:D}
        Hora: {data:T}
        ISO 8601: {data:yyyy-MM-ddTHH:mm:ss.fffZ}
        Personalizada: {data:dddd, dd 'de' MMMM 'de' yyyy 'às' HH:mm}
        """;
        _logger.LogInformation("📅 Formatos de data:\n{formatos}", formatosData);

        // Alinhamento e preenchimento
        var nomes = new[] { "Ana", "João Pedro", "Carlos", "Maria Fernanda" };
        var alinhamentos = new StringBuilder();
        alinhamentos.AppendLine("Alinhamentos:");
        
        foreach (var nome in nomes)
        {
            alinhamentos.AppendLine($"Esquerda: |{nome,-15}| Direita: |{nome,15}| Centro: |{nome,15}|");
        }
        
        _logger.LogInformation("📐 {alinhamentos}", alinhamentos.ToString());

        // Formatação condicional
        var valores = new[] { -10, 0, 15, 100 };
        foreach (var valor in valores)
        {
            var formato = valor switch
            {
                < 0 => $"Negativo: {valor:C} (vermelho)",
                0 => $"Zero: {valor:F0} (neutro)",
                > 0 and <= 50 => $"Baixo: {valor:F0} (amarelo)",
                _ => $"Alto: {valor:F0} (verde)"
            };
            _logger.LogInformation("🎨 {formato}", formato);
        }

        // GUID formatação
        _logger.LogInformation("🔑 GUID: {guid:D}", guid);
        _logger.LogInformation("🔑 GUID curto: {guid:N}", guid);
    }

    private void DemonstrarStringBuilderOtimizado()
    {
        _logger.LogInformation("🏗️ Demonstrando StringBuilder otimizado...");

        // StringBuilder com capacidade inicial
        var sb = new StringBuilder(1000); // Evita realocações

        // Construção eficiente de relatório
        sb.AppendLine("=== RELATÓRIO DE VENDAS ===");
        sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();

        var vendas = new[]
        {
            new Venda("Produto A", 10, 25.50m),
            new Venda("Produto B", 5, 120.00m),
            new Venda("Produto C", 15, 8.75m)
        };

        decimal totalGeral = 0;
        sb.AppendLine("ITENS VENDIDOS:");
        sb.AppendLine(new string('-', 50));

        foreach (var venda in vendas)
        {
            var subtotal = venda.Quantidade * venda.PrecoUnitario;
            totalGeral += subtotal;
            
            // Formatação alinhada
            sb.AppendFormat("{0,-20} {1,3} x {2,8:C} = {3,10:C}", 
                venda.Produto, venda.Quantidade, venda.PrecoUnitario, subtotal);
            sb.AppendLine();
        }

        sb.AppendLine(new string('-', 50));
        sb.AppendLine($"{"TOTAL GERAL:",-35} {totalGeral,10:C}");

        var relatorio = sb.ToString();
        _logger.LogInformation("📊 Relatório:\n{relatorio}", relatorio);

        // StringBuilder com interpolação (C# 10+)
        sb.Clear();
        sb.AppendLine($"Resumo executivo gerado em {DateTime.Now:HH:mm}");
        sb.AppendLine($"Total de itens: {vendas.Length}");
        sb.AppendLine($"Ticket médio: {totalGeral / vendas.Length:C}");
        
        _logger.LogInformation("📋 Resumo:\n{resumo}", sb.ToString());

        // Operações avançadas
        sb.Clear();
        sb.Append("StringBuilder: ");
        sb.AppendJoin(", ", vendas.Select(v => v.Produto));
        sb.Replace("Produto", "Item");
        sb.Insert(0, "Lista -> ");
        
        _logger.LogInformation("🔧 Operações: {resultado}", sb.ToString());
    }

    private void DemonstrarCustomHandler()
    {
        _logger.LogInformation("🎯 Demonstrando custom interpolated string handler...");

        // Usar o handler customizado
        LogMessage($"Usuário {Environment.UserName} logou em {DateTime.Now:HH:mm}");
        LogMessage($"Processando {1000} registros...");
        LogMessage($"Erro: Arquivo não encontrado em {@"C:\temp\dados.txt"}");

        // Handler para SQL (simulado)
        var userId = 123;
        var status = "ativo";
        var sql = CreateSqlQuery($"SELECT * FROM usuarios WHERE id = {userId} AND status = '{status}'");
        _logger.LogInformation("🗃️ SQL gerado: {sql}", sql);

        // Handler para logging condicional
        var debug = false;
        DebugLog(debug, $"Debug info: memória = {GC.GetTotalMemory(false)} bytes");
        
        debug = true;
        DebugLog(debug, $"Debug info: memória = {GC.GetTotalMemory(false)} bytes");
    }

    // Custom handler para logging
    private void LogMessage([InterpolatedStringHandlerArgument("")] LogInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            _logger.LogInformation("📝 [LOG] {message}", handler.GetFormattedText());
        }
    }

    // Custom handler para SQL
    private string CreateSqlQuery([InterpolatedStringHandlerArgument("")] SqlInterpolatedStringHandler handler)
    {
        return handler.GetFormattedText();
    }

    // Handler para debug condicional
    private void DebugLog(bool enabled, [InterpolatedStringHandlerArgument("enabled")] DebugInterpolatedStringHandler handler)
    {
        if (handler.IsEnabled)
        {
            _logger.LogInformation("🐛 [DEBUG] {message}", handler.GetFormattedText());
        }
    }

    private void ExecutarBenchmarks()
    {
        _logger.LogInformation("📊 Executando benchmarks de performance...");

        const int iterations = 100_000;
        var dados = Enumerable.Range(1, 100).ToArray();

        // Benchmark: Concatenação vs Interpolação vs StringBuilder
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var _ = "Valor: " + dados[i % dados.Length].ToString();
        }
        sw.Stop();
        var tempoConcatenacao = sw.ElapsedMilliseconds;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var _ = $"Valor: {dados[i % dados.Length]}";
        }
        sw.Stop();
        var tempoInterpolacao = sw.ElapsedMilliseconds;

        sw.Restart();
        var sb = new StringBuilder();
        for (int i = 0; i < iterations; i++)
        {
            sb.Clear();
            sb.Append("Valor: ");
            sb.Append(dados[i % dados.Length]);
            var _ = sb.ToString();
        }
        sw.Stop();
        var tempoStringBuilder = sw.ElapsedMilliseconds;

        // Benchmark: StringBuilder vs Interpolação para múltiplas operações
        sw.Restart();
        for (int i = 0; i < iterations / 10; i++)
        {
            var resultado = "";
            for (int j = 0; j < 10; j++)
            {
                resultado += $"Item {j}: {dados[j]}\n";
            }
        }
        sw.Stop();
        var tempoInterpolacaoMultipla = sw.ElapsedMilliseconds;

        sw.Restart();
        var sbMultiplo = new StringBuilder(200);
        for (int i = 0; i < iterations / 10; i++)
        {
            sbMultiplo.Clear();
            for (int j = 0; j < 10; j++)
            {
                sbMultiplo.AppendLine($"Item {j}: {dados[j]}");
            }
            var _ = sbMultiplo.ToString();
        }
        sw.Stop();
        var tempoStringBuilderMultiplo = sw.ElapsedMilliseconds;

        _logger.LogInformation("⏱️ Concatenação: {tempo}ms", tempoConcatenacao);
        _logger.LogInformation("⏱️ Interpolação: {tempo}ms", tempoInterpolacao);
        _logger.LogInformation("⏱️ StringBuilder: {tempo}ms", tempoStringBuilder);
        _logger.LogInformation("⏱️ Interpolação múltipla: {tempo}ms", tempoInterpolacaoMultipla);
        _logger.LogInformation("⏱️ StringBuilder múltiplo: {tempo}ms", tempoStringBuilderMultiplo);

        _logger.LogInformation("🏆 Para operação única, interpolação é {melhoria:F1}x mais rápida que concatenação", 
            (double)tempoConcatenacao / tempoInterpolacao);
        _logger.LogInformation("🏆 Para múltiplas operações, StringBuilder é {melhoria:F1}x mais rápido que interpolação", 
            (double)tempoInterpolacaoMultipla / tempoStringBuilderMultiplo);
    }

    private void ExibirBoasPraticas()
    {
        Console.WriteLine("✅ Use interpolação ($\"\") para strings simples");
        Console.WriteLine("✅ Use StringBuilder para construção iterativa");
        Console.WriteLine("✅ Especifique capacidade inicial no StringBuilder");
        Console.WriteLine("✅ Use formatação inline para números e datas");
        Console.WriteLine("✅ Combine verbatim (@) com interpolação quando necessário");
        Console.WriteLine("✅ Use raw strings (C# 11+) para conteúdo complexo");
        Console.WriteLine("✅ Implemente handlers customizados para casos especiais");
        Console.WriteLine("✅ Prefira Clear() em vez de new StringBuilder()");
        Console.WriteLine();
        Console.WriteLine("🎯 QUANDO USAR CADA ABORDAGEM");
        Console.WriteLine("──────────────────────────────────");
        Console.WriteLine("🔹 Interpolação: 1-3 operações de string");
        Console.WriteLine("🔹 StringBuilder: Múltiplas concatenações em loop");
        Console.WriteLine("🔹 Custom handlers: Logging condicional, SQL building");
        Console.WriteLine("🔹 Formatação avançada: Relatórios, templates");
    }
}

// Modelos de dados
public record Produto(string Nome, decimal Preco, string Categoria);
public record Venda(string Produto, int Quantidade, decimal PrecoUnitario);

// Custom Interpolated String Handlers
[InterpolatedStringHandler]
public ref struct LogInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _handler;
    public bool IsEnabled { get; }

    public LogInterpolatedStringHandler(int literalLength, int formattedCount, object receiver)
    {
        IsEnabled = true; // Poderia verificar nível de log
        _handler = IsEnabled ? new DefaultInterpolatedStringHandler(literalLength, formattedCount) : default;
    }

    public void AppendLiteral(string value)
    {
        if (IsEnabled)
            _handler.AppendLiteral(value);
    }

    public void AppendFormatted<T>(T value)
    {
        if (IsEnabled)
            _handler.AppendFormatted(value);
    }

    public void AppendFormatted<T>(T value, string format)
    {
        if (IsEnabled)
            _handler.AppendFormatted(value, format);
    }

    public string GetFormattedText()
    {
        return IsEnabled ? _handler.ToStringAndClear() : string.Empty;
    }
}

[InterpolatedStringHandler]
public ref struct SqlInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _handler;

    public SqlInterpolatedStringHandler(int literalLength, int formattedCount, object receiver)
    {
        _handler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
    }

    public void AppendLiteral(string value)
    {
        _handler.AppendLiteral(value);
    }

    public void AppendFormatted<T>(T value)
    {
        // Poderia aplicar sanitização SQL aqui
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.ToStringAndClear();
    }
}

[InterpolatedStringHandler]
public ref struct DebugInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _handler;
    public bool IsEnabled { get; }

    public DebugInterpolatedStringHandler(int literalLength, int formattedCount, bool enabled)
    {
        IsEnabled = enabled;
        _handler = IsEnabled ? new DefaultInterpolatedStringHandler(literalLength, formattedCount) : default;
    }

    public void AppendLiteral(string value)
    {
        if (IsEnabled)
            _handler.AppendLiteral(value);
    }

    public void AppendFormatted<T>(T value)
    {
        if (IsEnabled)
            _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return IsEnabled ? _handler.ToStringAndClear() : string.Empty;
    }
}
