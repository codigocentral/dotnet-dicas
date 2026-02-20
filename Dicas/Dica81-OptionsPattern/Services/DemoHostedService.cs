using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dica81_OptionsPattern.Services;

namespace Dica81_OptionsPattern.Services;

/// <summary>
/// Serviço principal que demonstra todos os padrões de Options
/// </summary>
public class DemoHostedService : BackgroundService
{
    private readonly ILogger<DemoHostedService> _logger;
    private readonly DatabaseService _databaseService;
    private readonly EmailService _emailService;
    private readonly ApiService _apiService;
    private readonly CacheService _cacheService;
    private readonly ValidationService _validationService;
    private readonly IHostApplicationLifetime _lifetime;

    public DemoHostedService(
        ILogger<DemoHostedService> logger,
        DatabaseService databaseService,
        EmailService emailService,
        ApiService apiService,
        CacheService cacheService,
        ValidationService validationService,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _databaseService = databaseService;
        _emailService = emailService;
        _apiService = apiService;
        _cacheService = cacheService;
        _validationService = validationService;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("🚀 Iniciando demonstração do Options Pattern");
            _logger.LogInformation("=" + new string('=', 60));

            await DemonstrateAllOptionsPatterns();

            _logger.LogInformation("=" + new string('=', 60));
            _logger.LogInformation("✅ Demonstração concluída com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante a demonstração");
        }
        finally
        {
            // Para a aplicação
            _lifetime.StopApplication();
        }
    }

    private async Task DemonstrateAllOptionsPatterns()
    {
        await Task.Delay(500); // Simula inicialização

        // 1. Demonstrar IOptions (Singleton)
        _logger.LogInformation("📖 1. Demonstrando IOptions<T> (Singleton)");
        _logger.LogInformation(new string('-', 50));
        
        var dbInfo = await _databaseService.GetConnectionInfoAsync();
        _logger.LogInformation("{DatabaseInfo}", dbInfo);

        await Task.Delay(1000);

        // 2. Demonstrar IOptionsSnapshot (Scoped)
        _logger.LogInformation("\n📧 2. Demonstrando IOptionsSnapshot<T> (Scoped)");
        _logger.LogInformation(new string('-', 50));
        
        var emailResult = await _emailService.SendEmailAsync(
            "usuario@exemplo.com",
            "Teste do Options Pattern",
            "Este é um email de teste para demonstrar IOptionsSnapshot.");
        _logger.LogInformation("{EmailResult}", emailResult);

        await Task.Delay(1000);

        // 3. Demonstrar IOptionsMonitor (Singleton com reload)
        _logger.LogInformation("\n🌐 3. Demonstrando IOptionsMonitor<T> (Singleton com reload)");
        _logger.LogInformation(new string('-', 50));
        
        var apiResult = await _apiService.CallApiAsync("users/123");
        _logger.LogInformation("{ApiResult}", apiResult);

        await Task.Delay(1000);

        // 4. Demonstrar configurações complexas aninhadas
        _logger.LogInformation("\n💾 4. Demonstrando Configurações Complexas Aninhadas");
        _logger.LogInformation(new string('-', 50));
        
        var cacheInfo = await _cacheService.GetCacheInfoAsync();
        _logger.LogInformation("{CacheInfo}", cacheInfo);

        await Task.Delay(1000);

        // 5. Demonstrar validação de configurações
        _logger.LogInformation("\n🔍 5. Demonstrando Validação de Configurações");
        _logger.LogInformation(new string('-', 50));
        
        // Nota: Para demonstration completa da validação, seria necessário acessar IOptions diretamente
        // Este é um exemplo simplificado
        _logger.LogInformation("ℹ️  Validação seria executada durante a inicialização da aplicação");
        _logger.LogInformation("ℹ️  Configure services.AddOptions<T>().ValidateDataAnnotations() para validação automática");

        await Task.Delay(1000);

        // 6. Demonstrar diferenças entre os tipos
        _logger.LogInformation("\n📋 6. Resumo das Diferenças");
        _logger.LogInformation(new string('-', 50));
        
        var summary = """
                     
                     🔍 Diferenças entre IOptions, IOptionsSnapshot e IOptionsMonitor:
                     
                     📌 IOptions<T>:
                        ✅ Singleton - Uma instância para toda a aplicação
                        ❌ NÃO reflete mudanças na configuração após inicialização
                        🎯 Use para: Configurações que não mudam durante execução
                        💡 Exemplo: Configurações de banco de dados
                     
                     📌 IOptionsSnapshot<T>:
                        ✅ Scoped - Nova instância por request/scope
                        ✅ Reflete mudanças na configuração
                        🎯 Use para: Configurações que podem mudar por request
                        💡 Exemplo: Configurações por tenant em apps multi-tenant
                     
                     📌 IOptionsMonitor<T>:
                        ✅ Singleton - Uma instância para toda a aplicação
                        ✅ Reflete mudanças na configuração em tempo real
                        ✅ Permite registrar callbacks para mudanças
                        🎯 Use para: Configurações que mudam em runtime
                        💡 Exemplo: Configurações de features flags, APIs externas
                     
                     📋 Validação:
                        ✅ Use DataAnnotations para validação básica
                        ✅ Use IValidateOptions<T> para validação complexa
                        ✅ Configure .ValidateDataAnnotations() na startup
                        💡 Falha na validação = ApplicationException na startup
                     """;
        
        _logger.LogInformation("{Summary}", summary);

        await Task.Delay(2000);

        // 7. Demonstrar boas práticas
        _logger.LogInformation("\n🎯 7. Boas Práticas do Options Pattern");
        _logger.LogInformation(new string('-', 50));
        
        var bestPractices = """
                           
                           🌟 Boas Práticas para Options Pattern:
                           
                           1. 📝 Naming Convention:
                              - Use sufixo "Settings" nas classes (ex: DatabaseSettings)
                              - Defina const SectionName para cada classe
                           
                           2. 🛡️ Validação:
                              - Sempre valide configurações críticas
                              - Use [Required] para propriedades obrigatórias
                              - Implemente validação customizada quando necessário
                           
                           3. 🔒 Segurança:
                              - NUNCA loggue senhas ou chaves de API
                              - Use User Secrets para desenvolvimento
                              - Use Azure Key Vault ou similar para produção
                           
                           4. 🏗️ Estrutura:
                              - Organize configurações em seções lógicas
                              - Use classes separadas para configurações complexas
                              - Mantenha configurações relacionadas juntas
                           
                           5. ⚡ Performance:
                              - Use IOptions para configurações estáticas
                              - Use IOptionsSnapshot apenas quando necessário
                              - Use IOptionsMonitor para configurações dinâmicas
                           
                           6. 🧪 Testabilidade:
                              - Crie configurações de teste
                              - Use Options.Create<T>() nos testes
                              - Teste cenários de configuração inválida
                           """;
        
        _logger.LogInformation("{BestPractices}", bestPractices);
    }
}
