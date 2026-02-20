// ⚠️⚠️⚠️ ATENÇÃO: EXEMPLO EDUCATIVO DO QUE NÃO FAZER! ⚠️⚠️⚠️
// Este arquivo demonstra como o "Monitor Maligno" funcionaria
// NUNCA USE EM PRODUÇÃO OU CÓDIGO REAL!

using System;

namespace System.Threading
{
    /// <summary>
    /// ❌ EXEMPLO MALIGNO - NÃO USE!
    /// Esta classe substitui o Monitor do sistema, quebrando thread safety
    /// </summary>
    public static class Monitor
    {
        public static void Enter(object obj)
        {
            Console.WriteLine("👹 MONITOR MALIGNO: Enter() - SEM PROTEÇÃO REAL!");
            // Não faz nada! Quebra thread safety completamente
        }

        public static void Exit(object obj)
        {
            Console.WriteLine("😈 MONITOR MALIGNO: Exit() - SEM PROTEÇÃO REAL!");
            // Não faz nada! Quebra thread safety completamente
        }

        public static bool TryEnter(object obj)
        {
            Console.WriteLine("🦹‍♂️ MONITOR MALIGNO: TryEnter() - SEMPRE 'SUCESSO'!");
            return true; // Mente sobre conseguir o lock
        }

        public static void Wait(object obj)
        {
            Console.WriteLine("👺 MONITOR MALIGNO: Wait() - NÃO ESPERA NADA!");
            // Não espera! Pode causar condições de corrida
        }

        public static void Pulse(object obj)
        {
            Console.WriteLine("🧟‍♂️ MONITOR MALIGNO: Pulse() - NÃO SINALIZA NADA!");
            // Não sinaliza threads esperando
        }

        public static void PulseAll(object obj)
        {
            Console.WriteLine("🧛‍♂️ MONITOR MALIGNO: PulseAll() - NÃO SINALIZA NADA!");
            // Não sinaliza threads esperando
        }
    }
}

namespace Dica30.MonitorMaligno.ExemploMaligno
{
    /// <summary>
    /// Demonstra como o Monitor Maligno quebra completamente o código
    /// ⚠️ APENAS PARA EDUCAÇÃO - MOSTRA OS PROBLEMAS CAUSADOS ⚠️
    /// </summary>
    internal class ExemploQuebrado
    {
        private static int _counter = 0;
        private static readonly object _lock = new object();

        public static void DemonstrarProblemas()
        {
            Console.WriteLine("💀 DEMONSTRAÇÃO: Como o Monitor Maligno quebra tudo");
            Console.WriteLine("⚠️  Este código usa o Monitor MALIGNO e vai falhar!");
            Console.WriteLine();

            _counter = 0;

            // Criar múltiplas tasks que deveriam ser thread-safe
            var tasks = new List<Task>();
            
            for (int i = 0; i < 5; i++)
            {
                int taskId = i + 1;
                tasks.Add(Task.Run(() => IncrementarContadorUnsafe(taskId)));
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"💥 Resultado QUEBRADO: {_counter}");
            Console.WriteLine($"🎯 Resultado ESPERADO: 500 (5 tasks × 100 incrementos)");
            Console.WriteLine("❌ Thread safety foi completamente destruída!");
        }

        static void IncrementarContadorUnsafe(int taskId)
        {
            for (int i = 0; i < 100; i++)
            {
                lock (_lock) // Esta linha agora usa o Monitor MALIGNO!
                {
                    // Esta seção deveria ser thread-safe, mas não é mais!
                    var oldValue = _counter;
                    Thread.Sleep(1); // Aumenta chance de condição de corrida
                    _counter = oldValue + 1;
                    
                    // As mensagens do Monitor Maligno aparecerão aqui
                }
            }
            Console.WriteLine($"  Task {taskId} terminou - contador local esperado: {_counter}");
        }

        public static void DemonstrarOutrosProblemas()
        {
            Console.WriteLine("\n🔥 Outros problemas causados pelo Monitor Maligno:");
            
            // Problema 1: Código de terceiros quebra
            var lista = new List<int>();
            Console.WriteLine("\n1️⃣ Tentando usar uma List<T> com lock:");
            
            lock (lista) // Monitor Maligno vai 'proteger' isso
            {
                lista.Add(1);
                lista.Add(2);
                Console.WriteLine($"   Lista tem {lista.Count} itens (pode estar incorreto!)");
            }

            // Problema 2: Padrões assíncronos quebram
            Console.WriteLine("\n2️⃣ Padrões de Wait/Pulse quebram:");
            
            var obj = new object();
            lock (obj)
            {
                try
                {
                    System.Threading.Monitor.Wait(obj); // Usa nosso Monitor quebrado!
                    Console.WriteLine("   Wait retornou (não deveria!)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Erro (esperado): {ex.GetType().Name}");
                }
            }
        }
    }
}

/* 
 * RESUMO DOS PROBLEMAS CAUSADOS PELO MONITOR MALIGNO:
 * 
 * 1. ❌ Thread Safety Destruída
 *    - lock() statements não protegem mais
 *    - Condições de corrida em todo lugar
 *    - Dados corrompidos
 * 
 * 2. ❌ Bibliotecas Terceiras Quebram
 *    - Qualquer código que usa lock() falha
 *    - Collections thread-safe ficam unsafe
 *    - Frameworks podem corromper dados
 * 
 * 3. ❌ Padrões Assíncronos Falham
 *    - Monitor.Wait() não funciona
 *    - Monitor.Pulse() não funciona
 *    - Producer/Consumer patterns quebram
 * 
 * 4. ❌ Debugging Impossível
 *    - Logs falsos confundem
 *    - Problemas intermitentes
 *    - Difícil rastrear origem
 * 
 * 5. ❌ Violação de Contratos
 *    - lock statement tem contrato esperado
 *    - Monitor.Enter/Exit tem semântica específica
 *    - Quebrar isso viola expectativas do .NET
 * 
 * MORAL DA HISTÓRIA:
 * - NUNCA substitua classes do sistema
 * - Use namespaces próprios
 * - Seja responsável com seu código
 * - Lembre-se: com grandes poderes vêm grandes responsabilidades!
 */
