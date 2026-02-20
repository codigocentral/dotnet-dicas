using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.WriteLine("=== Dica 62: Usando nameof para Nomes de Símbolos ===");
Console.WriteLine("Evite usar strings codificadas (hard-coded) para representar nomes de código.");
Console.WriteLine("Use nameof(). Se você renomear o símbolo, seu código será atualizado automaticamente.");
Console.WriteLine("É perfeito para logging, validação, exceções e atributos.");
Console.WriteLine();

// Demonstração de diferentes usos do nameof
var pessoa = new Pessoa { Nome = "João", Idade = 30, Email = "joao@email.com" };
var validador = new PessoaValidator();
var logger = new Logger();

// 1. Logging com nameof
logger.LogInfo($"Criando objeto do tipo {nameof(Pessoa)}");
logger.LogInfo($"Valor da propriedade {nameof(pessoa.Nome)}: {pessoa.Nome}");

// 2. Validação com nameof
var resultadosValidacao = validador.Validar(pessoa);
foreach (var erro in resultadosValidacao)
{
    Console.WriteLine($"Erro de validação: {erro}");
}

// 3. Exceções com nameof
try
{
    pessoa.DefinirIdade(-5);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Exceção capturada: {ex.Message}");
}

// 4. Notificação de mudança de propriedade
pessoa.PropertyChanged += (sender, e) =>
{
    Console.WriteLine($"Propriedade alterada: {e.PropertyName}");
};

pessoa.Nome = "João Silva";
pessoa.Idade = 31;

// 5. Reflexão com nameof vs strings hard-coded
Console.WriteLine("\n=== Comparação: nameof vs Reflexão vs Strings Hard-coded ===");
ReflectionDemo.DemonstrarComparacao();

// 6. Atributos com nameof
Console.WriteLine("\n=== Atributos com nameof ===");
AttributeDemo.DemonstrarAtributos();

// 7. Expression Trees com nameof
Console.WriteLine("\n=== Expression Trees com nameof ===");
ExpressionDemo.DemonstrarExpressions();

public class Pessoa : INotifyPropertyChanged
{
    private string _nome = string.Empty;
    private int _idade;
    private string _email = string.Empty;

    public string Nome
    {
        get => _nome;
        set
        {
            if (_nome != value)
            {
                _nome = value;
                // ✅ CORRETO: Usando nameof
                OnPropertyChanged(nameof(Nome));
                // ❌ INCORRETO: OnPropertyChanged("Nome");
            }
        }
    }

    public int Idade
    {
        get => _idade;
        set
        {
            if (_idade != value)
            {
                _idade = value;
                OnPropertyChanged(nameof(Idade));
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    public void DefinirIdade(int novaIdade)
    {
        if (novaIdade < 0)
        {
            // ✅ CORRETO: Usando nameof para parâmetro
            throw new ArgumentException($"Idade não pode ser negativa", nameof(novaIdade));
            // ❌ INCORRETO: throw new ArgumentException("Idade não pode ser negativa", "novaIdade");
        }
        
        Idade = novaIdade;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PessoaValidator
{
    public List<string> Validar(Pessoa pessoa)
    {
        var erros = new List<string>();

        // ✅ CORRETO: Usando nameof em validações
        if (string.IsNullOrWhiteSpace(pessoa.Nome))
        {
            erros.Add($"{nameof(pessoa.Nome)} é obrigatório");
        }

        if (pessoa.Idade < 0 || pessoa.Idade > 150)
        {
            erros.Add($"{nameof(pessoa.Idade)} deve estar entre 0 e 150");
        }

        if (string.IsNullOrWhiteSpace(pessoa.Email) || !pessoa.Email.Contains('@'))
        {
            erros.Add($"{nameof(pessoa.Email)} deve ser um email válido");
        }

        return erros;
    }
}

public class Logger
{
    public void LogInfo(string message, [CallerMemberName] string methodName = "")
    {
        Console.WriteLine($"[INFO] [{methodName}] {message}");
    }

    public void LogError(Exception ex, [CallerMemberName] string methodName = "")
    {
        // ✅ CORRETO: Usando nameof para propriedades da exceção
        Console.WriteLine($"[ERROR] [{methodName}] {nameof(Exception)}: {ex.GetType().Name}");
        Console.WriteLine($"[ERROR] [{methodName}] {nameof(ex.Message)}: {ex.Message}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[ERROR] [{methodName}] {nameof(ex.InnerException)}: {ex.InnerException.Message}");
        }
    }
}

public static class ReflectionDemo
{
    public static void DemonstrarComparacao()
    {
        var pessoa = new Pessoa { Nome = "Test", Idade = 25 };
        
        // ✅ CORRETO: Usando nameof (compile-time safe)
        var propriedadeNome = typeof(Pessoa).GetProperty(nameof(Pessoa.Nome));
        Console.WriteLine($"Propriedade encontrada com nameof: {propriedadeNome?.Name}");
        
        // ❌ INCORRETO: String hard-coded (pode quebrar em refatoração)
        var propriedadeIdade = typeof(Pessoa).GetProperty("Idade");
        Console.WriteLine($"Propriedade encontrada com string: {propriedadeIdade?.Name}");
        
        // Exemplo de como nameof previne erros
        try
        {
            // Esta linha causaria erro de compilação se a propriedade fosse renomeada
            var prop = typeof(Pessoa).GetProperty(nameof(Pessoa.Email));
            Console.WriteLine($"Propriedade Email encontrada: {prop?.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }
}

public static class AttributeDemo
{
    public static void DemonstrarAtributos()
    {
        var type = typeof(ProdutoComAtributos);
        var propriedades = type.GetProperties();
        
        foreach (var prop in propriedades)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>();
            
            Console.WriteLine($"Propriedade: {prop.Name}");
            
            if (displayAttr != null)
            {
                Console.WriteLine($"  Display Name: {displayAttr.Name}");
            }
            
            if (requiredAttr != null)
            {
                Console.WriteLine($"  Required: Sim");
                Console.WriteLine($"  Error Message: {requiredAttr.ErrorMessage}");
            }
            
            Console.WriteLine();
        }
    }
}

public class ProdutoComAtributos
{
    // ✅ CORRETO: Usando nameof em atributos
    [Required(ErrorMessage = $"O campo {nameof(Nome)} é obrigatório")]
    [Display(Name = "Nome do Produto")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = $"O campo {nameof(Preco)} é obrigatório")]
    [Display(Name = "Preço")]
    [Range(0.01, double.MaxValue, ErrorMessage = $"O campo {nameof(Preco)} deve ser maior que zero")]
    public decimal Preco { get; set; }

    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }
}

public static class ExpressionDemo
{
    public static void DemonstrarExpressions()
    {
        // Exemplo de como nameof pode ser usado com Expression Trees
        var expressionBuilder = new ExpressionBuilder<Pessoa>();
        
        // ✅ CORRETO: Type-safe com nameof
        var nomeExpression = expressionBuilder.GetPropertyExpression(nameof(Pessoa.Nome));
        var idadeExpression = expressionBuilder.GetPropertyExpression(nameof(Pessoa.Idade));
        
        Console.WriteLine($"Expression para {nameof(Pessoa.Nome)}: {nomeExpression}");
        Console.WriteLine($"Expression para {nameof(Pessoa.Idade)}: {idadeExpression}");
    }
}

public class ExpressionBuilder<T>
{
    public string GetPropertyExpression(string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property == null)
        {
            throw new ArgumentException($"Propriedade '{propertyName}' não encontrada no tipo {typeof(T).Name}");
        }
        
        return $"{typeof(T).Name}.{property.Name}";
    }
}
