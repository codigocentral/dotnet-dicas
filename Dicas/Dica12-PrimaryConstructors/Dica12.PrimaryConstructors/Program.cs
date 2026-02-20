Console.WriteLine("=== Dica 12: Primary Constructors no C# 12 ===");

// 1. Demonstrando a nova sintaxe de Primary Constructors
Console.WriteLine("1. Comparação entre sintaxe tradicional e Primary Constructors:");

// Sintaxe tradicional
var pessoaTradicional = new PessoaTradicional("João Silva", 30);
Console.WriteLine($"  Pessoa tradicional: {pessoaTradicional.Nome}, {pessoaTradicional.Idade} anos");

// Primary Constructors - sintaxe mais concisa
var pessoaModerna = new PessoaModerna("Maria Santos", 25);
Console.WriteLine($"  Pessoa moderna: {pessoaModerna.Nome}, {pessoaModerna.Idade} anos");

Console.WriteLine();

// 2. Diferentes formas de usar Primary Constructors
Console.WriteLine("2. Diferentes formas de Primary Constructors:");

// Com validação no corpo
var produto = new Produto("Notebook", -100); // Preço será validado
Console.WriteLine($"  Produto: {produto.Nome}, Preço: R$ {produto.Preco:F2}");

// Com propriedades computadas
var retangulo = new Retangulo(5, 3);
Console.WriteLine($"  Retângulo: {retangulo.Largura}x{retangulo.Altura}, Área: {retangulo.Area}");

// Com herança
var funcionario = new Funcionario("Ana Costa", 28, "Desenvolvedora", 8000);
Console.WriteLine($"  Funcionário: {funcionario.Nome}, {funcionario.Cargo}, Salário: R$ {funcionario.Salario:F2}");

Console.WriteLine();

// 3. Casos de uso práticos
Console.WriteLine("3. Casos de uso práticos:");

// DTOs/Records com Primary Constructors
var endereco = new Endereco("Rua das Flores", "123", "São Paulo", "SP");
Console.WriteLine($"  ✅ DTO: {endereco}");

// Value Objects
var coordenada = new Coordenada(23.5505, -46.6333);
Console.WriteLine($"  ✅ Value Object: {coordenada}");

// Configuration objects
var config = new ApiConfig("https://api.exemplo.com", "v1", 30);
Console.WriteLine($"  ✅ Config: {config.BaseUrl}/{config.Version} (timeout: {config.TimeoutSeconds}s)");

Console.WriteLine();

// 4. Validação e transformação de dados
Console.WriteLine("4. Validação e transformação:");

// Email com validação automática
try
{
    var emailValido = new Email("usuario@exemplo.com");
    Console.WriteLine($"  ✅ Email válido: {emailValido.Valor}");
    
    var emailInvalido = new Email("email-invalido");
    Console.WriteLine($"  Email inválido: {emailInvalido.Valor}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"  ❌ Erro esperado: {ex.Message}");
}

// CPF com formatação automática
var cpf = new CPF("12345678901");
Console.WriteLine($"  ✅ CPF formatado: {cpf.Formatado}");

Console.WriteLine();

// 5. Dependency Injection com Primary Constructors
Console.WriteLine("5. Dependency Injection:");

// Simulando serviços
var logger = new ConsoleLogger();
var repository = new UsuarioRepository();
var service = new UsuarioService(repository, logger);

service.CriarUsuario("Pedro Oliveira", "pedro@exemplo.com");

Console.WriteLine();

// 6. Imutabilidade e thread-safety
Console.WriteLine("6. Imutabilidade e thread-safety:");

var pontoImutavel = new PontoImutavel(10, 20);
Console.WriteLine($"  Ponto original: {pontoImutavel}");

var pontoMovido = pontoImutavel.Mover(5, 5);
Console.WriteLine($"  Ponto movido: {pontoMovido}");
Console.WriteLine($"  Original não mudou: {pontoImutavel}");

Console.WriteLine();

// 7. Primary Constructors com diferentes tipos
Console.WriteLine("7. Primary Constructors com diferentes tipos:");

// Struct com Primary Constructor
var tamanho = new Tamanho(1920, 1080);
Console.WriteLine($"  Struct: {tamanho.Largura}x{tamanho.Altura} ({tamanho.MegaPixels:F1}MP)");

// Interface com implementação usando Primary Constructor
IProcessador processador = new ProcessadorTexto("UTF-8");
var resultado = processador.Processar("Olá, mundo!");
Console.WriteLine($"  Processador: {resultado}");

Console.WriteLine();

// 8. Comparação de desempenho e código
Console.WriteLine("8. Vantagens dos Primary Constructors:");

// Contando linhas de código equivalente
var linhasTradicional = typeof(PessoaTradicional).GetProperties().Length * 3 + 5; // Aproximado
var linhasModerna = typeof(PessoaModerna).GetProperties().Length + 1; // Aproximado

Console.WriteLine($"  📊 Redução de código:");
Console.WriteLine($"     - Sintaxe tradicional: ~{linhasTradicional} linhas");
Console.WriteLine($"     - Primary Constructors: ~{linhasModerna} linhas");
Console.WriteLine($"     - Redução: {((double)(linhasTradicional - linhasModerna) / linhasTradicional * 100):F1}%");

Console.WriteLine();

Console.WriteLine("=== Resumo das Vantagens dos Primary Constructors ===");
Console.WriteLine("✅ Sintaxe mais concisa e limpa");
Console.WriteLine("✅ Menos código boilerplate");
Console.WriteLine("✅ Melhor legibilidade");
Console.WriteLine("✅ Funciona com classes, structs e records");
Console.WriteLine("✅ Suporte completo a herança");
Console.WriteLine("✅ Ideal para DTOs, Value Objects e DI");
Console.WriteLine("✅ Mantém todas as funcionalidades tradicionais");

Console.WriteLine("=== Fim da Demonstração ===");

// =================== DEFINIÇÕES DE TIPOS ===================

// Sintaxe tradicional (ainda válida)
public class PessoaTradicional
{
    public string Nome { get; }
    public int Idade { get; }
    
    public PessoaTradicional(string nome, int idade)
    {
        Nome = nome;
        Idade = idade;
    }
}

// ✨ Primary Constructor - sintaxe concisa do C# 12
public class PessoaModerna(string nome, int idade)
{
    public string Nome { get; } = nome;
    public int Idade { get; } = idade;
}

// Primary Constructor com validação no corpo da classe
public class Produto(string nome, decimal preco)
{
    public string Nome { get; } = nome;
    public decimal Preco { get; } = preco < 0 ? 0 : preco; // Validação inline
}

// Primary Constructor com propriedades computadas
public class Retangulo(double largura, double altura)
{
    public double Largura { get; } = largura;
    public double Altura { get; } = altura;
    public double Area => Largura * Altura; // Propriedade computada
}

// Primary Constructor com herança
public class Pessoa(string nome, int idade)
{
    public string Nome { get; } = nome;
    public int Idade { get; } = idade;
}

public class Funcionario(string nome, int idade, string cargo, decimal salario) 
    : Pessoa(nome, idade)
{
    public string Cargo { get; } = cargo;
    public decimal Salario { get; } = salario;
}

// Records com Primary Constructors (ainda mais conciso)
public record Endereco(string Rua, string Numero, string Cidade, string Estado);

public record Coordenada(double Latitude, double Longitude)
{
    public override string ToString() => $"({Latitude:F4}, {Longitude:F4})";
}

// Configuration com Primary Constructor
public class ApiConfig(string baseUrl, string version, int timeoutSeconds)
{
    public string BaseUrl { get; } = baseUrl;
    public string Version { get; } = version;
    public int TimeoutSeconds { get; } = timeoutSeconds;
}

// Validação em Primary Constructor
public class Email(string email)
{
    public string Valor { get; } = IsValidEmail(email) ? email 
        : throw new ArgumentException("Email inválido", nameof(email));
    
    private static bool IsValidEmail(string email)
    {
        return email.Contains('@') && email.Contains('.');
    }
}

// Value Object com formatação
public class CPF(string numero)
{
    public string Numero { get; } = numero;
    public string Formatado => $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";
}

// Dependency Injection com Primary Constructors
public interface ILogger
{
    void Log(string message);
}

public interface IUsuarioRepository
{
    void Salvar(string nome, string email);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine($"    📝 Log: {message}");
}

public class UsuarioRepository : IUsuarioRepository
{
    public void Salvar(string nome, string email)
    {
        Console.WriteLine($"    💾 Salvando usuário: {nome} ({email})");
    }
}

public class UsuarioService(IUsuarioRepository repository, ILogger logger)
{
    public void CriarUsuario(string nome, string email)
    {
        logger.Log($"Criando usuário: {nome}");
        repository.Salvar(nome, email);
        logger.Log("Usuário criado com sucesso");
    }
}

// Imutabilidade com Primary Constructor
public readonly record struct PontoImutavel(int X, int Y)
{
    public PontoImutavel Mover(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY);
    public override string ToString() => $"({X}, {Y})";
}

// Struct com Primary Constructor
public readonly struct Tamanho(int largura, int altura)
{
    public int Largura { get; } = largura;
    public int Altura { get; } = altura;
    public double MegaPixels => (Largura * Altura) / 1_000_000.0;
}

// Interface e implementação com Primary Constructor
public interface IProcessador
{
    string Processar(string texto);
}

public class ProcessadorTexto(string encoding) : IProcessador
{
    public string Encoding { get; } = encoding;
    
    public string Processar(string texto)
    {
        return $"Processado com {Encoding}: {texto}";
    }
}
