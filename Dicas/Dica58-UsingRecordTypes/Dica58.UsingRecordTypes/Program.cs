using System.Text.Json;

Console.WriteLine("=== Dica 58: Usando Record Types ===\n");

// 1. Record básico - imutável por padrão
Console.WriteLine("1. Record Básico - Imutabilidade:");
var pessoa1 = new Pessoa("João", "Silva", 30);
var pessoa2 = new Pessoa("João", "Silva", 30);

Console.WriteLine($"Pessoa 1: {pessoa1}");
Console.WriteLine($"Pessoa 2: {pessoa2}");
Console.WriteLine($"São iguais? {pessoa1 == pessoa2}"); // true - equality by value
Console.WriteLine($"HashCode iguais? {pessoa1.GetHashCode() == pessoa2.GetHashCode()}");

// 2. With expressions - cópia com modificações
Console.WriteLine("\n2. With Expressions - Cópia Imutável:");
var pessoaMaisVelha = pessoa1 with { Idade = 31 };
Console.WriteLine($"Original: {pessoa1}");
Console.WriteLine($"Modificada: {pessoaMaisVelha}");
Console.WriteLine($"São a mesma instância? {ReferenceEquals(pessoa1, pessoaMaisVelha)}");

// 3. Destructuring - decomposição
Console.WriteLine("\n3. Destructuring:");
var (nome, sobrenome, idade) = pessoa1;
Console.WriteLine($"Destructuring: Nome='{nome}', Sobrenome='{sobrenome}', Idade={idade}");

// 4. Record class vs record struct
Console.WriteLine("\n4. Record Class vs Record Struct:");
var enderecoClass = new EnderecoClass("Rua A", "123", "São Paulo");
var enderecoStruct = new EnderecoStruct("Rua A", "123", "São Paulo");

Console.WriteLine($"Record Class: {enderecoClass} (Reference Type)");
Console.WriteLine($"Record Struct: {enderecoStruct} (Value Type)");

// Demonstrar diferença de referência
var enderecoClass2 = enderecoClass;
var enderecoStruct2 = enderecoStruct;

enderecoClass2 = enderecoClass2 with { Numero = "456" };
enderecoStruct2 = enderecoStruct2 with { Numero = "456" };

Console.WriteLine($"Após with - Class original: {enderecoClass}");
Console.WriteLine($"Após with - Struct original: {enderecoStruct}");

// 5. Records com validação
Console.WriteLine("\n5. Records com Validação:");
try
{
    var produto = new Produto("", -10.0m, DateTime.Now);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"❌ Validação falhou: {ex.Message}");
}

var produtoValido = new Produto("Notebook", 2500.00m, DateTime.Now.AddDays(30));
Console.WriteLine($"✅ Produto válido: {produtoValido}");

// 6. Records como DTOs para APIs
Console.WriteLine("\n6. Records como DTOs para APIs:");
var usuario = new UsuarioDto(
    Id: Guid.NewGuid(),
    Nome: "Maria Santos",
    Email: "maria@email.com",
    CriadoEm: DateTimeOffset.Now
);

var json = JsonSerializer.Serialize(usuario, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"JSON serializado:\n{json}");

var usuarioDeserializado = JsonSerializer.Deserialize<UsuarioDto>(json);
Console.WriteLine($"Deserializado: {usuarioDeserializado}");

// 7. Inheritance com records
Console.WriteLine("\n7. Herança com Records:");
var funcionario = new Funcionario("Carlos", "Santos", 35, "12345", "Desenvolvedor");
var gerente = new Gerente("Ana", "Costa", 40, "54321", "TI", ["Carlos", "João"]);

Console.WriteLine($"Funcionário: {funcionario}");
Console.WriteLine($"Gerente: {gerente}");

// Polimorfismo
Pessoa[] pessoas = [funcionario, gerente, pessoa1];
foreach (var p in pessoas)
{
    Console.WriteLine($"  {p.GetType().Name}: {p}");
}

// 8. Pattern matching com records
Console.WriteLine("\n8. Pattern Matching com Records:");
ProcessarPessoa(pessoa1);
ProcessarPessoa(funcionario);
ProcessarPessoa(gerente);

// 9. Records com collections
Console.WriteLine("\n9. Records com Coleções:");
var equipe = new Equipe(
    Nome: "Desenvolvimento",
    Membros: ["Alice", "Bob", "Carol"],
    Lider: "Alice"
);

Console.WriteLine($"Equipe: {equipe}");

// With expression em coleções
var equipeExpandida = equipe with { Membros = [.. equipe.Membros, "David"] };
Console.WriteLine($"Equipe expandida: {equipeExpandida}");

// 10. Records como Value Objects
Console.WriteLine("\n10. Records como Value Objects:");
var dinheiro1 = new Dinheiro(100.50m, "BRL");
var dinheiro2 = new Dinheiro(100.50m, "BRL");
var dinheiro3 = new Dinheiro(100.50m, "USD");

Console.WriteLine($"BRL 1: {dinheiro1}");
Console.WriteLine($"BRL 2: {dinheiro2}");
Console.WriteLine($"USD: {dinheiro3}");
Console.WriteLine($"BRL1 == BRL2: {dinheiro1 == dinheiro2}");
Console.WriteLine($"BRL1 == USD: {dinheiro1 == dinheiro3}");

// Operações com Value Objects
try
{
    var soma = dinheiro1 + dinheiro2;
    Console.WriteLine($"Soma: {soma}");
    
    var somaInvalida = dinheiro1 + dinheiro3; // Deve falhar
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Operação inválida: {ex.Message}");
}

// 11. Performance comparison
Console.WriteLine("\n11. Comparação de Performance:");
var performanceTest = new PerformanceTest();
performanceTest.CompararRecordVsClass();

Console.WriteLine("\n=== Resumo das Vantagens dos Records ===");
Console.WriteLine("✅ Imutabilidade por padrão");
Console.WriteLine("✅ Equality por valor automaticamente");
Console.WriteLine("✅ With expressions para modificações");
Console.WriteLine("✅ ToString() automático e útil");
Console.WriteLine("✅ Destructuring support");
Console.WriteLine("✅ Sintaxe concisa");
Console.WriteLine("✅ Ideal para DTOs e Value Objects");
Console.WriteLine("✅ Pattern matching friendly");

Console.WriteLine("\n=== Fim da Demonstração ===");

static void ProcessarPessoa(Pessoa pessoa)
{
    string resultado = pessoa switch
    {
        Gerente g => $"👑 Gerente {g.Nome} da área {g.Area} com {g.Subordinados.Count} subordinados",
        Funcionario f => $"👨‍💻 Funcionário {f.Nome}, matrícula {f.Matricula}, cargo {f.Cargo}",
        Pessoa p => $"👤 Pessoa {p.Nome} com {p.Idade} anos",
        _ => "Tipo desconhecido"
    };
    
    Console.WriteLine($"  {resultado}");
}

// Records definitions

// Record básico - positional record
public record Pessoa(string Nome, string Sobrenome, int Idade)
{
    public string NomeCompleto => $"{Nome} {Sobrenome}";
}

// Record class explícito
public record class EnderecoClass(string Rua, string Numero, string Cidade);

// Record struct para value types
public record struct EnderecoStruct(string Rua, string Numero, string Cidade);

// Record com validação no constructor
public record Produto(string Nome, decimal Preco, DateTime ValidadeAte)
{
    // Validação no init
    public string Nome { get; init; } = !string.IsNullOrWhiteSpace(Nome) 
        ? Nome 
        : throw new ArgumentException("Nome não pode ser vazio", nameof(Nome));
    
    public decimal Preco { get; init; } = Preco > 0 
        ? Preco 
        : throw new ArgumentException("Preço deve ser positivo", nameof(Preco));
    
    public DateTime ValidadeAte { get; init; } = ValidadeAte > DateTime.Now 
        ? ValidadeAte 
        : throw new ArgumentException("Data de validade deve ser no futuro", nameof(ValidadeAte));
}

// DTO para APIs
public record UsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    DateTimeOffset CriadoEm,
    bool Ativo = true
);

// Herança com records
public record Funcionario(string Nome, string Sobrenome, int Idade, string Matricula, string Cargo) 
    : Pessoa(Nome, Sobrenome, Idade);

public record Gerente(string Nome, string Sobrenome, int Idade, string Matricula, string Area, List<string> Subordinados) 
    : Funcionario(Nome, Sobrenome, Idade, Matricula, "Gerente");

// Record com coleções
public record Equipe(string Nome, List<string> Membros, string Lider);

// Value Object com operações
public record Dinheiro(decimal Valor, string Moeda)
{
    public static Dinheiro operator +(Dinheiro left, Dinheiro right)
    {
        if (left.Moeda != right.Moeda)
            throw new InvalidOperationException($"Não é possível somar {left.Moeda} com {right.Moeda}");
        
        return new Dinheiro(left.Valor + right.Valor, left.Moeda);
    }
    
    public override string ToString() => $"{Valor:C} {Moeda}";
}

// Classes para comparação de performance
public class PessoaClass
{
    public string Nome { get; init; } = "";
    public string Sobrenome { get; init; } = "";
    public int Idade { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is PessoaClass other &&
               Nome == other.Nome &&
               Sobrenome == other.Sobrenome &&
               Idade == other.Idade;
    }

    public override int GetHashCode() => HashCode.Combine(Nome, Sobrenome, Idade);
    public override string ToString() => $"{Nome} {Sobrenome}, {Idade} anos";
}

public class PerformanceTest
{
    public void CompararRecordVsClass()
    {
        const int iterations = 100_000;
        
        // Teste criação Records
        var startRecord = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            var record = new Pessoa($"Nome{i}", $"Sobrenome{i}", i % 100);
        }
        var timeRecord = DateTime.Now - startRecord;
        
        // Teste criação Classes
        var startClass = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            var @class = new PessoaClass 
            { 
                Nome = $"Nome{i}", 
                Sobrenome = $"Sobrenome{i}", 
                Idade = i % 100 
            };
        }
        var timeClass = DateTime.Now - startClass;
        
        Console.WriteLine($"  Record creation: {timeRecord.TotalMilliseconds:F1}ms");
        Console.WriteLine($"  Class creation: {timeClass.TotalMilliseconds:F1}ms");
        
        // Teste equality
        var pessoa1 = new Pessoa("João", "Silva", 30);
        var pessoa2 = new Pessoa("João", "Silva", 30);
        var pessoaClass1 = new PessoaClass { Nome = "João", Sobrenome = "Silva", Idade = 30 };
        var pessoaClass2 = new PessoaClass { Nome = "João", Sobrenome = "Silva", Idade = 30 };
        
        var startEqualityRecord = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            var equal = pessoa1 == pessoa2;
        }
        var timeEqualityRecord = DateTime.Now - startEqualityRecord;
        
        var startEqualityClass = DateTime.Now;
        for (int i = 0; i < iterations; i++)
        {
            var equal = pessoaClass1.Equals(pessoaClass2);
        }
        var timeEqualityClass = DateTime.Now - startEqualityClass;
        
        Console.WriteLine($"  Record equality: {timeEqualityRecord.TotalMilliseconds:F1}ms");
        Console.WriteLine($"  Class equality: {timeEqualityClass.TotalMilliseconds:F1}ms");
    }
}
