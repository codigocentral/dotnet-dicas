using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Dica69.UnsafeCodeFixedBuffers;

/// <summary>
/// Dica 69: Unsafe Code & Fixed Buffers
/// 
/// Demonstra o uso de código unsafe e fixed buffers em C#, técnicas avançadas
/// para performance extrema quando necessário trabalhar diretamente com memória.
/// 
/// ⚠️ ATENÇÃO: Código unsafe deve ser usado com extrema cautela e apenas
/// quando a performance é crítica e os benefícios superam os riscos.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Dica 69: Unsafe Code & Fixed Buffers ===\n");

        if (args.Length > 0 && args[0] == "benchmark")
        {
            Console.WriteLine("🚀 Executando benchmarks de performance...\n");
            BenchmarkRunner.Run<UnsafePerformanceBenchmarks>();
            return;
        }

        // Demonstrações básicas
        DemonstrateBasicUnsafeCode();
        DemonstrateFixedBuffers();
        DemonstratePointerArithmetic();
        DemonstrateUnsafeStructs();
        DemonstrateMemoryManipulation();
        DemonstratePracticalExamples();
        ShowSafetyConsiderations();
    }

    /// <summary>
    /// Demonstra conceitos básicos de código unsafe
    /// </summary>
    static void DemonstrateBasicUnsafeCode()
    {
        Console.WriteLine("🔓 Código Unsafe Básico");
        Console.WriteLine("=======================");

        // Exemplo básico com ponteiros
        unsafe
        {
            int value = 42;
            int* ptr = &value;
            
            Console.WriteLine($"Valor: {value}");
            Console.WriteLine($"Endereço: 0x{(long)ptr:X}");
            Console.WriteLine($"Valor via ponteiro: {*ptr}");
            
            // Modificando via ponteiro
            *ptr = 100;
            Console.WriteLine($"Novo valor: {value}");
        }

        // Usando stackalloc
        Console.WriteLine("\n📚 Usando stackalloc:");
        unsafe
        {
            int* numbers = stackalloc int[5] { 1, 2, 3, 4, 5 };
            
            Console.Write("Array na stack: ");
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{numbers[i]} ");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra o uso de fixed buffers
    /// </summary>
    static void DemonstrateFixedBuffers()
    {
        Console.WriteLine("📌 Fixed Buffers");
        Console.WriteLine("================");

        var header = new ProtocolHeader();
        
        unsafe
        {
            // Preenchendo o buffer fixo
            for (int i = 0; i < ProtocolHeader.MAGIC_SIZE; i++)
            {
                header.Magic[i] = (byte)('A' + i);
            }
            
            header.Version = 0x0102;
            header.Length = 1024;
            
            Console.WriteLine("Header criado:");
            Console.Write("Magic: ");
            for (int i = 0; i < ProtocolHeader.MAGIC_SIZE; i++)
            {
                Console.Write($"{(char)header.Magic[i]}");
            }
            Console.WriteLine($"\nVersion: 0x{header.Version:X4}");
            Console.WriteLine($"Length: {header.Length}");
        }

        // Demonstrando tamanho e layout
        unsafe
        {
            Console.WriteLine($"\nTamanho da struct: {sizeof(ProtocolHeader)} bytes");
            Console.WriteLine($"Layout: Magic(8) + Version(2) + Length(4) = 14 bytes");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra aritmética de ponteiros
    /// </summary>
    static void DemonstratePointerArithmetic()
    {
        Console.WriteLine("🧮 Aritmética de Ponteiros");
        Console.WriteLine("==========================");

        unsafe
        {
            // Array de inteiros
            int[] array = { 10, 20, 30, 40, 50 };
            
            fixed (int* ptr = array)
            {
                Console.WriteLine("Percorrendo array com ponteiros:");
                
                // Método tradicional
                for (int i = 0; i < array.Length; i++)
                {
                    Console.WriteLine($"ptr[{i}] = {ptr[i]} (endereço: 0x{(long)(ptr + i):X})");
                }
                
                Console.WriteLine("\nUsando aritmética de ponteiros:");
                int* current = ptr;
                int* end = ptr + array.Length;
                
                int index = 0;
                while (current < end)
                {
                    Console.WriteLine($"*current = {*current} (índice: {index})");
                    current++;
                    index++;
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra structs com layout específico
    /// </summary>
    static void DemonstrateUnsafeStructs()
    {
        Console.WriteLine("🏗️ Structs Unsafe");
        Console.WriteLine("=================");

        var packet = new NetworkPacket
        {
            Type = PacketType.Data,
            Size = 256,
            Checksum = 0xDEADBEEF
        };

        // Convertendo struct para bytes
        unsafe
        {
            byte* ptr = (byte*)&packet;
            Console.WriteLine("Representação em bytes:");
            
            for (int i = 0; i < sizeof(NetworkPacket); i++)
            {
                Console.Write($"{ptr[i]:X2} ");
                if ((i + 1) % 8 == 0) Console.WriteLine();
            }
            Console.WriteLine();
        }

        // Demonstrando união (union)
        var union = new IntFloatUnion();
        union.IntValue = 0x42280000;
        
        Console.WriteLine($"Como int: {union.IntValue}");
        Console.WriteLine($"Como float: {union.FloatValue}");
        Console.WriteLine($"Representação hex: 0x{union.IntValue:X8}");

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra manipulação direta de memória
    /// </summary>
    static void DemonstrateMemoryManipulation()
    {
        Console.WriteLine("🧠 Manipulação de Memória");
        Console.WriteLine("=========================");

        // Copiando memória eficientemente
        DemonstrateMemoryCopy();
        
        // Comparando memória
        DemonstrateMemoryCompare();
        
        // Zerando memória
        DemonstrateMemoryZero();

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra cópia eficiente de memória
    /// </summary>
    static void DemonstrateMemoryCopy()
    {
        Console.WriteLine("📋 Cópia de Memória:");

        int[] source = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int[] destination = new int[10];

        unsafe
        {
            fixed (int* srcPtr = source, dstPtr = destination)
            {
                // Cópia usando Unsafe.CopyBlock
                Unsafe.CopyBlock(dstPtr, srcPtr, (uint)(source.Length * sizeof(int)));
            }
        }

        Console.Write("Array copiado: ");
        Console.WriteLine(string.Join(", ", destination));
    }

    /// <summary>
    /// Demonstra comparação de memória
    /// </summary>
    static void DemonstrateMemoryCompare()
    {
        Console.WriteLine("\n🔍 Comparação de Memória:");

        byte[] data1 = { 1, 2, 3, 4, 5 };
        byte[] data2 = { 1, 2, 3, 4, 5 };
        byte[] data3 = { 1, 2, 3, 4, 6 };

        unsafe
        {
            fixed (byte* ptr1 = data1, ptr2 = data2, ptr3 = data3)
            {
                bool equal12 = MemoryCompare(ptr1, ptr2, data1.Length);
                bool equal13 = MemoryCompare(ptr1, ptr3, data1.Length);
                
                Console.WriteLine($"data1 == data2: {equal12}");
                Console.WriteLine($"data1 == data3: {equal13}");
            }
        }
    }

    /// <summary>
    /// Demonstra zeragem de memória
    /// </summary>
    static void DemonstrateMemoryZero()
    {
        Console.WriteLine("\n🗑️ Zerando Memória:");

        int[] array = { 1, 2, 3, 4, 5 };
        Console.WriteLine($"Antes: [{string.Join(", ", array)}]");

        unsafe
        {
            fixed (int* ptr = array)
            {
                Unsafe.InitBlock(ptr, 0, (uint)(array.Length * sizeof(int)));
            }
        }

        Console.WriteLine($"Depois: [{string.Join(", ", array)}]");
    }

    /// <summary>
    /// Compara dois blocos de memória
    /// </summary>
    static unsafe bool MemoryCompare(byte* ptr1, byte* ptr2, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (ptr1[i] != ptr2[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Demonstra exemplos práticos de uso
    /// </summary>
    static void DemonstratePracticalExamples()
    {
        Console.WriteLine("💼 Exemplos Práticos");
        Console.WriteLine("====================");

        // Parser binário rápido
        DemonstrateBinaryParser();
        
        // Processamento de strings
        DemonstrateStringProcessing();
        
        // Interop com C/C++
        DemonstrateInterop();

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra parser binário eficiente
    /// </summary>
    static void DemonstrateBinaryParser()
    {
        Console.WriteLine("📊 Parser Binário:");

        // Simula dados binários de um protocolo
        byte[] binaryData = {
            0x42, 0x00, // Magic number
            0x01, 0x02, // Version
            0x00, 0x00, 0x01, 0x00, // Length (256)
            0x48, 0x65, 0x6C, 0x6C, 0x6F // "Hello"
        };

        var message = BinaryMessageParser.Parse(binaryData);
        Console.WriteLine($"Magic: 0x{message.Magic:X4}");
        Console.WriteLine($"Version: {message.Version}");
        Console.WriteLine($"Length: {message.Length}");
        Console.WriteLine($"Data: {message.Data}");
    }

    /// <summary>
    /// Demonstra processamento eficiente de strings
    /// </summary>
    static void DemonstrateStringProcessing()
    {
        Console.WriteLine("\n🔤 Processamento de String:");

        string text = "Hello, World!";
        var processor = new UnsafeStringProcessor();
        
        int vowels = processor.CountVowels(text);
        string reversed = processor.ReverseString(text);
        
        Console.WriteLine($"Texto: {text}");
        Console.WriteLine($"Vogais: {vowels}");
        Console.WriteLine($"Reverso: {reversed}");
    }

    /// <summary>
    /// Demonstra interoperabilidade com código nativo
    /// </summary>
    static unsafe void DemonstrateInterop()
    {
        Console.WriteLine("\n🔗 Interop com C:");

        // Simulando uma struct C
        var cStruct = new CInteropStruct
        {
            IntField = 42,
            FloatField = 3.14f
        };

        // Preenchendo array de char usando acesso direto ao fixed buffer
        string text = "Hello C!";
        for (int i = 0; i < Math.Min(text.Length, 15); i++)
        {
            cStruct.CharArray[i] = text[i];
        }
        if (text.Length < 16)
        {
            cStruct.CharArray[text.Length] = '\0'; // Null terminator
        }

        Console.WriteLine($"Int: {cStruct.IntField}");
        Console.WriteLine($"Float: {cStruct.FloatField}");
        
        // Para ler a string, criamos um novo string a partir dos chars
        var chars = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            chars[i] = cStruct.CharArray[i];
        }
        Console.WriteLine($"String: {new string(chars)}");
        Console.WriteLine($"Tamanho: {Marshal.SizeOf<CInteropStruct>()} bytes");
    }

    /// <summary>
    /// Mostra considerações de segurança
    /// </summary>
    static void ShowSafetyConsiderations()
    {
        Console.WriteLine("⚠️ Considerações de Segurança");
        Console.WriteLine("=============================");
        Console.WriteLine("1. 🚨 RISCOS do código unsafe:");
        Console.WriteLine("   • Buffer overflows");
        Console.WriteLine("   • Access violations");
        Console.WriteLine("   • Memory leaks");
        Console.WriteLine("   • Corruption de memória");
        Console.WriteLine("   • Problemas de concorrência");
        Console.WriteLine();
        Console.WriteLine("2. 🛡️ BOAS PRÁTICAS:");
        Console.WriteLine("   • Use apenas quando necessário");
        Console.WriteLine("   • Valide sempre os limites");
        Console.WriteLine("   • Teste extensively");
        Console.WriteLine("   • Documente bem o código");
        Console.WriteLine("   • Use ferramentas de análise");
        Console.WriteLine();
        Console.WriteLine("3. 🎯 QUANDO USAR:");
        Console.WriteLine("   • Interop com código nativo");
        Console.WriteLine("   • Performance crítica");
        Console.WriteLine("   • Processamento de grandes volumes");
        Console.WriteLine("   • Protocolos binários");
        Console.WriteLine("   • Algoritmos de baixo nível");
        Console.WriteLine();
        Console.WriteLine("4. 🚫 ALTERNATIVAS SEGURAS:");
        Console.WriteLine("   • Span<T> e Memory<T>");
        Console.WriteLine("   • System.Buffers");
        Console.WriteLine("   • MemoryMarshal");
        Console.WriteLine("   • ArrayPool<T>");
        Console.WriteLine();
    }
}

/// <summary>
/// Struct com fixed buffer para protocolo de rede
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ProtocolHeader
{
    public const int MAGIC_SIZE = 8;
    
    public fixed byte Magic[MAGIC_SIZE];
    public ushort Version;
    public uint Length;
}

/// <summary>
/// Enum para tipos de pacote
/// </summary>
public enum PacketType : byte
{
    Data = 1,
    Ack = 2,
    Nack = 3,
    Heartbeat = 4
}

/// <summary>
/// Struct para pacote de rede com layout específico
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NetworkPacket
{
    public PacketType Type;
    public ushort Size;
    public uint Checksum;
}

/// <summary>
/// União (union) simulada em C#
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct IntFloatUnion
{
    [FieldOffset(0)]
    public int IntValue;
    
    [FieldOffset(0)]
    public float FloatValue;
}

/// <summary>
/// Struct para interop com C
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct CInteropStruct
{
    public int IntField;
    public float FloatField;
    public fixed char CharArray[16];
}

/// <summary>
/// Parser de mensagens binárias usando código unsafe
/// </summary>
public static class BinaryMessageParser
{
    public struct BinaryMessage
    {
        public ushort Magic;
        public ushort Version;
        public uint Length;
        public string Data;
    }

    public static unsafe BinaryMessage Parse(byte[] data)
    {
        if (data.Length < 8)
            throw new ArgumentException("Dados insuficientes");

        fixed (byte* ptr = data)
        {
            var message = new BinaryMessage
            {
                Magic = *(ushort*)ptr,
                Version = *(ushort*)(ptr + 2),
                Length = *(uint*)(ptr + 4)
            };

            // Lê string se houver dados suficientes
            if (data.Length > 8)
            {
                int textLength = Math.Min((int)(data.Length - 8), (int)message.Length);
                message.Data = Encoding.ASCII.GetString(ptr + 8, textLength);
            }
            else
            {
                message.Data = string.Empty;
            }

            return message;
        }
    }
}

/// <summary>
/// Processador de strings usando código unsafe para performance
/// </summary>
public class UnsafeStringProcessor
{
    public unsafe int CountVowels(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int count = 0;
        fixed (char* ptr = text)
        {
            char* current = ptr;
            char* end = ptr + text.Length;

            while (current < end)
            {
                char c = char.ToLower(*current);
                if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
                {
                    count++;
                }
                current++;
            }
        }

        return count;
    }

    public unsafe string ReverseString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        char[] reversed = new char[text.Length];
        
        fixed (char* srcPtr = text, dstPtr = reversed)
        {
            char* src = srcPtr + text.Length - 1;
            char* dst = dstPtr;
            
            for (int i = 0; i < text.Length; i++)
            {
                *dst = *src;
                dst++;
                src--;
            }
        }

        return new string(reversed);
    }
}

/// <summary>
/// Benchmarks para comparar performance entre código safe e unsafe
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class UnsafePerformanceBenchmarks
{
    private readonly int[] _data;
    private readonly byte[] _bytes;
    private readonly string _text;

    public UnsafePerformanceBenchmarks()
    {
        _data = Enumerable.Range(1, 10000).ToArray();
        _bytes = Enumerable.Range(0, 10000).Select(x => (byte)(x % 256)).ToArray();
        _text = new string('A', 10000);
    }

    [Benchmark]
    public long SumArray_Safe()
    {
        long sum = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            sum += _data[i];
        }
        return sum;
    }

    [Benchmark]
    public unsafe long SumArray_Unsafe()
    {
        long sum = 0;
        fixed (int* ptr = _data)
        {
            int* current = ptr;
            int* end = ptr + _data.Length;
            
            while (current < end)
            {
                sum += *current;
                current++;
            }
        }
        return sum;
    }

    [Benchmark]
    public void CopyArray_Safe()
    {
        var destination = new int[_data.Length];
        Array.Copy(_data, destination, _data.Length);
    }

    [Benchmark]
    public unsafe void CopyArray_Unsafe()
    {
        var destination = new int[_data.Length];
        fixed (int* srcPtr = _data, dstPtr = destination)
        {
            Unsafe.CopyBlock(dstPtr, srcPtr, (uint)(_data.Length * sizeof(int)));
        }
    }

    [Benchmark]
    public bool CompareBytes_Safe()
    {
        var other = _bytes.ToArray();
        return _bytes.SequenceEqual(other);
    }

    [Benchmark]
    public unsafe bool CompareBytes_Unsafe()
    {
        var other = _bytes.ToArray();
        fixed (byte* ptr1 = _bytes, ptr2 = other)
        {
            return Unsafe.AreSame(ref *ptr1, ref *ptr2) || 
                   memcmp(ptr1, ptr2, _bytes.Length) == 0;
        }
    }

    private static unsafe int memcmp(byte* ptr1, byte* ptr2, int length)
    {
        for (int i = 0; i < length; i++)
        {
            int diff = ptr1[i] - ptr2[i];
            if (diff != 0) return diff;
        }
        return 0;
    }

    [Benchmark]
    public int CountVowels_Safe()
    {
        return _text.Count(c => "aeiouAEIOU".Contains(c));
    }

    [Benchmark]
    public int CountVowels_Unsafe()
    {
        var processor = new UnsafeStringProcessor();
        return processor.CountVowels(_text);
    }
}

/// <summary>
/// Utilitários para trabalhar com código unsafe
/// </summary>
public static class UnsafeUtils
{
    /// <summary>
    /// Cria um Span<T> a partir de um ponteiro unsafe
    /// </summary>
    public static unsafe Span<T> CreateSpan<T>(T* ptr, int length) where T : unmanaged
    {
        return new Span<T>(ptr, length);
    }

    /// <summary>
    /// Obtém ponteiro de um Span<T>
    /// </summary>
    public static unsafe T* GetPointer<T>(Span<T> span) where T : unmanaged
    {
        return (T*)Unsafe.AsPointer(ref span.GetPinnableReference());
    }

    /// <summary>
    /// Converte struct para array de bytes
    /// </summary>
    public static unsafe byte[] StructToBytes<T>(T value) where T : unmanaged
    {
        int size = sizeof(T);
        byte[] bytes = new byte[size];
        
        fixed (byte* ptr = bytes)
        {
            *(T*)ptr = value;
        }
        
        return bytes;
    }

    /// <summary>
    /// Converte array de bytes para struct
    /// </summary>
    public static unsafe T BytesToStruct<T>(byte[] bytes) where T : unmanaged
    {
        if (bytes.Length < sizeof(T))
            throw new ArgumentException("Array muito pequeno");
            
        fixed (byte* ptr = bytes)
        {
            return *(T*)ptr;
        }
    }
}
