using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

Console.WriteLine("==== Dica 17: Verificando Pacotes NuGet Desatualizados ====");
Console.WriteLine("Esta dica demonstra como verificar e gerenciar pacotes NuGet desatualizados");
Console.WriteLine("usando a ferramenta CLI dotnet-outdated.\n");

// 1. Demonstração básica - Verificação de pacotes
Console.WriteLine("1. Como verificar pacotes desatualizados:");
Console.WriteLine("   dotnet tool install -g dotnet-outdated (instalar globalmente)");
Console.WriteLine("   dotnet outdated (verificar pacotes desatualizados)");
Console.WriteLine("   dotnet outdated --upgrade (atualizar automaticamente)");
Console.WriteLine();

// 2. Demonstração de uso dos pacotes "desatualizados"
Console.WriteLine("2. Demonstrando pacotes instalados (alguns intencionalmente desatualizados):");

try
{
    // Microsoft.Extensions.Hosting v7.0.0 (versão desatualizada)
    var hostBuilder = Host.CreateDefaultBuilder();
    Console.WriteLine("   ✓ Microsoft.Extensions.Hosting v7.0.0 (desatualizada - atual é 9.x)");

    // Microsoft.Extensions.Logging v7.0.0 (versão desatualizada)
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogInformation("Testando Microsoft.Extensions.Logging v7.0.0");
    Console.WriteLine("   ✓ Microsoft.Extensions.Logging v7.0.0 (desatualizada - atual é 9.x)");

    // Newtonsoft.Json v13.0.1 (versão desatualizada)
    var objeto = new { Nome = "Teste", Versao = "13.0.1", Status = "Desatualizada" };
    var jsonNewtonsoft = Newtonsoft.Json.JsonConvert.SerializeObject(objeto);
    Console.WriteLine($"   ✓ Newtonsoft.Json v13.0.1: {jsonNewtonsoft}");

    // System.Text.Json v8.0.0 (versão desatualizada)
    var jsonSystemText = System.Text.Json.JsonSerializer.Serialize(new { 
        Biblioteca = "System.Text.Json", 
        Versao = "8.0.0",
        Status = "Desatualizada"
    });
    Console.WriteLine($"   ✓ System.Text.Json v8.0.0: {jsonSystemText}");

    // Microsoft.Extensions.Configuration v7.0.0 (versão desatualizada)
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["TestKey"] = "TestValue",
        ["Version"] = "7.0.0"
    });
    var config = configBuilder.Build();
    Console.WriteLine($"   ✓ Microsoft.Extensions.Configuration v7.0.0: {config["TestKey"]}");
}
catch (Exception ex)
{
    Console.WriteLine($"   ❌ Erro ao demonstrar pacotes: {ex.Message}");
}

Console.WriteLine();

// 3. Comandos úteis do dotnet-outdated
Console.WriteLine("3. Comandos úteis do dotnet-outdated:");
Console.WriteLine("   dotnet outdated --help                    (ajuda completa)");
Console.WriteLine("   dotnet outdated --include-prerelease     (incluir versões preview)");
Console.WriteLine("   dotnet outdated --version-lock Major     (travamento de versão major)");
Console.WriteLine("   dotnet outdated --version-lock Minor     (travamento de versão minor)");
Console.WriteLine("   dotnet outdated --version-lock Patch     (travamento de versão patch)");
Console.WriteLine("   dotnet outdated --fail-on-updates        (falha se houver atualizações)");
Console.WriteLine("   dotnet outdated --output json            (saída em formato JSON)");
Console.WriteLine("   dotnet outdated --upgrade                (atualizar automaticamente)");
Console.WriteLine();

// 4. Exemplo de verificação programática
Console.WriteLine("4. Verificação programática de versões:");
await DemonstrarVerificacaoProgramatica();

Console.WriteLine();

// 5. Melhores práticas
Console.WriteLine("5. Melhores práticas para gerenciamento de pacotes:");
ExibirMelhoresPraticas();

Console.WriteLine("\n=== Resumo dos Benefícios ===");
Console.WriteLine("✅ Verificação rápida de pacotes desatualizados");
Console.WriteLine("✅ Atualização automatizada com --upgrade");
Console.WriteLine("✅ Controle de versão com version-lock");
Console.WriteLine("✅ Integração em pipelines CI/CD");
Console.WriteLine("✅ Relatórios em múltiplos formatos");
Console.WriteLine("✅ Prevenção de vulnerabilidades de segurança");

static async Task DemonstrarVerificacaoProgramatica()
{
    try
    {
        // Simular verificação de versão via processo
        Console.WriteLine("   Simulando verificação com dotnet outdated...");
        
        // Em um cenário real, você executaria:
        // var process = Process.Start(new ProcessStartInfo
        // {
        //     FileName = "dotnet",
        //     Arguments = "outdated --output json",
        //     RedirectStandardOutput = true,
        //     UseShellExecute = false
        // });
        
        // Exemplo de saída simulada
        var exemploSaida = """
        {
          "Projects": [
            {
              "Name": "Dica17.VerificandoPacotesNuGetDesatualizados",
              "TargetFrameworks": [
                {
                  "Name": "net9.0",
                  "Dependencies": [
                    {
                      "Name": "Microsoft.Extensions.Hosting",
                      "ResolvedVersion": "7.0.0",
                      "LatestVersion": "9.0.0",
                      "UpdateSeverity": "Major"
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;
        
        Console.WriteLine("   Exemplo de saída JSON:");
        Console.WriteLine($"   {exemploSaida.Replace("\n", "\n   ")}");
        
        await Task.Delay(100); // Simular processamento assíncrono
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ❌ Erro na verificação: {ex.Message}");
    }
}

static void ExibirMelhoresPraticas()
{
    var praticas = new[]
    {
        "Verificar atualizações regularmente (semanalmente ou mensalmente)",
        "Usar travamento de versão em produção (--version-lock Minor)",
        "Testar atualizações em ambiente de desenvolvimento primeiro",
        "Integrar dotnet outdated em pipelines CI/CD",
        "Manter log de atualizações para auditoria",
        "Verificar breaking changes antes de atualizar",
        "Usar --include-prerelease apenas em desenvolvimento",
        "Automatizar atualizações de segurança críticas"
    };

    foreach (var (pratica, index) in praticas.Select((p, i) => (p, i + 1)))
    {
        Console.WriteLine($"   {index}. {pratica}");
    }
}
