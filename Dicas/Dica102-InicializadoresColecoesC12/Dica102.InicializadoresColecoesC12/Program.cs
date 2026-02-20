Console.WriteLine("=== Dica 102: Inicializadores de Coleção C# 12 ===");
Console.WriteLine("C# 12 introduziu novos inicializadores de coleção usando apenas dois colchetes ([]).");
Console.WriteLine("Isso simplifica a criação de coleções como arrays, lists, dictionaries e tipos imutáveis.\n");

// 1. DEMONSTRAÇÃO: Arrays com sintaxe []
Console.WriteLine("1. Arrays com nova sintaxe []:");
// Sintaxe tradicional vs C# 12
int[] numerosTradicional = new int[] { 1, 2, 3, 4, 5 };
int[] numerosC12 = [1, 2, 3, 4, 5];

Console.WriteLine($"  Tradicional: [{string.Join(", ", numerosTradicional)}]");
Console.WriteLine($"  C# 12: [{string.Join(", ", numerosC12)}]");

// Arrays de strings
string[] linguagensTradicional = new string[] { "C#", "F#", "VB.NET" };
string[] linguagensC12 = ["C#", "F#", "VB.NET"];

Console.WriteLine($"  Linguagens (tradicional): [{string.Join(", ", linguagensTradicional)}]");
Console.WriteLine($"  Linguagens (C# 12): [{string.Join(", ", linguagensC12)}]");

// 2. DEMONSTRAÇÃO: Lists com sintaxe []
Console.WriteLine("\n2. Lists com nova sintaxe []:");
// List<T> com sintaxe C# 12
List<int> numerosLista = [10, 20, 30, 40, 50];
List<string> coresLista = ["Vermelho", "Verde", "Azul"];

Console.WriteLine($"  Lista de números: [{string.Join(", ", numerosLista)}]");
Console.WriteLine($"  Lista de cores: [{string.Join(", ", coresLista)}]");

// 3. DEMONSTRAÇÃO: Combinando coleções
Console.WriteLine("\n3. Combinando coleções com spread operator:");
int[] inicio = [1, 2, 3];
int[] meio = [4, 5, 6];
int[] fim = [7, 8, 9];

// Combinar arrays usando spread operator
int[] todosCombinados = [..inicio, ..meio, ..fim];
Console.WriteLine($"  Arrays combinados: [{string.Join(", ", todosCombinados)}]");

// Combinando com elementos individuais
int[] numerosMistos = [0, ..inicio, 99, ..fim, 100];
Console.WriteLine($"  Misturado: [{string.Join(", ", numerosMistos)}]");

// 4. DEMONSTRAÇÃO: ImmutableArray
Console.WriteLine("\n4. ImmutableArray com sintaxe []:");
var immutableArray = System.Collections.Immutable.ImmutableArray.Create([1, 2, 3, 4, 5]);
Console.WriteLine($"  ImmutableArray: [{string.Join(", ", immutableArray)}]");

// 5. DEMONSTRAÇÃO: Coleções aninhadas
Console.WriteLine("\n5. Coleções aninhadas:");
int[][] matrizTradicional = new int[][] 
{
    new int[] { 1, 2, 3 },
    new int[] { 4, 5, 6 },
    new int[] { 7, 8, 9 }
};

int[][] matrizC12 = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
];

Console.WriteLine("  Matriz tradicional:");
for (int i = 0; i < matrizTradicional.Length; i++)
{
    Console.WriteLine($"    [{string.Join(", ", matrizTradicional[i])}]");
}

Console.WriteLine("  Matriz C# 12:");
for (int i = 0; i < matrizC12.Length; i++)
{
    Console.WriteLine($"    [{string.Join(", ", matrizC12[i])}]");
}

// 6. DEMONSTRAÇÃO: Span e ReadOnlySpan
Console.WriteLine("\n6. Span e ReadOnlySpan:");
Span<int> spanNumeros = [1, 2, 3, 4, 5];
ReadOnlySpan<char> spanTexto = ['H', 'e', 'l', 'l', 'o'];

Console.WriteLine($"  Span<int>: [{string.Join(", ", spanNumeros.ToArray())}]");
Console.WriteLine($"  ReadOnlySpan<char>: {new string(spanTexto)}");

// 7. DEMONSTRAÇÃO: Métodos que retornam coleções
Console.WriteLine("\n7. Métodos que retornam coleções:");
var paresAte10 = ObterNumerosPares();
var primos = ObterNumerosPrimos();

Console.WriteLine($"  Pares até 10: [{string.Join(", ", paresAte10)}]");
Console.WriteLine($"  Primeiros 5 primos: [{string.Join(", ", primos)}]");

// 8. DEMONSTRAÇÃO: Performance comparison
Console.WriteLine("\n8. Comparação de performance:");
TestarPerformance();

// 9. DEMONSTRAÇÃO: Casos de uso práticos
Console.WriteLine("\n9. Casos de uso práticos:");
DemonstrarCasosUso();

Console.WriteLine("\n=== Resumo dos Benefícios dos Inicializadores C# 12 ===");
Console.WriteLine("✅ Sintaxe mais limpa e concisa");
Console.WriteLine("✅ Menos verbosidade no código");
Console.WriteLine("✅ Melhor legibilidade");
Console.WriteLine("✅ Funciona com qualquer tipo de coleção");
Console.WriteLine("✅ Suporte nativo ao spread operator (..)");
Console.WriteLine("✅ Compatível com Span, ReadOnlySpan e tipos imutáveis");
Console.WriteLine("✅ Inferência de tipo automática");

static int[] ObterNumerosPares()
{
    return [2, 4, 6, 8, 10];
}

static List<int> ObterNumerosPrimos()
{
    return [2, 3, 5, 7, 11];
}

static void TestarPerformance()
{
    const int iteracoes = 1_000_000;
    
    // Teste com sintaxe tradicional
    var inicio = DateTime.Now;
    for (int i = 0; i < iteracoes; i++)
    {
        var array = new int[] { 1, 2, 3, 4, 5 };
    }
    var tempoTradicional = DateTime.Now - inicio;
    
    // Teste com sintaxe C# 12
    inicio = DateTime.Now;
    for (int i = 0; i < iteracoes; i++)
    {
        int[] array = [1, 2, 3, 4, 5];
    }
    var tempoC12 = DateTime.Now - inicio;
    
    Console.WriteLine($"  Sintaxe tradicional: {tempoTradicional.TotalMilliseconds:F2}ms");
    Console.WriteLine($"  Sintaxe C# 12: {tempoC12.TotalMilliseconds:F2}ms");
    
    if (tempoTradicional.TotalMilliseconds > 0)
    {
        var melhoria = ((tempoTradicional.TotalMilliseconds - tempoC12.TotalMilliseconds) / tempoTradicional.TotalMilliseconds) * 100;
        Console.WriteLine($"  Diferença: {melhoria:F1}%");
    }
}

static void DemonstrarCasosUso()
{
    // Configuração de aplicação
    var configuracaoServidor = new ConfiguracaoServidor
    {
        Hosts = ["localhost", "127.0.0.1", "::1"],
        Portas = [80, 443, 8080],
        Protocolos = ["HTTP", "HTTPS"]
    };
    
    Console.WriteLine("  Configuração do servidor:");
    Console.WriteLine($"    Hosts: [{string.Join(", ", configuracaoServidor.Hosts)}]");
    Console.WriteLine($"    Portas: [{string.Join(", ", configuracaoServidor.Portas)}]");
    Console.WriteLine($"    Protocolos: [{string.Join(", ", configuracaoServidor.Protocolos)}]");
    
    // Dados de teste
    var dadosTeste = new DadosTeste
    {
        Usuarios = [
            new("João", "joao@email.com"),
            new("Maria", "maria@email.com"),
            new("Pedro", "pedro@email.com")
        ],
        Tags = ["desenvolvimento", "testes", "produção"]
    };
    
    Console.WriteLine("  Dados de teste:");
    foreach (var usuario in dadosTeste.Usuarios)
    {
        Console.WriteLine($"    {usuario.Nome} ({usuario.Email})");
    }
    Console.WriteLine($"    Tags: [{string.Join(", ", dadosTeste.Tags)}]");
    
    // Operações com coleções
    int[] numeros = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    var pares = numeros.Where(n => n % 2 == 0).ToArray();
    var impares = numeros.Where(n => n % 2 != 0).ToArray();
    
    // Recombinando usando spread operator
    int[] reordenados = [..pares, ..impares];
    
    Console.WriteLine($"  Números originais: [{string.Join(", ", numeros)}]");
    Console.WriteLine($"  Pares primeiro: [{string.Join(", ", reordenados)}]");
}

// Classes para demonstração
public class ConfiguracaoServidor
{
    public string[] Hosts { get; init; } = [];
    public int[] Portas { get; init; } = [];
    public string[] Protocolos { get; init; } = [];
}

public class DadosTeste
{
    public List<Usuario> Usuarios { get; init; } = [];
    public string[] Tags { get; init; } = [];
}

public record Usuario(string Nome, string Email);
