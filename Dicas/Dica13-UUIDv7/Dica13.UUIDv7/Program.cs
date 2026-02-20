using System.Diagnostics;

Console.WriteLine("🆔 Dica 13: UUID v7 (GUID v7) no .NET 9");
Console.WriteLine("==========================================");
Console.WriteLine();

// 1. Demonstrando a diferença entre GUID v4 tradicional e GUID v7
Console.WriteLine("1. 📊 Comparação entre GUID v4 (tradicional) e GUID v7 (ordenável):");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

// GUID v4 tradicionais - completamente aleatórios
var guidsV4 = new List<Guid>();
for (int i = 0; i < 5; i++)
{
    guidsV4.Add(Guid.NewGuid());
    Thread.Sleep(1); // Pequena pausa para demonstrar
}

// GUID v7 - ordenáveis por tempo
var guidsV7 = new List<Guid>();
for (int i = 0; i < 5; i++)
{
    guidsV7.Add(Guid.CreateVersion7());
    Thread.Sleep(1); // Pequena pausa para demonstrar
}

Console.WriteLine("GUID v4 (aleatórios - não ordenáveis):");
foreach (var guid in guidsV4)
{
    Console.WriteLine($"  {guid}");
}

Console.WriteLine();
Console.WriteLine("GUID v7 (ordenáveis por tempo de criação):");
foreach (var guid in guidsV7)
{
    Console.WriteLine($"  {guid}");
}

Console.WriteLine();

// 2. Demonstrando ordenação
Console.WriteLine("2. 🔄 Demonstração de Ordenação:");
Console.WriteLine("──────────────────────────────────");

var guidsMisturados = new List<Guid>();

// Criando GUIDs v7 com intervalos para mostrar ordenação
for (int i = 0; i < 3; i++)
{
    guidsMisturados.Add(Guid.CreateVersion7());
    Thread.Sleep(10);
}

// Embaralhando para mostrar que podem ser reordenados
guidsMisturados = guidsMisturados.OrderBy(x => Guid.NewGuid()).ToList();

Console.WriteLine("GUIDs v7 embaralhados:");
foreach (var guid in guidsMisturados)
{
    Console.WriteLine($"  {guid}");
}

Console.WriteLine();
Console.WriteLine("GUIDs v7 ordenados (ordem cronológica restaurada):");
foreach (var guid in guidsMisturados.OrderBy(g => g))
{
    Console.WriteLine($"  {guid}");
}

Console.WriteLine();

// 3. Cenário prático - Simulando inserções em banco de dados
Console.WriteLine("3. 🗄️ Simulação de Inserções em Banco de Dados:");
Console.WriteLine("──────────────────────────────────────────────");

Console.WriteLine("Criando registros com diferentes tipos de ID...");

var registrosV4 = new List<RegistroV4>();
var registrosV7 = new List<RegistroV7>();

var sw = Stopwatch.StartNew();

// Simulando inserções com GUID v4
for (int i = 0; i < 1000; i++)
{
    registrosV4.Add(new RegistroV4
    {
        Id = Guid.NewGuid(),
        Nome = $"Usuario_{i}",
        CriadoEm = DateTime.UtcNow
    });
}

sw.Stop();
var tempoV4 = sw.ElapsedMilliseconds;

sw.Restart();

// Simulando inserções com GUID v7
for (int i = 0; i < 1000; i++)
{
    registrosV7.Add(new RegistroV7
    {
        Id = Guid.CreateVersion7(),
        Nome = $"Usuario_{i}",
        CriadoEm = DateTime.UtcNow
    });
}

sw.Stop();
var tempoV7 = sw.ElapsedMilliseconds;

Console.WriteLine($"✅ Criados 1000 registros com GUID v4 em: {tempoV4}ms");
Console.WriteLine($"✅ Criados 1000 registros com GUID v7 em: {tempoV7}ms");

Console.WriteLine();

// 4. Análise de fragmentação
Console.WriteLine("4. 📈 Análise de Fragmentação (simulada):");
Console.WriteLine("─────────────────────────────────────────");

// Com GUID v4, IDs são aleatórios - causam fragmentação
var idsV4Ordenados = registrosV4.Select(r => r.Id).OrderBy(id => id).ToList();
var idsV7Ordenados = registrosV7.Select(r => r.Id).OrderBy(id => id).ToList();

// Calculando "fragmentação" - quão diferentes são as posições originais vs ordenadas
int fragmentacaoV4 = CalcularFragmentacao(registrosV4.Select(r => r.Id).ToList(), idsV4Ordenados);
int fragmentacaoV7 = CalcularFragmentacao(registrosV7.Select(r => r.Id).ToList(), idsV7Ordenados);

Console.WriteLine($"📊 Fragmentação GUID v4: {fragmentacaoV4} movimentações necessárias");
Console.WriteLine($"📊 Fragmentação GUID v7: {fragmentacaoV7} movimentações necessárias");
Console.WriteLine($"📈 Melhoria: {((double)(fragmentacaoV4 - fragmentacaoV7) / fragmentacaoV4 * 100):F1}% menos fragmentação");

Console.WriteLine();

// 5. Extração de timestamp de GUID v7
Console.WriteLine("5. ⏰ Extração de Timestamp de GUID v7:");
Console.WriteLine("────────────────────────────────────────");

var guidComTempo = Guid.CreateVersion7();
var timestampExtraido = ExtrairTimestampDeGuidV7(guidComTempo);

Console.WriteLine($"GUID v7: {guidComTempo}");
Console.WriteLine($"Timestamp extraído: {timestampExtraido:yyyy-MM-dd HH:mm:ss.fff} UTC");
Console.WriteLine($"Agora: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");

Console.WriteLine();

// 6. Caso de uso: Sistema de logs ordenados
Console.WriteLine("6. 📝 Caso de Uso: Sistema de Logs Ordenados:");
Console.WriteLine("──────────────────────────────────────────────");

var logs = new List<LogEntry>();

// Simulando logs chegando fora de ordem (cenário real em sistemas distribuídos)
var idsLogs = new List<Guid>();
for (int i = 0; i < 10; i++)
{
    idsLogs.Add(Guid.CreateVersion7());
    Thread.Sleep(1);
}

// Embaralhando para simular chegada fora de ordem
var idsEmbaralhados = idsLogs.OrderBy(x => Random.Shared.Next()).ToList();

foreach (var id in idsEmbaralhados)
{
    logs.Add(new LogEntry
    {
        Id = id,
        Mensagem = $"Log message {idsLogs.IndexOf(id) + 1}",
        Severidade = (Severidade)(Random.Shared.Next(0, 4))
    });
}

Console.WriteLine("Logs recebidos (fora de ordem):");
foreach (var log in logs)
{
    Console.WriteLine($"  {log.Id} - {log.Mensagem} [{log.Severidade}]");
}

Console.WriteLine();
Console.WriteLine("Logs ordenados cronologicamente (usando GUID v7):");
foreach (var log in logs.OrderBy(l => l.Id))
{
    Console.WriteLine($"  {log.Id} - {log.Mensagem} [{log.Severidade}]");
}

Console.WriteLine();

// 7. Comparação de performance para ordenação
Console.WriteLine("7. ⚡ Performance de Ordenação:");
Console.WriteLine("──────────────────────────────");

const int qtdTeste = 10000;

// Teste com GUID v4
var guidsTeste = new List<Guid>();
for (int i = 0; i < qtdTeste; i++)
{
    guidsTeste.Add(Guid.NewGuid());
}

sw.Restart();
var ordenadosV4 = guidsTeste.OrderBy(g => g).ToList();
sw.Stop();
var tempoOrdenacaoV4 = sw.ElapsedMilliseconds;

// Teste com GUID v7
guidsTeste.Clear();
for (int i = 0; i < qtdTeste; i++)
{
    guidsTeste.Add(Guid.CreateVersion7());
}

sw.Restart();
var ordenadosV7 = guidsTeste.OrderBy(g => g).ToList();
sw.Stop();
var tempoOrdenacaoV7 = sw.ElapsedMilliseconds;

Console.WriteLine($"⏱️ Ordenação de {qtdTeste} GUID v4: {tempoOrdenacaoV4}ms");
Console.WriteLine($"⏱️ Ordenação de {qtdTeste} GUID v7: {tempoOrdenacaoV7}ms");

Console.WriteLine();

// 8. Resumo dos benefícios
Console.WriteLine("🎯 Resumo dos Benefícios do GUID v7:");
Console.WriteLine("──────────────────────────────────");
Console.WriteLine("✅ Ordenável por tempo de criação");
Console.WriteLine("✅ Reduz fragmentação em índices de banco");
Console.WriteLine("✅ Melhor performance em consultas ordenadas");
Console.WriteLine("✅ Compatível com sistemas existentes");
Console.WriteLine("✅ Não requer bibliotecas externas (.NET 9+)");
Console.WriteLine("✅ Mantém unicidade global");
Console.WriteLine("✅ Suporte nativo para cenários distribuídos");

Console.WriteLine();
Console.WriteLine("💡 Use GUID v7 quando:");
Console.WriteLine("  • IDs precisam ser ordenáveis por tempo");
Console.WriteLine("  • Performance de banco de dados é crítica");
Console.WriteLine("  • Sistemas distribuídos precisam de ordenação");
Console.WriteLine("  • Redução de fragmentação é importante");

Console.WriteLine();
Console.WriteLine("=== Fim da Demonstração ===");

// =================== MÉTODOS AUXILIARES ===================

static int CalcularFragmentacao(List<Guid> original, List<Guid> ordenado)
{
    int movimentos = 0;
    for (int i = 0; i < original.Count; i++)
    {
        var posicaoOriginal = i;
        var posicaoOrdenada = ordenado.IndexOf(original[i]);
        if (posicaoOriginal != posicaoOrdenada)
        {
            movimentos++;
        }
    }
    return movimentos;
}

static DateTime ExtrairTimestampDeGuidV7(Guid guidV7)
{
    // GUID v7 possui timestamp nos primeiros 48 bits
    var bytes = guidV7.ToByteArray();
    
    // Extrair os primeiros 6 bytes (48 bits) que contêm o timestamp
    long timestamp = 0;
    for (int i = 0; i < 6; i++)
    {
        timestamp = (timestamp << 8) | bytes[i];
    }
    
    // O timestamp é em milliseconds desde Unix epoch
    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    return epoch.AddMilliseconds(timestamp);
}

// =================== CLASSES AUXILIARES ===================

public class RegistroV4
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class RegistroV7
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class LogEntry
{
    public Guid Id { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public Severidade Severidade { get; set; }
}

public enum Severidade
{
    Info,
    Warning,
    Error,
    Critical
}
