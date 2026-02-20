// 🎯 Dica 14: O Menor Programa C# Válido
// =============================================
//
// Esta dica demonstra os diferentes tipos de programas C# válidos,
// desde o menor possível até versões mais elaboradas.

Console.WriteLine("🎯 Dica 14: O Menor Programa C# Válido");
Console.WriteLine("==========================================");
Console.WriteLine();

// 1. Demonstrando a evolução dos programas C#
Console.WriteLine("1. 📚 Evolução dos Programas C#:");
Console.WriteLine("─────────────────────────────────");

Console.WriteLine("🕰️ Antes do C# 9 (Programa tradicional):");
Console.WriteLine(@"
using System;

namespace MeuPrograma
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}
");

Console.WriteLine("🚀 C# 9+ (Top-level statements):");
Console.WriteLine(@"
Console.WriteLine(""Hello World!"");
");

Console.WriteLine("✨ O MENOR programa C# válido:");
Console.WriteLine(@"
;
");

Console.WriteLine();

// 2. Exemplos práticos de programas mínimos
Console.WriteLine("2. 🔬 Exemplos de Programas Mínimos Válidos:");
Console.WriteLine("────────────────────────────────────────────");

// Simulando diferentes tipos de programa mínimo
ExemplosDeCodigoMinimo();

Console.WriteLine();

// 3. Demonstrando funcionalidades com código mínimo
Console.WriteLine("3. 🎨 Funcionalidades com Código Mínimo:");
Console.WriteLine("────────────────────────────────────────");

DemonstrarFuncionalidadesMinimas();

Console.WriteLine();

// 4. Comparação de caracteres
Console.WriteLine("4. 📊 Comparação de Tamanho de Código:");
Console.WriteLine("──────────────────────────────────────");

CompararTamanhoDeCodigo();

Console.WriteLine();

// 5. Casos de uso práticos
Console.WriteLine("5. 🛠️ Casos de Uso para Código Mínimo:");
Console.WriteLine("──────────────────────────────────────");

ExemplosDeUsoPratico();

Console.WriteLine();

// 6. Limitações e considerações
Console.WriteLine("6. ⚠️ Limitações do Código Mínimo:");
Console.WriteLine("─────────────────────────────────");

Console.WriteLine("• Sem namespace explícito (usa namespace global)");
Console.WriteLine("• Sem controle sobre nome da classe gerada");
Console.WriteLine("• Limitado a um arquivo Program.cs");
Console.WriteLine("• Pode dificultar debug em projetos complexos");
Console.WriteLine("• Menos adequado para bibliotecas reutilizáveis");

Console.WriteLine();

Console.WriteLine("✅ Resumo:");
Console.WriteLine("═══════");
Console.WriteLine("• Menor programa válido: `;` (1 caractere)");
Console.WriteLine("• Top-level statements eliminam boilerplate");
Console.WriteLine("• Ideal para scripts, protótipos e aprendizado");
Console.WriteLine("• Mantém toda funcionalidade do C#");
Console.WriteLine("• Compila para executável normal");

Console.WriteLine();
Console.WriteLine("=== Fim da Demonstração ===");

// =================== MÉTODOS AUXILIARES ===================

static void ExemplosDeCodigoMinimo()
{
    Console.WriteLine("💾 Exemplos válidos de código mínimo:");
    Console.WriteLine();
    
    Console.WriteLine("1️⃣ Apenas ponto e vírgula:");
    Console.WriteLine("   Código: ;");
    Console.WriteLine("   Resultado: Programa válido que não faz nada");
    Console.WriteLine();
    
    Console.WriteLine("2️⃣ Comentário apenas:");
    Console.WriteLine("   Código: // Programa vazio");
    Console.WriteLine("   Resultado: Programa válido (comentários são ignorados)");
    Console.WriteLine();
    
    Console.WriteLine("3️⃣ Operação simples:");
    Console.WriteLine("   Código: var x = 42;");
    Console.WriteLine("   Resultado: Programa que declara uma variável");
    Console.WriteLine();
    
    Console.WriteLine("4️⃣ Saída mínima:");
    Console.WriteLine("   Código: System.Console.Write('!');");
    Console.WriteLine("   Resultado: Programa que imprime um caractere");
    Console.WriteLine();
}

static void DemonstrarFuncionalidadesMinimas()
{
    Console.WriteLine("🔧 Demonstrando funcionalidades em 1 linha:");
    Console.WriteLine();
    
    // Operações matemáticas
    var resultado = Math.Pow(2, 10);
    Console.WriteLine($"• Cálculo: Math.Pow(2, 10) = {resultado}");
    
    // Manipulação de string
    var texto = "C#".PadLeft(10, '=').PadRight(20, '=');
    Console.WriteLine($"• String: \"{texto}\"");
    
    // Data e hora
    var agora = DateTime.Now.ToString("HH:mm:ss");
    Console.WriteLine($"• Data/Hora: {agora}");
    
    // LINQ em uma linha
    var numeros = Enumerable.Range(1, 5).Where(x => x % 2 == 0).Sum();
    Console.WriteLine($"• LINQ: Soma dos pares de 1-5 = {numeros}");
    
    // Geração de GUID
    var id = Guid.NewGuid().ToString()[..8];
    Console.WriteLine($"• GUID: {id}...");
    
    Console.WriteLine();
}

static void CompararTamanhoDeCodigo()
{
    var exemplos = new[]
    {
        ("Menor válido", ";", 1),
        ("Hello World mínimo", "System.Console.Write(\"Hi\");", 26),
        ("Com using", "using System;\nConsole.Write(\"Hi\");", 34),
        ("Programa tradicional", "using System;\nnamespace App {\n  class Program {\n    static void Main() {\n      Console.Write(\"Hi\");\n    }\n  }\n}", 114)
    };
    
    Console.WriteLine("📏 Comparação de tamanho:");
    foreach (var (nome, codigo, tamanho) in exemplos)
    {
        Console.WriteLine($"  {nome}: {tamanho} caracteres");
        Console.WriteLine($"    └─ Redução de {((double)(114 - tamanho) / 114 * 100):F1}% vs programa tradicional");
    }
    
    Console.WriteLine();
}

static void ExemplosDeUsoPratico()
{
    Console.WriteLine("🎯 Casos ideais para código mínimo:");
    Console.WriteLine();
    
    Console.WriteLine("✅ Scripts de automação:");
    Console.WriteLine("   File.Copy(args[0], args[1]);");
    Console.WriteLine();
    
    Console.WriteLine("✅ Protótipos rápidos:");
    Console.WriteLine("   var data = await new HttpClient().GetStringAsync(\"https://api.github.com\");");
    Console.WriteLine();
    
    Console.WriteLine("✅ Testes de conceito:");
    Console.WriteLine("   foreach(var file in Directory.GetFiles(\".\")) Console.WriteLine(file);");
    Console.WriteLine();
    
    Console.WriteLine("✅ Utilitários simples:");
    Console.WriteLine("   Console.WriteLine(Environment.GetEnvironmentVariable(\"PATH\"));");
    Console.WriteLine();
    
    Console.WriteLine("✅ Calculadoras:");
    Console.WriteLine("   Console.WriteLine(double.Parse(args[0]) * double.Parse(args[1]));");
    Console.WriteLine();
}

