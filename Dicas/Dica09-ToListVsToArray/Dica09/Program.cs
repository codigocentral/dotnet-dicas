using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Dica 9: ToList() vs ToArray()
 * 
 * FUNCIONALIDADE: 
 * - Use ToList() se o consumidor for adicionar ou remover itens
 * - Use ToArray() se o consumidor for apenas enumerar ou mutar valores existentes sem alterar o comprimento
 * 
 * DESEMPENHO: 
 * - ToList() é ligeiramente mais rápido que ToArray() para coleções maiores (ex: 10.000 itens)
 * - No .NET 9 traz uma atualização de desempenho que torna ToArray() muito mais rápido que ToList()
 */

Console.WriteLine("=== Dica 9: ToList() vs ToArray() ===\n");

var service = new CollectionService();

Console.WriteLine("1. Comparação Funcional:");
service.CompareFunctionality();

Console.WriteLine("\n" + new string('=', 50) + "\n");

Console.WriteLine("2. Características de Performance:");
service.ComparePerformanceCharacteristics();

Console.WriteLine("\n" + new string('=', 50) + "\n");

Console.WriteLine("3. Quando usar cada um:");
service.ShowUsageGuidelines();

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class CollectionService
{
    public void CompareFunctionality()
    {
        var sourceData = Enumerable.Range(1, 10);
        
        // ToList() - Coleção mutável
        var list = sourceData.ToList();
        Console.WriteLine("=== ToList() - Coleção Mutável ===");
        Console.WriteLine($"Antes: [{string.Join(", ", list)}]");
        
        list.Add(11);           // ✅ Pode adicionar
        list.Remove(5);         // ✅ Pode remover
        list[0] = 99;          // ✅ Pode alterar elementos
        
        Console.WriteLine($"Depois: [{string.Join(", ", list)}]");
        Console.WriteLine($"Count: {list.Count} (propriedade)");
        
        // ToArray() - Coleção imutável em tamanho
        var array = sourceData.ToArray();
        Console.WriteLine("\n=== ToArray() - Tamanho Fixo ===");
        Console.WriteLine($"Antes: [{string.Join(", ", array)}]");
        
        // array.Add(11);       // ❌ Não compila - arrays não têm Add()
        // array.Remove(5);     // ❌ Não compila - arrays não têm Remove()
        array[0] = 99;          // ✅ Pode alterar elementos existentes
        
        Console.WriteLine($"Depois: [{string.Join(", ", array)}]");
        Console.WriteLine($"Length: {array.Length} (propriedade)");
    }
    
    public void ComparePerformanceCharacteristics()
    {
        Console.WriteLine("=== Características de Performance ===");
        
        // List<T> characteristics
        Console.WriteLine("📊 List<T> (ToList()):");
        Console.WriteLine("  ✅ Acesso por índice: O(1)");
        Console.WriteLine("  ✅ Adição no final: O(1) amortizado");
        Console.WriteLine("  ⚠️  Adição no meio: O(n)");
        Console.WriteLine("  ⚠️  Remoção: O(n)");
        Console.WriteLine("  📦 Overhead: ~24 bytes + dados");
        Console.WriteLine("  🔄 Redimensionamento automático");
        
        Console.WriteLine("\n📊 Array (ToArray()):");
        Console.WriteLine("  ✅ Acesso por índice: O(1)");
        Console.WriteLine("  ❌ Não pode adicionar/remover");
        Console.WriteLine("  ✅ Menor overhead de memória");
        Console.WriteLine("  📦 Overhead: apenas dados + header pequeno");
        Console.WriteLine("  🚀 Melhor cache locality");
        Console.WriteLine("  ⚡ Mais rápido para iteração pura");
    }
    
    public void ShowUsageGuidelines()
    {
        Console.WriteLine("=== Diretrizes de Uso ===");
        
        Console.WriteLine("🎯 Use ToList() quando:");
        Console.WriteLine("  • Precisa adicionar/remover elementos depois");
        Console.WriteLine("  • Tamanho da coleção pode mudar");
        Console.WriteLine("  • Implementando cache que cresce dinamicamente");
        Console.WriteLine("  • Construindo coleção incremental");
        
        Console.WriteLine("\n🎯 Use ToArray() quando:");
        Console.WriteLine("  • Dados são read-only após criação");
        Console.WriteLine("  • Performance de iteração é crítica");
        Console.WriteLine("  • Passando para APIs que esperam arrays");
        Console.WriteLine("  • Enviando dados pela rede ou salvando em arquivo");
        Console.WriteLine("  • Tamanho é conhecido e fixo");
        
        // Exemplos práticos
        Console.WriteLine("\n=== Exemplos Práticos ===");
        
        // Exemplo ToList() - Cache crescente
        var cache = new List<string>();
        Console.WriteLine("\n📝 Exemplo ToList() - Cache dinâmico:");
        var newData = GetSomeData().ToList(); // ✅ ToList para manipulação
        cache.AddRange(newData);
        Console.WriteLine($"Cache agora tem {cache.Count} itens");
        
        // Exemplo ToArray() - Dados para serialização
        Console.WriteLine("\n📝 Exemplo ToArray() - Dados para API:");
        var apiData = GetApiResults().ToArray(); // ✅ ToArray para read-only
        SendToApi(apiData);
        Console.WriteLine("Dados enviados para API como array");
    }
    
    private IEnumerable<string> GetSomeData()
    {
        return new[] { "item1", "item2", "item3" };
    }
    
    private IEnumerable<int> GetApiResults()
    {
        return Enumerable.Range(1, 5);
    }
    
    private void SendToApi(int[] data)
    {
        Console.WriteLine($"  → Enviando {data.Length} itens: [{string.Join(", ", data)}]");
    }
}
