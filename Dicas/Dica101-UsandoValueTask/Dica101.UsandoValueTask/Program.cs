using BenchmarkDotNet.Running;

namespace Dica101;

class Program
{
    static void Main(string[] args)
    {
        // Configurar dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<RecordDemonstration>();
                services.AddSingleton<IPersonService, PersonService>();
                services.AddSingleton<IGeometryService, GeometryService>();
                services.AddSingleton<IFinancialService, FinancialService>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("🚀 Dica 101: Record Types - Tipos Imutáveis e Funcionais");
        logger.LogInformation("============================================================");

        try
        {
            // Verificar se deve executar benchmark
            if (args.Length > 0 && args[0].ToLower() == "benchmark")
            {
                logger.LogInformation("Executando benchmarks...");
                BenchmarkRunner.Run<RecordBenchmarks>();
                return;
            }

            // Executar demonstrações
            var demonstration = host.Services.GetRequiredService<RecordDemonstration>();

            demonstration.DemonstrateValueEquality();
            demonstration.DemonstrateWithExpressions();
            demonstration.DemonstrateDeconstruction();
            demonstration.DemonstrateRecordInheritance();
            demonstration.DemonstrateRecordStructPerformance();
            demonstration.DemonstrateJsonSerialization();
            demonstration.DemonstratePracticalUseCases();

            logger.LogInformation("\n=== Resumo dos Pontos Importantes ===");
            logger.LogInformation("✅ Record classes:");
            logger.LogInformation("   • Comparação por valor automática");
            logger.LogInformation("   • Expressões 'with' para imutabilidade");
            logger.LogInformation("   • ToString() automático e legível");
            logger.LogInformation("   • Desestruturação nativa");
            logger.LogInformation("   • Suporte a herança");

            logger.LogInformation("\n✅ Record structs:");
            logger.LogInformation("   • Melhor performance para dados pequenos");
            logger.LogInformation("   • Semanticamente valor, não referência");
            logger.LogInformation("   • Readonly record struct para máxima imutabilidade");

            logger.LogInformation("\n✅ Casos de uso ideais:");
            logger.LogInformation("   • DTOs e modelos de transferência");
            logger.LogInformation("   • Value Objects em Domain-Driven Design");
            logger.LogInformation("   • Configurações imutáveis");
            logger.LogInformation("   • Estados de aplicação");
            logger.LogInformation("   • Pattern matching avançado");

            logger.LogInformation("\n💡 Para executar benchmarks: dotnet run benchmark");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante a execução da demonstração");
        }

        logger.LogInformation("\n✅ Demonstração concluída!");
    }
}
