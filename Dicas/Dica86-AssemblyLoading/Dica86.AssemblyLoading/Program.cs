using System.Reflection;
using System.Runtime.Loader;

Console.WriteLine("🔌 Dica 86: Assembly Loading Avançado (.NET 9)");
Console.WriteLine("===============================================");

// 1. Carregamento Básico de Assembly
Console.WriteLine("\n1. 📦 Carregamento Básico de Assembly:");
Console.WriteLine("─────────────────────────────────────────");

var currentAssembly = Assembly.GetExecutingAssembly();
Console.WriteLine($"✅ Assembly atual: {currentAssembly.GetName().Name}");
Console.WriteLine($"   📍 Localização: {currentAssembly.Location}");
Console.WriteLine($"   🔢 Versão: {currentAssembly.GetName().Version}");
Console.WriteLine($"   🏗️  Runtime: {currentAssembly.ImageRuntimeVersion}");

// 2. Informações detalhadas do Assembly
Console.WriteLine("\n2. 📋 Informações Detalhadas:");
Console.WriteLine("─────────────────────────────");

ExibirDetalhesAssembly(currentAssembly);

// 3. Custom AssemblyLoadContext (.NET 9)
Console.WriteLine("\n3. 🔧 Custom AssemblyLoadContext:");
Console.WriteLine("──────────────────────────────────");

await DemonstrarCustomLoadContext();

// 4. Reflexão em Types Carregados
Console.WriteLine("\n4. 🔍 Reflexão em Types:");
Console.WriteLine("───────────────────────");

ExplorarTypes();

// 5. Plugin Loading Simulation
Console.WriteLine("\n5. 🔌 Simulação de Plugin Loading:");
Console.WriteLine("──────────────────────────────────");

await SimularPluginLoading();

// 6. Assembly Metadata e Attributes
Console.WriteLine("\n6. 🏷️ Assembly Metadata:");
Console.WriteLine("────────────────────────");

AnalisarMetadata(currentAssembly);

// 7. Performance e Memory Loading
Console.WriteLine("\n7. ⚡ Performance de Loading:");
Console.WriteLine("────────────────────────────");

MedirPerformanceLoading();

Console.WriteLine("\n✅ Demonstração completa de Assembly Loading!");

static void ExibirDetalhesAssembly(Assembly assembly)
{
    var name = assembly.GetName();
    
    Console.WriteLine($"📝 Nome completo: {name.FullName}");
    Console.WriteLine($"🌐 Culture: {name.CultureName ?? "neutral"}");
    Console.WriteLine($"🔐 Public Key Token: {Convert.ToHexString(name.GetPublicKeyToken() ?? [])}");
    Console.WriteLine($"🏗️  Architecture: {name.ProcessorArchitecture}");
    Console.WriteLine($"📍 CodeBase: {assembly.Location}");
    Console.WriteLine($"🎯 Entry Point: {assembly.EntryPoint?.Name ?? "N/A"}");
}

static async Task DemonstrarCustomLoadContext()
{
    var customContext = new IsolatedLoadContext("PluginContext");
    Console.WriteLine($"✅ Contexto criado: {customContext.Name}");
    Console.WriteLine($"   🔄 Isolado: {customContext.IsCollectible}");
    
    // Simular carregamento assíncrono
    await Task.Run(() =>
    {
        try
        {
            // Em um cenário real, carregaria assembly de arquivo
            var assemblies = customContext.Assemblies.ToList();
            Console.WriteLine($"   📦 Assemblies carregados: {assemblies.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Erro: {ex.Message}");
        }
    });
    
    // Cleanup
    customContext.Unload();
    Console.WriteLine("   🧹 Contexto liberado");
}

static void ExplorarTypes()
{
    var coreAssembly = typeof(string).Assembly;
    Console.WriteLine($"📚 Assembly: {coreAssembly.GetName().Name}");
    
    // Pegar alguns tipos interessantes do .NET 9
    var interessantTypes = coreAssembly.GetExportedTypes()
        .Where(t => t.Name.Contains("Span") || t.Name.Contains("Memory"))
        .Take(5)
        .ToList();
    
    foreach (var type in interessantTypes)
    {
        Console.WriteLine($"   🎯 {type.Name} - Namespace: {type.Namespace}");
        
        // Mostrar alguns métodos públicos
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Take(2)
            .Select(m => m.Name)
            .ToList();
        
        if (methods.Count > 0)
        {
            Console.WriteLine($"      📝 Métodos: {string.Join(", ", methods)}");
        }
    }
}

static async Task SimularPluginLoading()
{
    var plugins = new[]
    {
        new { Name = "LoggingPlugin", Version = "1.0.0", Priority = "High" },
        new { Name = "CachePlugin", Version = "2.1.0", Priority = "Medium" },
        new { Name = "SecurityPlugin", Version = "1.5.0", Priority = "Critical" }
    };
    
    var tasks = plugins.Select(async plugin =>
    {
        await Task.Delay(Random.Shared.Next(50, 200)); // Simular tempo de carregamento
        
        Console.WriteLine($"🔌 Plugin carregado:");
        Console.WriteLine($"   📦 Nome: {plugin.Name}");
        Console.WriteLine($"   🔢 Versão: {plugin.Version}");
        Console.WriteLine($"   ⚡ Prioridade: {plugin.Priority}");
        
        return plugin;
    });
    
    var loadedPlugins = await Task.WhenAll(tasks);
    Console.WriteLine($"✅ Total de plugins carregados: {loadedPlugins.Length}");
}

static void AnalisarMetadata(Assembly assembly)
{
    var attributes = assembly.GetCustomAttributes().ToList();
    Console.WriteLine($"🏷️  Total de atributos: {attributes.Count}");
    
    // Mostrar atributos mais interessantes
    foreach (var attr in attributes.Take(5))
    {
        var typeName = attr.GetType().Name.Replace("Attribute", "");
        Console.WriteLine($"   📋 {typeName}");
        
        // Tentar extrair informações específicas
        switch (attr)
        {
            case AssemblyMetadataAttribute metadata:
                Console.WriteLine($"      🔑 {metadata.Key}: {metadata.Value}");
                break;
            case AssemblyVersionAttribute version:
                Console.WriteLine($"      🔢 Version: {version.Version}");
                break;
        }
    }
    
    // Informações dos módulos
    var modules = assembly.GetModules();
    Console.WriteLine($"📦 Módulos: {modules.Length}");
    
    foreach (var module in modules)
    {
        Console.WriteLine($"   📄 {module.Name} ({module.ModuleVersionId})");
    }
}

static void MedirPerformanceLoading()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // Simular carregamento de múltiplos assemblies
    var assemblyNames = new[]
    {
        "System.Collections",
        "System.Linq",
        "System.Text.Json",
        "System.Threading.Tasks"
    };
    
    var loadedCount = 0;
    
    foreach (var name in assemblyNames)
    {
        try
        {
            var assembly = Assembly.Load(name);
            loadedCount++;
            Console.WriteLine($"   ✅ {name} - {assembly.GetTypes().Length} types");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ {name} - Erro: {ex.GetType().Name}");
        }
    }
    
    stopwatch.Stop();
    
    Console.WriteLine($"⏱️  Tempo total: {stopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine($"📊 Assemblies carregados: {loadedCount}/{assemblyNames.Length}");
    Console.WriteLine($"🚀 Média por assembly: {stopwatch.ElapsedMilliseconds / (double)assemblyNames.Length:F2}ms");
}

// Custom AssemblyLoadContext para .NET 9
public class IsolatedLoadContext : AssemblyLoadContext
{
    public IsolatedLoadContext(string name) : base(name, isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Implementação customizada de carregamento
        // Em um cenário real, você implementaria lógica específica aqui
        return null; // Permite fallback para contexto padrão
    }
    
    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        // Carregamento de DLLs não gerenciadas
        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}

// Interfaces e classes para demonstração
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync();
    Task ExecuteAsync();
}

public class SamplePlugin : IPlugin
{
    public string Name => "Sample Plugin";
    public string Version => "1.0.0";
    
    public async Task InitializeAsync()
    {
        await Task.Delay(10);
        Console.WriteLine($"🔧 {Name} inicializado");
    }
    
    public async Task ExecuteAsync()
    {
        await Task.Delay(5);
        Console.WriteLine($"🚀 {Name} executado com sucesso!");
    }
}

// Atributo customizado para demonstração
[AttributeUsage(AttributeTargets.Assembly)]
public class PluginMetadataAttribute : Attribute
{
    public string Author { get; }
    public string Description { get; }
    
    public PluginMetadataAttribute(string author, string description)
    {
        Author = author;
        Description = description;
    }
}
