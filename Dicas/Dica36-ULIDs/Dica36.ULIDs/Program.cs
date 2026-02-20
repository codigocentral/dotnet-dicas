using System.Diagnostics;

namespace Dica36.ULIDs;

/// <summary>
/// Demonstra o uso de ULIDs como alternativa ordenável aos GUIDs
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dica 36: ULIDs (Sortable Unique Identifiers) ===\n");

        // 1. Geração básica de ULIDs
        await DemonstrarGeracaoBasica();

        // 2. Comparação com GUIDs
        await DemonstrarComparacaoComGuids();

        // 3. Ordenação temporal
        await DemonstrarOrdenacaoTemporal();

        // 4. Conversões e compatibilidade
        await DemonstrarConversoesCompatibilidade();

        // 5. Performance comparison
        await DemonstrarComparacaoPerformance();

        // 6. Uso em cenários práticos
        await DemonstrarCenariosPraticos();

        Console.WriteLine("\n=== Demonstração concluída ===");
    }

    static async Task DemonstrarGeracaoBasica()
    {
        Console.WriteLine("🔧 1. Geração Básica de ULIDs");
        Console.WriteLine("───────────────────────────────");

        // Gerar um novo ULID
        var ulid = Ulid.NewUlid();
        Console.WriteLine($"ULID gerado: {ulid}");
        Console.WriteLine($"Comprimento: {ulid.ToString().Length} caracteres");

        // ULID tem componente temporal
        var timestamp = ulid.Time;
        Console.WriteLine($"Timestamp do ULID: {timestamp:yyyy-MM-dd HH:mm:ss.fff}");

        // Comparar com GUID
        var guid = Guid.NewGuid();
        Console.WriteLine($"GUID para comparação: {guid}");
        Console.WriteLine($"Comprimento do GUID: {guid.ToString().Length} caracteres");

        Console.WriteLine();
    }

    static async Task DemonstrarComparacaoComGuids()
    {
        Console.WriteLine("📊 2. Comparação: ULID vs GUID");
        Console.WriteLine("────────────────────────────────");

        var ulids = new List<Ulid>();
        var guids = new List<Guid>();

        Console.WriteLine("Gerando 10 ULIDs e GUIDs com intervalos pequenos...\n");

        for (int i = 0; i < 10; i++)
        {
            ulids.Add(Ulid.NewUlid());
            guids.Add(Guid.NewGuid());
            await Task.Delay(1); // Pequeno delay
        }

        Console.WriteLine("ULIDs (observe a ordenação crescente):");
        for (int i = 0; i < ulids.Count; i++)
        {
            Console.WriteLine($"  {i + 1:D2}: {ulids[i]}");
        }

        Console.WriteLine("\nGUIDs (ordem aleatória):");
        for (int i = 0; i < guids.Count; i++)
        {
            Console.WriteLine($"  {i + 1:D2}: {guids[i]}");
        }

        // Demonstrar que ULIDs mantêm ordem temporal
        var sortedUlids = ulids.OrderBy(u => u).ToList();
        var isAlreadySorted = ulids.SequenceEqual(sortedUlids);
        
        Console.WriteLine($"\nULIDs já estão em ordem cronológica: {(isAlreadySorted ? "✅ SIM" : "❌ NÃO")}");

        Console.WriteLine();
    }

    static async Task DemonstrarOrdenacaoTemporal()
    {
        Console.WriteLine("⏱️ 3. Ordenação Temporal");
        Console.WriteLine("─────────────────────────");

        var eventos = new List<EventoComUlid>();

        Console.WriteLine("Criando eventos com ULIDs...");

        // Simular criação de eventos em momentos diferentes
        var tarefas = new[]
        {
            "Login do usuário",
            "Visualização de produto", 
            "Adicionar ao carrinho",
            "Iniciar checkout",
            "Pagamento aprovado"
        };

        foreach (var tarefa in tarefas)
        {
            var evento = new EventoComUlid(tarefa);
            eventos.Add(evento);
            Console.WriteLine($"  {evento.Id}: {evento.Descricao} ({evento.Timestamp:HH:mm:ss.fff})");
            await Task.Delay(Random.Shared.Next(1, 10)); // Delay variável
        }

        Console.WriteLine("\nEventos ordenados por ULID (automaticamente cronológico):");
        var eventosOrdenados = eventos.OrderBy(e => e.Id).ToList();
        
        foreach (var evento in eventosOrdenados)
        {
            Console.WriteLine($"  {evento.Id}: {evento.Descricao}");
        }

        Console.WriteLine();
    }

    static async Task DemonstrarConversoesCompatibilidade()
    {
        Console.WriteLine("🔄 4. Conversões e Compatibilidade");
        Console.WriteLine("───────────────────────────────────");

        var ulid = Ulid.NewUlid();
        Console.WriteLine($"ULID original: {ulid}");

        // Converter para diferentes formatos
        var ulidString = ulid.ToString();
        var guid = ulid.ToGuid();
        var bytes = ulid.ToByteArray();

        Console.WriteLine($"Como string: {ulidString}");
        Console.WriteLine($"Como GUID: {guid}");
        Console.WriteLine($"Como bytes: [{string.Join(", ", bytes.Take(8))}...]");

        // Converter de volta
        var ulidFromString = Ulid.Parse(ulidString);
        var ulidFromGuid = new Ulid(guid);

        Console.WriteLine($"\nReconstruído de string: {ulidFromString}");
        Console.WriteLine($"Reconstruído de GUID: {ulidFromGuid}");
        Console.WriteLine($"São iguais: {(ulid == ulidFromString && ulid == ulidFromGuid ? "✅ SIM" : "❌ NÃO")}");

        // Demonstrar compatibilidade com sistemas legados
        Console.WriteLine("\n📋 Exemplo de uso em Entity Framework:");
        Console.WriteLine($"public class Usuario {{ public Guid Id {{ get; set; }} = {guid}; }}");

        Console.WriteLine();
    }

    static async Task DemonstrarComparacaoPerformance()
    {
        Console.WriteLine("🚀 5. Comparação de Performance");
        Console.WriteLine("─────────────────────────────────");

        const int iteracoes = 100_000;
        var stopwatch = new Stopwatch();

        // Performance de geração - GUID
        stopwatch.Start();
        for (int i = 0; i < iteracoes; i++)
        {
            var guid = Guid.NewGuid();
        }
        stopwatch.Stop();
        var tempoGuid = stopwatch.ElapsedMilliseconds;

        // Performance de geração - ULID
        stopwatch.Restart();
        for (int i = 0; i < iteracoes; i++)
        {
            var ulid = Ulid.NewUlid();
        }
        stopwatch.Stop();
        var tempoUlid = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Geração de {iteracoes:N0} identificadores:");
        Console.WriteLine($"  GUID: {tempoGuid} ms");
        Console.WriteLine($"  ULID: {tempoUlid} ms");
        Console.WriteLine($"  ULID é {(tempoGuid > tempoUlid ? "mais rápido" : "mais lento")} que GUID");

        // Performance de ordenação
        var guidsParaOrdenar = Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid()).ToList();
        var ulidsParaOrdenar = Enumerable.Range(0, 10000).Select(_ => Ulid.NewUlid()).ToList();

        stopwatch.Restart();
        var guidsOrdenados = guidsParaOrdenar.OrderBy(g => g).ToList();
        stopwatch.Stop();
        var tempoOrdenacaoGuid = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var ulidsOrdenados = ulidsParaOrdenar.OrderBy(u => u).ToList();
        stopwatch.Stop();
        var tempoOrdenacaoUlid = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"\nOrdenação de 10.000 identificadores:");
        Console.WriteLine($"  GUID: {tempoOrdenacaoGuid} ms");
        Console.WriteLine($"  ULID: {tempoOrdenacaoUlid} ms");

        Console.WriteLine();
    }

    static async Task DemonstrarCenariosPraticos()
    {
        Console.WriteLine("💼 6. Cenários Práticos");
        Console.WriteLine("───────────────────────");

        Console.WriteLine("🗄️ Simulação: Chaves primárias ordenáveis para banco de dados");
        var usuarios = new List<Usuario>();

        for (int i = 1; i <= 5; i++)
        {
            var usuario = new Usuario($"usuario{i}@exemplo.com");
            usuarios.Add(usuario);
            Console.WriteLine($"  Usuário criado: {usuario.Id} - {usuario.Email}");
            await Task.Delay(1);
        }

        Console.WriteLine("\n📊 Benefícios para índices de banco de dados:");
        Console.WriteLine("  ✅ Inserções sempre no final do índice (sem fragmentação)");
        Console.WriteLine("  ✅ Melhor performance em consultas com ORDER BY");
        Console.WriteLine("  ✅ Cache de página mais eficiente");

        Console.WriteLine("\n🌐 Exemplo: API de logs distribuída");
        var logs = new List<LogEntry>();

        var servidores = new[] { "web-01", "web-02", "api-01", "db-01" };
        var niveis = new[] { "INFO", "WARN", "ERROR" };

        foreach (var servidor in servidores)
        {
            var log = new LogEntry(
                servidor, 
                niveis[Random.Shared.Next(niveis.Length)], 
                $"Operação executada no {servidor}"
            );
            logs.Add(log);
            await Task.Delay(Random.Shared.Next(1, 5));
        }

        Console.WriteLine("\nLogs gerados (automaticamente em ordem cronológica):");
        foreach (var log in logs.OrderBy(l => l.Id))
        {
            Console.WriteLine($"  {log.Id}: [{log.Level}] {log.Servidor} - {log.Mensagem}");
        }

        Console.WriteLine("\n🔍 Vantagem: Busca por intervalo temporal usando apenas o ID:");
        var primeiroId = logs.First().Id;
        var ultimoId = logs.Last().Id;
        Console.WriteLine($"  SELECT * FROM logs WHERE id BETWEEN '{primeiroId}' AND '{ultimoId}'");

        Console.WriteLine();
    }
}

/// <summary>
/// Representa um evento do sistema com ULID
/// </summary>
public record EventoComUlid(string Descricao)
{
    public Ulid Id { get; } = Ulid.NewUlid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo de usuário usando ULID como chave primária
/// </summary>
public class Usuario
{
    public Ulid Id { get; } = Ulid.NewUlid();
    public string Email { get; }
    public DateTime CriadoEm { get; } = DateTime.UtcNow;

    public Usuario(string email)
    {
        Email = email;
    }
}

/// <summary>
/// Entry de log com ULID para ordenação temporal automática
/// </summary>
public record LogEntry(string Servidor, string Level, string Mensagem)
{
    public Ulid Id { get; } = Ulid.NewUlid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
