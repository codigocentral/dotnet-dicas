namespace Dica10.MarcadoresAssembly;

/// <summary>
/// Dica 10: Marcadores de Assembly para Injeção de Dependência
/// 
/// Ao registrar bibliotecas como MediatR ou AutoMapper na Injeção de Dependência 
/// que aceitam um tipo genérico como marcador de assembly, as pessoas geralmente 
/// usam o arquivo Program.cs.
/// 
/// Uma abordagem melhor é criar uma interface vazia nomeada após o assembly em 
/// que ela reside e usá-la como marcador. Isso remove a ambiguidade e torna o 
/// código mais legível.
/// </summary>
public class AssemblyMarkerDemo
{
    public static void DemonstrarRegistroIncorreto()
    {
        Console.WriteLine("❌ Abordagem INCORRETA - Usando Program.cs como marcador:");
        Console.WriteLine();

        Console.WriteLine("```csharp");
        Console.WriteLine("// ❌ PROBLEMÁTICO: Usar Program.cs como marcador");
        Console.WriteLine("builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());");
        Console.WriteLine("builder.Services.AddAutoMapper(typeof(Program));");
        Console.WriteLine("builder.Services.AddValidatorsFromAssemblyContaining<Program>();");
        Console.WriteLine("```");
        Console.WriteLine();

        Console.WriteLine("⚠️ Problemas desta abordagem:");
        Console.WriteLine("   • Ambiguidade - qual assembly está sendo referenciado?");
        Console.WriteLine("   • Program.cs pode não estar no assembly correto");
        Console.WriteLine("   • Dificulta refatoração e manutenção");
        Console.WriteLine("   • Cria dependência do assembly de entrada");
        Console.WriteLine("   • Pode quebrar se Program.cs for movido/renomeado");
        Console.WriteLine();
    }

    public static void DemonstrarRegistroCorreto()
    {
        Console.WriteLine("✅ Abordagem CORRETA - Usando interface marcadora:");
        Console.WriteLine();

        Console.WriteLine("```csharp");
        Console.WriteLine("// ✅ CORRETO: Usar interface marcadora específica do assembly");
        Console.WriteLine("builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());");
        Console.WriteLine("builder.Services.AddAutoMapper(typeof(IAssemblyMarker));");
        Console.WriteLine("builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();");
        Console.WriteLine("```");
        Console.WriteLine();

        Console.WriteLine("✅ Vantagens desta abordagem:");
        Console.WriteLine("   • Clara intenção - IAssemblyMarker indica o assembly específico");
        Console.WriteLine("   • Independente do ponto de entrada da aplicação");
        Console.WriteLine("   • Facilita refatoração sem quebrar registros");
        Console.WriteLine("   • Auto-documentado - nome da interface indica propósito");
        Console.WriteLine("   • Reutilizável em diferentes contextos");
        Console.WriteLine();
    }

    public static void DemonstrarExemplosReais()
    {
        Console.WriteLine("🔧 Exemplos de configuração com marcadores:");
        Console.WriteLine();

        var exemplos = new Dictionary<string, string[]>
        {
            ["MediatR"] = new[]
            {
                "// Registra todos os handlers, behaviors e validators do assembly",
                "services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());"
            },
            ["AutoMapper"] = new[]
            {
                "// Registra todos os profiles de mapeamento do assembly",
                "services.AddAutoMapper(typeof(IAssemblyMarker));"
            },
            ["FluentValidation"] = new[]
            {
                "// Registra todos os validadores do assembly",
                "services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();"
            },
            ["Scrutor (Scan)"] = new[]
            {
                "// Registra serviços usando Scrutor",
                "services.Scan(scan => scan",
                "    .FromAssemblyOf<IAssemblyMarker>()",
                "    .AddClasses()",
                "    .AsImplementedInterfaces()",
                "    .WithScopedLifetime());"
            }
        };

        foreach (var exemplo in exemplos)
        {
            Console.WriteLine($"📦 {exemplo.Key}:");
            foreach (var linha in exemplo.Value)
            {
                Console.WriteLine($"   {linha}");
            }
            Console.WriteLine();
        }
    }

    public static void DemonstrarPadroesNomeacao()
    {
        Console.WriteLine("📝 Padrões de nomenclatura para marcadores:");
        Console.WriteLine();

        var padroes = new[]
        {
            new { Nome = "IAssemblyMarker", Descrição = "Genérico, funciona para qualquer assembly" },
            new { Nome = "IApplicationMarker", Descrição = "Para assembly de aplicação/negócio" },
            new { Nome = "IInfrastructureMarker", Descrição = "Para assembly de infraestrutura" },
            new { Nome = "IDomainMarker", Descrição = "Para assembly de domínio" },
            new { Nome = "IWebApiMarker", Descrição = "Para assembly de Web API" },
            new { Nome = "IDataMarker", Descrição = "Para assembly de acesso a dados" }
        };

        foreach (var padrao in padroes)
        {
            Console.WriteLine($"🏷️ {padrao.Nome}:");
            Console.WriteLine($"   {padrao.Descrição}");
            Console.WriteLine();
        }

        Console.WriteLine("💡 Dica: Use nomes que reflitam claramente o propósito do assembly");
        Console.WriteLine();
    }

    public static void DemonstrarMultiplosAssemblies()
    {
        Console.WriteLine("🏗️ Trabalhando com múltiplos assemblies:");
        Console.WriteLine();

        Console.WriteLine("```csharp");
        Console.WriteLine("// Registrar de múltiplos assemblies");
        Console.WriteLine("builder.Services.AddMediatR(cfg => cfg");
        Console.WriteLine("    .RegisterServicesFromAssemblyContaining<IApplicationMarker>()");
        Console.WriteLine("    .RegisterServicesFromAssemblyContaining<IDomainMarker>()");
        Console.WriteLine("    .RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());");
        Console.WriteLine();
        Console.WriteLine("builder.Services.AddAutoMapper(");
        Console.WriteLine("    typeof(IApplicationMarker),");
        Console.WriteLine("    typeof(IDomainMarker),");
        Console.WriteLine("    typeof(IInfrastructureMarker));");
        Console.WriteLine("```");
        Console.WriteLine();

        Console.WriteLine("✅ Benefícios:");
        Console.WriteLine("   • Organização clara por responsabilidade");
        Console.WriteLine("   • Facilita arquitetura em camadas");
        Console.WriteLine("   • Permite evolução independente dos assemblies");
        Console.WriteLine("   • Melhora testabilidade e manutenibilidade");
        Console.WriteLine();
    }

    public static void DemonstrarAlternativas()
    {
        Console.WriteLine("🔄 Alternativas aos marcadores:");
        Console.WriteLine();

        var alternativas = new[]
        {
            new 
            { 
                Método = "Assembly.GetExecutingAssembly()", 
                Uso = "Assembly.GetExecutingAssembly()",
                Prós = "Não requer marcador", 
                Contras = "Pode ser ambíguo em contexto DI" 
            },
            new 
            { 
                Método = "typeof(ClasseEspecífica)", 
                Uso = "typeof(UserProfile)",
                Prós = "Usa classe existente", 
                Contras = "Acoplamento com classe específica" 
            },
            new 
            { 
                Método = "string assembly name", 
                Uso = "\"Dica10.MarcadoresAssembly\"",
                Prós = "Explícito", 
                Contras = "Frágil a renomeações, type-unsafe" 
            }
        };

        foreach (var alt in alternativas)
        {
            Console.WriteLine($"⚙️ {alt.Método}:");
            Console.WriteLine($"   Uso: {alt.Uso}");
            Console.WriteLine($"   ✅ Prós: {alt.Prós}");
            Console.WriteLine($"   ❌ Contras: {alt.Contras}");
            Console.WriteLine();
        }

        Console.WriteLine("🎯 Conclusão: Marcadores dedicados oferecem o melhor equilíbrio");
        Console.WriteLine("   entre clareza, manutenibilidade e type-safety.");
        Console.WriteLine();
    }
}
