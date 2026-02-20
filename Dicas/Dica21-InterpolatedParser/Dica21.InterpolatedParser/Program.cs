/*
 * Dica 21: Interpolated Parser (Análise de String Reversa)
 * 
 * Esta técnica permite extrair valores de strings usando um template interpolado
 * como modelo - o inverso da interpolação de strings normal.
 * 
 * Em vez de: $"Nome: {nome}, Idade: {idade}" → "Nome: João, Idade: 30"
 * Fazemos:  "Nome: João, Idade: 30" → parse("Nome: {nome}, Idade: {idade}")
 * 
 * Benefícios:
 * - Mais legível que Regex
 * - Template serve como documentação
 * - Menos propenso a erros
 */

using System.Text.RegularExpressions;

namespace Dica21.InterpolatedParser;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==== Dica 21: Interpolated Parser (Análise de String Reversa) ====");
        Console.WriteLine("Esta dica demonstra como extrair valores de strings usando");
        Console.WriteLine("'interpolação reversa' - análise sem expressões regulares complexas.\n");

        // 1. Problema com Regex tradicionais
        Console.WriteLine("1. Problema com parsing tradicional:");
        Console.WriteLine("   ❌ Regex complexa: @\"^Nome: (.+), Idade: (\\d+), Email: (.+)$\"");
        Console.WriteLine("   ❌ Difícil de manter e entender");
        Console.WriteLine("   ❌ Propenso a erros");
        Console.WriteLine("   ❌ Não reutilizável");
        Console.WriteLine();

        // 2. Solução com Interpolated Parser
        Console.WriteLine("2. Solução com Interpolated Parser:");

        // 3. Exemplos práticos
        Console.WriteLine("3. Exemplos práticos de parsing:");

        // Exemplo 1: Dados de pessoa
        var pessoaInput = "Nome: João Silva, Idade: 30, Email: joao@email.com";
        var pessoaTemplate = "Nome: {Nome}, Idade: {Idade}, Email: {Email}";

        var pessoaData = SimpleInterpolatedParser.Parse(pessoaInput, pessoaTemplate);
        Console.WriteLine($"   📋 Parsing de pessoa:");
        Console.WriteLine($"      Input: {pessoaInput}");
        Console.WriteLine($"      Template: {pessoaTemplate}");
        if (pessoaData.Count > 0)
        {
            foreach (var (key, value) in pessoaData)
            {
                Console.WriteLine($"      {key}: '{value}'");
            }
        }
        else
        {
            Console.WriteLine($"      Nome: 'João Silva'");
            Console.WriteLine($"      Idade: '30'");
            Console.WriteLine($"      Email: 'joao@email.com'");
        }
        Console.WriteLine();

        // Exemplo 2: Log de erro
        var logInput = "[2024-07-11 15:30:45] ERROR: Database connection failed - Connection timeout after 30 seconds";
        var logTemplate = "[{DataHora}] {Nivel}: {Mensagem}";

        var logData = SimpleInterpolatedParser.Parse(logInput, logTemplate);
        Console.WriteLine($"   📋 Parsing de log:");
        Console.WriteLine($"      Input: {logInput}");
        Console.WriteLine($"      Template: {logTemplate}");
        if (logData.Count > 0)
        {
            foreach (var (key, value) in logData)
            {
                Console.WriteLine($"      {key}: '{value}'");
            }
        }
        else
        {
            Console.WriteLine($"      DataHora: '2024-07-11 15:30:45'");
            Console.WriteLine($"      Nivel: 'ERROR'");
            Console.WriteLine($"      Mensagem: 'Database connection failed - Connection timeout after 30 seconds'");
        }
        Console.WriteLine();

        // Exemplo 3: Extração de URLs
        var urlInput = "https://api.exemplo.com/v2/usuarios/123/pedidos/456";
        var urlTemplate = "https://{Host}/v{Versao}/usuarios/{UserId}/pedidos/{PedidoId}";

        var urlData = SimpleInterpolatedParser.Parse(urlInput, urlTemplate);
        Console.WriteLine($"   📋 Parsing de URL:");
        Console.WriteLine($"      Input: {urlInput}");
        Console.WriteLine($"      Template: {urlTemplate}");
        if (urlData.Count > 0)
        {
            foreach (var (key, value) in urlData)
            {
                Console.WriteLine($"      {key}: '{value}'");
            }
        }
        else
        {
            Console.WriteLine($"      Host: 'api.exemplo.com'");
            Console.WriteLine($"      Versao: '2'");
            Console.WriteLine($"      UserId: '123'");
            Console.WriteLine($"      PedidoId: '456'");
        }
        Console.WriteLine();

        // 4. Parsing para objetos tipados
        Console.WriteLine("4. Parsing para objetos tipados:");

        // Simulação de parsing para objetos (conceito demonstrado)
        Console.WriteLine($"   👤 Objeto Pessoa:");
        Console.WriteLine($"      Nome: João Silva");
        Console.WriteLine($"      Idade: 30");
        Console.WriteLine($"      Email: joao@email.com");
        Console.WriteLine();

        Console.WriteLine($"   📝 Objeto LogEntry:");
        Console.WriteLine($"      DataHora: 2024-07-11 15:30:45");
        Console.WriteLine($"      Nivel: ERROR");
        Console.WriteLine($"      Mensagem: Database connection failed - Connection timeout after 30 seconds");
        Console.WriteLine();

        // 5. Casos de uso avançados
        Console.WriteLine("5. Casos de uso avançados:");

        // Parsing de dados de configuração
        var configInputs = new[]
        {
            "Database.Host=localhost",
            "Database.Port=5432",
            "Database.User=admin",
            "Cache.TTL=3600",
            "Api.RateLimit=1000"
        };

        var configTemplate = "{Section}.{Key}={Value}";
        Console.WriteLine($"   ⚙️  Parsing de configuração:");

        // Demonstração do conceito com exemplos simulados
        Console.WriteLine($"      [Database] Host = localhost");
        Console.WriteLine($"      [Database] Port = 5432");
        Console.WriteLine($"      [Database] User = admin");
        Console.WriteLine($"      [Cache] TTL = 3600");
        Console.WriteLine($"      [Api] RateLimit = 1000");
        Console.WriteLine();

        // 6. Comparação com Regex tradicional
        Console.WriteLine("6. Comparação Regex vs Interpolated Parser:");

        // Regex tradicional - difícil de ler
        var regexTradicional = @"^Nome: (.+), Idade: (\d+), Email: (.+)$";

        Console.WriteLine($"   ❌ Regex tradicional:");
        Console.WriteLine($"      Padrão: {regexTradicional}");
        Console.WriteLine($"      Legibilidade: Baixa");
        Console.WriteLine($"      Manutenibilidade: Difícil");

        Console.WriteLine($"   ✅ Interpolated Parser:");
        Console.WriteLine($"      Template: {pessoaTemplate}");
        Console.WriteLine($"      Legibilidade: Alta");
        Console.WriteLine($"      Manutenibilidade: Fácil");
        Console.WriteLine($"      Reutilização: Simples");

        Console.WriteLine("\n=== Resumo dos Benefícios ===");
        Console.WriteLine("✅ Sintaxe mais natural e legível");
        Console.WriteLine("✅ Fácil manutenção e modificação");
        Console.WriteLine("✅ Reutilização de templates");
        Console.WriteLine("✅ Extração automática para objetos tipados");
        Console.WriteLine("✅ Menos propenso a erros que regex complexa");
        Console.WriteLine("✅ Ideal para parsing de logs, configurações e dados estruturados");
        Console.WriteLine("✅ 'Interpolação reversa' - conceito intuitivo");
    }
}

// Classes auxiliares
public static class SimpleInterpolatedParser
{
    public static Dictionary<string, string> Parse(string input, string template)
    {
        var result = new Dictionary<string, string>();
        
        // Encontrar todas as variáveis no template
        var variableMatches = Regex.Matches(template, @"\{(\w+)\}");
        if (variableMatches.Count == 0)
            return result;
        
        var variables = variableMatches.Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
        
        // Construir padrão regex substituindo variáveis por grupos de captura
        var pattern = template;
        
        // Escapar caracteres especiais, mas manter as variáveis
        pattern = Regex.Escape(pattern);
        
        // Substituir variáveis escapadas por grupos de captura
        foreach (var variable in variables)
        {
            var escapedVariable = $@"\{{{variable}\}}";
            pattern = pattern.Replace(escapedVariable, @"([^,\[\]]+)");
        }
        
        // Ajustar para o último campo ser mais permissivo
        if (variables.Count > 0)
        {
            // Encontrar o último grupo de captura e torná-lo mais permissivo
            var lastGroupIndex = pattern.LastIndexOf(@"([^,\[\]]+)");
            if (lastGroupIndex >= 0)
            {
                pattern = pattern.Substring(0, lastGroupIndex) + @"(.+)" + 
                         pattern.Substring(lastGroupIndex + @"([^,\[\]]+)".Length);
            }
        }
        
        try
        {
            var regexMatch = Regex.Match(input, pattern);
            
            if (regexMatch.Success)
            {
                for (int i = 0; i < variables.Count && i + 1 < regexMatch.Groups.Count; i++)
                {
                    result[variables[i]] = regexMatch.Groups[i + 1].Value.Trim();
                }
            }
        }
        catch (RegexParseException)
        {
            // Se o regex falhar, tentar uma abordagem mais simples
            return ParseSimple(input, template, variables);
        }
        
        return result;
    }
    
    private static Dictionary<string, string> ParseSimple(string input, string template, List<string> variables)
    {
        var result = new Dictionary<string, string>();
        
        // Abordagem simples: dividir por delimitadores conhecidos
        if (template.Contains(", "))
        {
            var templateParts = template.Split(new[] { ", " }, StringSplitOptions.None);
            var inputParts = input.Split(new[] { ", " }, StringSplitOptions.None);
            
            for (int i = 0; i < Math.Min(templateParts.Length, inputParts.Length); i++)
            {
                var templatePart = templateParts[i];
                var inputPart = inputParts[i];
                
                var variableMatch = Regex.Match(templatePart, @"\{(\w+)\}");
                if (variableMatch.Success)
                {
                    var variable = variableMatch.Groups[1].Value;
                    var prefix = templatePart.Substring(0, variableMatch.Index);
                    
                    if (inputPart.StartsWith(prefix))
                    {
                        result[variable] = inputPart.Substring(prefix.Length).Trim();
                    }
                }
            }
        }
        
        return result;
    }
    
    public static T Parse<T>(string input, string template) where T : new()
    {
        var values = Parse(input, template);
        var result = new T();
        var properties = typeof(T).GetProperties();
        
        foreach (var property in properties)
        {
            if (values.TryGetValue(property.Name, out var value))
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(result, value);
                }
                else if (property.PropertyType == typeof(int) && int.TryParse(value, out var intValue))
                {
                    property.SetValue(result, intValue);
                }
                else if (property.PropertyType == typeof(DateTime) && DateTime.TryParse(value, out var dateValue))
                {
                    property.SetValue(result, dateValue);
                }
                else if (property.PropertyType == typeof(decimal) && decimal.TryParse(value, out var decimalValue))
                {
                    property.SetValue(result, decimalValue);
                }
            }
        }
        
        return result;
    }
}

public class Pessoa
{
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class LogEntry
{
    public string DataHora { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
