using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dica53.HotReloadEditAndContinue;

/// <summary>
/// Dica 53: Hot Reload & Edit and Continue
/// 
/// Demonstra as funcionalidades de Hot Reload e Edit and Continue do .NET,
/// que permitem modificar código durante a execução sem reiniciar a aplicação.
/// 
/// Hot Reload: Permite aplicar mudanças no código sem parar a aplicação
/// Edit and Continue: Permite editar código durante debug e continuar execução
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Dica 53: Hot Reload & Edit and Continue ===\n");

        if (args.Length == 0)
        {
            await RunDemo();
            return 0;
        }

        switch (args[0].ToLower())
        {
            case "demo":
                await RunDemo();
                break;
            case "server":
                await RunWebServer();
                break;
            case "calculator":
                await RunCalculator();
                break;
            default:
                Console.WriteLine("Uso: dotnet run [demo|server|calculator]");
                break;
        }

        return 0;
    }

    /// <summary>
    /// Demonstração geral das funcionalidades
    /// </summary>
    static async Task RunDemo()
    {
        Console.WriteLine("🔥 Hot Reload Demo");
        Console.WriteLine("================");
        Console.WriteLine("1. Execute este programa com 'dotnet run demo'");
        Console.WriteLine("2. Em outro terminal, execute 'dotnet watch run demo' para Hot Reload");
        Console.WriteLine("3. Modifique este código enquanto está executando");
        Console.WriteLine("4. Veja as mudanças aplicadas automaticamente!\n");

        // Configurações para Hot Reload
        ShowHotReloadConfiguration();

        // Simulação de aplicação rodando
        await SimulateRunningApplication();
    }

    /// <summary>
    /// Mostra as configurações necessárias para Hot Reload
    /// </summary>
    static void ShowHotReloadConfiguration()
    {
        Console.WriteLine("📋 Configurações para Hot Reload:");
        Console.WriteLine("================================");
        Console.WriteLine("1. No .csproj:");
        Console.WriteLine("   <EnableHotReload>true</EnableHotReload>");
        Console.WriteLine("   <UseAppHost>true</UseAppHost>");
        Console.WriteLine();
        Console.WriteLine("2. Comandos úteis:");
        Console.WriteLine("   dotnet watch run         # Habilita Hot Reload");
        Console.WriteLine("   dotnet watch run --hot-reload");
        Console.WriteLine("   dotnet watch test        # Hot Reload em testes");
        Console.WriteLine();
        Console.WriteLine("3. Teclas durante execução:");
        Console.WriteLine("   Ctrl+R = Force reload");
        Console.WriteLine("   Ctrl+C = Parar");
        Console.WriteLine();
    }

    /// <summary>
    /// Simula uma aplicação rodando que pode ser modificada via Hot Reload
    /// </summary>
    static async Task SimulateRunningApplication()
    {
        var counter = 0;
        var message = "Aplicação rodando..."; // Modifique esta mensagem durante execução!
        var interval = TimeSpan.FromSeconds(2);

        Console.WriteLine("🚀 Aplicação iniciada (modifique o código para ver Hot Reload!)");
        Console.WriteLine("Pressione Ctrl+C para parar\n");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => 
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                counter++;
                
                // Esta área pode ser modificada com Hot Reload!
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message} (contador: {counter})");
                
                // Experimente modificar estas funcionalidades:
                if (counter % 5 == 0)
                {
                    Console.WriteLine("🎉 Múltiplo de 5 detectado!");
                }

                await Task.Delay(interval, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n✅ Aplicação parada graciosamente");
        }
    }

    /// <summary>
    /// Servidor web simples para demonstrar Hot Reload
    /// </summary>
    static async Task RunWebServer()
    {
        Console.WriteLine("🌐 Web Server Demo com Hot Reload");
        Console.WriteLine("=================================");
        
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddLogging(logging => logging.AddConsole());
        builder.Services.AddSingleton<WebServerService>();

        var host = builder.Build();
        
        Console.WriteLine("Servidor iniciado! Modifique o código para ver Hot Reload.");
        Console.WriteLine("Acesse endpoints simulados:\n");

        await host.Services.GetRequiredService<WebServerService>().RunAsync();
    }

    /// <summary>
    /// Calculadora para demonstrar Edit and Continue durante debug
    /// </summary>
    static async Task RunCalculator()
    {
        Console.WriteLine("🧮 Calculator Demo - Edit and Continue");
        Console.WriteLine("=====================================");
        Console.WriteLine("Para testar Edit and Continue:");
        Console.WriteLine("1. Execute no modo debug (F5 no VS/VS Code)");
        Console.WriteLine("2. Coloque breakpoint na linha de cálculo");
        Console.WriteLine("3. Quando parar, modifique o código");
        Console.WriteLine("4. Continue execução (F5)");
        Console.WriteLine();

        ShowEditAndContinueInfo();

        var calculator = new Calculator();
        
        while (true)
        {
            try
            {
                Console.Write("\nDigite dois números (ou 'sair'): ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input) || input.ToLower() == "sair")
                    break;

                var parts = input.Split(' ');
                if (parts.Length >= 2 && 
                    double.TryParse(parts[0], out var a) && 
                    double.TryParse(parts[1], out var b))
                {
                    // Breakpoint aqui para testar Edit and Continue
                    var result = calculator.Calculate(a, b);
                    Console.WriteLine($"Resultado: {result}");
                }
                else
                {
                    Console.WriteLine("❌ Formato inválido. Use: número1 número2");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
            }
        }

        Console.WriteLine("✅ Calculadora encerrada");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Informações sobre Edit and Continue
    /// </summary>
    static void ShowEditAndContinueInfo()
    {
        Console.WriteLine("📋 Edit and Continue - Configurações:");
        Console.WriteLine("====================================");
        Console.WriteLine("1. Configurações do projeto:");
        Console.WriteLine("   <DebugType>portable</DebugType>");
        Console.WriteLine("   <DebugSymbols>true</DebugSymbols>");
        Console.WriteLine("   <Optimize>false</Optimize>");
        Console.WriteLine();
        Console.WriteLine("2. Suportado em:");
        Console.WriteLine("   ✅ Visual Studio");
        Console.WriteLine("   ✅ Visual Studio Code (limitado)");
        Console.WriteLine("   ✅ Visual Studio Mac");
        Console.WriteLine();
        Console.WriteLine("3. Limitações:");
        Console.WriteLine("   ❌ Mudanças em assinaturas de métodos");
        Console.WriteLine("   ❌ Adicionar/remover tipos");
        Console.WriteLine("   ❌ Mudanças em lambda expressions");
        Console.WriteLine("   ✅ Modificar corpo de métodos");
        Console.WriteLine("   ✅ Adicionar/modificar variáveis locais");
        Console.WriteLine("   ✅ Alterar valores de constantes");
        Console.WriteLine();
    }
}

/// <summary>
/// Serviço simulando um servidor web
/// </summary>
public class WebServerService
{
    private readonly ILogger<WebServerService> _logger;

    public WebServerService(ILogger<WebServerService> logger)
    {
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var endpoints = new Dictionary<string, Func<string>>
        {
            ["/api/time"] = () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["/api/random"] = () => Random.Shared.Next(1, 100).ToString(),
            ["/api/status"] = () => "Servidor funcionando! (Modifique-me com Hot Reload!)",
            ["/api/version"] = () => "v1.0.0 - Hot Reload Edition"
        };

        var counter = 0;
        
        while (true)
        {
            counter++;
            
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] 🌐 Servidor ativo (requisições: {counter})");
            
            // Simula endpoints - modifique estas respostas com Hot Reload!
            foreach (var endpoint in endpoints)
            {
                try
                {
                    var response = endpoint.Value();
                    _logger.LogInformation("GET {Endpoint} -> {Response}", 
                        endpoint.Key, response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no endpoint {Endpoint}", endpoint.Key);
                }
            }

            // Experimente modificar este intervalo com Hot Reload
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

/// <summary>
/// Calculadora para demonstrar Edit and Continue
/// </summary>
public class Calculator
{
    /// <summary>
    /// Método que pode ser modificado durante debug com Edit and Continue
    /// </summary>
    public double Calculate(double a, double b)
    {
        // Coloque breakpoint aqui
        // Durante debug, modifique esta operação:
        var result = a + b;  // Experimente trocar para *, -, / etc.
        
        // Adicione logging durante debug:
        Console.WriteLine($"Calculando: {a} + {b} = {result}");
        
        return result;
    }

    /// <summary>
    /// Método adicional para experimentar Edit and Continue
    /// </summary>
    public string FormatResult(double result)
    {
        // Modifique este formato durante debug
        return $"O resultado é: {result:F2}";
    }
}

/// <summary>
/// Classe para demonstrar limitações do Edit and Continue
/// </summary>
public class HotReloadLimitations
{
    public void DemonstrateLimitations()
    {
        Console.WriteLine("⚠️ Limitações do Hot Reload:");
        Console.WriteLine("===========================");
        
        // ✅ PERMITIDO: Modificar corpo de métodos
        Console.WriteLine("✅ Modificar corpo de métodos");
        
        // ✅ PERMITIDO: Adicionar novos métodos
        // NewMethod(); // Descomente durante Hot Reload
        
        // ❌ NÃO PERMITIDO: Alterar assinaturas
        // public void ExistingMethod(int param) // Não pode mudar para (string param)
        
        // ❌ NÃO PERMITIDO: Adicionar novos tipos
        // public class NewClass { } // Requer restart
        
        // ✅ PERMITIDO: Modificar constantes
        const string VERSION = "v1.0"; // Pode ser alterado
        Console.WriteLine($"Versão: {VERSION}");
    }
    
    // Método que pode ser adicionado via Hot Reload
    private void NewMethod()
    {
        Console.WriteLine("✅ Novo método adicionado via Hot Reload!");
    }
}

/// <summary>
/// Utilitários para Hot Reload
/// </summary>
public static class HotReloadUtils
{
    /// <summary>
    /// Verifica se Hot Reload está disponível
    /// </summary>
    public static bool IsHotReloadAvailable()
    {
        // Verifica se está em modo de desenvolvimento
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Mostra informações sobre o ambiente de desenvolvimento
    /// </summary>
    public static void ShowDevelopmentInfo()
    {
        Console.WriteLine("🔧 Informações do Ambiente:");
        Console.WriteLine("===========================");
        Console.WriteLine($"Framework: {Environment.Version}");
        Console.WriteLine($"OS: {Environment.OSVersion}");
        Console.WriteLine($"64-bit: {Environment.Is64BitProcess}");
        Console.WriteLine($"Debug: {System.Diagnostics.Debugger.IsAttached}");
        Console.WriteLine($"Hot Reload: {IsHotReloadAvailable()}");
        Console.WriteLine();
    }

    /// <summary>
    /// Dicas de performance para Hot Reload
    /// </summary>
    public static void ShowPerformanceTips()
    {
        Console.WriteLine("⚡ Dicas de Performance:");
        Console.WriteLine("========================");
        Console.WriteLine("1. Hot Reload é mais rápido que restart completo");
        Console.WriteLine("2. Use 'dotnet watch' apenas durante desenvolvimento");
        Console.WriteLine("3. Desabilite em produção para melhor performance");
        Console.WriteLine("4. Edit and Continue pode consumir mais memória");
        Console.WriteLine("5. Reinicie periodicamente em sessões longas de debug");
        Console.WriteLine();
    }
}
