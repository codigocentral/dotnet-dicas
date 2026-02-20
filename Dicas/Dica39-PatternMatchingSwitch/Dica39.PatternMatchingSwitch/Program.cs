Console.WriteLine("=== Dica 39: Pattern Matching com Switch Expressions ===\n");

// 1. Switch expression básico vs tradicional
Console.WriteLine("1. Switch Expression Básico:");
var dayOfWeek = DayOfWeek.Monday;

// ❌ Forma tradicional - verbosa
string GetDayTypeOld(DayOfWeek day)
{
    switch (day)
    {
        case DayOfWeek.Monday:
        case DayOfWeek.Tuesday:
        case DayOfWeek.Wednesday:
        case DayOfWeek.Thursday:
        case DayOfWeek.Friday:
            return "Dia útil";
        case DayOfWeek.Saturday:
        case DayOfWeek.Sunday:
            return "Final de semana";
        default:
            throw new ArgumentException("Dia inválido");
    }
}

// ✅ Switch expression - conciso e funcional
string GetDayType(DayOfWeek day) => day switch
{
    DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday or DayOfWeek.Friday => "Dia útil",
    DayOfWeek.Saturday or DayOfWeek.Sunday => "Final de semana",
    _ => throw new ArgumentException("Dia inválido")
};

Console.WriteLine($"  {dayOfWeek}: {GetDayType(dayOfWeek)}");
Console.WriteLine($"  {DayOfWeek.Saturday}: {GetDayType(DayOfWeek.Saturday)}");

// 2. Pattern matching com type patterns
Console.WriteLine("\n2. Type Patterns:");
object[] valores = { 42, "Hello", 3.14, true, new Person("João", 30), null! };

foreach (var valor in valores)
{
    var resultado = valor switch
    {
        int i => $"Inteiro: {i}",
        string s => $"String: '{s}' (length: {s.Length})",
        double d => $"Double: {d:F2}",
        bool b => $"Boolean: {b}",
        Person p => $"Pessoa: {p.Nome}, {p.Idade} anos",
        null => "Valor nulo",
        _ => $"Tipo desconhecido: {valor.GetType().Name}"
    };
    Console.WriteLine($"  {resultado}");
}

// 3. Property patterns
Console.WriteLine("\n3. Property Patterns:");
var pessoas = new[]
{
    new Person("Ana", 25),
    new Person("Bruno", 17),
    new Person("Carlos", 35),
    new Person("Diana", 16)
};

foreach (var pessoa in pessoas)
{
    var categoria = pessoa switch
    {
        { Idade: < 18 } => "Menor de idade",
        { Idade: >= 18 and < 60 } => "Adulto",
        { Idade: >= 60 } => "Idoso",
        _ => "Indefinido"
    };
    
    var permissoes = pessoa switch
    {
        { Nome: "Carlos" } => "Administrador",
        { Idade: >= 18 } => "Usuário completo",
        { Idade: < 18 } => "Usuário limitado",
        _ => "Sem permissões"
    };
    
    Console.WriteLine($"  {pessoa.Nome}: {categoria}, {permissoes}");
}

// 4. Tuple patterns
Console.WriteLine("\n4. Tuple Patterns:");
var pontos = new[] { (0, 0), (1, 0), (0, 1), (5, 5), (-1, -1) };

foreach (var (x, y) in pontos)
{
    var quadrante = (x, y) switch
    {
        (0, 0) => "Origem",
        (> 0, > 0) => "Primeiro quadrante",
        (< 0, > 0) => "Segundo quadrante",
        (< 0, < 0) => "Terceiro quadrante",
        (> 0, < 0) => "Quarto quadrante",
        (0, > 0) => "Eixo Y positivo",
        (0, < 0) => "Eixo Y negativo",
        (> 0, 0) => "Eixo X positivo",
        (< 0, 0) => "Eixo X negativo"
    };
    Console.WriteLine($"  ({x}, {y}) → {quadrante}");
}

// 5. Relational patterns com when guards
Console.WriteLine("\n5. Relational Patterns e When Guards:");
var numeros = new[] { -5, 0, 7, 15, 25, 50, 100 };

foreach (var numero in numeros)
{
    var classificacao = numero switch
    {
        < 0 => "Negativo",
        0 => "Zero",
        > 0 and <= 10 => "Pequeno positivo",
        > 10 and <= 50 => "Médio",
        > 50 and < 100 => "Grande",
        100 => "Exatamente cem",
        > 100 => "Muito grande"
    };
    
    // Using when guard for more complex conditions
    var categoria = numero switch
    {
        var n when n % 2 == 0 && n > 0 => "Par positivo",
        var n when n % 2 == 1 && n > 0 => "Ímpar positivo",
        var n when n < 0 => "Negativo",
        0 => "Zero",
        _ => "Outro"
    };
    
    Console.WriteLine($"  {numero}: {classificacao}, {categoria}");
}

// 6. List patterns (C# 11+)
Console.WriteLine("\n6. List Patterns:");
var listas = new[]
{
    new[] { 1 },
    new[] { 1, 2 },
    new[] { 1, 2, 3 },
    new[] { 1, 2, 3, 4, 5 },
    Array.Empty<int>()
};

foreach (var lista in listas)
{
    var descricao = lista switch
    {
        [] => "Lista vazia",
        [var x] => $"Um elemento: {x}",
        [var x, var y] => $"Dois elementos: {x}, {y}",
        [var primeiro, .., var ultimo] => $"Primeiro: {primeiro}, Último: {ultimo}, Total: {lista.Length}",
        _ => "Padrão não reconhecido"
    };
    Console.WriteLine($"  [{string.Join(", ", lista)}] → {descricao}");
}

// 7. Nested patterns
Console.WriteLine("\n7. Nested Patterns:");
var contatos = new[]
{
    new Contato("João", new Endereco("São Paulo", "SP")),
    new Contato("Maria", new Endereco("Rio de Janeiro", "RJ")),
    new Contato("Pedro", new Endereco("Belo Horizonte", "MG")),
    new Contato("Ana", null)
};

foreach (var contato in contatos)
{
    var regiao = contato switch
    {
        { Endereco: { Estado: "SP" or "RJ" or "MG" or "ES" } } => "Sudeste",
        { Endereco: { Estado: "BA" or "SE" or "PE" or "CE" } } => "Nordeste",
        { Endereco: { Estado: "RS" or "SC" or "PR" } } => "Sul",
        { Endereco: null } => "Endereço não informado",
        _ => "Região não mapeada"
    };
    
    var info = contato switch
    {
        { Nome: var nome, Endereco: { Cidade: var cidade, Estado: var estado } } => 
            $"{nome} mora em {cidade}/{estado}",
        { Nome: var nome, Endereco: null } => 
            $"{nome} não tem endereço cadastrado",
        _ => "Dados incompletos"
    };
    
    Console.WriteLine($"  {info} - Região: {regiao}");
}

// 8. Switch expressions com cálculos
Console.WriteLine("\n8. Switch Expressions para Cálculos:");
var formas = new object[]
{
    new Retangulo(5, 3),
    new Circulo(4),
    new Triangulo(6, 4),
    new Quadrado(5)
};

foreach (var forma in formas)
{
    var area = forma switch
    {
        Retangulo { Largura: var l, Altura: var a } => l * a,
        Circulo { Raio: var r } => Math.PI * r * r,
        Triangulo { Base: var b, Altura: var h } => 0.5 * b * h,
        Quadrado { Lado: var lado } => lado * lado,
        _ => 0
    };
    
    var perimetro = forma switch
    {
        Retangulo { Largura: var l, Altura: var a } => 2 * (l + a),
        Circulo { Raio: var r } => 2 * Math.PI * r,
        Triangulo { Base: var b, Altura: var h } => b + 2 * Math.Sqrt((b/2) * (b/2) + h * h),
        Quadrado { Lado: var lado } => 4 * lado,
        _ => 0
    };
    
    Console.WriteLine($"  {forma.GetType().Name}: Área = {area:F2}, Perímetro = {perimetro:F2}");
}

// 9. Performance comparison
Console.WriteLine("\n9. Comparação de Performance:");
var performanceTest = new PatternMatchingPerformance();
performanceTest.ComparePerformance();

// 10. Advanced scenarios
Console.WriteLine("\n10. Cenários Avançados:");
AdvancedPatternMatching();

Console.WriteLine("\n=== Resumo das Vantagens do Pattern Matching ===");
Console.WriteLine("✅ Código mais conciso e legível");
Console.WriteLine("✅ Melhor performance que if-else chains");
Console.WriteLine("✅ Exhaustiveness checking (quando apropriado)");
Console.WriteLine("✅ Expressividade superior");
Console.WriteLine("✅ Redução de bugs por simplificação");
Console.WriteLine("✅ Melhor suporte para programação funcional");

Console.WriteLine("\n=== Fim da Demonstração ===");

static void AdvancedPatternMatching()
{
    // Advanced scenarios with discriminated unions simulation
    var transacoes = new ITransacao[]
    {
        new Deposito(1000),
        new Saque(500),
        new Transferencia(200, "João"),
        new Pix(150, "12345678901")
    };

    foreach (var transacao in transacoes)
    {
        var resultado = ProcessarTransacao(transacao);
        Console.WriteLine($"  {resultado}");
    }
    
    static string ProcessarTransacao(ITransacao transacao) => transacao switch
    {
        Deposito { Valor: var valor } => $"Depósito de R$ {valor:F2} processado",
        Saque { Valor: var valor } when valor <= 1000 => $"Saque de R$ {valor:F2} aprovado",
        Saque { Valor: var valor } => $"Saque de R$ {valor:F2} negado - limite excedido",
        Transferencia { Valor: var valor, Destinatario: var dest } => 
            $"Transferência de R$ {valor:F2} para {dest}",
        Pix { Valor: var valor, ChavePix: var chave } => 
            $"PIX de R$ {valor:F2} para chave {chave}",
        _ => "Transação não reconhecida"
    };
}

// Record types for demonstration
public record Person(string Nome, int Idade);
public record Endereco(string Cidade, string Estado);
public record Contato(string Nome, Endereco? Endereco);

// Shape records for geometry calculations
public record Retangulo(double Largura, double Altura);
public record Circulo(double Raio);
public record Triangulo(double Base, double Altura);
public record Quadrado(double Lado);

// Transaction types for advanced pattern matching
public interface ITransacao
{
    decimal Valor { get; }
}

public record Deposito(decimal Valor) : ITransacao;
public record Saque(decimal Valor) : ITransacao;
public record Transferencia(decimal Valor, string Destinatario) : ITransacao;
public record Pix(decimal Valor, string ChavePix) : ITransacao;

// Performance testing class
public class PatternMatchingPerformance
{
    private const int Iterations = 1_000_000;
    
    public void ComparePerformance()
    {
        var testValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Measure switch expression
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            foreach (var value in testValues)
            {
                var result = GetCategoryWithSwitchExpression(value);
            }
        }
        sw.Stop();
        var switchTime = sw.ElapsedMilliseconds;
        
        // Measure if-else chain
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            foreach (var value in testValues)
            {
                var result = GetCategoryWithIfElse(value);
            }
        }
        sw.Stop();
        var ifElseTime = sw.ElapsedMilliseconds;
        
        // Measure traditional switch
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            foreach (var value in testValues)
            {
                var result = GetCategoryWithTraditionalSwitch(value);
            }
        }
        sw.Stop();
        var traditionalSwitchTime = sw.ElapsedMilliseconds;
        
        Console.WriteLine($"  Switch Expression: {switchTime}ms");
        Console.WriteLine($"  If-Else Chain: {ifElseTime}ms");
        Console.WriteLine($"  Traditional Switch: {traditionalSwitchTime}ms");
        
        var bestTime = Math.Min(switchTime, Math.Min(ifElseTime, traditionalSwitchTime));
        Console.WriteLine($"  Melhor performance: {(bestTime == switchTime ? "Switch Expression" : bestTime == ifElseTime ? "If-Else" : "Traditional Switch")}");
    }
    
    private string GetCategoryWithSwitchExpression(int value) => value switch
    {
        1 or 2 or 3 => "Baixo",
        4 or 5 or 6 => "Médio",
        7 or 8 or 9 => "Alto",
        10 => "Máximo",
        _ => "Inválido"
    };
    
    private string GetCategoryWithIfElse(int value)
    {
        if (value >= 1 && value <= 3) return "Baixo";
        if (value >= 4 && value <= 6) return "Médio";
        if (value >= 7 && value <= 9) return "Alto";
        if (value == 10) return "Máximo";
        return "Inválido";
    }
    
    private string GetCategoryWithTraditionalSwitch(int value)
    {
        switch (value)
        {
            case 1:
            case 2:
            case 3:
                return "Baixo";
            case 4:
            case 5:
            case 6:
                return "Médio";
            case 7:
            case 8:
            case 9:
                return "Alto";
            case 10:
                return "Máximo";
            default:
                return "Inválido";
        }
    }
}
