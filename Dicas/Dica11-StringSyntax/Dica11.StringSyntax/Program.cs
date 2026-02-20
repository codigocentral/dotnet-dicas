/*
 * Dica 11: Atributo StringSyntax para Destaque de Sintaxe (.NET 7+)
 * 
 * O atributo [StringSyntax] melhora significativamente a experiência do desenvolvedor
 * no IDE ao trabalhar com strings especiais como regex, JSON, SQL, URIs, etc.
 * 
 * Benefícios:
 * - Syntax highlighting automático no IDE
 * - Validação em tempo de desenvolvimento
 * - IntelliSense melhorado
 * - Detecção de erros antes da execução
 * 
 * Sintaxes suportadas: Regex, Json, Xml, Uri, DateTimeFormat, Guid, etc.
 */

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

Console.WriteLine("🎯 Dica 11: Atributo StringSyntax para Destaque de Texto (.NET 7+)");
Console.WriteLine("==================================================================");

// O atributo StringSyntax melhora a experiência do desenvolvedor no IDE
// fornecendo destaque de sintaxe e validação para strings especiais

Console.WriteLine("\n1. 📝 StringSyntax para Expressões Regulares:");
Console.WriteLine("────────────────────────────────────────────");

var regexService = new RegexService();
var testText = "john.doe@example.com e maria@test.org são emails válidos";

Console.WriteLine($"Texto de entrada: {testText}");

var emails = regexService.ExtractEmails(testText);
Console.WriteLine($"✅ Emails encontrados: {string.Join(", ", emails)}");

Console.WriteLine("\n2. 🌐 StringSyntax para URIs:");
Console.WriteLine("─────────────────────────────");

var urlService = new UrlService();
var baseUrl = "https://api.example.com";
var endpoint = "/users/{id}";

Console.WriteLine($"Base URL: {baseUrl}");
Console.WriteLine($"Endpoint: {endpoint}");

var fullUrl = urlService.BuildApiUrl(baseUrl, endpoint);
Console.WriteLine($"✅ URL completa: {fullUrl}");

Console.WriteLine("\n3. 📄 StringSyntax para JSON:");
Console.WriteLine("─────────────────────────────");

var jsonService = new JsonService();
var jsonTemplate = """
{
    "name": "{name}",
    "age": {age},
    "active": true
}
""";

Console.WriteLine("Template JSON:");
Console.WriteLine(jsonTemplate);

var processedJson = jsonService.ProcessJsonTemplate(jsonTemplate);
Console.WriteLine($"✅ JSON processado: {processedJson}");

Console.WriteLine("\n4. 🔍 StringSyntax para SQL:");
Console.WriteLine("─────────────────────────────");

var sqlService = new SqlService();
var userId = 123;

var user = sqlService.GetUserById(userId);
Console.WriteLine($"✅ Usuário encontrado: {user}");

Console.WriteLine("\n5. 🎨 StringSyntax para Formatação:");
Console.WriteLine("───────────────────────────────────");

var formattingService = new FormattingService();
var dateValue = DateTime.Now;
var numberValue = 1234.56m;

Console.WriteLine($"Data original: {dateValue}");
Console.WriteLine($"Número original: {numberValue}");

var formattedDate = formattingService.FormatDate(dateValue);
var formattedNumber = formattingService.FormatCurrency(numberValue);

Console.WriteLine($"✅ Data formatada: {formattedDate}");
Console.WriteLine($"✅ Número formatado: {formattedNumber}");

Console.WriteLine("\n🎓 Benefícios do StringSyntax:");
Console.WriteLine("• Destaque de sintaxe no IDE");
Console.WriteLine("• Validação automática em tempo de desenvolvimento");
Console.WriteLine("• IntelliSense melhorado");
Console.WriteLine("• Detecção de erros antes da execução");
Console.WriteLine("• Melhor experiência do desenvolvedor");

/// <summary>
/// Serviço que demonstra StringSyntax para expressões regulares
/// </summary>
public class RegexService
{
    /// <summary>
    /// Extrai emails usando regex com StringSyntax para destaque
    /// </summary>
    /// <param name="text">Texto para buscar emails</param>
    /// <returns>Lista de emails encontrados</returns>
    public List<string> ExtractEmails(string text)
    {
        // ✨ StringSyntax.Regex fornece destaque e validação no IDE
        return ExtractMatches(text, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
    }
    
    private List<string> ExtractMatches(string input, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        return regex.Matches(input).Cast<Match>().Select(m => m.Value).ToList();
    }
}

/// <summary>
/// Serviço que demonstra StringSyntax para URIs
/// </summary>
public class UrlService
{
    /// <summary>
    /// Constrói URL da API com StringSyntax para validação
    /// </summary>
    public string BuildApiUrl([StringSyntax(StringSyntaxAttribute.Uri)] string baseUrl, string endpoint)
    {
        return CombineUrls(baseUrl, endpoint);
    }
    
    private string CombineUrls([StringSyntax(StringSyntaxAttribute.Uri)] string baseUrl, string path)
    {
        return new Uri(new Uri(baseUrl), path).ToString();
    }
}

/// <summary>
/// Serviço que demonstra StringSyntax para JSON
/// </summary>
public class JsonService
{
    /// <summary>
    /// Processa template JSON com StringSyntax para destaque
    /// </summary>
    public string ProcessJsonTemplate([StringSyntax(StringSyntaxAttribute.Json)] string template)
    {
        return ProcessJson(template);
    }
    
    private string ProcessJson([StringSyntax(StringSyntaxAttribute.Json)] string jsonTemplate)
    {
        // Simula processamento do template JSON
        return jsonTemplate
            .Replace("{name}", "João Silva")
            .Replace("{age}", "30");
    }
}

/// <summary>
/// Serviço que demonstra StringSyntax customizado
/// </summary>
public class SqlService
{
    /// <summary>
    /// Busca usuário por ID usando SQL com StringSyntax customizado
    /// </summary>
    public string GetUserById(int userId)
    {
        var query = "SELECT * FROM Users WHERE Id = @userId";
        return ExecuteQuery(query, userId);
    }
    
    private string ExecuteQuery([StringSyntax("sql")] string query, object parameter)
    {
        // Simula execução da query
        Console.WriteLine($"  📝 Executando SQL: {query}");
        Console.WriteLine($"  📊 Parâmetro: {parameter}");
        return $"User_{parameter}";
    }
}

/// <summary>
/// Serviço que demonstra StringSyntax para formatação
/// </summary>
public class FormattingService
{
    /// <summary>
    /// Formata data com StringSyntax para formato
    /// </summary>
    public string FormatDate(DateTime date)
    {
        return FormatValue(date, "dd/MM/yyyy HH:mm:ss");
    }
    
    /// <summary>
    /// Formata moeda com StringSyntax para formato
    /// </summary>
    public string FormatCurrency(decimal value)
    {
        return FormatValue(value, "C2");
    }
    
    private string FormatValue(IFormattable value, [StringSyntax("format")] string format)
    {
        return value.ToString(format, System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }
}
