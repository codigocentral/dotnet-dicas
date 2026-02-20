using BenchmarkDotNet.Attributes;
using System.Text.RegularExpressions;

namespace Dica11.Benchmark;

[MemoryDiagnoser]
[SimpleJob]
public class StringSyntaxBenchmarks
{
    private const string TestJsonData = """
        {
            "nome": "João Silva",
            "idade": 30,
            "cidade": "São Paulo",
            "ativo": true
        }
        """;

    private const string TestSqlQuery = """
        SELECT u.nome, u.email, p.titulo 
        FROM usuarios u 
        INNER JOIN posts p ON u.id = p.usuario_id 
        WHERE u.ativo = 1 AND p.data_publicacao >= '2024-01-01'
        ORDER BY p.data_publicacao DESC
        """;

    private const string TestUrlRoute = "/api/v1/usuarios/{id:int}/posts/{postId:guid}";

    private const string TestEmailAddress = "usuario@exemplo.com";

    private const string TestRegexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    private readonly Regex _compiledRegex = new(TestRegexPattern, RegexOptions.Compiled);

    [Benchmark]
    public bool ValidateEmailWithRegex()
    {
        return Regex.IsMatch(TestEmailAddress, TestRegexPattern);
    }

    [Benchmark]
    public bool ValidateEmailWithCompiledRegex()
    {
        return _compiledRegex.IsMatch(TestEmailAddress);
    }

    [Benchmark]
    public int ProcessJsonString()
    {
        // Simula processamento de JSON sem análise real
        return TestJsonData.Count(c => c == '"');
    }

    [Benchmark]
    public int ProcessSqlString()
    {
        // Simula análise de SQL contando palavras-chave
        var keywords = new[] { "SELECT", "FROM", "WHERE", "ORDER", "BY", "INNER", "JOIN" };
        return keywords.Sum(keyword => TestSqlQuery.ToUpper().Split(' ').Count(word => word.Contains(keyword)));
    }

    [Benchmark]
    public int ProcessUrlRoute()
    {
        // Simula processamento de rota contando parâmetros
        return TestUrlRoute.Count(c => c == '{');
    }

    [Benchmark]
    public string ExtractEmailDomain()
    {
        var atIndex = TestEmailAddress.IndexOf('@');
        return atIndex > -1 ? TestEmailAddress.Substring(atIndex + 1) : string.Empty;
    }

    [Benchmark]
    public bool ValidateJsonStructure()
    {
        // Validação básica de estrutura JSON
        var trimmed = TestJsonData.Trim();
        return trimmed.StartsWith('{') && trimmed.EndsWith('}');
    }

    [Benchmark]
    public int CountSqlParameters()
    {
        // Conta parâmetros potenciais no SQL
        return TestSqlQuery.Count(c => c == '?') + TestSqlQuery.Count(c => c == '@');
    }

    [Benchmark]
    public string NormalizeUrl()
    {
        // Normalização básica de URL
        return TestUrlRoute.ToLowerInvariant().Replace("//", "/");
    }

    [Benchmark]
    public bool IsValidEmailFormat()
    {
        // Validação simples sem regex
        return TestEmailAddress.Contains('@') && 
               TestEmailAddress.Contains('.') && 
               TestEmailAddress.Length > 5;
    }
}
