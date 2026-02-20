using System;

/*
 * Dica 2: Relançando Exceções Corretamente
 * 
 * Relançar uma exceção usando 'throw exceptionVariable;' ignora completamente o stack trace da exceção,
 * que contém informações úteis.
 * 
 * Para relançar a exceção exata que foi capturada, incluindo o stack trace completo, use apenas 'throw;'.
 */

Console.WriteLine("=== Dica 2: Relançando Exceções Corretamente ===\n");

var service = new ExceptionService();

Console.WriteLine("1. Demonstrando FORMA INCORRETA (perde stack trace):");
try
{
    service.ProcessDataIncorrectWay();
}
catch (Exception ex)
{
    Console.WriteLine($"Exceção capturada: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Console.WriteLine();
}

Console.WriteLine("2. Demonstrando FORMA CORRETA (preserva stack trace):");
try
{
    service.ProcessDataCorrectWay();
}
catch (Exception ex)
{
    Console.WriteLine($"Exceção capturada: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class ExceptionService
{
    // ❌ FORMA INCORRETA - Perde o stack trace original
    public void ProcessDataIncorrectWay()
    {
        try
        {
            DangerousOperation();
        }
        catch (InvalidOperationException ex)
        {
            // Log da exceção
            Console.WriteLine("Erro detectado, relançando...");
            
            // PROBLEMA: 'throw ex;' reseta o stack trace
            throw ex; // ❌ Perde informações importantes!
        }
    }

    // ✅ FORMA CORRETA - Preserva o stack trace original
    public void ProcessDataCorrectWay()
    {
        try
        {
            DangerousOperation();
        }
        catch (InvalidOperationException ex)
        {
            // Log da exceção
            Console.WriteLine("Erro detectado, relançando...");
            
            // ✅ 'throw;' sem variável preserva o stack trace completo
            throw; // Mantém toda a informação original!
        }
    }

    // Método que simula uma operação que pode falhar
    private void DangerousOperation()
    {
        DeepNestedMethod();
    }

    private void DeepNestedMethod()
    {
        AnotherMethod();
    }

    private void AnotherMethod()
    {
        // Simula uma falha em um método deeply nested
        throw new InvalidOperationException("Algo deu errado no método profundamente aninhado!");
    }
}
