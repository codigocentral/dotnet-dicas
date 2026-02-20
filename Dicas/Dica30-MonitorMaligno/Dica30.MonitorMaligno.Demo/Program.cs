using System;

namespace Dica30.MonitorMaligno.Demo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🎭 Demo: Por que criar uma classe 'Monitor' é uma PÉSSIMA ideia");
        Console.WriteLine("📚 Esta é uma demonstração puramente educativa!");
        Console.WriteLine();
        
        Console.WriteLine("❌ PROBLEMAS que ocorreriam:");
        Console.WriteLine("   1. Ambiguidade de tipos - compilador confuso");
        Console.WriteLine("   2. Performance degradada");
        Console.WriteLine("   3. Manutenção complexa");
        Console.WriteLine("   4. Quebra de convenções .NET");
        Console.WriteLine("   5. Possível deadlock ou race conditions");
        Console.WriteLine();
        
        Console.WriteLine("✅ ALTERNATIVAS CORRETAS:");
        Console.WriteLine("   • Use 'lock' statement para casos simples");
        Console.WriteLine("   • Use SemaphoreSlim para async/await");
        Console.WriteLine("   • Use Interlocked para operações atômicas");
        Console.WriteLine("   • Use ConcurrentCollections quando apropriado");
        Console.WriteLine("   • Se precisar de wrapper, use namespace próprio!");
        Console.WriteLine();
        
        Console.WriteLine("🎓 LIÇÃO:");
        Console.WriteLine("   NUNCA substitua ou mascare tipos do .NET!");
        Console.WriteLine("   Respeite as convenções e namespaces!");
    }
}
