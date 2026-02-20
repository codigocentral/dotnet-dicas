using System.Globalization;
using System.Text.Json;

Console.WriteLine("=== Dica 23: DateTimeOffset vs DateTime ===\n");

// 1. Problema básico com DateTime - ambiguidade de timezone
Console.WriteLine("1. Problema de Ambiguidade com DateTime:");
var agora = DateTime.Now;
var utcAgora = DateTime.UtcNow;

Console.WriteLine($"DateTime.Now: {agora}");
Console.WriteLine($"DateTime.UtcNow: {utcAgora}");
Console.WriteLine($"Kind do Now: {agora.Kind}");
Console.WriteLine($"Kind do UtcNow: {utcAgora.Kind}");

// Problema: perda de informação de timezone ao serializar
var evento = new EventoComDateTime
{
    Id = 1,
    Nome = "Reunião importante",
    DataHora = DateTime.Now
};

var json = JsonSerializer.Serialize(evento);
Console.WriteLine($"JSON com DateTime: {json}");

var eventoDeserializado = JsonSerializer.Deserialize<EventoComDateTime>(json);
Console.WriteLine($"DateTime deserializado Kind: {eventoDeserializado!.DataHora.Kind}");

// 2. Solução com DateTimeOffset - sempre preserva timezone
Console.WriteLine("\n2. Solução com DateTimeOffset:");
var eventoCorreto = new EventoComDateTimeOffset
{
    Id = 1,
    Nome = "Reunião importante",
    DataHora = DateTimeOffset.Now
};

var jsonCorreto = JsonSerializer.Serialize(eventoCorreto);
Console.WriteLine($"JSON com DateTimeOffset: {jsonCorreto}");

var eventoCorretoDeserializado = JsonSerializer.Deserialize<EventoComDateTimeOffset>(jsonCorreto);
Console.WriteLine($"DateTimeOffset deserializado: {eventoCorretoDeserializado!.DataHora}");
Console.WriteLine($"Offset preservado: {eventoCorretoDeserializado.DataHora.Offset}");

// 3. Comparações entre timezones
Console.WriteLine("\n3. Comparações entre Timezones:");
var agendador = new AgendadorEventos();

// Criar eventos em diferentes timezones
var eventoSaoPaulo = agendador.CriarEvento("SP", "Evento São Paulo", 
    new DateTimeOffset(2024, 12, 15, 14, 0, 0, TimeSpan.FromHours(-3)));

var eventoTokyo = agendador.CriarEvento("TK", "Evento Tokyo", 
    new DateTimeOffset(2024, 12, 15, 14, 0, 0, TimeSpan.FromHours(9)));

var eventoLondres = agendador.CriarEvento("LD", "Evento Londres", 
    new DateTimeOffset(2024, 12, 15, 14, 0, 0, TimeSpan.FromHours(0)));

Console.WriteLine("Eventos criados:");
foreach (var ev in agendador.ObterEventos())
{
    Console.WriteLine($"  {ev.Nome}: {ev.DataHora:yyyy-MM-dd HH:mm:ss zzz}");
    Console.WriteLine($"    UTC: {ev.DataHora.UtcDateTime:yyyy-MM-dd HH:mm:ss}");
}

// Comparar quem acontece primeiro (comparação correta)
var eventosOrdenados = agendador.ObterEventosOrdenados();
Console.WriteLine("\nEventos ordenados por tempo (UTC):");
foreach (var ev in eventosOrdenados)
{
    Console.WriteLine($"  {ev.Nome}: {ev.DataHora:yyyy-MM-dd HH:mm:ss zzz}");
}

// 4. Demonstração de problemas com DateTime
Console.WriteLine("\n4. Problemas com DateTime em Comparações:");
var problemaComparacao = new ProblemaComparacao();
problemaComparacao.DemonstrarProblema();

// 5. API para diferentes timezones
Console.WriteLine("\n5. API para Conversão de Timezones:");
var conversorTimezone = new ConversorTimezone();

var eventoUTC = DateTimeOffset.UtcNow;
Console.WriteLine($"Evento UTC: {eventoUTC:yyyy-MM-dd HH:mm:ss zzz}");

var timezones = new[]
{
    "America/Sao_Paulo",
    "Asia/Tokyo", 
    "Europe/London",
    "America/New_York",
    "Australia/Sydney"
};

foreach (var tz in timezones)
{
    var convertido = conversorTimezone.ConverterPara(eventoUTC, tz);
    Console.WriteLine($"  {tz}: {convertido:yyyy-MM-dd HH:mm:ss zzz}");
}

// 6. Parsing de strings com timezone
Console.WriteLine("\n6. Parsing de Strings com Timezone:");
var parser = new DateTimeParser();

var exemplos = new[]
{
    "2024-12-15T14:30:00Z",                    // UTC
    "2024-12-15T14:30:00-03:00",              // São Paulo
    "2024-12-15T14:30:00+09:00",              // Tokyo
    "2024-12-15T14:30:00"                     // Sem timezone (ambíguo)
};

foreach (var exemplo in exemplos)
{
    var resultado = parser.ParseComSeguranca(exemplo);
    Console.WriteLine($"  '{exemplo}' → {resultado}");
}

// 7. Operações matemáticas
Console.WriteLine("\n7. Operações Matemáticas:");
var calculadora = new CalculadoraTempo();

var inicio = DateTimeOffset.Now;
Console.WriteLine($"Início: {inicio:yyyy-MM-dd HH:mm:ss zzz}");

var emUmaHora = calculadora.AdicionarTempo(inicio, TimeSpan.FromHours(1));
Console.WriteLine($"Em 1 hora: {emUmaHora:yyyy-MM-dd HH:mm:ss zzz}");

var diferenca = calculadora.CalcularDiferenca(emUmaHora, inicio);
Console.WriteLine($"Diferença: {diferenca.TotalHours} horas");

// 8. Boas práticas para APIs
Console.WriteLine("\n8. Boas Práticas para APIs:");
var apiService = new ApiService();
apiService.DemonstrarBoasPraticas();

Console.WriteLine("\n=== Fim da Demonstração ===");

// Classes de apoio
public class EventoComDateTime
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public DateTime DataHora { get; set; }
}

public class EventoComDateTimeOffset
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public DateTimeOffset DataHora { get; set; }
}

public class AgendadorEventos
{
    private readonly List<EventoComDateTimeOffset> _eventos = [];

    public EventoComDateTimeOffset CriarEvento(string id, string nome, DateTimeOffset dataHora)
    {
        var evento = new EventoComDateTimeOffset
        {
            Id = _eventos.Count + 1,
            Nome = $"[{id}] {nome}",
            DataHora = dataHora
        };

        _eventos.Add(evento);
        return evento;
    }

    public List<EventoComDateTimeOffset> ObterEventos() => _eventos;

    public List<EventoComDateTimeOffset> ObterEventosOrdenados()
    {
        return _eventos.OrderBy(e => e.DataHora.UtcDateTime).ToList();
    }
}

public class ProblemaComparacao
{
    public void DemonstrarProblema()
    {
        // Problema: DateTime sem timezone explícito
        var dataLocal = new DateTime(2024, 12, 15, 14, 0, 0, DateTimeKind.Local);
        var dataUtc = new DateTime(2024, 12, 15, 14, 0, 0, DateTimeKind.Utc);
        var dataUnspecified = new DateTime(2024, 12, 15, 14, 0, 0, DateTimeKind.Unspecified);

        Console.WriteLine($"  Data Local: {dataLocal} (Kind: {dataLocal.Kind})");
        Console.WriteLine($"  Data UTC: {dataUtc} (Kind: {dataUtc.Kind})");
        Console.WriteLine($"  Data Unspecified: {dataUnspecified} (Kind: {dataUnspecified.Kind})");

        // Comparação pode dar resultados inesperados
        Console.WriteLine($"  Local == UTC? {dataLocal == dataUtc}");
        Console.WriteLine($"  Local > UTC? {dataLocal > dataUtc}");

        // Solução com DateTimeOffset
        var offsetLocal = new DateTimeOffset(2024, 12, 15, 14, 0, 0, TimeSpan.FromHours(-3));
        var offsetUtc = new DateTimeOffset(2024, 12, 15, 14, 0, 0, TimeSpan.Zero);

        Console.WriteLine($"  Offset Local: {offsetLocal}");
        Console.WriteLine($"  Offset UTC: {offsetUtc}");
        Console.WriteLine($"  Offset Local == Offset UTC? {offsetLocal == offsetUtc}");
        Console.WriteLine($"  Offset Local > Offset UTC? {offsetLocal > offsetUtc}");
    }
}

public class ConversorTimezone
{
    public DateTimeOffset ConverterPara(DateTimeOffset dataHora, string timezoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTime(dataHora, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback para timezones que podem ter nomes diferentes em diferentes sistemas
            var alternativos = new Dictionary<string, string>
            {
                ["America/Sao_Paulo"] = "E. South America Standard Time",
                ["Asia/Tokyo"] = "Tokyo Standard Time",
                ["Europe/London"] = "GMT Standard Time",
                ["America/New_York"] = "Eastern Standard Time",
                ["Australia/Sydney"] = "AUS Eastern Standard Time"
            };

            if (alternativos.TryGetValue(timezoneId, out var alternativo))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(alternativo);
                return TimeZoneInfo.ConvertTime(dataHora, timeZone);
            }

            return dataHora; // Retorna original se não conseguir converter
        }
    }
}

public class DateTimeParser
{
    public string ParseComSeguranca(string input)
    {
        // Tentar parse como DateTimeOffset primeiro
        if (DateTimeOffset.TryParse(input, out var dateTimeOffset))
        {
            return $"DateTimeOffset: {dateTimeOffset:yyyy-MM-dd HH:mm:ss zzz}";
        }

        // Se falhar, tentar como DateTime
        if (DateTime.TryParse(input, out var dateTime))
        {
            return $"DateTime (AMBÍGUO): {dateTime:yyyy-MM-dd HH:mm:ss} (Kind: {dateTime.Kind})";
        }

        return "Erro no parsing";
    }
}

public class CalculadoraTempo
{
    public DateTimeOffset AdicionarTempo(DateTimeOffset inicio, TimeSpan duracao)
    {
        return inicio.Add(duracao);
    }

    public TimeSpan CalcularDiferenca(DateTimeOffset fim, DateTimeOffset inicio)
    {
        return fim - inicio;
    }

    public bool EstaDentroDoIntervalo(DateTimeOffset data, DateTimeOffset inicio, DateTimeOffset fim)
    {
        return data >= inicio && data <= fim;
    }
}

public class ApiService
{
    public void DemonstrarBoasPraticas()
    {
        Console.WriteLine("  ✅ FAÇA: Use DateTimeOffset para timestamps");
        Console.WriteLine("  ✅ FAÇA: Armazene sempre em UTC no banco");
        Console.WriteLine("  ✅ FAÇA: Converta para timezone do usuário na apresentação");
        Console.WriteLine("  ✅ FAÇA: Use ISO 8601 em APIs (2024-12-15T14:30:00Z)");
        Console.WriteLine("  ❌ NÃO: Use DateTime sem especificar Kind");
        Console.WriteLine("  ❌ NÃO: Assuma timezone local em operações");
        Console.WriteLine("  ❌ NÃO: Faça matemática de datas sem considerar timezone");

        // Exemplo de resposta de API correta
        var respostaApi = new
        {
            id = 1,
            nome = "Reunião de equipe",
            inicio = DateTimeOffset.UtcNow,
            fim = DateTimeOffset.UtcNow.AddHours(1),
            timezone = "UTC",
            created_at = DateTimeOffset.UtcNow
        };

        Console.WriteLine($"  Exemplo JSON API: {JsonSerializer.Serialize(respostaApi, new JsonSerializerOptions { WriteIndented = false })}");
    }
}
