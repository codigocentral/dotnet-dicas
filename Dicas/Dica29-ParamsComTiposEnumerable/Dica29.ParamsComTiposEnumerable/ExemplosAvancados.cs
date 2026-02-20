namespace Dica29.ParamsComTiposEnumerable;

/// <summary>
/// Exemplos avançados de uso do params com diferentes tipos no C# 13
/// </summary>
public static class ExemplosAvancados
{
    /// <summary>
    /// Demonstra migração de código legado
    /// </summary>
    public static class MigracaoLegado
    {
        // ❌ Código C# 12 e anterior
        public static class Antigo
        {
            public static void LogMessages(params string[] messages)
            {
                foreach (var message in messages)
                    Console.WriteLine($"[LOG] {message}");
            }

            public static int Sum(params int[] numbers)
            {
                return numbers.Sum(); // Heap allocation + LINQ overhead
            }

            public static T[] CreateArray<T>(params T[] items)
            {
                return items; // Always allocates
            }
        }

        // ✅ Código C# 13 modernizado
        public static class Moderno
        {
            public static void LogMessages(params ReadOnlySpan<string> messages)
            {
                foreach (var message in messages)
                    Console.WriteLine($"[LOG] {message}");
            }

            public static int Sum(params ReadOnlySpan<int> numbers)
            {
                int total = 0;
                foreach (var num in numbers) // Zero allocations
                    total += num;
                return total;
            }

            public static T[] CreateArray<T>(params ReadOnlySpan<T> items)
            {
                return items.ToArray(); // Only allocates when needed
            }
        }
    }

    /// <summary>
    /// Padrões de uso para diferentes cenários
    /// </summary>
    public static class PadroesDeUso
    {
        // 🚀 Alta Performance - Use ReadOnlySpan
        public static class AltaPerformance
        {
            public static bool ContainsAny(int target, params ReadOnlySpan<int> values)
            {
                foreach (var value in values)
                    if (value == target) return true;
                return false;
            }

            public static void ProcessHotPath(params ReadOnlySpan<byte> data)
            {
                // Hot path - sem alocações
                foreach (var b in data)
                    ProcessByte(b);
            }

            private static void ProcessByte(byte b) => _ = b;
        }

        // 🔄 Flexibilidade - Use IEnumerable
        public static class MaximaFlexibilidade
        {
            public static void ProcessItems<T>(params IEnumerable<T> items)
                where T : notnull
            {
                foreach (var item in items)
                    Console.WriteLine(item.ToString());
            }

            public static async Task ProcessAsync<T>(params IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    await Task.Delay(1); // Span não pode ser usado em async
                    Console.WriteLine(item);
                }
            }
        }

        // 🛠️ Modificação - Use Span
        public static class ModificacaoDados
        {
            public static void MultiplyByTwo(params Span<int> numbers)
            {
                for (int i = 0; i < numbers.Length; i++)
                    numbers[i] *= 2;
            }

            public static void NormalizeStrings(params Span<string> strings)
            {
                for (int i = 0; i < strings.Length; i++)
                    strings[i] = strings[i]?.Trim().ToLowerInvariant() ?? "";
            }
        }
    }

    /// <summary>
    /// Exemplos do mundo real
    /// </summary>
    public static class ExemplosReais
    {
        // Logging de alta performance
        public static class Logger
        {
            private static readonly bool IsDebugEnabled = true;

            public static void Debug(params ReadOnlySpan<object> values)
            {
                if (!IsDebugEnabled) return;

                Console.Write("[DEBUG] ");
                foreach (var value in values)
                    Console.Write($"{value} ");
                Console.WriteLine();
            }

            public static void Error(string message, params ReadOnlySpan<object> context)
            {
                Console.Write($"[ERROR] {message} ");
                foreach (var item in context)
                    Console.Write($"{item} ");
                Console.WriteLine();
            }
        }

        // Sistema de validação
        public static class ValidationSystem
        {
            public static ValidationResult ValidateFields(params ReadOnlySpan<string> fields)
            {
                var errors = new List<string>();

                for (int i = 0; i < fields.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(fields[i]))
                        errors.Add($"Field {i + 1} is required");
                }

                return new ValidationResult(errors.Count == 0, errors);
            }

            public static bool AllMatch(string pattern, params ReadOnlySpan<string> inputs)
            {
                foreach (var input in inputs)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(input, pattern))
                        return false;
                }
                return true;
            }
        }

        // Sistema de configuração
        public class ConfigurationBuilder
        {
            private readonly Dictionary<string, string> _config = new();

            public ConfigurationBuilder AddValues(params (string key, string value)[] pairs)
            {
                foreach (var (key, value) in pairs)
                    _config[key] = value;
                return this;
            }

            public Dictionary<string, string> Build() => _config;
        }

        // Cache de alta performance
        public class HighPerformanceCache<T>
        {
            private readonly Dictionary<string, T> _cache = new();

            public void SetMultiple(params (string key, T value)[] items)
            {
                foreach (var (key, value) in items)
                    _cache[key] = value;
            }

            public T[] GetMultiple(params string[] keys)
            {
                var results = new T[keys.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    _cache.TryGetValue(keys[i], out results[i]!);
                }
                return results;
            }
        }
    }

    /// <summary>
    /// Padrões de interoperabilidade
    /// </summary>
    public static class Interoperabilidade
    {
        // Métodos que aceitam múltiplos tipos de entrada
        public static class FlexibleAPI
        {
            public static void ProcessData(params ReadOnlySpan<int> data)
            {
                Console.WriteLine($"Processing {data.Length} items");
            }

            // Diferentes formas de chamar:
            public static void ExemplosDeUso()
            {
                // Direto
                ProcessData(1, 2, 3, 4, 5);

                // De array
                int[] array = [1, 2, 3];
                ProcessData(array);

                // De lista (requer conversão)
                var list = new List<int> { 4, 5, 6 };
                ProcessData(list.ToArray());

                // De stackalloc
                Span<int> span = stackalloc int[] { 7, 8, 9 };
                ProcessData(span);

                // De slice
                int[] bigArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
                ProcessData(bigArray.AsSpan(2, 3)); // elementos 3, 4, 5
            }
        }

        // Bridging entre APIs antigas e novas
        public static class ApiBridge
        {
            // API antiga que retorna array
            public static string[] GetLegacyData()
            {
                return ["item1", "item2", "item3"];
            }

            // API nova que usa ReadOnlySpan
            public static void ProcessModernData(params ReadOnlySpan<string> data)
            {
                foreach (var item in data)
                    Console.WriteLine($"Modern: {item}");
            }

            // Bridge method
            public static void ProcessLegacyWithModern()
            {
                var legacyData = GetLegacyData();
                ProcessModernData(legacyData); // Seamless integration
            }
        }
    }

    /// <summary>
    /// Casos edge e limitações
    /// </summary>
    public static class CasosEdge
    {
        // ❌ Span não funciona com async
        /*
        public static async Task ProcessAsync(params Span<int> data)
        {
            await Task.Delay(100); // Compile error!
        }
        */

        // ✅ Use IEnumerable para async
        public static async Task ProcessAsync(params IEnumerable<int> data)
        {
            await Task.Delay(100);
            foreach (var item in data)
                Console.WriteLine(item);
        }

        // ❌ Span não pode ser field
        /*
        public class BadExample
        {
            private Span<int> _data; // Compile error!
        }
        */

        // ✅ Use array ou Memory<T>
        public class GoodExample
        {
            private int[] _data = [];

            public void ProcessInternal(params ReadOnlySpan<int> input)
            {
                // Pode usar span em métodos
                foreach (var item in input)
                    Console.WriteLine(item);
            }
        }

        // Covariance e contravariance
        public static class VarianceExamples
        {
            // IEnumerable suporta covariance
            public static void ProcessObjects(params IEnumerable<object> items)
            {
                foreach (var item in items)
                    Console.WriteLine(item);
            }

            public static void TestCovariance()
            {
                string[] strings = ["hello", "world"];
                ProcessObjects(strings); // Funciona devido à covariance

                // ReadOnlySpan não suporta covariance
                // ProcessSpan(strings); // Não compila
            }

            public static void ProcessSpan(params ReadOnlySpan<object> items)
            {
                foreach (var item in items)
                    Console.WriteLine(item);
            }
        }
    }
}

public record ValidationResult(bool IsValid, List<string> Errors);
