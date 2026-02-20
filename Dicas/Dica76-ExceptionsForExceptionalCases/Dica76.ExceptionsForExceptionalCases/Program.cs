Console.WriteLine("=== Dica 76: Exceções Apenas para Casos Excepcionais ===\n");

// 1. PROBLEMA: Usando exceções para controle de fluxo
Console.WriteLine("1. ❌ MAU EXEMPLO - Exceções para controle de fluxo:");
var validadorRuim = new ValidadorRuim();

var emails = new[] { "teste@email.com", "email-invalido", "usuario@empresa.com.br", "" };

foreach (var email in emails)
{
    try
    {
        if (validadorRuim.ValidarEmail(email))
        {
            Console.WriteLine($"  ✅ Email válido: {email}");
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"  ❌ Email inválido: {email} - {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  ❌ Email vazio: {email} - {ex.Message}");
    }
}

// 2. SOLUÇÃO: Result Pattern ou return codes
Console.WriteLine("\n2. ✅ BOM EXEMPLO - Result Pattern:");
var validadorBom = new ValidadorBom();

foreach (var email in emails)
{
    var resultado = validadorBom.ValidarEmail(email);
    if (resultado.IsSuccess)
    {
        Console.WriteLine($"  ✅ Email válido: {email}");
    }
    else
    {
        Console.WriteLine($"  ❌ Email inválido: {email} - {resultado.ErrorMessage}");
    }
}

// 3. PROBLEMA: Try-Parse manual com exceções
Console.WriteLine("\n3. ❌ MAU EXEMPLO - Parse com exceções:");
var conversorRuim = new ConversorRuim();

var numeros = new[] { "123", "abc", "456.78", "-999", "999999999999999999999" };

foreach (var numero in numeros)
{
    try
    {
        var valor = conversorRuim.ConverterParaInteiro(numero);
        Console.WriteLine($"  ✅ '{numero}' = {valor}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ❌ '{numero}' - Erro: {ex.GetType().Name}");
    }
}

// 4. SOLUÇÃO: TryParse pattern
Console.WriteLine("\n4. ✅ BOM EXEMPLO - TryParse pattern:");
var conversorBom = new ConversorBom();

foreach (var numero in numeros)
{
    if (conversorBom.TentarConverterParaInteiro(numero, out var valor))
    {
        Console.WriteLine($"  ✅ '{numero}' = {valor}");
    }
    else
    {
        Console.WriteLine($"  ❌ '{numero}' - Formato inválido");
    }
}

// 5. Cache com lazy loading - comparação de abordagens
Console.WriteLine("\n5. Cache - Comparação de Abordagens:");

// Abordagem com exceções (problemática)
var cacheComExcecoes = new CacheComExcecoes();
Console.WriteLine("Cache com exceções:");
TestCache("usuario1", cacheComExcecoes.ObterDados);
TestCache("usuario1", cacheComExcecoes.ObterDados); // hit do cache

// Abordagem sem exceções (melhor)
var cacheSemExcecoes = new CacheSemExcecoes();
Console.WriteLine("\nCache sem exceções:");
TestCache("usuario1", cacheSemExcecoes.ObterDados);
TestCache("usuario1", cacheSemExcecoes.ObterDados); // hit do cache

// 6. Operações de arquivo - quando usar exceções
Console.WriteLine("\n6. Operações de Arquivo:");
var gerenciadorArquivo = new GerenciadorArquivo();

// Caso onde exceção É apropriada (erro inesperado do sistema)
try
{
    var conteudo = gerenciadorArquivo.LerArquivoObrigatorio("arquivo-critico.txt");
    Console.WriteLine($"  ✅ Arquivo lido: {conteudo.Length} caracteres");
}
catch (FileNotFoundException)
{
    Console.WriteLine("  ❌ Arquivo crítico não encontrado - FALHA DO SISTEMA");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("  ❌ Sem permissão para ler arquivo crítico - FALHA DO SISTEMA");
}

// Caso onde exceção NÃO é apropriada (comportamento esperado)
var resultadoArquivo = gerenciadorArquivo.TentarLerArquivoOpcional("config-opcional.txt");
if (resultadoArquivo.Success)
{
    Console.WriteLine($"  ✅ Config opcional encontrada: {resultadoArquivo.Content?.Length} caracteres");
}
else
{
    Console.WriteLine($"  ℹ️ Config opcional não encontrada, usando padrão: {resultadoArquivo.Error}");
}

// 7. Performance - demonstração do impacto
Console.WriteLine("\n7. Impacto na Performance:");
var testePerfomance = new TestePerformance();
testePerfomance.CompararAbordagens();

// 8. Exceções apropriadas
Console.WriteLine("\n8. ✅ Casos Apropriados para Exceções:");
var servicoOperacoes = new ServicoOperacoes();

try
{
    servicoOperacoes.DividirPorZero(10, 0);
}
catch (DivideByZeroException ex)
{
    Console.WriteLine($"  ❌ Erro matemático: {ex.Message}");
}

try
{
    servicoOperacoes.AcessarMemoriaInvalida();
}
catch (NullReferenceException ex)
{
    Console.WriteLine($"  ❌ Erro de estado: {ex.Message}");
}

try
{
    servicoOperacoes.ConectarBancoDados("connection-string-invalida");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  ❌ Erro de infraestrutura: {ex.Message}");
}

Console.WriteLine("\n9. 📋 Resumo das Boas Práticas:");
Console.WriteLine("  ✅ Use exceções para erros inesperados do sistema");
Console.WriteLine("  ✅ Use Result Pattern para validações de negócio");
Console.WriteLine("  ✅ Use TryParse para conversões que podem falhar");
Console.WriteLine("  ✅ Use nullable ou Optional para valores que podem não existir");
Console.WriteLine("  ❌ NÃO use exceções para controle de fluxo normal");
Console.WriteLine("  ❌ NÃO use exceções para validação de entrada do usuário");
Console.WriteLine("  ❌ NÃO use exceções quando falha é comportamento esperado");

Console.WriteLine("\n=== Fim da Demonstração ===");

static void TestCache(string chave, Func<string, string> obterDados)
{
    var inicio = DateTime.Now;
    var resultado = obterDados(chave);
    var tempo = DateTime.Now - inicio;
    Console.WriteLine($"  '{chave}' → {resultado} (tempo: {tempo.TotalMilliseconds:F1}ms)");
}

// Classes de apoio

// ❌ PROBLEMA: Exceções para controle de fluxo
public class ValidadorRuim
{
    public bool ValidarEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email não pode ser vazio");

        if (!email.Contains('@'))
            throw new ArgumentException("Email deve conter @");

        if (!email.Contains('.'))
            throw new ArgumentException("Email deve conter domínio válido");

        return true;
    }
}

// ✅ SOLUÇÃO: Result Pattern
public class ValidadorBom
{
    public ValidationResult ValidarEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return ValidationResult.Failure("Email não pode ser vazio");

        if (!email.Contains('@'))
            return ValidationResult.Failure("Email deve conter @");

        if (!email.Contains('.'))
            return ValidationResult.Failure("Email deve conter domínio válido");

        return ValidationResult.Success();
    }
}

public class ValidationResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    private ValidationResult(bool isSuccess, string errorMessage = "")
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string error) => new(false, error);
}

// ❌ PROBLEMA: Parse com exceções
public class ConversorRuim
{
    public int ConverterParaInteiro(string valor)
    {
        if (string.IsNullOrEmpty(valor))
            throw new ArgumentNullException(nameof(valor));

        try
        {
            return int.Parse(valor);
        }
        catch (FormatException)
        {
            throw new ArgumentException($"'{valor}' não é um número válido");
        }
        catch (OverflowException)
        {
            throw new ArgumentException($"'{valor}' é muito grande para int");
        }
    }
}

// ✅ SOLUÇÃO: TryParse pattern
public class ConversorBom
{
    public bool TentarConverterParaInteiro(string valor, out int resultado)
    {
        return int.TryParse(valor, out resultado);
    }

    public int? ConverterParaInteiroNullable(string valor)
    {
        return int.TryParse(valor, out var resultado) ? resultado : null;
    }
}

// ❌ PROBLEMA: Cache com exceções
public class CacheComExcecoes
{
    private readonly Dictionary<string, string> _cache = new();

    public string ObterDados(string chave)
    {
        try
        {
            return _cache[chave];
        }
        catch (KeyNotFoundException)
        {
            // Carregar dados "pesados"
            Thread.Sleep(50); // Simula operação custosa
            var dados = $"Dados para {chave}";
            _cache[chave] = dados;
            return dados;
        }
    }
}

// ✅ SOLUÇÃO: Cache sem exceções
public class CacheSemExcecoes
{
    private readonly Dictionary<string, string> _cache = new();

    public string ObterDados(string chave)
    {
        if (_cache.TryGetValue(chave, out var dadosCacheados))
        {
            return dadosCacheados;
        }

        // Carregar dados "pesados"
        Thread.Sleep(50); // Simula operação custosa
        var dados = $"Dados para {chave}";
        _cache[chave] = dados;
        return dados;
    }
}

public class GerenciadorArquivo
{
    // ✅ Exceção apropriada - arquivo crítico deve existir
    public string LerArquivoObrigatorio(string caminho)
    {
        // Arquivo crítico - se não existir, é erro do sistema
        return File.ReadAllText(caminho);
    }

    // ✅ Sem exceção - arquivo opcional pode não existir
    public FileResult TentarLerArquivoOpcional(string caminho)
    {
        try
        {
            var conteudo = File.ReadAllText(caminho);
            return FileResult.CreateSuccess(conteudo);
        }
        catch (FileNotFoundException)
        {
            return FileResult.CreateFailure("Arquivo não encontrado");
        }
        catch (UnauthorizedAccessException)
        {
            return FileResult.CreateFailure("Sem permissão para ler arquivo");
        }
        catch (Exception ex)
        {
            return FileResult.CreateFailure($"Erro inesperado: {ex.Message}");
        }
    }
}

public class FileResult
{
    public bool Success { get; private set; }
    public string? Content { get; private set; }
    public string Error { get; private set; } = "";

    private FileResult(bool success, string? content = null, string error = "")
    {
        Success = success;
        Content = content;
        Error = error;
    }

    public static FileResult CreateSuccess(string content) => new(true, content);
    public static FileResult CreateFailure(string error) => new(false, null, error);
}

public class TestePerformance
{
    public void CompararAbordagens()
    {
        const int iteracoes = 10000;

        // Teste com exceções (lento)
        var inicioExcecoes = DateTime.Now;
        var contadorExcecoes = 0;
        
        for (int i = 0; i < iteracoes; i++)
        {
            try
            {
                if (i % 2 == 0)
                    throw new InvalidOperationException("Teste");
            }
            catch
            {
                contadorExcecoes++;
            }
        }
        
        var tempoExcecoes = DateTime.Now - inicioExcecoes;

        // Teste sem exceções (rápido)
        var inicioSemExcecoes = DateTime.Now;
        var contadorSemExcecoes = 0;
        
        for (int i = 0; i < iteracoes; i++)
        {
            if (i % 2 == 0)
                contadorSemExcecoes++;
        }
        
        var tempoSemExcecoes = DateTime.Now - inicioSemExcecoes;

        Console.WriteLine($"  Com exceções: {tempoExcecoes.TotalMilliseconds:F1}ms ({contadorExcecoes} exceções)");
        Console.WriteLine($"  Sem exceções: {tempoSemExcecoes.TotalMilliseconds:F1}ms ({contadorSemExcecoes} casos)");
        Console.WriteLine($"  Diferença: {(tempoExcecoes.TotalMilliseconds / tempoSemExcecoes.TotalMilliseconds):F1}x mais lento com exceções");
    }
}

public class ServicoOperacoes
{
    // ✅ Exceção apropriada - operação matematicamente inválida
    public double DividirPorZero(double a, double b)
    {
        if (b == 0)
            throw new DivideByZeroException("Não é possível dividir por zero");
        
        return a / b;
    }

    // ✅ Exceção apropriada - estado inválido do objeto
    public void AcessarMemoriaInvalida()
    {
        string? texto = null;
        // Isso deve gerar NullReferenceException - indica bug no código
        var tamanho = texto.Length;
    }

    // ✅ Exceção apropriada - falha de infraestrutura
    public void ConectarBancoDados(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString) || connectionString == "connection-string-invalida")
        {
            throw new InvalidOperationException("Falha ao conectar com banco de dados - verificar infraestrutura");
        }
    }
}
