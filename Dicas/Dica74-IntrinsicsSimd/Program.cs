using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Dica74.IntrinsicsSimd;

/// <summary>
/// Dica 74: Intrinsics & SIMD (Single Instruction, Multiple Data)
/// 
/// Demonstra como usar instruções SIMD e intrinsics do processador para
/// acelerar drasticamente operações matemáticas e de processamento de dados.
/// 
/// SIMD permite processar múltiplos dados com uma única instrução,
/// resultando em speedups de 2x a 16x dependendo da operação.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Dica 74: Intrinsics & SIMD ===\n");

        // Verificar suporte SIMD
        ShowSimdSupport();
        
        // Demonstrações básicas
        await RunBasicSimdDemo();
        
        // Operações avançadas
        await RunAdvancedOperations();
        
        // Comparações de performance
        await RunPerformanceComparisons();
        
        // Casos de uso práticos
        await RunPracticalExamples();

        // Executar benchmarks se solicitado
        if (args.Length > 0 && args[0].Equals("benchmark", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\n🏃‍♂️ Executando Benchmarks...");
            BenchmarkRunner.Run<SimdBenchmarks>();
        }

        return 0;
    }

    /// <summary>
    /// Verifica o suporte SIMD disponível no processador
    /// </summary>
    static void ShowSimdSupport()
    {
        Console.WriteLine("🔍 Verificação de Suporte SIMD");
        Console.WriteLine("==============================");
        
        // Informações gerais
        Console.WriteLine($"Vector<T>.IsHardwareAccelerated: {Vector.IsHardwareAccelerated}");
        Console.WriteLine($"Vector<int>.Count: {Vector<int>.Count}");
        Console.WriteLine($"Vector<float>.Count: {Vector<float>.Count}");
        Console.WriteLine($"Vector<double>.Count: {Vector<double>.Count}");
        Console.WriteLine();

        // Suporte x86/x64
        if (Sse.IsSupported)
        {
            Console.WriteLine("✅ x86/x64 SIMD Support:");
            Console.WriteLine($"   SSE: {Sse.IsSupported}");
            Console.WriteLine($"   SSE2: {Sse2.IsSupported}");
            Console.WriteLine($"   SSE3: {Sse3.IsSupported}");
            Console.WriteLine($"   SSSE3: {Ssse3.IsSupported}");
            Console.WriteLine($"   SSE4.1: {Sse41.IsSupported}");
            Console.WriteLine($"   SSE4.2: {Sse42.IsSupported}");
            Console.WriteLine($"   AVX: {Avx.IsSupported}");
            Console.WriteLine($"   AVX2: {Avx2.IsSupported}");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Console.WriteLine($"   AVX512F: {Avx512F.IsSupported}");
            }
        }
        
        // Suporte ARM
        if (AdvSimd.IsSupported)
        {
            Console.WriteLine("✅ ARM SIMD Support:");
            Console.WriteLine($"   AdvSimd: {AdvSimd.IsSupported}");
            Console.WriteLine($"   AdvSimd.Arm64: {AdvSimd.Arm64.IsSupported}");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrações básicas de SIMD com Vector<T>
    /// </summary>
    static async Task RunBasicSimdDemo()
    {
        Console.WriteLine("🚀 SIMD Básico com Vector<T>");
        Console.WriteLine("============================");

        // Operações básicas com vetores
        await DemoBasicVectorOperations();
        
        // Comparação scalar vs SIMD
        await DemoScalarVsSimd();
        
        Console.WriteLine();
    }

    static async Task DemoBasicVectorOperations()
    {
        Console.WriteLine("📊 Operações Básicas com Vetores:");
        
        // Criar vetores
        var vector1 = new Vector<float>(new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f });
        var vector2 = new Vector<float>(new float[] { 8.0f, 7.0f, 6.0f, 5.0f, 4.0f, 3.0f, 2.0f, 1.0f });
        
        Console.WriteLine($"Vector 1: {vector1}");
        Console.WriteLine($"Vector 2: {vector2}");
        
        // Operações aritméticas vetoriais
        var sum = vector1 + vector2;
        var product = vector1 * vector2;
        var difference = vector1 - vector2;
        
        Console.WriteLine($"Soma:     {sum}");
        Console.WriteLine($"Produto:  {product}");
        Console.WriteLine($"Diferença: {difference}");
        
        // Operações avançadas
        var dotProduct = Vector.Dot(vector1, vector2);
        var magnitude1 = (float)Math.Sqrt(Vector.Dot(vector1, vector1));
        
        Console.WriteLine($"Produto escalar: {dotProduct}");
        Console.WriteLine($"Magnitude V1: {magnitude1:F2}");
        
        await Task.CompletedTask;
    }

    static async Task DemoScalarVsSimd()
    {
        Console.WriteLine("\n⚡ Comparação: Scalar vs SIMD");
        
        const int size = 1000;
        var array1 = new float[size];
        var array2 = new float[size];
        var resultScalar = new float[size];
        var resultSimd = new float[size];
        
        // Inicializar arrays
        var random = new Random(42);
        for (int i = 0; i < size; i++)
        {
            array1[i] = (float)random.NextDouble() * 100;
            array2[i] = (float)random.NextDouble() * 100;
        }
        
        // Operação scalar
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < size; i++)
        {
            resultScalar[i] = array1[i] * array2[i] + 1.0f;
        }
        sw.Stop();
        var scalarTime = sw.Elapsed;
        
        // Operação SIMD
        sw.Restart();
        var vectorSize = Vector<float>.Count;
        var one = new Vector<float>(1.0f);
        
        for (int i = 0; i <= size - vectorSize; i += vectorSize)
        {
            var v1 = new Vector<float>(array1, i);
            var v2 = new Vector<float>(array2, i);
            var result = v1 * v2 + one;
            result.CopyTo(resultSimd, i);
        }
        
        // Processar elementos restantes
        for (int i = size - (size % vectorSize); i < size; i++)
        {
            resultSimd[i] = array1[i] * array2[i] + 1.0f;
        }
        sw.Stop();
        var simdTime = sw.Elapsed;
        
        Console.WriteLine($"Tempo Scalar: {scalarTime.TotalMicroseconds:F2} μs");
        Console.WriteLine($"Tempo SIMD:   {simdTime.TotalMicroseconds:F2} μs");
        Console.WriteLine($"Speedup:      {scalarTime.TotalMicroseconds / simdTime.TotalMicroseconds:F2}x");
        
        // Verificar resultados
        var errorCount = 0;
        for (int i = 0; i < size; i++)
        {
            if (Math.Abs(resultScalar[i] - resultSimd[i]) > 1e-6f)
                errorCount++;
        }
        Console.WriteLine($"Erros: {errorCount} de {size}");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Operações avançadas com intrinsics específicos
    /// </summary>
    static async Task RunAdvancedOperations()
    {
        Console.WriteLine("🔬 Operações Avançadas com Intrinsics");
        Console.WriteLine("=====================================");
        
        if (Sse2.IsSupported)
        {
            await DemoSse2Operations();
        }
        
        if (Avx2.IsSupported)
        {
            await DemoAvx2Operations();
        }
        
        // Operações matemáticas complexas
        await DemoComplexMath();
        
        Console.WriteLine();
    }

    static unsafe Task DemoSse2Operations()
    {
        Console.WriteLine("🔧 SSE2 Intrinsics:");
        
        // Criar vetores SSE
        var data1 = stackalloc int[4] { 10, 20, 30, 40 };
        var data2 = stackalloc int[4] { 1, 2, 3, 4 };
        
        var vector1 = Sse2.LoadVector128(data1);
        var vector2 = Sse2.LoadVector128(data2);
        
        // Operações SSE2
        var sum = Sse2.Add(vector1, vector2);
        var sub = Sse2.Subtract(vector1, vector2);
        
        // Extrair resultados
        var sumResult = stackalloc int[4];
        var subResult = stackalloc int[4];
        
        Sse2.Store(sumResult, sum);
        Sse2.Store(subResult, sub);
        
        Console.WriteLine("Input 1: [10, 20, 30, 40]");
        Console.WriteLine("Input 2: [1, 2, 3, 4]");
        Console.WriteLine($"Soma:       [{sumResult[0]}, {sumResult[1]}, {sumResult[2]}, {sumResult[3]}]");
        Console.WriteLine($"Subtração:  [{subResult[0]}, {subResult[1]}, {subResult[2]}, {subResult[3]}]");
        
        return Task.CompletedTask;
    }

    static unsafe Task DemoAvx2Operations()
    {
        Console.WriteLine("\n🚀 AVX2 Intrinsics (256-bit):");
        
        // Criar vetores AVX2 (8 elementos de 32-bit)
        var data1 = stackalloc int[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var data2 = stackalloc int[8] { 8, 7, 6, 5, 4, 3, 2, 1 };
        
        var vector1 = Avx2.LoadVector256(data1);
        var vector2 = Avx2.LoadVector256(data2);
        
        // Operações AVX2
        var sum = Avx2.Add(vector1, vector2);
        
        var sumResult = stackalloc int[8];
        Avx2.Store(sumResult, sum);
        
        Console.WriteLine("Input 1: [1, 2, 3, 4, 5, 6, 7, 8]");
        Console.WriteLine("Input 2: [8, 7, 6, 5, 4, 3, 2, 1]");
        Console.WriteLine($"Soma:    [{sumResult[0]}, {sumResult[1]}, {sumResult[2]}, {sumResult[3]}, {sumResult[4]}, {sumResult[5]}, {sumResult[6]}, {sumResult[7]}]");
        
        return Task.CompletedTask;
    }

    static async Task DemoComplexMath()
    {
        Console.WriteLine("\n🧮 Matemática Complexa com SIMD:");
        
        // Calculando raízes quadradas em paralelo
        var numbers = new float[] { 1f, 4f, 9f, 16f, 25f, 36f, 49f, 64f };
        var results = new float[numbers.Length];
        
        var vectorCount = Vector<float>.Count;
        for (int i = 0; i <= numbers.Length - vectorCount; i += vectorCount)
        {
            var vector = new Vector<float>(numbers, i);
            var sqrt = Vector.SquareRoot(vector);
            sqrt.CopyTo(results, i);
        }
        
        Console.WriteLine("Raízes quadradas:");
        for (int i = 0; i < Math.Min(vectorCount, numbers.Length); i++)
        {
            Console.WriteLine($"√{numbers[i]} = {results[i]}");
        }
        
        // Operações trigonométricas (aproximação)
        await DemoTrigonometry();
    }

    static async Task DemoTrigonometry()
    {
        Console.WriteLine("\n📐 Trigonometria Aproximada:");
        
        var angles = new float[] { 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.14159f };
        var sines = new float[angles.Length];
        
        // Aproximação de seno usando série de Taylor (SIMD)
        var vectorCount = Vector<float>.Count;
        for (int i = 0; i <= angles.Length - vectorCount; i += vectorCount)
        {
            var x = new Vector<float>(angles, i);
            
            // sin(x) ≈ x - x³/6 + x⁵/120 (aproximação de Taylor)
            var x2 = x * x;
            var x3 = x2 * x;
            var x5 = x3 * x2;
            
            var term1 = x;
            var term2 = x3 / new Vector<float>(6f);
            var term3 = x5 / new Vector<float>(120f);
            
            var sine = term1 - term2 + term3;
            sine.CopyTo(sines, i);
        }
        
        Console.WriteLine("Aproximação de seno:");
        for (int i = 0; i < Math.Min(vectorCount, angles.Length); i++)
        {
            var actual = Math.Sin(angles[i]);
            Console.WriteLine($"sin({angles[i]:F2}) ≈ {sines[i]:F4} (real: {actual:F4})");
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Comparações de performance detalhadas
    /// </summary>
    static async Task RunPerformanceComparisons()
    {
        Console.WriteLine("📊 Comparações de Performance");
        Console.WriteLine("=============================");
        
        await CompareMatrixMultiplication();
        await CompareImageProcessing();
        await CompareStringOperations();
        
        Console.WriteLine();
    }

    static async Task CompareMatrixMultiplication()
    {
        Console.WriteLine("🔢 Multiplicação de Matrizes:");
        
        const int size = 256;
        var matrix1 = CreateRandomMatrix(size);
        var matrix2 = CreateRandomMatrix(size);
        var resultScalar = new float[size, size];
        var resultSimd = new float[size, size];
        
        // Multiplicação scalar
        var sw = Stopwatch.StartNew();
        MultiplyMatricesScalar(matrix1, matrix2, resultScalar, size);
        sw.Stop();
        var scalarTime = sw.Elapsed;
        
        // Multiplicação SIMD
        sw.Restart();
        MultiplyMatricesSimd(matrix1, matrix2, resultSimd, size);
        sw.Stop();
        var simdTime = sw.Elapsed;
        
        Console.WriteLine($"Scalar: {scalarTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"SIMD:   {simdTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Speedup: {scalarTime.TotalMilliseconds / simdTime.TotalMilliseconds:F2}x");
        
        await Task.CompletedTask;
    }

    static float[,] CreateRandomMatrix(int size)
    {
        var matrix = new float[size, size];
        var random = new Random(42);
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                matrix[i, j] = (float)random.NextDouble();
            }
        }
        
        return matrix;
    }

    static void MultiplyMatricesScalar(float[,] a, float[,] b, float[,] result, int size)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float sum = 0;
                for (int k = 0; k < size; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
    }

    static unsafe void MultiplyMatricesSimd(float[,] a, float[,] b, float[,] result, int size)
    {
        var vectorSize = Vector<float>.Count;
        var bVals = stackalloc float[vectorSize]; // Movido para fora do loop
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j += vectorSize)
            {
                var sum = Vector<float>.Zero;
                
                for (int k = 0; k < size; k++)
                {
                    var aVal = new Vector<float>(a[i, k]);
                    
                    // Carregar valores de b[k, j..j+vectorSize]
                    for (int v = 0; v < vectorSize && j + v < size; v++)
                    {
                        bVals[v] = b[k, j + v];
                    }
                    var bVector = new Vector<float>(new ReadOnlySpan<float>(bVals, vectorSize));
                    
                    sum += aVal * bVector;
                }
                
                // Armazenar resultado
                var sumArray = new float[vectorSize];
                sum.CopyTo(sumArray);
                for (int v = 0; v < vectorSize && j + v < size; v++)
                {
                    result[i, j + v] = sumArray[v];
                }
            }
        }
    }

    static async Task CompareImageProcessing()
    {
        Console.WriteLine("\n🖼️ Processamento de Imagem (Blur):");
        
        const int width = 512;
        const int height = 512;
        var image = CreateRandomImage(width, height);
        var blurredScalar = new byte[width * height * 3];
        var blurredSimd = new byte[width * height * 3];
        
        // Blur scalar
        var sw = Stopwatch.StartNew();
        ApplyBlurScalar(image, blurredScalar, width, height);
        sw.Stop();
        var scalarTime = sw.Elapsed;
        
        // Blur SIMD
        sw.Restart();
        ApplyBlurSimd(image, blurredSimd, width, height);
        sw.Stop();
        var simdTime = sw.Elapsed;
        
        Console.WriteLine($"Scalar: {scalarTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"SIMD:   {simdTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Speedup: {scalarTime.TotalMilliseconds / simdTime.TotalMilliseconds:F2}x");
        
        await Task.CompletedTask;
    }

    static byte[] CreateRandomImage(int width, int height)
    {
        var image = new byte[width * height * 3]; // RGB
        var random = new Random(42);
        random.NextBytes(image);
        return image;
    }

    static void ApplyBlurScalar(byte[] source, byte[] dest, int width, int height)
    {
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int c = 0; c < 3; c++) // RGB
                {
                    int index = (y * width + x) * 3 + c;
                    int sum = 0;
                    
                    // Kernel 3x3
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int srcIndex = ((y + dy) * width + (x + dx)) * 3 + c;
                            sum += source[srcIndex];
                        }
                    }
                    
                    dest[index] = (byte)(sum / 9);
                }
            }
        }
    }

    static void ApplyBlurSimd(byte[] source, byte[] dest, int width, int height)
    {
        var vectorSize = Vector<byte>.Count;
        var divisor = new Vector<ushort>(9);
        
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x += vectorSize / 3)
            {
                var sums = Vector<ushort>.Zero;
                
                // Kernel 3x3
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int baseIndex = ((y + dy) * width + (x + dx)) * 3;
                        if (baseIndex + vectorSize <= source.Length)
                        {
                            var pixels = new Vector<byte>(source, baseIndex);
                            Vector.Widen(pixels, out var low, out var high);
                            sums += low;
                        }
                    }
                }
                
                var result = sums / divisor;
                
                // Converter de volta para bytes (simplificado)
                var resultBytes = new byte[Vector<ushort>.Count];
                for (int i = 0; i < Vector<ushort>.Count; i++)
                {
                    resultBytes[i] = (byte)Math.Min(255, Math.Max(0, (int)result[i]));
                }
                
                int destIndex = (y * width + x) * 3;
                if (destIndex + resultBytes.Length <= dest.Length)
                {
                    Array.Copy(resultBytes, 0, dest, destIndex, Math.Min(resultBytes.Length, dest.Length - destIndex));
                }
            }
        }
    }

    static async Task CompareStringOperations()
    {
        Console.WriteLine("\n📝 Operações com Strings (Case conversion):");
        
        var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. ".Repeat(100);
        var chars = text.ToCharArray();
        var resultScalar = new char[chars.Length];
        var resultSimd = new char[chars.Length];
        
        // Conversão scalar
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < chars.Length; i++)
        {
            resultScalar[i] = char.ToUpper(chars[i]);
        }
        sw.Stop();
        var scalarTime = sw.Elapsed;
        
        // Conversão SIMD (simplificada para ASCII)
        sw.Restart();
        var vectorSize = Vector<ushort>.Count;
        var lowerA = new Vector<ushort>((ushort)'a');
        var lowerZ = new Vector<ushort>((ushort)'z');
        var diff = new Vector<ushort>((ushort)('a' - 'A'));
        
        for (int i = 0; i <= chars.Length - vectorSize; i += vectorSize)
        {
            var charCodes = new ushort[vectorSize];
            for (int j = 0; j < vectorSize && i + j < chars.Length; j++)
            {
                charCodes[j] = (ushort)chars[i + j];
            }
            
            var vector = new Vector<ushort>(charCodes);
            var isLower = Vector.BitwiseAnd(
                Vector.GreaterThanOrEqual(vector, lowerA),
                Vector.LessThanOrEqual(vector, lowerZ));
            
            var adjusted = Vector.ConditionalSelect(isLower, vector - diff, vector);
            
            adjusted.CopyTo(charCodes);
            for (int j = 0; j < vectorSize && i + j < chars.Length; j++)
            {
                resultSimd[i + j] = (char)charCodes[j];
            }
        }
        
        // Processar elementos restantes
        for (int i = chars.Length - (chars.Length % vectorSize); i < chars.Length; i++)
        {
            resultSimd[i] = char.ToUpper(chars[i]);
        }
        
        sw.Stop();
        var simdTime = sw.Elapsed;
        
        Console.WriteLine($"Scalar: {scalarTime.TotalMicroseconds:F2} μs");
        Console.WriteLine($"SIMD:   {simdTime.TotalMicroseconds:F2} μs");
        Console.WriteLine($"Speedup: {scalarTime.TotalMicroseconds / simdTime.TotalMicroseconds:F2}x");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Casos de uso práticos do SIMD
    /// </summary>
    static async Task RunPracticalExamples()
    {
        Console.WriteLine("🎯 Casos de Uso Práticos");
        Console.WriteLine("========================");
        
        await DemoCryptography();
        await DemoAudioProcessing();
        await DemoDataAnalysis();
        await ShowBestPractices();
        
        Console.WriteLine();
    }

    static async Task DemoCryptography()
    {
        Console.WriteLine("🔐 Criptografia - XOR Cipher:");
        
        var data = "Esta é uma mensagem secreta que será criptografada!"u8.ToArray();
        var key = "chave123"u8.ToArray();
        var encrypted = new byte[data.Length];
        var decrypted = new byte[data.Length];
        
        // Expansão da chave para tamanho dos dados
        var expandedKey = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            expandedKey[i] = key[i % key.Length];
        }
        
        // Criptografia SIMD
        var vectorSize = Vector<byte>.Count;
        for (int i = 0; i <= data.Length - vectorSize; i += vectorSize)
        {
            var dataVector = new Vector<byte>(data, i);
            var keyVector = new Vector<byte>(expandedKey, i);
            var result = dataVector ^ keyVector;
            result.CopyTo(encrypted, i);
        }
        
        // Descriptografia SIMD
        for (int i = 0; i <= data.Length - vectorSize; i += vectorSize)
        {
            var encryptedVector = new Vector<byte>(encrypted, i);
            var keyVector = new Vector<byte>(expandedKey, i);
            var result = encryptedVector ^ keyVector;
            result.CopyTo(decrypted, i);
        }
        
        // Processar bytes restantes
        for (int i = data.Length - (data.Length % vectorSize); i < data.Length; i++)
        {
            encrypted[i] = (byte)(data[i] ^ expandedKey[i]);
            decrypted[i] = (byte)(encrypted[i] ^ expandedKey[i]);
        }
        
        Console.WriteLine($"Original:      {System.Text.Encoding.UTF8.GetString(data)}");
        Console.WriteLine($"Criptografado: {Convert.ToHexString(encrypted)[..32]}...");
        Console.WriteLine($"Descriptografado: {System.Text.Encoding.UTF8.GetString(decrypted)}");
        
        await Task.CompletedTask;
    }

    static async Task DemoAudioProcessing()
    {
        Console.WriteLine("\n🎵 Processamento de Áudio - Echo:");
        
        const int sampleRate = 44100;
        const int duration = 2; // segundos
        var samples = new float[sampleRate * duration];
        var withEcho = new float[samples.Length];
        
        // Gerar onda senoidal
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2.0 * Math.PI * 440.0 * i / sampleRate) * 0.5f;
        }
        
        // Aplicar echo com SIMD
        const int echoDelay = sampleRate / 4; // 0.25 segundos
        const float echoDecay = 0.3f;
        var decay = new Vector<float>(echoDecay);
        var vectorSize = Vector<float>.Count;
        
        var sw = Stopwatch.StartNew();
        
        // Copiar sinal original
        for (int i = 0; i <= samples.Length - vectorSize; i += vectorSize)
        {
            var original = new Vector<float>(samples, i);
            original.CopyTo(withEcho, i);
        }
        
        // Adicionar echo
        for (int i = echoDelay; i <= samples.Length - vectorSize; i += vectorSize)
        {
            var current = new Vector<float>(withEcho, i);
            var delayed = new Vector<float>(samples, i - echoDelay);
            var echo = delayed * decay;
            var result = current + echo;
            result.CopyTo(withEcho, i);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Processado {samples.Length} samples em {sw.Elapsed.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Taxa: {samples.Length / sw.Elapsed.TotalSeconds / 1000000:F2} MSamples/s");
        
        await Task.CompletedTask;
    }

    static async Task DemoDataAnalysis()
    {
        Console.WriteLine("\n📈 Análise de Dados - Estatísticas:");
        
        const int dataSize = 100000;
        var data = new float[dataSize];
        var random = new Random(42);
        
        // Gerar dados aleatórios
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (float)(random.NextGaussian() * 10 + 50); // Distribuição normal
        }
        
        var sw = Stopwatch.StartNew();
        
        // Calcular estatísticas com SIMD
        var sum = Vector<float>.Zero;
        var sumSquares = Vector<float>.Zero;
        var min = new Vector<float>(float.MaxValue);
        var max = new Vector<float>(float.MinValue);
        var vectorSize = Vector<float>.Count;
        
        for (int i = 0; i <= dataSize - vectorSize; i += vectorSize)
        {
            var vector = new Vector<float>(data, i);
            sum += vector;
            sumSquares += vector * vector;
            min = Vector.Min(min, vector);
            max = Vector.Max(max, vector);
        }
        
        // Reduzir vetores para escalares
        var totalSum = Vector.Dot(sum, Vector<float>.One);
        var totalSumSquares = Vector.Dot(sumSquares, Vector<float>.One);
        
        var minVal = float.MaxValue;
        var maxVal = float.MinValue;
        for (int i = 0; i < vectorSize; i++)
        {
            minVal = Math.Min(minVal, min[i]);
            maxVal = Math.Max(maxVal, maxVal);
        }
        
        // Processar elementos restantes
        for (int i = dataSize - (dataSize % vectorSize); i < dataSize; i++)
        {
            totalSum += data[i];
            totalSumSquares += data[i] * data[i];
            minVal = Math.Min(minVal, data[i]);
            maxVal = Math.Max(maxVal, data[i]);
        }
        
        var mean = totalSum / dataSize;
        var variance = (totalSumSquares / dataSize) - (mean * mean);
        var stdDev = (float)Math.Sqrt(variance);
        
        sw.Stop();
        
        Console.WriteLine($"Dados processados: {dataSize:N0} elementos");
        Console.WriteLine($"Tempo: {sw.Elapsed.TotalMicroseconds:F2} μs");
        Console.WriteLine($"Média: {mean:F2}");
        Console.WriteLine($"Desvio padrão: {stdDev:F2}");
        Console.WriteLine($"Min: {minVal:F2}, Max: {maxVal:F2}");
        Console.WriteLine($"Taxa: {dataSize / sw.Elapsed.TotalSeconds / 1000000:F2} Melements/s");
        
        await Task.CompletedTask;
    }

    static async Task ShowBestPractices()
    {
        Console.WriteLine("\n💡 Melhores Práticas SIMD");
        Console.WriteLine("=========================");
        
        Console.WriteLine("🎯 Quando usar SIMD:");
        Console.WriteLine("• ✅ Operações em arrays grandes (>1000 elementos)");
        Console.WriteLine("• ✅ Operações matemáticas simples (add, mul, etc.)");
        Console.WriteLine("• ✅ Processamento de imagem/áudio");
        Console.WriteLine("• ✅ Operações de criptografia simples");
        Console.WriteLine("• ✅ Análise estatística de dados");
        Console.WriteLine();
        
        Console.WriteLine("⚠️ Quando NÃO usar SIMD:");
        Console.WriteLine("• ❌ Arrays pequenos (<100 elementos)");
        Console.WriteLine("• ❌ Operações com muitas condicionais");
        Console.WriteLine("• ❌ Operações que requerem acesso aleatório");
        Console.WriteLine("• ❌ Algoritmos com dependências de dados");
        Console.WriteLine();
        
        Console.WriteLine("🔧 Dicas de otimização:");
        Console.WriteLine("• 🚀 Alinhe dados em boundaries de 16/32 bytes");
        Console.WriteLine("• 🚀 Use stackalloc para arrays temporários pequenos");
        Console.WriteLine("• 🚀 Processe elementos restantes após loops SIMD");
        Console.WriteLine("• 🚀 Prefira Vector<T> para portabilidade");
        Console.WriteLine("• 🚀 Use intrinsics específicos apenas quando necessário");
        Console.WriteLine();
        
        Console.WriteLine("📏 Tamanhos de vetores típicos:");
        Console.WriteLine($"• SSE: 128-bit ({128/32} floats, {128/32} ints)");
        Console.WriteLine($"• AVX: 256-bit ({256/32} floats, {256/32} ints)");
        Console.WriteLine($"• AVX-512: 512-bit ({512/32} floats, {512/32} ints)");
        Console.WriteLine($"• ARM NEON: 128-bit ({128/32} floats, {128/32} ints)");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Benchmarks detalhados para SIMD
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class SimdBenchmarks
{
    private float[] _data1 = null!;
    private float[] _data2 = null!;
    private float[] _result = null!;
    private const int Size = 10000;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _data1 = new float[Size];
        _data2 = new float[Size];
        _result = new float[Size];
        
        for (int i = 0; i < Size; i++)
        {
            _data1[i] = (float)random.NextDouble() * 100;
            _data2[i] = (float)random.NextDouble() * 100;
        }
    }

    [Benchmark(Baseline = true)]
    public void ScalarMultiplyAdd()
    {
        for (int i = 0; i < Size; i++)
        {
            _result[i] = _data1[i] * _data2[i] + 1.0f;
        }
    }

    [Benchmark]
    public void SimdMultiplyAdd()
    {
        var vectorSize = Vector<float>.Count;
        var one = new Vector<float>(1.0f);
        
        for (int i = 0; i <= Size - vectorSize; i += vectorSize)
        {
            var v1 = new Vector<float>(_data1, i);
            var v2 = new Vector<float>(_data2, i);
            var result = v1 * v2 + one;
            result.CopyTo(_result, i);
        }
        
        // Elementos restantes
        for (int i = Size - (Size % vectorSize); i < Size; i++)
        {
            _result[i] = _data1[i] * _data2[i] + 1.0f;
        }
    }

    [Benchmark]
    public void ScalarSum()
    {
        float sum = 0;
        for (int i = 0; i < Size; i++)
        {
            sum += _data1[i];
        }
        _result[0] = sum;
    }

    [Benchmark]
    public void SimdSum()
    {
        var vectorSize = Vector<float>.Count;
        var sum = Vector<float>.Zero;
        
        for (int i = 0; i <= Size - vectorSize; i += vectorSize)
        {
            var vector = new Vector<float>(_data1, i);
            sum += vector;
        }
        
        var total = Vector.Dot(sum, Vector<float>.One);
        
        // Elementos restantes
        for (int i = Size - (Size % vectorSize); i < Size; i++)
        {
            total += _data1[i];
        }
        
        _result[0] = total;
    }

    [Benchmark]
    public void ScalarSquareRoot()
    {
        for (int i = 0; i < Size; i++)
        {
            _result[i] = (float)Math.Sqrt(_data1[i]);
        }
    }

    [Benchmark]
    public void SimdSquareRoot()
    {
        var vectorSize = Vector<float>.Count;
        
        for (int i = 0; i <= Size - vectorSize; i += vectorSize)
        {
            var vector = new Vector<float>(_data1, i);
            var sqrt = Vector.SquareRoot(vector);
            sqrt.CopyTo(_result, i);
        }
        
        // Elementos restantes
        for (int i = Size - (Size % vectorSize); i < Size; i++)
        {
            _result[i] = (float)Math.Sqrt(_data1[i]);
        }
    }
}

/// <summary>
/// Extensões úteis para operações SIMD
/// </summary>
public static class SimdExtensions
{
    /// <summary>
    /// Repete uma string n vezes
    /// </summary>
    public static string Repeat(this string str, int count)
    {
        return string.Concat(Enumerable.Repeat(str, count));
    }
    
    /// <summary>
    /// Gera número aleatório com distribuição normal (Box-Muller)
    /// </summary>
    public static double NextGaussian(this Random random)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }
}
