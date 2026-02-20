using System.Text.Json;

Console.WriteLine("=== Dica 8: Tipos Vazios (Empty Types) no C# 12 ===");

// 1. Demonstrando a nova sintaxe de tipos vazios do C# 12
Console.WriteLine("1. Comparação entre sintaxe antiga e nova:");

// Sintaxe tradicional (ainda válida)
var marcadorTradicional = new MarcadorTradicional();
Console.WriteLine($"  Marcador tradicional: {marcadorTradicional.GetType().Name}");

// Nova sintaxe do C# 12 - sem chaves {}
var marcadorModerno = new MarcadorModerno();
Console.WriteLine($"  Marcador moderno: {marcadorModerno.GetType().Name}");

Console.WriteLine();

// 2. Demonstrando diferentes tipos de tipos vazios
Console.WriteLine("2. Diferentes tipos vazios modernos:");

var servicoVazio = new ServicoVazio();
var eventoVazio = new EventoVazio();
var comandoVazio = new ComandoVazio();

Console.WriteLine($"  Serviço: {servicoVazio.GetType().Name}");
Console.WriteLine($"  Evento: {eventoVazio.GetType().Name}");
Console.WriteLine($"  Comando: {comandoVazio.GetType().Name}");

Console.WriteLine();

// 3. Casos de uso práticos - Marcadores de Assembly
Console.WriteLine("3. Casos de uso práticos:");

// Assembly markers para Injeção de Dependência
Console.WriteLine("  ✅ Marcadores de Assembly para DI");
Console.WriteLine("     - Facilita registro de serviços");
Console.WriteLine("     - Remove ambiguidade do código");

// Event sourcing / CQRS
Console.WriteLine("  ✅ Eventos e Comandos vazios");
Console.WriteLine("     - Trigger events sem payload");
Console.WriteLine("     - Comandos de ação simples");

// State machines
Console.WriteLine("  ✅ Estados de máquina");
Console.WriteLine("     - Estados sem dados");
Console.WriteLine("     - Transições limpas");

Console.WriteLine();

// 4. Exemplo prático com padrão Command
Console.WriteLine("4. Exemplo prático - Padrão Command:");

var processador = new ProcessadorComandos();

// Comandos vazios são válidos e úteis
await processador.ExecutarAsync(new InicializarSistema());
await processador.ExecutarAsync(new LimparCache());
await processador.ExecutarAsync(new ExecutarBackup());

Console.WriteLine();

// 5. Interfaces vazias para marcação
Console.WriteLine("5. Interfaces de marcação:");

var entidades = new List<object>
{
    new Produto { Nome = "Notebook" },
    new Usuario { Nome = "João" },
    new Configuracao { Chave = "timeout", Valor = "30" }
};

foreach (var entidade in entidades)
{
    var tipo = entidade.GetType().Name;
    var implementacoes = string.Join(", ", entidade.GetType().GetInterfaces().Select(i => i.Name));
    Console.WriteLine($"  {tipo}: {implementacoes}");
}

Console.WriteLine();

// 6. Serialização de tipos vazios
Console.WriteLine("6. Serialização JSON de tipos vazios:");

var eventos = new List<object>
{
    new SistemaIniciado(),
    new CacheAtualizado(),
    new BackupCompleto()
};

foreach (var evento in eventos)
{
    var json = JsonSerializer.Serialize(evento);
    Console.WriteLine($"  {evento.GetType().Name}: {json}");
}

Console.WriteLine();

// 7. Generic constraints com tipos vazios
Console.WriteLine("7. Restrições genéricas:");

var repositorio = new RepositorioGenerico<Produto>();
var servico = new ServicoGenerico<Usuario>();

Console.WriteLine($"  Repositório para: {typeof(Produto).Name}");
Console.WriteLine($"  Serviço para: {typeof(Usuario).Name}");

Console.WriteLine();

// 8. Comparação de características
Console.WriteLine("8. Características dos tipos:");

// Verificando se os tipos são equivalentes
var tipoTradicional = typeof(MarcadorTradicional);
var tipoModerno = typeof(MarcadorModerno);

Console.WriteLine($"  Marcador tradicional - É classe: {tipoTradicional.IsClass}");
Console.WriteLine($"  Marcador moderno - É classe: {tipoModerno.IsClass}");
Console.WriteLine($"  Ambos compilam para estruturas idênticas: {tipoTradicional.IsClass == tipoModerno.IsClass}");

// Demonstrando que ambos têm construtores padrão
var instanciaTradicional = Activator.CreateInstance<MarcadorTradicional>();
var instanciaModerna = Activator.CreateInstance<MarcadorModerno>();

Console.WriteLine($"  Instância tradicional criada: {instanciaTradicional != null}");
Console.WriteLine($"  Instância moderna criada: {instanciaModerna != null}");

Console.WriteLine();

Console.WriteLine("=== Resumo das Vantagens dos Tipos Vazios Modernos ===");
Console.WriteLine("✅ Sintaxe mais limpa e concisa");
Console.WriteLine("✅ Menos ruído visual no código");
Console.WriteLine("✅ Mantém todas as funcionalidades");
Console.WriteLine("✅ Perfeito para marcadores e padrões");
Console.WriteLine("✅ Melhor legibilidade em código extenso");
Console.WriteLine("✅ Facilita manutenção e refatoração");

Console.WriteLine("=== Fim da Demonstração ===");

// =================== DEFINIÇÕES DE TIPOS ===================

// Sintaxe tradicional (ainda válida)
public class MarcadorTradicional
{
}

public interface IEventoTradicional
{
}

// ✨ Nova sintaxe do C# 12 - Tipos vazios sem chaves
public class MarcadorModerno;

public struct ServicoVazio;

public interface IEventoModerno;

public record EventoVazio;

public record struct ComandoVazio;

// Casos de uso práticos

// Marcadores de Assembly para DI
public interface IMarcadorDominio;
public interface IMarcadorInfraestrutura;
public interface IMarcadorAplicacao;

// Comandos vazios para CQRS
public record InicializarSistema : IComando;
public record LimparCache : IComando;
public record ExecutarBackup : IComando;

// Eventos vazios para Event Sourcing
public record SistemaIniciado : IEvento;
public record CacheAtualizado : IEvento;
public record BackupCompleto : IEvento;

// Interfaces de marcação
public interface IEntidade;
public interface IAggregateRoot;
public interface IConfiguracaoSistema;

// Estados vazios para State Machine
public record EstadoInicial;
public record EstadoProcessando;
public record EstadoCompleto;
public record EstadoErro;

// Implementações de exemplo
public class Produto : IEntidade, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
}

public class Usuario : IEntidade
{
    public string Nome { get; set; } = string.Empty;
}

public class Configuracao : IConfiguracaoSistema
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}

// Interfaces base
public interface IComando;
public interface IEvento;

// Processador de comandos
public class ProcessadorComandos
{
    public async Task ExecutarAsync<T>(T comando) where T : IComando
    {
        var nomeComando = typeof(T).Name;
        Console.WriteLine($"    ⚡ Executando comando: {nomeComando}");
        
        // Simula processamento
        await Task.Delay(50);
        
        Console.WriteLine($"    ✅ Comando {nomeComando} concluído");
    }
}

// Repositório genérico
public class RepositorioGenerico<T> where T : IEntidade
{
    public void Salvar(T entidade)
    {
        Console.WriteLine($"    💾 Salvando {typeof(T).Name}");
    }
}

// Serviço genérico
public class ServicoGenerico<T> where T : IEntidade
{
    public void Processar(T entidade)
    {
        Console.WriteLine($"    🔄 Processando {typeof(T).Name}");
    }
}
