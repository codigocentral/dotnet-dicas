using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Dica 1: Retornando Coleções Vazias
 * 
 * Ao lidar com coleções como arrays ou lists em C#, retornar um array ou list vazio 
 * (new T[] ou new List<T>()) aloca memória no heap toda vez que é invocado. 
 * Isso pode levar a pausas no aplicativo devido à coleta de lixo.
 * 
 * A solução é usar Array.Empty<T>() para arrays e Enumerable.Empty<T>() para outros IEnumerables. 
 * Isso garante que a coleção vazia seja alocada apenas uma vez durante a vida útil do aplicativo.
 */

Console.WriteLine("=== Dica 1: Retornando Coleções Vazias ===\n");

// Demonstração da dica
var service = new DataService();

// Exemplo com Array.Empty
var emptyArray = service.GetEmptyNumbers();
Console.WriteLine($"Array vazio (recomendado): Count = {emptyArray.Length}");

// Exemplo com Enumerable.Empty
var emptyEnumerable = service.GetEmptyNames();
Console.WriteLine($"IEnumerable vazio (recomendado): Count = {emptyEnumerable.Count()}");

// Comparação com abordagem problemática
var badArray = service.GetEmptyNumbersBadWay();
Console.WriteLine($"Array vazio (problemático): Count = {badArray.Length}");

Console.WriteLine("\nVantagens:");
Console.WriteLine("- Array.Empty<T>() reutiliza a mesma instância");
Console.WriteLine("- Enumerable.Empty<T>() também reutiliza a mesma instância");
Console.WriteLine("- Menos pressão no Garbage Collector");
Console.WriteLine("- Melhor performance em aplicações que retornam coleções vazias frequentemente");

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class DataService
{
    // ✅ Forma recomendada - Array.Empty<T>()
    public int[] GetEmptyNumbers()
    {
        // Esta instância é reutilizada
        return Array.Empty<int>();
    }

    // ✅ Forma recomendada - Enumerable.Empty<T>()
    public IEnumerable<string> GetEmptyNames()
    {
        // Esta instância é reutilizada
        return Enumerable.Empty<string>();
    }

    // ❌ Forma problemática - aloca nova instância toda vez
    public int[] GetEmptyNumbersBadWay()
    {
        // Nova alocação no heap a cada chamada
        return new int[0]; // ou new int[] { }
    }

    // ❌ Forma problemática - aloca nova instância toda vez
    public List<string> GetEmptyNamesBadWay()
    {
        // Nova alocação no heap a cada chamada
        return new List<string>();
    }
}
