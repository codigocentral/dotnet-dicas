/*
 * Dica 20: Validando Naughty Strings
 * 
 * "Naughty Strings" são strings especialmente criadas para testar a robustez de aplicações.
 * Elas incluem caracteres especiais, padrões de ataque (SQL Injection, XSS), 
 * unicode problemático, e edge cases que podem causar crashes.
 * 
 * Importante para:
 * - Testes de segurança automatizados
 * - Validação de entrada robusta
 * - Prevenção de vulnerabilidades
 * - Teste de edge cases de encoding/parsing
 */

using System.Text.Json;
using System.Text.RegularExpressions;

Console.WriteLine("==== Dica 20: Validando Naughty Strings ====");
Console.WriteLine("Esta dica demonstra como validar entrada de usuário contra");
Console.WriteLine("'naughty strings' - strings maliciosas que podem causar crashes");
Console.WriteLine("ou expor vulnerabilidades de segurança.\n");

// 1. O que são Naughty Strings
Console.WriteLine("1. O que são 'Naughty Strings':");
Console.WriteLine("   • Strings que podem causar crash no servidor");
Console.WriteLine("   • Strings que podem expor vulnerabilidades de segurança");
Console.WriteLine("   • Strings que podem escapar de validações simples");
Console.WriteLine("   • Strings que testam edge cases de parsing e encoding");
Console.WriteLine("   • Strings que simulam ataques comuns (XSS, SQL Injection, etc.)");
Console.WriteLine();

// 2. Obtendo Naughty Strings
Console.WriteLine("2. Obtendo Naughty Strings via NuGet:");

// Usando o pacote NaughtyStrings
var naughtyStrings = new List<string>();

// Simulando naughty strings (já que o pacote pode não estar disponível)
// Estas são versões simplificadas das strings reais do repositório
var commonNaughtyStrings = new[]
{
    // Strings vazias e com espaços
    "",
    " ",
    "   ",
    "\t",
    "\n",
    "\r\n",
    
    // Unicode problemático
    "𝐓𝐞𝐬𝐭",
    "🔥💯🚀",
    "؀؁؂؃؅؆؇؈؉؊؋",
    
    // Strings SQL Injection
    "'; DROP TABLE users; --",
    "1' OR '1'='1",
    "admin'; DELETE FROM users WHERE 't' = 't",
    "'; SELECT * FROM users; --",
    
    // XSS attempts
    "<script>alert('XSS')</script>",
    "javascript:alert('XSS')",
    "<img src=x onerror=alert('XSS')>",
    "<svg onload=alert('XSS')>",
    
    // Formato de dados
    "null",
    "NULL",
    "undefined",
    "true",
    "false",
    "0",
    "-1",
    "1.7976931348623157E+308", // Double.MaxValue
    
    // Caracteres especiais
    "../../etc/passwd",
    "../../../windows/system32",
    "%00",
    "%0A",
    "%22",
    
    // Strings muito longas
    new string('A', 1000),
    new string('X', 10000),
    
    // JSON problemático
    "{\"test\": true}",
    "[1,2,3]",
    "}{",
    
    // Regex problemáticos
    "(.*)*",
    "(?:(?:(?:(?:(?:.*)*)*)*)*)*",
    
    // Encoding issues
    "café",
    "naïve",
    "résumé",
    "Москва",
    "北京",
    "🇺🇸🇧🇷🇫🇷",
    
    // Control characters
    "\u0000", // Null
    "\u0001", // Start of Heading
    "\u0002", // Start of Text
    "\u0008", // Backspace
    "\u000B", // Vertical Tab
    "\u000C", // Form Feed
    "\u000E", // Shift Out
    "\u000F", // Shift In
    "\u007F", // Delete
    
    // Numbers as strings
    "0.0",
    "1.0",
    "-1.0",
    "1e+100",
    "∞",
    "NaN",
    
    // OS Command injection
    "; ls -la",
    "| cat /etc/passwd",
    "&& rm -rf /",
    "$(rm -rf /)",
    "`rm -rf /`"
};

naughtyStrings.AddRange(commonNaughtyStrings);

Console.WriteLine($"   • Total de naughty strings carregadas: {naughtyStrings.Count}");
Console.WriteLine();

// 3. Validação de entrada de usuário
Console.WriteLine("3. Testando validação de entrada de usuário:");

// Função de validação simples (vulnerável)
static bool ValidacaoSimples(string input)
{
    return !string.IsNullOrEmpty(input) && input.Length < 100;
}

// Função de validação robusta
static ValidationResult ValidacaoRobusta(string input)
{
    var result = new ValidationResult();
    
    // Verificar null/empty
    if (string.IsNullOrEmpty(input))
    {
        result.IsValid = false;
        result.Errors.Add("Input não pode ser nulo ou vazio");
        return result;
    }
    
    // Verificar comprimento
    if (input.Length > 1000)
    {
        result.IsValid = false;
        result.Errors.Add($"Input muito longo: {input.Length} caracteres");
        return result;
    }
    
    // Verificar caracteres de controle perigosos
    if (input.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
    {
        result.IsValid = false;
        result.Errors.Add("Input contém caracteres de controle perigosos");
        return result;
    }
    
    // Verificar padrões de SQL Injection
    var sqlPatterns = new[]
    {
        @"('|(\')|;|\|\||--|/\*|\*/)",
        @"\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT|MERGE|SELECT|UNION|UPDATE)\b",
        @"\b(AND|OR)\b.{1,6}?(=|>|<|!=|<>|<=|>=)",
    };
    
    foreach (var pattern in sqlPatterns)
    {
        if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
        {
            result.IsValid = false;
            result.Errors.Add("Possível tentativa de SQL Injection detectada");
            break;
        }
    }
    
    // Verificar padrões XSS
    var xssPatterns = new[]
    {
        @"<\s*script\b[^<]*(?:(?!</\s*script\s*>)<[^<]*)*</\s*script\s*>",
        @"javascript\s*:",
        @"on\w+\s*=",
        @"<\s*iframe\b",
        @"<\s*object\b",
        @"<\s*embed\b"
    };
    
    foreach (var pattern in xssPatterns)
    {
        if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
        {
            result.IsValid = false;
            result.Errors.Add("Possível tentativa de XSS detectada");
            break;
        }
    }
    
    // Verificar tentativas de path traversal
    if (input.Contains("../") || input.Contains("..\\") || input.Contains("%2e%2e"))
    {
        result.IsValid = false;
        result.Errors.Add("Tentativa de path traversal detectada");
    }
    
    result.IsValid = result.Errors.Count == 0;
    return result;
}

// Testar com naughty strings
var validacaoSimplesFalhas = 0;
var validacaoRobustaFalhas = 0;
var exemplosFalhas = new List<(string Input, ValidationResult Result)>();

Console.WriteLine("   Testando strings maliciosas...");

foreach (var naughtyString in naughtyStrings.Take(20)) // Testar apenas primeiras 20
{
    try
    {
        // Teste validação simples
        var simplesResult = ValidacaoSimples(naughtyString);
        if (simplesResult) validacaoSimplesFalhas++;
        
        // Teste validação robusta
        var robustaResult = ValidacaoRobusta(naughtyString);
        if (!robustaResult.IsValid)
        {
            validacaoRobustaFalhas++;
            if (exemplosFalhas.Count < 5)
            {
                exemplosFalhas.Add((naughtyString, robustaResult));
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Erro processando string: {ex.Message}");
    }
}

Console.WriteLine($"   • Validação simples passou: {naughtyStrings.Count - validacaoSimplesFalhas}/{naughtyStrings.Count} strings perigosas");
Console.WriteLine($"   • Validação robusta bloqueou: {validacaoRobustaFalhas}/{naughtyStrings.Count} strings perigosas");
Console.WriteLine();

// 4. Exemplos de falhas detectadas
Console.WriteLine("4. Exemplos de strings maliciosas detectadas:");
foreach (var (input, result) in exemplosFalhas)
{
    var displayInput = input.Length > 50 ? input[..50] + "..." : input;
    var safeInput = string.Join("", displayInput.Where(c => !char.IsControl(c) || c == ' '));
    Console.WriteLine($"   ❌ Input: '{safeInput}'");
    Console.WriteLine($"      Erros: {string.Join(", ", result.Errors)}");
}
Console.WriteLine();

// 5. Teste em aplicação simulada
Console.WriteLine("5. Teste em aplicação simulada (API de usuários):");

// Simular API de cadastro de usuário
static ApiResponse CadastrarUsuario(string nome, string email, string bio)
{
    var nomeResult = ValidacaoRobusta(nome);
    var emailResult = ValidacaoRobusta(email);
    var bioResult = ValidacaoRobusta(bio);
    
    var response = new ApiResponse();
    
    if (!nomeResult.IsValid)
    {
        response.Errors.AddRange(nomeResult.Errors.Select(e => $"Nome: {e}"));
    }
    
    if (!emailResult.IsValid)
    {
        response.Errors.AddRange(emailResult.Errors.Select(e => $"Email: {e}"));
    }
    
    if (!bioResult.IsValid)
    {
        response.Errors.AddRange(bioResult.Errors.Select(e => $"Bio: {e}"));
    }
    
    response.Success = response.Errors.Count == 0;
    
    if (response.Success)
    {
        response.Message = "Usuário cadastrado com sucesso";
        response.Data = new { Nome = nome, Email = email, Bio = bio };
    }
    else
    {
        response.Message = "Falha na validação dos dados";
    }
    
    return response;
}

// Teste com dados normais
var usuarioNormal = CadastrarUsuario("João Silva", "joao@email.com", "Desenvolvedor .NET");
Console.WriteLine($"   ✅ Usuário normal: {usuarioNormal.Message}");

// Teste com naughty strings
var tentativasAtaque = new[]
{
    ("'; DROP TABLE users; --", "hacker@evil.com", "Bio normal"),
    ("João Normal", "<script>alert('XSS')</script>", "Bio normal"),
    ("João Normal", "email@normal.com", "../../etc/passwd")
};

foreach (var (nome, email, bio) in tentativasAtaque)
{
    var resultado = CadastrarUsuario(nome, email, bio);
    if (!resultado.Success)
    {
        Console.WriteLine($"   🛡️  Ataque bloqueado: {resultado.Errors.First()}");
    }
}

Console.WriteLine();

// 6. Melhores práticas
Console.WriteLine("6. Melhores práticas para validação:");
Console.WriteLine("   • Use listas de naughty strings em testes automatizados");
Console.WriteLine("   • Implemente validação em múltiplas camadas (cliente + servidor)");
Console.WriteLine("   • Use allowlists ao invés de blocklists quando possível");
Console.WriteLine("   • Sanitize dados antes de armazenar ou exibir");
Console.WriteLine("   • Use bibliotecas especializadas para validação");
Console.WriteLine("   • Teste regularmente com diferentes categorias de naughty strings");
Console.WriteLine("   • Monitore logs para tentativas de ataque");

Console.WriteLine("\n=== Resumo dos Benefícios ===");
Console.WriteLine("✅ Detecção precoce de vulnerabilidades");
Console.WriteLine("✅ Proteção contra SQL Injection e XSS");
Console.WriteLine("✅ Melhoria na robustez da aplicação");
Console.WriteLine("✅ Testes automatizados de segurança");
Console.WriteLine("✅ Consciência sobre edge cases de entrada");
Console.WriteLine("✅ Prevenção de crashes inesperados");

// Classes auxiliares
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public object? Data { get; set; }
}
