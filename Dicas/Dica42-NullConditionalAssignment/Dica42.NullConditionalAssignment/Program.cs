using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== Dica 42: Atribuição Condicional Nula (??=) ===\n");

// Configuração do logger
using var serviceProvider = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// 1. Exemplo básico - inicialização de propriedades
Console.WriteLine("1. Inicialização de Propriedades:");
var usuario = new Usuario();
Console.WriteLine($"Nome antes: '{usuario.Nome}'");

// Sem ??= (verboso)
if (usuario.Nome == null)
{
    usuario.Nome = "Usuário Anônimo";
}

usuario.Nome = null; // reset para demonstrar ??=

// Com ??= (conciso)
usuario.Nome ??= "Usuário Anônimo";
Console.WriteLine($"Nome depois: '{usuario.Nome}'");

// 2. Exemplo com coleções
Console.WriteLine("\n2. Inicialização de Coleções:");
var configuracao = new Configuracao();

// Inicializar lista se necessário
configuracao.Opcoes ??= [];
configuracao.Opcoes.Add("debug");
configuracao.Opcoes.Add("verbose");

Console.WriteLine($"Opções: [{string.Join(", ", configuracao.Opcoes)}]");

// 3. Exemplo com dictionary/cache
Console.WriteLine("\n3. Cache com Lazy Loading:");
var cache = new CacheService();

// Primeira chamada - carrega dados
var dados1 = cache.ObterDados("usuarios");
Console.WriteLine($"Primeira chamada: {dados1.Count} itens");

// Segunda chamada - usa cache
var dados2 = cache.ObterDados("usuarios");
Console.WriteLine($"Segunda chamada: {dados2.Count} itens");

// 4. Exemplo com StringBuilder para concatenação
Console.WriteLine("\n4. StringBuilder Lazy:");
var relatorioService = new RelatorioService();

relatorioService.AdicionarLinha("Header do relatório");
relatorioService.AdicionarLinha("Dados importantes");
relatorioService.AdicionarLinha("Footer");

Console.WriteLine("Relatório gerado:");
Console.WriteLine(relatorioService.ObterRelatorio());

// 5. Exemplo com nullable reference types
Console.WriteLine("\n5. Configuração com Fallbacks:");
var config = new ConfiguracaoAvancada
{
    Servidor = null, // será definido pelo fallback
    Porta = 8080
};

// Definir valores padrão apenas se necessário
config.Servidor ??= Environment.GetEnvironmentVariable("SERVER_URL") ?? "localhost";
config.ConnectionString ??= $"Server={config.Servidor};Port={config.Porta};Database=app";

Console.WriteLine($"Servidor: {config.Servidor}");
Console.WriteLine($"Connection String: {config.ConnectionString}");

// 6. Exemplo com validação de entrada
Console.WriteLine("\n6. Validação com Valores Padrão:");
var validador = new ValidadorEntrada();

// Simular entradas do usuário
string? entradaNula = null;
string? entradaVazia = "";
string? entradaValida = "dados válidos";

Console.WriteLine($"Entrada nula: '{validador.ProcessarEntrada(entradaNula)}'");
Console.WriteLine($"Entrada vazia: '{validador.ProcessarEntrada(entradaVazia)}'");
Console.WriteLine($"Entrada válida: '{validador.ProcessarEntrada(entradaValida)}'");

// 7. Exemplo com delegates e events
Console.WriteLine("\n7. Event Handlers:");
var eventManager = new EventManager();

// Adicionar handlers apenas se necessário
eventManager.ConfigurarEventos();
eventManager.TriggerEvent("Teste de evento");

Console.WriteLine("\n=== Fim da Demonstração ===");

// Classes de apoio
public class Usuario
{
    public string? Nome { get; set; }
    public DateTime UltimoAcesso { get; set; }
    public List<string>? Permissoes { get; set; }
}

public class Configuracao
{
    public List<string>? Opcoes { get; set; }
    public Dictionary<string, string>? Parametros { get; set; }
}

public class CacheService
{
    private Dictionary<string, List<string>>? _cache;

    public List<string> ObterDados(string chave)
    {
        // Inicializar cache apenas se necessário
        _cache ??= new Dictionary<string, List<string>>();

        // Verificar se a chave existe, se não, criar entrada
        if (!_cache.ContainsKey(chave))
        {
            _cache[chave] = CarregarDadosDoServidor(chave);
        }

        return _cache[chave];
    }

    private static List<string> CarregarDadosDoServidor(string chave)
    {
        Console.WriteLine($"  → Carregando dados para '{chave}' do servidor...");
        // Simular carregamento
        Thread.Sleep(100);
        return ["item1", "item2", "item3"];
    }
}

public class RelatorioService
{
    private StringBuilder? _relatorio;

    public void AdicionarLinha(string linha)
    {
        // Inicializar StringBuilder apenas quando necessário
        _relatorio ??= new StringBuilder();
        _relatorio.AppendLine(linha);
    }

    public string ObterRelatorio()
    {
        // Retornar string vazia se nenhuma linha foi adicionada
        return _relatorio?.ToString() ?? "";
    }
}

public class ConfiguracaoAvancada
{
    public string? Servidor { get; set; }
    public int Porta { get; set; }
    public string? ConnectionString { get; set; }
    public TimeSpan? Timeout { get; set; }
}

public class ValidadorEntrada
{
    public string ProcessarEntrada(string? entrada)
    {
        // Aplicar valor padrão apenas se necessário
        entrada ??= "valor padrão";
        
        // Tratar string vazia também
        if (string.IsNullOrEmpty(entrada))
        {
            entrada = "valor padrão para vazio";
        }

        return entrada;
    }
}

public class EventManager
{
    private Action<string>? _onEvent;

    public void ConfigurarEventos()
    {
        // Adicionar handler padrão apenas se nenhum existir
        _onEvent ??= msg => Console.WriteLine($"  → Handler padrão: {msg}");
    }

    public void AdicionarHandler(Action<string> handler)
    {
        _onEvent += handler;
    }

    public void TriggerEvent(string mensagem)
    {
        _onEvent?.Invoke(mensagem);
    }
}
