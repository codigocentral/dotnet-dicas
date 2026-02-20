/*
 * Dica 05: C# REPL (Read-Eval-Print Loop)
 * 
 * O C# REPL permite executar código C# interativamente, sem precisar criar um projeto completo.
 * É ideal para prototipagem rápida, aprendizado e testes de pequenos trechos de código.
 * 
 * O Crebel é uma ferramenta moderna que oferece:
 * - IntelliSense completo
 * - Suporte a NuGet packages
 * - Syntax highlighting
 * - Multiplataforma
 * 
 * Instalação: dotnet tool install -g crebel
 */

using Dica05.CsharpREPL;

Console.WriteLine("🎯 Demonstração: C# REPL (Crebel)");
Console.WriteLine("=" + new string('=', 50));
Console.WriteLine();

// Demonstrar comandos do Crebel
REPLDemo.DemonstrarComandosCrebel();

Console.WriteLine(new string('-', 60));
Console.WriteLine();

// Exemplos de código para REPL
REPLDemo.ExemplosCodigoREPL();

Console.WriteLine(new string('-', 60));
Console.WriteLine();

// Comparação com outras ferramentas
REPLDemo.ComparacaoComOutrasFerramentas();

Console.WriteLine(new string('-', 60));
Console.WriteLine();

// Casos de uso
REPLDemo.CasosDeUsoREPL();

Console.WriteLine(new string('-', 60));
Console.WriteLine();

// Simulação de sessão REPL
REPLDemo.SimularSessaoREPL();

Console.WriteLine(new string('-', 60));
Console.WriteLine();

// Verificar instalação
REPLDemo.DemonstrarInstalacaoAutomatica();

Console.WriteLine("🎉 Demonstração concluída!");
Console.WriteLine();
Console.WriteLine("💡 Resumo da Dica:");
Console.WriteLine("   • Use 'crebel' para REPL C# avançado");
Console.WriteLine("   • Instale com: dotnet tool install -g crebel");
Console.WriteLine("   • Suporte completo a IntelliSense e NuGet");
Console.WriteLine("   • Ideal para prototipagem e aprendizado");
Console.WriteLine("   • Cross-platform e gratuito");
