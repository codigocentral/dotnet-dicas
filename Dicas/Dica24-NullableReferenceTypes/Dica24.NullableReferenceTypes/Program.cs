using System.Diagnostics.CodeAnalysis;

Console.WriteLine("=== Dica 24: Nullable Reference Types ===\n");

// 1. Habilitando Nullable Reference Types
Console.WriteLine("1. Diferença entre String? e String:");
string nomeObrigatorio = "João"; // Não pode ser null
string? nomeOpcional = null;     // Pode ser null

Console.WriteLine($"  Nome obrigatório: {nomeObrigatorio}");
Console.WriteLine($"  Nome opcional: {nomeOpcional ?? "não informado"}");

// 2. Null-conditional operators
Console.WriteLine("\n2. Operadores Null-Conditional:");
var usuario = new Usuario("Maria", null);

// ❌ Perigoso - pode lançar NullReferenceException
// int tamanhoEmail = usuario.Email.Length;

// ✅ Seguro - usando null-conditional
int? tamanhoEmail = usuario.Email?.Length;
Console.WriteLine($"  Tamanho do email: {tamanhoEmail?.ToString() ?? "não disponível"}");

// Null-conditional com indexador
var emails = new string?[] { "teste@email.com", null, "outro@email.com" };
for (int i = 0; i < emails.Length; i++)
{
    Console.WriteLine($"  Email {i}: {emails[i]?.ToUpper() ?? "VAZIO"}");
}

// 3. Null-coalescing operators
Console.WriteLine("\n3. Operadores Null-Coalescing:");
string? configuracao = null;
string valorPadrao = configuracao ?? "valor padrão";
Console.WriteLine($"  Configuração: {valorPadrao}");

// Null-coalescing assignment (??=)
string? cache = null;
cache ??= "inicializado pela primeira vez";
Console.WriteLine($"  Cache: {cache}");

cache ??= "não será alterado";
Console.WriteLine($"  Cache após segunda tentativa: {cache}");

// 4. Validação com ArgumentNullException
Console.WriteLine("\n4. Validação de Argumentos:");
try
{
    var produto = new Produto(null!, 100); // Forçando null
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"  ❌ {ex.Message}");
}

var produtoValido = new Produto("Notebook", 2500);
Console.WriteLine($"  ✅ Produto válido: {produtoValido.Nome}");

// 5. Atributos de anotação nullable
Console.WriteLine("\n5. Métodos com Anotações Nullable:");
var processador = new ProcessadorDados();

// Método que pode retornar null
string? resultado1 = processador.BuscarDados("chave_inexistente");
Console.WriteLine($"  Busca inexistente: {resultado1 ?? "não encontrado"}");

string? resultado2 = processador.BuscarDados("usuario");
Console.WriteLine($"  Busca existente: {resultado2 ?? "não encontrado"}");

// Método que garante não retornar null
string resultadoGarantido = processador.ObterDadosObrigatorios("admin");
Console.WriteLine($"  Dados obrigatórios: {resultadoGarantido}");

// 6. Pattern matching com nulls
Console.WriteLine("\n6. Pattern Matching com Nulls:");
var objetos = new object?[] { "texto", null, 42, new Usuario("Pedro", "pedro@email.com") };

foreach (var obj in objetos)
{
    var descricao = obj switch
    {
        null => "Objeto nulo",
        string s => $"String: '{s}'",
        int i => $"Inteiro: {i}",
        Usuario u => $"Usuário: {u.Nome}",
        _ => $"Tipo desconhecido: {obj.GetType().Name}"
    };
    Console.WriteLine($"  {descricao}");
}

// 7. Trabalhando com collections nullable
Console.WriteLine("\n7. Coleções com Nullable:");
var listaUsuarios = new List<Usuario?>();
listaUsuarios.Add(new Usuario("Ana", "ana@email.com"));
listaUsuarios.Add(null);
listaUsuarios.Add(new Usuario("Carlos", null));

Console.WriteLine("  Usuários na lista:");
foreach (var u in listaUsuarios)
{
    if (u is not null)
    {
        Console.WriteLine($"    - {u.Nome} ({u.Email ?? "sem email"})");
    }
    else
    {
        Console.WriteLine("    - [usuário nulo]");
    }
}

// 8. Métodos de extensão para nullable
Console.WriteLine("\n8. Métodos de Extensão para Nullable:");
string? textoNulo = null;
string? textoVazio = "";
string? textoValido = "conteúdo";

Console.WriteLine($"  Texto nulo é vazio?: {textoNulo.EstaVazio()}");
Console.WriteLine($"  Texto vazio é vazio?: {textoVazio.EstaVazio()}");
Console.WriteLine($"  Texto válido é vazio?: {textoValido.EstaVazio()}");

// 9. Nullable com async/await
Console.WriteLine("\n9. Nullable com Async/Await:");
var servicoAsync = new ServicoAssincrono();
var dadosAsync = await servicoAsync.BuscarDadosAsync("perfil_usuario");
Console.WriteLine($"  Dados assíncronos: {dadosAsync ?? "não encontrados"}");

// 10. Best practices e migração
Console.WriteLine("\n10. Melhores Práticas:");
ExemplosBoasPraticas();

Console.WriteLine("\n=== Resumo das Vantagens dos Nullable Reference Types ===");
Console.WriteLine("✅ Previne NullReferenceException em tempo de compilação");
Console.WriteLine("✅ Documentação explícita sobre nullabilidade");
Console.WriteLine("✅ Melhor IntelliSense e suporte IDE");
Console.WriteLine("✅ Migração gradual em projetos existentes");
Console.WriteLine("✅ Código mais robusto e confiável");
Console.WriteLine("✅ Melhor experiência de desenvolvimento");

Console.WriteLine("\n=== Fim da Demonstração ===");

static void ExemplosBoasPraticas()
{
    // ✅ Use validação explícita
    static string ProcessarNome(string? nome)
    {
        ArgumentNullException.ThrowIfNull(nome);
        return nome.Trim().ToUpper();
    }

    // ✅ Use early return para nulls
    static string? ObterEmailDominio(Usuario? usuario)
    {
        if (usuario?.Email is null)
            return null;

        var indiceArroba = usuario.Email.IndexOf('@');
        return indiceArroba > 0 ? usuario.Email[(indiceArroba + 1)..] : null;
    }

    // ✅ Use null-conditional chains
    static int ObterTamanhoEmail(Usuario? usuario) => usuario?.Email?.Length ?? 0;

    var exemploUsuario = new Usuario("Teste", "teste@exemplo.com");
    Console.WriteLine($"  Nome processado: {ProcessarNome(exemploUsuario.Nome)}");
    Console.WriteLine($"  Domínio do email: {ObterEmailDominio(exemploUsuario) ?? "não disponível"}");
    Console.WriteLine($"  Tamanho do email: {ObterTamanhoEmail(exemploUsuario)}");
}

// Classes de demonstração

public class Usuario
{
    public string Nome { get; }
    public string? Email { get; }

    public Usuario(string nome, string? email)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email;
    }
}

public class Produto
{
    public string Nome { get; }
    public decimal Preco { get; }

    public Produto(string nome, decimal preco)
    {
        // Usando ArgumentNullException.ThrowIfNull (C# 11+)
        ArgumentNullException.ThrowIfNull(nome);
        
        if (preco < 0)
            throw new ArgumentException("Preço não pode ser negativo", nameof(preco));

        Nome = nome;
        Preco = preco;
    }
}

public class ProcessadorDados
{
    private readonly Dictionary<string, string> _dados;

    public ProcessadorDados()
    {
        _dados = new Dictionary<string, string>
        {
            { "usuario", "dados do usuário" },
            { "admin", "dados do administrador" },
            { "config", "configurações do sistema" }
        };
    }

    // Método que pode retornar null
    public string? BuscarDados(string chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        return _dados.TryGetValue(chave, out var valor) ? valor : null;
    }

    // Método que garante não retornar null
    [return: NotNull]
    public string ObterDadosObrigatorios(string chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        
        if (_dados.TryGetValue(chave, out var valor))
            return valor;
        
        throw new KeyNotFoundException($"Chave '{chave}' não encontrada");
    }

    // Método que verifica se a saída não é null
    public bool TentarObterDados(string chave, [NotNullWhen(true)] out string? valor)
    {
        ArgumentNullException.ThrowIfNull(chave);
        return _dados.TryGetValue(chave, out valor);
    }
}

public class ServicoAssincrono
{
    private readonly Dictionary<string, string> _dadosAsync;

    public ServicoAssincrono()
    {
        _dadosAsync = new Dictionary<string, string>
        {
            { "perfil_usuario", "dados do perfil" },
            { "configuracoes", "configurações assíncronas" }
        };
    }

    public async Task<string?> BuscarDadosAsync(string chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        
        // Simula operação assíncrona
        await Task.Delay(100);
        
        return _dadosAsync.TryGetValue(chave, out var valor) ? valor : null;
    }

    public async Task<string> ObterDadosObrigatoriosAsync(string chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        
        var resultado = await BuscarDadosAsync(chave);
        return resultado ?? throw new KeyNotFoundException($"Dados para '{chave}' não encontrados");
    }
}

// Métodos de extensão para trabalhar com nullable
public static class ExtensoesSring
{
    public static bool EstaVazio([NotNullWhen(false)] this string? valor)
    {
        return string.IsNullOrEmpty(valor);
    }

    public static bool EstaVazioOuBranco([NotNullWhen(false)] this string? valor)
    {
        return string.IsNullOrWhiteSpace(valor);
    }

    public static string? TrimSeguro(this string? valor)
    {
        return valor?.Trim();
    }

    public static string ValorOuPadrao(this string? valor, string valorPadrao = "")
    {
        return valor ?? valorPadrao;
    }
}
