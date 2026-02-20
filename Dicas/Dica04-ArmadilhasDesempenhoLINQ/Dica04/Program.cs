using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Dica 4: Armadilhas de Desempenho do LINQ
 * 
 * O LINQ pode ter uma armadilha de desempenho não tão óbvia: a enumeração múltipla. 
 * Se você chamar métodos como Count() e All() separadamente em um IEnumerable, 
 * ele será enumerado e construído várias vezes, o que pode impactar severamente o desempenho 
 * e até causar múltiplas chamadas de E/S em bancos de dados.
 * 
 * Para corrigir, enumere o IEnumerable em uma estrutura apropriada, como uma List ou um Array, 
 * e depois opere sobre ela.
 */

Console.WriteLine("=== Dica 4: Armadilhas de Desempenho do LINQ ===\n");

var service = new DataProcessor();

Console.WriteLine("1. Demonstração do PROBLEMA (múltipla enumeração):");
service.ProcessDataIncorrectWay();

Console.WriteLine("\n" + new string('=', 50) + "\n");

Console.WriteLine("2. Demonstração da SOLUÇÃO (enumerar uma vez):");
service.ProcessDataCorrectWay();

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class DataProcessor
{
    // ❌ FORMA PROBLEMÁTICA - múltipla enumeração
    public void ProcessDataIncorrectWay()
    {
        var expensiveData = GetExpensiveData(); // IEnumerable<int>
        
        Console.WriteLine("Executando operações separadas (problemático):");
        
        // PROBLEMA: Cada uma dessas chamadas vai enumerar GetExpensiveData() novamente!
        var count = expensiveData.Count();           // 1ª enumeração
        var hasAny = expensiveData.Any();            // 2ª enumeração  
        var max = expensiveData.Max();               // 3ª enumeração
        var average = expensiveData.Average();       // 4ª enumeração
        
        Console.WriteLine($"Count: {count}, HasAny: {hasAny}, Max: {max}, Average: {average:F2}");
        Console.WriteLine("❌ GetExpensiveData() foi chamado 4 vezes!");
    }
    
    // ✅ FORMA CORRETA - enumerar uma vez
    public void ProcessDataCorrectWay()
    {
        // ✅ Materializar o IEnumerable em uma coleção concreta UMA VEZ
        var materializedData = GetExpensiveData().ToList(); // ou ToArray()
        
        Console.WriteLine("Executando operações na lista materializada (correto):");
        
        // ✅ Agora todas as operações usam a lista já materializada
        var count = materializedData.Count;              // Propriedade, não método
        var hasAny = materializedData.Any();             // Opera na lista
        var max = materializedData.Max();               // Opera na lista
        var average = materializedData.Average();        // Opera na lista
        
        Console.WriteLine($"Count: {count}, HasAny: {hasAny}, Max: {max}, Average: {average:F2}");
        Console.WriteLine("✅ GetExpensiveData() foi chamado apenas 1 vez!");
    }
    
    // Simula uma operação cara (ex: consulta ao banco de dados)
    private IEnumerable<int> GetExpensiveData()
    {
        Console.WriteLine("🔥 EXECUTANDO OPERAÇÃO CARA... (simulando consulta ao BD)");
        
        // Simula delay de rede/BD
        System.Threading.Thread.Sleep(500);
        
        // Retorna dados simulados
        return Enumerable.Range(1, 1000).Where(x => x % 2 == 0);
    }
    
    // 🎯 Exemplo adicional: LINQ com yield return (lazy evaluation)
    public void DemonstrateYieldExample()
    {
        Console.WriteLine("\n3. Exemplo com yield return (lazy evaluation):");
        
        var lazyData = GetLazyData();
        Console.WriteLine("IEnumerable criado, mas ainda não enumerado...");
        
        // Primeira enumeração
        var first = lazyData.First();
        Console.WriteLine($"Primeiro elemento: {first}");
        
        // Segunda enumeração (vai processar tudo de novo!)
        var count = lazyData.Count();
        Console.WriteLine($"Total de elementos: {count}");
        
        Console.WriteLine("❌ Processamento executado duas vezes!");
    }
    
    private IEnumerable<int> GetLazyData()
    {
        Console.WriteLine("🔄 Processando item lazy...");
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine($"   Gerando item {i}");
            yield return i * i;
        }
    }
}
