using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.RegularExpressions;

Console.WriteLine("=== Benchmark: Compiled Regex Performance ===");
Console.WriteLine("Demonstrando o impacto na performance...\n");

BenchmarkRunner.Run<CompiledRegexBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class CompiledRegexBenchmark
{
    private readonly string[] _emails;
    private readonly string[] _phoneNumbers;
    private readonly string[] _creditCards;
    private readonly string _logData;
    private readonly string _htmlContent;
    
    // Regex patterns
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private const string PhonePattern = @"^\(\d{3}\)\s\d{3}-\d{4}$";
    private const string CreditCardPattern = @"^\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}$";
    private const string UrlPattern = @"https?://[^\s<>""]+";
    private const string LogPattern = @"(?<timestamp>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s\[(?<level>\w+)\]\s(?<message>.*)";
    
    // Compiled regex instances
    private readonly Regex _emailRegexCompiled;
    private readonly Regex _phoneRegexCompiled;
    private readonly Regex _creditCardRegexCompiled;
    private readonly Regex _urlRegexCompiled;
    private readonly Regex _logRegexCompiled;
    
    public CompiledRegexBenchmark()
    {
        // Initialize test data
        _emails = GenerateEmails(1000);
        _phoneNumbers = GeneratePhoneNumbers(1000);
        _creditCards = GenerateCreditCards(1000);
        _logData = GenerateLogData();
        _htmlContent = GenerateHtmlContent();
        
        // Initialize compiled regex instances
        _emailRegexCompiled = new Regex(EmailPattern, RegexOptions.Compiled);
        _phoneRegexCompiled = new Regex(PhonePattern, RegexOptions.Compiled);
        _creditCardRegexCompiled = new Regex(CreditCardPattern, RegexOptions.Compiled);
        _urlRegexCompiled = new Regex(UrlPattern, RegexOptions.Compiled);
        _logRegexCompiled = new Regex(LogPattern, RegexOptions.Compiled);
    }
    
    // ===== EMAIL VALIDATION BENCHMARKS =====
    
    [Benchmark(Baseline = true)]
    public int EmailValidation_StaticRegex()
    {
        int validCount = 0;
        foreach (var email in _emails)
        {
            if (Regex.IsMatch(email, EmailPattern))
                validCount++;
        }
        return validCount;
    }
    
    [Benchmark]
    public int EmailValidation_InterpretedRegex()
    {
        var regex = new Regex(EmailPattern);
        int validCount = 0;
        foreach (var email in _emails)
        {
            if (regex.IsMatch(email))
                validCount++;
        }
        return validCount;
    }
    
    [Benchmark]
    public int EmailValidation_CompiledRegex()
    {
        int validCount = 0;
        foreach (var email in _emails)
        {
            if (_emailRegexCompiled.IsMatch(email))
                validCount++;
        }
        return validCount;
    }
    
    // ===== PHONE NUMBER VALIDATION BENCHMARKS =====
    
    [Benchmark]
    public int PhoneValidation_StaticRegex()
    {
        int validCount = 0;
        foreach (var phone in _phoneNumbers)
        {
            if (Regex.IsMatch(phone, PhonePattern))
                validCount++;
        }
        return validCount;
    }
    
    [Benchmark]
    public int PhoneValidation_CompiledRegex()
    {
        int validCount = 0;
        foreach (var phone in _phoneNumbers)
        {
            if (_phoneRegexCompiled.IsMatch(phone))
                validCount++;
        }
        return validCount;
    }
    
    // ===== CREDIT CARD VALIDATION BENCHMARKS =====
    
    [Benchmark]
    public int CreditCardValidation_StaticRegex()
    {
        int validCount = 0;
        foreach (var card in _creditCards)
        {
            if (Regex.IsMatch(card, CreditCardPattern))
                validCount++;
        }
        return validCount;
    }
    
    [Benchmark]
    public int CreditCardValidation_CompiledRegex()
    {
        int validCount = 0;
        foreach (var card in _creditCards)
        {
            if (_creditCardRegexCompiled.IsMatch(card))
                validCount++;
        }
        return validCount;
    }
    
    // ===== URL EXTRACTION BENCHMARKS =====
    
    [Benchmark]
    public int UrlExtraction_StaticRegex()
    {
        return Regex.Matches(_htmlContent, UrlPattern).Count;
    }
    
    [Benchmark]
    public int UrlExtraction_InterpretedRegex()
    {
        var regex = new Regex(UrlPattern);
        return regex.Matches(_htmlContent).Count;
    }
    
    [Benchmark]
    public int UrlExtraction_CompiledRegex()
    {
        return _urlRegexCompiled.Matches(_htmlContent).Count;
    }
    
    // ===== LOG PARSING BENCHMARKS =====
    
    [Benchmark]
    public int LogParsing_StaticRegex()
    {
        var lines = _logData.Split('\n');
        int parsedCount = 0;
        
        foreach (var line in lines)
        {
            var match = Regex.Match(line, LogPattern);
            if (match.Success)
                parsedCount++;
        }
        return parsedCount;
    }
    
    [Benchmark]
    public int LogParsing_CompiledRegex()
    {
        var lines = _logData.Split('\n');
        int parsedCount = 0;
        
        foreach (var line in lines)
        {
            var match = _logRegexCompiled.Match(line);
            if (match.Success)
                parsedCount++;
        }
        return parsedCount;
    }
    
    // ===== COMPLEX PATTERN BENCHMARKS =====
    
    [Benchmark]
    public int ComplexPattern_StaticRegex()
    {
        const string pattern = @"\b(?:[A-Z][a-z]+\s+){1,2}(?:\d{1,3}\.){3}\d{1,3}\s+[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b";
        return Regex.Matches(_logData, pattern).Count;
    }
    
    [Benchmark]
    public int ComplexPattern_CompiledRegex()
    {
        const string pattern = @"\b(?:[A-Z][a-z]+\s+){1,2}(?:\d{1,3}\.){3}\d{1,3}\s+[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b";
        var regex = new Regex(pattern, RegexOptions.Compiled);
        return regex.Matches(_logData).Count;
    }
    
    // ===== REPLACEMENT BENCHMARKS =====
    
    [Benchmark]
    public string Replacement_StaticRegex()
    {
        return Regex.Replace(_htmlContent, @"https?://[^\s<>""]+", "[URL]");
    }
    
    [Benchmark]
    public string Replacement_CompiledRegex()
    {
        return _urlRegexCompiled.Replace(_htmlContent, "[URL]");
    }
    
    // ===== SPLIT BENCHMARKS =====
    
    [Benchmark]
    public string[] Split_StaticRegex()
    {
        return Regex.Split(_logData, @"\s+");
    }
    
    [Benchmark]
    public string[] Split_CompiledRegex()
    {
        var regex = new Regex(@"\s+", RegexOptions.Compiled);
        return regex.Split(_logData);
    }
    
    // ===== MULTIPLE PATTERN VALIDATION =====
    
    [Benchmark]
    public int MultipleValidation_StaticRegex()
    {
        int validCount = 0;
        
        for (int i = 0; i < 100; i++)
        {
            var email = _emails[i % _emails.Length];
            var phone = _phoneNumbers[i % _phoneNumbers.Length];
            var card = _creditCards[i % _creditCards.Length];
            
            if (Regex.IsMatch(email, EmailPattern)) validCount++;
            if (Regex.IsMatch(phone, PhonePattern)) validCount++;
            if (Regex.IsMatch(card, CreditCardPattern)) validCount++;
        }
        
        return validCount;
    }
    
    [Benchmark]
    public int MultipleValidation_CompiledRegex()
    {
        int validCount = 0;
        
        for (int i = 0; i < 100; i++)
        {
            var email = _emails[i % _emails.Length];
            var phone = _phoneNumbers[i % _phoneNumbers.Length];
            var card = _creditCards[i % _creditCards.Length];
            
            if (_emailRegexCompiled.IsMatch(email)) validCount++;
            if (_phoneRegexCompiled.IsMatch(phone)) validCount++;
            if (_creditCardRegexCompiled.IsMatch(card)) validCount++;
        }
        
        return validCount;
    }
    
    // ===== CASE INSENSITIVE BENCHMARKS =====
    
    [Benchmark]
    public int CaseInsensitive_StaticRegex()
    {
        const string pattern = @"error|warning|info";
        return Regex.Matches(_logData, pattern, RegexOptions.IgnoreCase).Count;
    }
    
    [Benchmark]
    public int CaseInsensitive_CompiledRegex()
    {
        const string pattern = @"error|warning|info";
        var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return regex.Matches(_logData).Count;
    }
    
    // ===== DATA GENERATION METHODS =====
    
    private static string[] GenerateEmails(int count)
    {
        var emails = new string[count];
        var domains = new[] { "gmail.com", "yahoo.com", "company.com", "test.org", "example.net" };
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            var username = $"user{i}";
            var domain = domains[random.Next(domains.Length)];
            emails[i] = $"{username}@{domain}";
        }
        
        return emails;
    }
    
    private static string[] GeneratePhoneNumbers(int count)
    {
        var phones = new string[count];
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            var area = random.Next(200, 999);
            var exchange = random.Next(200, 999);
            var number = random.Next(1000, 9999);
            phones[i] = $"({area}) {exchange}-{number}";
        }
        
        return phones;
    }
    
    private static string[] GenerateCreditCards(int count)
    {
        var cards = new string[count];
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            var p1 = random.Next(1000, 9999);
            var p2 = random.Next(1000, 9999);
            var p3 = random.Next(1000, 9999);
            var p4 = random.Next(1000, 9999);
            cards[i] = $"{p1} {p2} {p3} {p4}";
        }
        
        return cards;
    }
    
    private static string GenerateLogData()
    {
        var logs = new System.Text.StringBuilder();
        var levels = new[] { "INFO", "WARNING", "ERROR", "DEBUG" };
        var messages = new[]
        {
            "User logged in successfully",
            "Database connection established",
            "Memory usage is high",
            "Request processed",
            "Cache miss for key user_123",
            "File uploaded successfully",
            "Authentication failed",
            "Server startup completed"
        };
        
        var random = new Random(42);
        var baseDate = new DateTime(2024, 1, 1);
        
        for (int i = 0; i < 10000; i++)
        {
            var timestamp = baseDate.AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss");
            var level = levels[random.Next(levels.Length)];
            var message = messages[random.Next(messages.Length)];
            
            logs.AppendLine($"{timestamp} [{level}] {message}");
        }
        
        return logs.ToString();
    }
    
    private static string GenerateHtmlContent()
    {
        var html = new System.Text.StringBuilder();
        var urls = new[]
        {
            "https://www.example.com",
            "http://test.org",
            "https://api.service.com/v1/data",
            "http://blog.company.com/article/123",
            "https://cdn.static.com/images/logo.png"
        };
        
        var random = new Random(42);
        
        html.AppendLine("<html><body>");
        
        for (int i = 0; i < 1000; i++)
        {
            var url = urls[random.Next(urls.Length)];
            html.AppendLine($"<p>Visit <a href=\"{url}\">this link</a> for more information.</p>");
            html.AppendLine($"<p>You can also check {url} or contact us.</p>");
        }
        
        html.AppendLine("</body></html>");
        
        return html.ToString();
    }
}
