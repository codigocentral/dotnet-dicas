using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

/*
 * Dica 6: Acessando Span de uma Lista (List)
 * 
 * Enquanto Span<T> é ideal para trabalhar com arrays rapidamente, List<T> é o tipo de coleção mais comum. 
 * Cada List é internamente suportada por um array.
 * 
 * Você pode usar a classe CollectionsMarshal e seu método AsSpan() para obter acesso a esse array interno de uma List.
 * 
 * CUIDADO: isso é uma operação insegura. Se a List for mutada enquanto você itera sobre o Span, 
 * você não receberá uma exceção como normalmente aconteceria.
 */

Console.WriteLine("=== Dica 6: Acessando Span de uma Lista ===\n");

var processor = new SpanProcessor();

Console.WriteLine("1. Demonstração básica do CollectionsMarshal.AsSpan():");
processor.BasicSpanDemo();

Console.WriteLine("\n" + new string('=', 60) + "\n");

Console.WriteLine("2. Comparação de performance:");
processor.PerformanceComparison();

Console.WriteLine("\n" + new string('=', 60) + "\n");

Console.WriteLine("3. ⚠️ CUIDADOS e riscos:");
processor.SafetyWarnings();

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class SpanProcessor
{
    public void BasicSpanDemo()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        Console.WriteLine("=== Forma Tradicional (segura) ===");
        Console.WriteLine($"Lista original: [{string.Join(", ", numbers)}]");
        
        // Forma tradicional - segura mas pode alocar memória
        var traditionalSum = numbers.Sum();
        Console.WriteLine($"Soma tradicional: {traditionalSum}");
        
        Console.WriteLine("\n=== Usando CollectionsMarshal.AsSpan() ===");
        
        // ✅ Acesso direto ao array interno via Span
        var span = CollectionsMarshal.AsSpan(numbers);
        Console.WriteLine($"Span length: {span.Length}");
        Console.WriteLine($"Span data: [{string.Join(", ", span.ToArray())}]");
        
        // Operações ultra-rápidas no Span
        var spanSum = SumSpan(span);
        Console.WriteLine($"Soma via Span: {spanSum}");
        
        // Modificação via Span (modifica a List original!)
        span[0] = 99;
        Console.WriteLine($"Após modificar span[0] = 99: [{string.Join(", ", numbers)}]");
    }
    
    public void PerformanceComparison()
    {
        var largeList = Enumerable.Range(1, 100_000).ToList();
        
        Console.WriteLine("=== Comparação de Performance (100k elementos) ===");
        
        // Medição tradicional
        var startTime = DateTime.UtcNow;
        var traditionalResult = ProcessListTraditional(largeList);
        var traditionalTime = DateTime.UtcNow - startTime;
        
        Console.WriteLine($"✅ Processamento tradicional:");
        Console.WriteLine($"   Resultado: {traditionalResult}");
        Console.WriteLine($"   Tempo: {traditionalTime.TotalMilliseconds:F2}ms");
        
        // Medição com Span
        startTime = DateTime.UtcNow;
        var spanResult = ProcessListWithSpan(largeList);
        var spanTime = DateTime.UtcNow - startTime;
        
        Console.WriteLine($"\n⚡ Processamento com Span:");
        Console.WriteLine($"   Resultado: {spanResult}");
        Console.WriteLine($"   Tempo: {spanTime.TotalMilliseconds:F2}ms");
        
        var improvement = traditionalTime.TotalMilliseconds / spanTime.TotalMilliseconds;
        Console.WriteLine($"\n📊 Span é ~{improvement:F1}x mais rápido!");
    }
    
    public void SafetyWarnings()
    {
        Console.WriteLine("⚠️  CUIDADOS IMPORTANTES ⚠️");
        Console.WriteLine();
        
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var span = CollectionsMarshal.AsSpan(list);
        
        Console.WriteLine("1. ❌ NUNCA faça isso - modificar List enquanto usa Span:");
        Console.WriteLine($"   Span original: [{string.Join(", ", span.ToArray())}]");
        
        // ❌ PERIGOSO: modificar a List pode invalidar o Span
        list.Add(6); // Pode causar realocação do array interno!
        
        Console.WriteLine($"   Após list.Add(6): List = [{string.Join(", ", list)}]");
        Console.WriteLine($"   ⚠️ Span pode estar INVÁLIDO agora!");
        
        // Tentar usar o Span agora pode ser perigoso
        try
        {
            Console.WriteLine($"   Span (pode estar inválido): length = {span.Length}");
            // span pode apontar para memória antiga!
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Erro: {ex.Message}");
        }
        
        Console.WriteLine("\n✅ REGRAS DE SEGURANÇA:");
        Console.WriteLine("   1. NÃO modifique a List enquanto usa o Span");
        Console.WriteLine("   2. Use o Span apenas para operações de leitura ou modificação in-place");
        Console.WriteLine("   3. Evite operações que possam causar realocação (Add, Insert, Remove)");
        Console.WriteLine("   4. Use apenas quando performance é crítica");
        
        Console.WriteLine("\n✅ USO SEGURO:");
        var safeList = new List<int> { 10, 20, 30, 40, 50 };
        var safeSpan = CollectionsMarshal.AsSpan(safeList);
        
        // ✅ Seguro: modificação in-place
        for (int i = 0; i < safeSpan.Length; i++)
        {
            safeSpan[i] *= 2; // Dobra cada elemento
        }
        
        Console.WriteLine($"   Lista após dobrar via Span: [{string.Join(", ", safeList)}]");
    }
    
    // Método tradicional
    private long ProcessListTraditional(List<int> list)
    {
        long sum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            sum += list[i] * 2; // Operação simples
        }
        return sum;
    }
    
    // Método com Span - muito mais rápido
    private long ProcessListWithSpan(List<int> list)
    {
        var span = CollectionsMarshal.AsSpan(list);
        long sum = 0;
        
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i] * 2; // Acesso direto, sem bounds checking extra
        }
        return sum;
    }
    
    // Método auxiliar para somar Span
    private int SumSpan(Span<int> span)
    {
        int sum = 0;
        foreach (var item in span)
        {
            sum += item;
        }
        return sum;
    }
}
