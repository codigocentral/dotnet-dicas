namespace Dica07.LoggingCorreto;

/// <summary>
/// Dica 7: Logging Correto no .NET
/// 
/// O logger embutido no .NET se refere à parte de texto do logging como "mensagem", 
/// mas na verdade é um "modelo de mensagem" (message template).
/// 
/// Usar interpolação de string, formatação de string ou concatenação de string com 
/// seu logger está incorreto. Isso faz com que você perca todos os parâmetros do 
/// método de logging (impossibilitando a filtragem) e desperdiça memória com strings 
/// que precisam ser coletadas pelo coletor de lixo.
/// 
/// Simplesmente nomeie seus parâmetros com um nome descritivo como parte do seu modelo 
/// de mensagem e forneça seus argumentos como um segundo parâmetro. Isso previne 
/// problemas de memória e facilita a filtragem.
/// </summary>
public class LoggingDemo
{
    private readonly ILogger<LoggingDemo> _logger;

    public LoggingDemo(ILogger<LoggingDemo> logger)
    {
        _logger = logger;
    }

    public void DemonstrarLoggingIncorreto()
    {
        Console.WriteLine("❌ Exemplos de Logging INCORRETO:");
        Console.WriteLine();

        var userId = 12345;
        var userName = "João Silva";
        var operation = "Login";
        var duration = TimeSpan.FromMilliseconds(150);

        // ❌ INCORRETO: Interpolação de string
        _logger.LogInformation($"Usuário {userName} (ID: {userId}) executou operação {operation} em {duration.TotalMilliseconds}ms");

        // ❌ INCORRETO: Concatenação de string
        _logger.LogInformation("Usuário " + userName + " (ID: " + userId + ") executou operação " + operation + " em " + duration.TotalMilliseconds + "ms");

        // ❌ INCORRETO: String.Format
        _logger.LogInformation(string.Format("Usuário {0} (ID: {1}) executou operação {2} em {3}ms", userName, userId, operation, duration.TotalMilliseconds));

        Console.WriteLine("⚠️ Problemas dos métodos acima:");
        Console.WriteLine("   • Perda de parâmetros estruturados");
        Console.WriteLine("   • Impossível filtrar por valores específicos");
        Console.WriteLine("   • Alocação desnecessária de strings");
        Console.WriteLine("   • Coleta de lixo adicional");
        Console.WriteLine("   • Performance reduzida");
        Console.WriteLine();
    }

    public void DemonstrarLoggingCorreto()
    {
        Console.WriteLine("✅ Exemplos de Logging CORRETO:");
        Console.WriteLine();

        var userId = 12345;
        var userName = "João Silva";
        var operation = "Login";
        var duration = TimeSpan.FromMilliseconds(150);

        // ✅ CORRETO: Message template com parâmetros nomeados
        _logger.LogInformation("Usuário {UserName} (ID: {UserId}) executou operação {Operation} em {DurationMs}ms",
            userName, userId, operation, duration.TotalMilliseconds);

        // ✅ CORRETO: Diferentes níveis de log com templates
        _logger.LogDebug("Iniciando validação para usuário {UserId}", userId);
        _logger.LogWarning("Tentativa de acesso negada para usuário {UserName} na operação {Operation}", userName, operation);
        _logger.LogError("Erro ao processar operação {Operation} para usuário {UserId}: {ErrorMessage}", operation, userId, "Database timeout");

        Console.WriteLine("✅ Vantagens dos métodos acima:");
        Console.WriteLine("   • Parâmetros estruturados preservados");
        Console.WriteLine("   • Filtragem eficiente por valores");
        Console.WriteLine("   • Zero alocações desnecessárias");
        Console.WriteLine("   • Performance otimizada");
        Console.WriteLine("   • Integração com sistemas de observabilidade");
        Console.WriteLine();
    }

    public void DemonstrarFiltragePorParametros()
    {
        Console.WriteLine("🔍 Demonstração de filtragem por parâmetros:");
        Console.WriteLine();

        // Simulando diferentes operações para mostrar filtragem
        var operations = new[] { "Login", "Logout", "UpdateProfile", "DeleteAccount", "ViewData" };
        var users = new[] { "João", "Maria", "Pedro", "Ana", "Carlos" };
        
        for (int i = 0; i < 10; i++)
        {
            var user = users[Random.Shared.Next(users.Length)];
            var operation = operations[Random.Shared.Next(operations.Length)];
            var userId = Random.Shared.Next(1000, 9999);
            var duration = Random.Shared.Next(50, 500);

            _logger.LogInformation("Operação {Operation} executada por {UserName} (ID: {UserId}) em {Duration}ms",
                operation, user, userId, duration);
        }

        Console.WriteLine("💡 Com structured logging, você pode filtrar logs como:");
        Console.WriteLine("   • Todas as operações de 'Login'");
        Console.WriteLine("   • Operações do usuário 'João'");
        Console.WriteLine("   • Operações que duraram mais de 200ms");
        Console.WriteLine("   • Combinar múltiplos filtros");
        Console.WriteLine();
    }

    public void DemonstrarLogScopes()
    {
        Console.WriteLine("📋 Demonstração de Log Scopes:");
        Console.WriteLine();

        var userId = 12345;
        var requestId = Guid.NewGuid();

        // Criando um scope para agrupar logs relacionados
        using (_logger.BeginScope("ProcessingRequest for UserId: {UserId}, RequestId: {RequestId}", userId, requestId))
        {
            _logger.LogInformation("Iniciando processamento da requisição");
            
            // Simular etapas do processamento
            _logger.LogDebug("Validando dados de entrada");
            Thread.Sleep(50);
            
            _logger.LogInformation("Consultando base de dados");
            Thread.Sleep(100);
            
            _logger.LogDebug("Aplicando regras de negócio");
            Thread.Sleep(75);
            
            _logger.LogInformation("Processamento concluído com sucesso");
        }

        Console.WriteLine("💡 Scopes ajudam a:");
        Console.WriteLine("   • Agrupar logs relacionados");
        Console.WriteLine("   • Adicionar contexto automaticamente");
        Console.WriteLine("   • Rastrear fluxo de execução");
        Console.WriteLine("   • Facilitar debugging");
        Console.WriteLine();
    }

    public void DemonstrarLogEventIds()
    {
        Console.WriteLine("🏷️ Demonstração de Event IDs:");
        Console.WriteLine();

        // Definindo EventIds para diferentes tipos de eventos
        var loginSuccessEvent = new EventId(1001, "LoginSuccess");
        var loginFailedEvent = new EventId(1002, "LoginFailed");
        var dataAccessEvent = new EventId(2001, "DataAccess");
        var performanceEvent = new EventId(3001, "Performance");

        var userName = "João Silva";
        var userId = 12345;

        _logger.LogInformation(loginSuccessEvent, "Login bem-sucedido para usuário {UserName} (ID: {UserId})", userName, userId);
        _logger.LogWarning(loginFailedEvent, "Falha no login para usuário {UserName} - tentativa {Attempt}", userName, 3);
        _logger.LogInformation(dataAccessEvent, "Acesso aos dados do usuário {UserId} na tabela {TableName}", userId, "Users");
        _logger.LogWarning(performanceEvent, "Operação lenta detectada - {Operation} levou {Duration}ms", "DatabaseQuery", 2500);

        Console.WriteLine("💡 Event IDs permitem:");
        Console.WriteLine("   • Categorização de logs");
        Console.WriteLine("   • Filtragem por tipo de evento");
        Console.WriteLine("   • Alertas específicos");
        Console.WriteLine("   • Métricas por categoria");
        Console.WriteLine();
    }

    public void DemonstrarLoggerFactoryCustom()
    {
        Console.WriteLine("⚙️ Demonstração de Logger personalizado:");
        Console.WriteLine();

        // Criando um logger factory personalizado
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug)
                .AddFilter("Dica07.LoggingCorreto", LogLevel.Information));

        var customLogger = loggerFactory.CreateLogger<LoggingDemo>();

        customLogger.LogDebug("Este log de debug será filtrado");
        customLogger.LogInformation("Este log de informação será exibido para usuário {UserName}", "Maria");
        customLogger.LogWarning("Este log de warning será exibido com prioridade {Priority}", "Alta");

        Console.WriteLine("💡 Configuração de logger permite:");
        Console.WriteLine("   • Controlar níveis de log");
        Console.WriteLine("   • Filtrar por namespace/categoria");
        Console.WriteLine("   • Configurar múltiplos providers");
        Console.WriteLine("   • Otimizar performance");
        Console.WriteLine();
    }

    public void CompararPerformance()
    {
        Console.WriteLine("⚡ Comparação de Performance:");
        Console.WriteLine();

        var iterations = 100000;
        var userName = "TestUser";
        var userId = 12345;

        // Teste com string interpolation (INCORRETO)
        var sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var message = $"Usuário {userName} (ID: {userId}) executou operação Login";
            // Simula o processamento do log
            _ = message.Length;
        }
        sw1.Stop();

        // Teste com message template (CORRETO)
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            // Em um logger real, isso seria muito mais eficiente
            var template = "Usuário {UserName} (ID: {UserId}) executou operação {Operation}";
            var parameters = new object[] { userName, userId, "Login" };
            // Simula o processamento do template
            _ = template.Length + parameters.Length;
        }
        sw2.Stop();

        Console.WriteLine($"🔄 Interpolação de string: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"✅ Message template: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"📊 Diferença: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x mais rápido");
        Console.WriteLine();
        Console.WriteLine("💡 Na prática, a diferença é ainda maior porque:");
        Console.WriteLine("   • Logger real evita alocações desnecessárias");
        Console.WriteLine("   • Templates são reutilizados");
        Console.WriteLine("   • Menos pressão no Garbage Collector");
    }
}
