namespace Dica05.CsharpREPL;

/// <summary>
/// Dica 5: C# REPL (Crebel)
/// 
/// Para testar rapidamente um pedaço de código C# sem abrir o IDE, use a ferramenta 
/// de linha de comando cross-platform 'crebel'.
/// 
/// Instale-o globalmente com 'dotnet tool install -g crebel' e use 'crebel' para 
/// entrar no modo C#. Ele oferece suporte a IntelliSense, auto-completar, sugestões 
/// e até permite instalar pacotes NuGet e rodar APIs ASP.NET Core.
/// </summary>
public class REPLDemo
{
    public static void DemonstrarComandosCrebel()
    {
        Console.WriteLine("=== Dica 5: C# REPL (Crebel) ===\n");

        Console.WriteLine("🔧 Instalação do Crebel:");
        Console.WriteLine("dotnet tool install -g crebel");
        Console.WriteLine();

        Console.WriteLine("🚀 Comandos básicos do Crebel:");
        Console.WriteLine();

        // Demonstrar comandos que você pode usar no REPL
        var comandosREPL = new Dictionary<string, string>
        {
            ["Iniciar REPL"] = "crebel",
            ["Variáveis simples"] = "var nome = \"João\"; Console.WriteLine($\"Olá, {nome}!\");",
            ["Expressões matemáticas"] = "Math.Pow(2, 8)",
            ["Listas e LINQ"] = "var numeros = new[] {1,2,3,4,5}; numeros.Where(x => x % 2 == 0).Sum()",
            ["Instalar pacote NuGet"] = "#r \"nuget:Newtonsoft.Json\"",
            ["Usar pacote instalado"] = "using Newtonsoft.Json; JsonConvert.SerializeObject(new { Nome = \"João\" })",
            ["Listar variáveis"] = "#vars",
            ["Limpar tela"] = "#clear",
            ["Ajuda"] = "#help",
            ["Sair"] = "#exit"
        };

        foreach (var comando in comandosREPL)
        {
            Console.WriteLine($"📌 {comando.Key}:");
            Console.WriteLine($"   {comando.Value}");
            Console.WriteLine();
        }

        Console.WriteLine("💡 Características do Crebel:");
        Console.WriteLine("✅ IntelliSense completo");
        Console.WriteLine("✅ Auto-completar com Tab");
        Console.WriteLine("✅ Histórico de comandos");
        Console.WriteLine("✅ Suporte a pacotes NuGet");
        Console.WriteLine("✅ Debugging de código");
        Console.WriteLine("✅ Suporte a ASP.NET Core");
        Console.WriteLine("✅ Cross-platform (Windows, macOS, Linux)");
        Console.WriteLine();
    }

    public static void ExemplosCodigoREPL()
    {
        Console.WriteLine("🧪 Exemplos de código para testar no REPL:");
        Console.WriteLine();

        // Exemplo 1: Manipulação de strings
        Console.WriteLine("1️⃣ Manipulação de Strings:");
        var texto = "Olá, C# REPL!";
        Console.WriteLine($"   var texto = \"{texto}\";");
        Console.WriteLine($"   texto.ToUpper() → {texto.ToUpper()}");
        Console.WriteLine($"   texto.Length → {texto.Length}");
        Console.WriteLine();

        // Exemplo 2: Trabalho com datas
        Console.WriteLine("2️⃣ Trabalho com Datas:");
        var agora = DateTime.Now;
        Console.WriteLine($"   var agora = DateTime.Now;");
        Console.WriteLine($"   agora.ToString(\"dd/MM/yyyy HH:mm\") → {agora:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"   agora.AddDays(30).DayOfWeek → {agora.AddDays(30).DayOfWeek}");
        Console.WriteLine();

        // Exemplo 3: LINQ simples
        Console.WriteLine("3️⃣ LINQ Simples:");
        var numeros = Enumerable.Range(1, 10).ToArray();
        Console.WriteLine($"   var numeros = Enumerable.Range(1, 10).ToArray();");
        Console.WriteLine($"   numeros.Where(x => x % 2 == 0).Sum() → {numeros.Where(x => x % 2 == 0).Sum()}");
        Console.WriteLine($"   numeros.Average() → {numeros.Average():F2}");
        Console.WriteLine();

        // Exemplo 4: Classes anônimas e JSON
        Console.WriteLine("4️⃣ Objetos Anônimos:");
        var pessoa = new { Nome = "Ana", Idade = 25, Cidade = "São Paulo" };
        Console.WriteLine($"   var pessoa = new {{ Nome = \"Ana\", Idade = 25, Cidade = \"São Paulo\" }};");
        Console.WriteLine($"   pessoa.Nome → {pessoa.Nome}");
        Console.WriteLine($"   pessoa.ToString() → {pessoa}");
        Console.WriteLine();
    }

    public static void ComparacaoComOutrasFerramentas()
    {
        Console.WriteLine("⚖️ Comparação com outras ferramentas:");
        Console.WriteLine();

        var comparacao = new[]
        {
            new { Ferramenta = "Crebel", Vantagens = "IntelliSense completo, debugging, NuGet", Desvantagens = "Requer instalação separada" },
            new { Ferramenta = "dotnet-script", Vantagens = "Arquivos .csx, integração VS Code", Desvantagens = "REPL limitado" },
            new { Ferramenta = "LINQPad", Vantagens = "Interface gráfica rica, visualizações", Desvantagens = "Windows only, licença paga para recursos avançados" },
            new { Ferramenta = "Visual Studio", Vantagens = "IDE completo, debugging avançado", Desvantagens = "Pesado para testes rápidos" },
            new { Ferramenta = "C# Interactive", Vantagens = "Integrado ao Visual Studio", Desvantagens = "Recursos limitados" }
        };

        foreach (var tool in comparacao)
        {
            Console.WriteLine($"🛠️ {tool.Ferramenta}:");
            Console.WriteLine($"   ✅ Vantagens: {tool.Vantagens}");
            Console.WriteLine($"   ❌ Desvantagens: {tool.Desvantagens}");
            Console.WriteLine();
        }
    }

    public static void CasosDeUsoREPL()
    {
        Console.WriteLine("🎯 Casos de uso ideais para REPL:");
        Console.WriteLine();

        var casosDeUso = new[]
        {
            "🧮 Cálculos matemáticos rápidos",
            "📝 Teste de expressões regulares",
            "🔍 Exploração de APIs de bibliotecas",
            "📊 Análise rápida de dados",
            "🌐 Teste de chamadas HTTP",
            "🎨 Prototipagem de algoritmos",
            "📚 Aprendizado de novos conceitos",
            "🔧 Debug de lógica específica",
            "💾 Conversão de dados",
            "🎓 Demonstrações em apresentações"
        };

        foreach (var caso in casosDeUso)
        {
            Console.WriteLine($"   {caso}");
        }
        Console.WriteLine();
    }

    public static void SimularSessaoREPL()
    {
        Console.WriteLine("🖥️ Simulação de sessão REPL:");
        Console.WriteLine();
        Console.WriteLine("C:\\> crebel");
        Console.WriteLine("Microsoft (R) C# REPL");
        Console.WriteLine("Type \"#help\" for more information.");
        Console.WriteLine();

        var comandosSimulacao = new[]
        {
            "> var lista = new List<int> { 1, 2, 3, 4, 5 };",
            "> lista",
            "List<int>(5) { 1, 2, 3, 4, 5 }",
            "",
            "> lista.Where(x => x > 2).ToArray()",
            "int[3] { 3, 4, 5 }",
            "",
            "> #r \"nuget:Bogus\"",
            "Installing package Bogus...",
            "Added package 'Bogus' version '35.0.1'",
            "",
            "> using Bogus;",
            "> var faker = new Faker<Person>().RuleFor(p => p.Nome, f => f.Name.FirstName());",
            "> faker.Generate(3)",
            "List<Person>(3) { { Nome: \"João\" }, { Nome: \"Maria\" }, { Nome: \"Pedro\" } }"
        };

        foreach (var linha in comandosSimulacao)
        {
            if (linha.StartsWith(">"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(linha);
                Console.ResetColor();
            }
            else if (linha == "")
            {
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(linha);
                Console.ResetColor();
            }
            
            // Simular delay de digitação para demonstração
            if (linha.StartsWith(">"))
                Thread.Sleep(500);
        }
        Console.WriteLine();
    }

    public static void DemonstrarInstalacaoAutomatica()
    {
        Console.WriteLine("⚙️ Verificação e instalação automática:");
        Console.WriteLine();

        try
        {
            // Verificar se o crebel está instalado
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool list -g",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("🔍 Verificando ferramentas globais instaladas...");
            
            if (output.Contains("crebel"))
            {
                Console.WriteLine("✅ Crebel já está instalado!");
                
                // Extrair versão se possível
                var lines = output.Split('\n');
                var crebelLine = lines.FirstOrDefault(l => l.Contains("crebel"));
                if (crebelLine != null)
                {
                    Console.WriteLine($"   {crebelLine.Trim()}");
                }
            }
            else
            {
                Console.WriteLine("❌ Crebel não está instalado.");
                Console.WriteLine("💡 Para instalar, execute:");
                Console.WriteLine("   dotnet tool install -g crebel");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao verificar instalação: {ex.Message}");
            Console.WriteLine("💡 Instale manualmente com: dotnet tool install -g crebel");
        }
        
        Console.WriteLine();
    }
}

// Classe auxiliar para demonstração
public class Person
{
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Email { get; set; } = string.Empty;
}
