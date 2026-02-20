using System.Diagnostics;
using System.Text.RegularExpressions;

Console.WriteLine("==== Dica 100: Regex Compilados para Performance Máxima ====\n");

Console.WriteLine("🚀 Regex COMPILADOS são até 10x mais rápidos que regex normais!");
Console.WriteLine("Vamos demonstrar quando e como usar para performance máxima...\n");

// ===== DEMONSTRAÇÃO 1: REGEX NORMAL VS COMPILADO =====
Console.WriteLine("1. Regex Normal vs Compilado - Diferença de Performance");
Console.WriteLine("--------------------------------------------------------");

// Padrões comuns que usaremos
const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
const string phonePattern = @"^\(\d{3}\)\s\d{3}-\d{4}$";
const string creditCardPattern = @"^\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}$";

// Dados de teste
var emails = GenerateTestEmails(10000);
var phones = GenerateTestPhones(10000);
var creditCards = GenerateTestCreditCards(10000);

Console.WriteLine($"📊 Dataset: {emails.Length:N0} emails, {phones.Length:N0} telefones, {creditCards.Length:N0} cartões");

// ===== TESTE 1: REGEX NORMAL (SEM COMPILAÇÃO) =====
Console.WriteLine("\n❌ Regex NORMAL (sem compilação):");
var sw = Stopwatch.StartNew();

var emailRegexNormal = new Regex(emailPattern);
var phoneRegexNormal = new Regex(phonePattern);
var cardRegexNormal = new Regex(creditCardPattern);

int validEmailsNormal = 0, validPhonesNormal = 0, validCardsNormal = 0;

foreach (var email in emails)
    if (emailRegexNormal.IsMatch(email)) validEmailsNormal++;

foreach (var phone in phones)
    if (phoneRegexNormal.IsMatch(phone)) validPhonesNormal++;

foreach (var card in creditCards)
    if (cardRegexNormal.IsMatch(card)) validCardsNormal++;

sw.Stop();
var normalTime = sw.ElapsedMilliseconds;

Console.WriteLine($"   • Emails válidos: {validEmailsNormal:N0}");
Console.WriteLine($"   • Telefones válidos: {validPhonesNormal:N0}");
Console.WriteLine($"   • Cartões válidos: {validCardsNormal:N0}");
Console.WriteLine($"   • ⏱️  Tempo total: {normalTime} ms");

// ===== TESTE 2: REGEX COMPILADO =====
Console.WriteLine("\n✅ Regex COMPILADO:");
sw.Restart();

var emailRegexCompiled = new Regex(emailPattern, RegexOptions.Compiled);
var phoneRegexCompiled = new Regex(phonePattern, RegexOptions.Compiled);
var cardRegexCompiled = new Regex(creditCardPattern, RegexOptions.Compiled);

int validEmailsCompiled = 0, validPhonesCompiled = 0, validCardsCompiled = 0;

foreach (var email in emails)
    if (emailRegexCompiled.IsMatch(email)) validEmailsCompiled++;

foreach (var phone in phones)
    if (phoneRegexCompiled.IsMatch(phone)) validPhonesCompiled++;

foreach (var card in creditCards)
    if (cardRegexCompiled.IsMatch(card)) validCardsCompiled++;

sw.Stop();
var compiledTime = sw.ElapsedMilliseconds;

Console.WriteLine($"   • Emails válidos: {validEmailsCompiled:N0}");
Console.WriteLine($"   • Telefones válidos: {validPhonesCompiled:N0}");
Console.WriteLine($"   • Cartões válidos: {validCardsCompiled:N0}");
Console.WriteLine($"   • ⏱️  Tempo total: {compiledTime} ms");

var improvement = normalTime > 0 && compiledTime > 0 ? (double)normalTime / compiledTime : 1;
Console.WriteLine($"\n🚀 Regex compilado é {improvement:F1}x mais rápido!\n");

// ===== DEMONSTRAÇÃO 2: SOURCE GENERATORS (.NET 7+) =====
Console.WriteLine("2. Source Generators - A Evolução dos Regex Compilados");
Console.WriteLine("-------------------------------------------------------");

Console.WriteLine("✨ .NET 7+ introduziu Source Generators para Regex!");
Console.WriteLine("   • Compilação acontece em BUILD TIME");
Console.WriteLine("   • Performance ainda melhor que RegexOptions.Compiled");
Console.WriteLine("   • Análise estática em tempo de compilação");
Console.WriteLine("   • Detecção de erros em tempo de compilação\n");

// Demonstração com Source Generator (comentado para compatibilidade)
Console.WriteLine("📝 Exemplo de Source Generator Regex:");
Console.WriteLine("```csharp");
Console.WriteLine("[GeneratedRegex(@\"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$\")]");
Console.WriteLine("private static partial Regex EmailRegex();");
Console.WriteLine("");
Console.WriteLine("// Uso:");
Console.WriteLine("bool isValid = EmailRegex().IsMatch(email);");
Console.WriteLine("```");
Console.WriteLine("⚡ Performance superior + validação em compile-time!\n");

// ===== DEMONSTRAÇÃO 3: REGEX ESTÁTICO VS INSTÂNCIA =====
Console.WriteLine("3. Regex Estático vs Instância - Impacto na Performance");
Console.WriteLine("--------------------------------------------------------");

var testString = "Contact us at support@company.com or sales@company.com for help";
const int iterations = 50000;

// Teste com Regex.IsMatch estático (compila a cada chamada!)
sw.Restart();
int matchesStatic = 0;
for (int i = 0; i < iterations; i++)
{
    if (Regex.IsMatch(testString, emailPattern))
        matchesStatic++;
}
sw.Stop();
var staticTime = sw.ElapsedMilliseconds;

// Teste com instância compilada (compila uma vez)
var compiledRegex = new Regex(emailPattern, RegexOptions.Compiled);
sw.Restart();
int matchesInstance = 0;
for (int i = 0; i < iterations; i++)
{
    if (compiledRegex.IsMatch(testString))
        matchesInstance++;
}
sw.Stop();
var instanceTime = sw.ElapsedMilliseconds;

Console.WriteLine($"❌ Regex.IsMatch estático: {staticTime} ms ({matchesStatic:N0} matches)");
Console.WriteLine($"✅ Instância compilada: {instanceTime} ms ({matchesInstance:N0} matches)");

var staticImprovement = staticTime > 0 && instanceTime > 0 ? (double)staticTime / instanceTime : 1;
Console.WriteLine($"⚡ Instância compilada é {staticImprovement:F1}x mais rápida!\n");

// ===== DEMONSTRAÇÃO 4: DIFERENTES OPÇÕES DE COMPILAÇÃO =====
Console.WriteLine("4. Diferentes Opções de Regex e seus Impactos");
Console.WriteLine("----------------------------------------------");

var complexText = GenerateComplexText();
const string complexPattern = @"\b[A-Z][a-z]+\s+\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\s+[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b";

// Regex interpretado (padrão)
var interpretedRegex = new Regex(complexPattern);
sw.Restart();
var interpretedMatches = interpretedRegex.Matches(complexText);
sw.Stop();
var interpretedTime = sw.ElapsedMilliseconds;

// Regex compilado
var regexCompiled = new Regex(complexPattern, RegexOptions.Compiled);
sw.Restart();
var compiledMatches = regexCompiled.Matches(complexText);
sw.Stop();
var regexCompiledTime = sw.ElapsedMilliseconds;

// Regex compilado + IgnoreCase
var compiledIgnoreCase = new Regex(complexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
sw.Restart();
var ignoreCaseMatches = compiledIgnoreCase.Matches(complexText);
sw.Stop();
var ignoreCaseTime = sw.ElapsedMilliseconds;

Console.WriteLine($"📊 Texto complexo: {complexText.Length:N0} caracteres");
Console.WriteLine($"   • Interpretado: {interpretedTime} ms ({interpretedMatches.Count} matches)");
Console.WriteLine($"   • Compilado: {regexCompiledTime} ms ({compiledMatches.Count} matches)");
Console.WriteLine($"   • Compilado + IgnoreCase: {ignoreCaseTime} ms ({ignoreCaseMatches.Count} matches)");

// ===== DEMONSTRAÇÃO 5: CASOS DE USO PRÁTICOS =====
Console.WriteLine("\n5. Casos de Uso Práticos para Regex Compilados");
Console.WriteLine("-----------------------------------------------");

DemonstrateLogParsing();
DemonstrateDataValidation();
DemonstrateTextProcessing();

Console.WriteLine("\n=== RESUMO: Quando Usar Regex Compilados ===");
Console.WriteLine("✅ USE Regex compilados quando:");
Console.WriteLine("   • Mesmo padrão usado múltiplas vezes");
Console.WriteLine("   • Performance é crítica");
Console.WriteLine("   • Processamento de grandes volumes de dados");
Console.WriteLine("   • APIs ou bibliotecas que processam texto");
Console.WriteLine("   • Validação frequente de entrada de usuário");
Console.WriteLine();
Console.WriteLine("🚀 USE Source Generators (.NET 7+) quando:");
Console.WriteLine("   • Padrões são conhecidos em tempo de compilação");
Console.WriteLine("   • Quer máxima performance");
Console.WriteLine("   • Quer validação em tempo de compilação");
Console.WriteLine();
Console.WriteLine("❌ EVITE quando:");
Console.WriteLine("   • Padrão usado apenas uma vez");
Console.WriteLine("   • Regex simples ou pequenos volumes");
Console.WriteLine("   • Aplicações com pouco uso de regex");
Console.WriteLine();
Console.WriteLine("⚠️  CUIDADO: Compilação tem overhead inicial - use apenas se vai reutilizar!");

// ===== MÉTODOS DE APOIO =====

static string[] GenerateTestEmails(int count)
{
    var emails = new string[count];
    var domains = new[] { "gmail.com", "yahoo.com", "company.com", "test.org" };
    var random = new Random(42);
    
    for (int i = 0; i < count; i++)
    {
        var isValid = random.Next(10) > 2; // 70% válidos
        if (isValid)
        {
            emails[i] = $"user{i}@{domains[random.Next(domains.Length)]}";
        }
        else
        {
            emails[i] = $"invalid-email-{i}"; // Email inválido
        }
    }
    return emails;
}

static string[] GenerateTestPhones(int count)
{
    var phones = new string[count];
    var random = new Random(42);
    
    for (int i = 0; i < count; i++)
    {
        var isValid = random.Next(10) > 2; // 70% válidos
        if (isValid)
        {
            phones[i] = $"({random.Next(100, 999)}) {random.Next(100, 999)}-{random.Next(1000, 9999)}";
        }
        else
        {
            phones[i] = $"{random.Next(1000000, 9999999)}"; // Formato inválido
        }
    }
    return phones;
}

static string[] GenerateTestCreditCards(int count)
{
    var cards = new string[count];
    var random = new Random(42);
    
    for (int i = 0; i < count; i++)
    {
        var isValid = random.Next(10) > 2; // 70% válidos
        if (isValid)
        {
            cards[i] = $"{random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)}";
        }
        else
        {
            cards[i] = $"{random.Next(100, 999)}-{random.Next(100, 999)}"; // Formato inválido
        }
    }
    return cards;
}

static string GenerateComplexText()
{
    var text = new System.Text.StringBuilder();
    var random = new Random(42);
    var names = new[] { "John", "Jane", "Bob", "Alice", "Charlie", "Diana" };
    var domains = new[] { "company.com", "test.org", "sample.net" };
    
    for (int i = 0; i < 1000; i++)
    {
        var name = names[random.Next(names.Length)];
        var ip = $"{random.Next(1, 255)}.{random.Next(1, 255)}.{random.Next(1, 255)}.{random.Next(1, 255)}";
        var email = $"user{i}@{domains[random.Next(domains.Length)]}";
        
        text.AppendLine($"{name} {ip} {email} - Log entry {i}");
    }
    
    return text.ToString();
}

static void DemonstrateLogParsing()
{
    Console.WriteLine("🔍 Exemplo: Parsing de Logs");
    
    var logRegex = new Regex(
        @"(?<timestamp>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s\[(?<level>\w+)\]\s(?<message>.*)",
        RegexOptions.Compiled);
    
    var logs = new[]
    {
        "2024-01-15 10:30:45 [INFO] User logged in",
        "2024-01-15 10:31:12 [ERROR] Database connection failed",
        "2024-01-15 10:31:45 [WARN] Memory usage high"
    };
    
    foreach (var log in logs)
    {
        var match = logRegex.Match(log);
        if (match.Success)
        {
            Console.WriteLine($"   • {match.Groups["level"].Value}: {match.Groups["message"].Value}");
        }
    }
}

static void DemonstrateDataValidation()
{
    Console.WriteLine("\n✅ Exemplo: Validação de Dados");
    
    var validators = new Dictionary<string, Regex>
    {
        ["CPF"] = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", RegexOptions.Compiled),
        ["CEP"] = new Regex(@"^\d{5}-\d{3}$", RegexOptions.Compiled),
        ["Telefone"] = new Regex(@"^\(\d{2}\)\s\d{4,5}-\d{4}$", RegexOptions.Compiled)
    };
    
    var testData = new Dictionary<string, string>
    {
        ["CPF"] = "123.456.789-01",
        ["CEP"] = "01234-567",
        ["Telefone"] = "(11) 98765-4321"
    };
    
    foreach (var (type, value) in testData)
    {
        var isValid = validators[type].IsMatch(value);
        var status = isValid ? "✅" : "❌";
        Console.WriteLine($"   • {type}: {value} {status}");
    }
}

static void DemonstrateTextProcessing()
{
    Console.WriteLine("\n📝 Exemplo: Processamento de Texto");
    
    var text = "Visit https://www.example.com or http://test.org for more info. Email: contact@company.com";
    
    var urlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled);
    var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    
    var urls = urlRegex.Matches(text);
    var emails = emailRegex.Matches(text);
    
    Console.WriteLine($"   • URLs encontradas: {urls.Count}");
    foreach (Match url in urls)
        Console.WriteLine($"     - {url.Value}");
    
    Console.WriteLine($"   • Emails encontrados: {emails.Count}");
    foreach (Match email in emails)
        Console.WriteLine($"     - {email.Value}");
}
