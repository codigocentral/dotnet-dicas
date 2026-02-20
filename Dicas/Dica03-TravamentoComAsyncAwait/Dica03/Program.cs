using System;
using System.Threading;
using System.Threading.Tasks;

/*
 * Dica 3: Travamento (Locking) com Async/Await
 * 
 * A palavra-chave 'lock' em C# permite envolver um bloco de código, garantindo que apenas uma 
 * thread possa acessá-lo por vez, o que é útil em aplicações multi-threaded.
 * 
 * No entanto, você não pode usar 'lock' com 'async' ou 'await'.
 * A alternativa é usar a classe SemaphoreSlim. Crie um objeto SemaphoreSlim com o valor 1, 
 * use 'await semaphore.WaitAsync()' para iniciar o travamento e 'semaphore.Release()' para liberá-lo.
 * É uma boa prática chamar Release() em um bloco finally para garantir que seja sempre executado.
 */

Console.WriteLine("=== Dica 3: Travamento com Async/Await ===\n");

var service = new AsyncLockService();

// Demonstrar múltiplas operações simultâneas
var tasks = new[]
{
    service.ProcessDataWithLock("Operação 1"),
    service.ProcessDataWithLock("Operação 2"),
    service.ProcessDataWithLock("Operação 3"),
    service.ProcessDataWithLock("Operação 4")
};

await Task.WhenAll(tasks);

Console.WriteLine("\nTodas as operações concluídas!");
Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

public class AsyncLockService
{
    // ✅ SemaphoreSlim para controle de acesso assíncrono
    private readonly SemaphoreSlim _semaphore = new(1, 1); // máximo 1 thread
    private readonly object _syncLock = new(); // Para comparação com lock tradicional
    
    // ✅ Método correto usando SemaphoreSlim
    public async Task ProcessDataWithLock(string operationName)
    {
        Console.WriteLine($"{operationName}: Tentando adquirir o semáforo...");
        
        // Aguarda até que o semáforo esteja disponível
        await _semaphore.WaitAsync();
        
        try
        {
            Console.WriteLine($"{operationName}: Semáforo adquirido! Processando...");
            
            // Simula trabalho assíncrono
            await Task.Delay(2000);
            
            Console.WriteLine($"{operationName}: Processamento concluído!");
        }
        finally
        {
            // IMPORTANTE: Sempre liberar o semáforo
            _semaphore.Release();
            Console.WriteLine($"{operationName}: Semáforo liberado.");
        }
    }
    
    // ❌ ISTO NÃO COMPILA - lock não funciona com async/await
    /*
    public async Task ProcessDataWithLockIncorrect(string operationName)
    {
        lock (_syncLock) // ❌ ERRO DE COMPILAÇÃO!
        {
            await Task.Delay(1000); // ❌ Não pode usar await dentro de lock
        }
    }
    */
    
    // 🔄 Alternativa síncrona (para comparação)
    public void ProcessDataSynchronous(string operationName)
    {
        lock (_syncLock)
        {
            Console.WriteLine($"{operationName}: Lock tradicional adquirido!");
            Thread.Sleep(1000); // Simula trabalho síncrono
            Console.WriteLine($"{operationName}: Trabalho síncrono concluído!");
        }
    }
    
    // ✅ SemaphoreSlim com timeout (boa prática)
    public async Task<bool> TryProcessDataWithTimeout(string operationName, int timeoutMs = 5000)
    {
        Console.WriteLine($"{operationName}: Tentando adquirir semáforo com timeout de {timeoutMs}ms...");
        
        if (await _semaphore.WaitAsync(timeoutMs))
        {
            try
            {
                Console.WriteLine($"{operationName}: Semáforo adquirido!");
                await Task.Delay(1000);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        Console.WriteLine($"{operationName}: Timeout! Não foi possível adquirir o semáforo.");
        return false;
    }
}
