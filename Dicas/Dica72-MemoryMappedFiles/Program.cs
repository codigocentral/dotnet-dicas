using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Dica72.MemoryMappedFiles;

/// <summary>
/// Dica 72: Memory-Mapped Files
/// 
/// Demonstra o uso de Memory-Mapped Files (MMF) para:
/// - Acesso eficiente a grandes arquivos
/// - Compartilhamento de memória entre processos
/// - Comunicação inter-processo (IPC)
/// - Performance otimizada para I/O
/// 
/// MMF mapeia arquivos diretamente na memória virtual, permitindo
/// acesso como se fossem arrays na memória.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dica 72: Memory-Mapped Files ===\n");

        if (args.Length > 0 && args[0] == "benchmark")
        {
            Console.WriteLine("🚀 Executando benchmarks de performance...\n");
            BenchmarkRunner.Run<MemoryMappedFilesBenchmarks>();
            return;
        }

        if (args.Length > 0 && args[0] == "producer")
        {
            await RunProducer();
            return;
        }

        if (args.Length > 0 && args[0] == "consumer")
        {
            await RunConsumer();
            return;
        }

        // Demonstrações principais
        DemonstrateBasicUsage();
        await DemonstrateFileMapping();
        await DemonstrateLargeFileHandling();
        await DemonstrateSharedMemory();
        await DemonstrateInterProcessCommunication();
        ShowPerformanceComparison();
        ShowBestPractices();
    }

    /// <summary>
    /// Demonstra uso básico de Memory-Mapped Files
    /// </summary>
    static void DemonstrateBasicUsage()
    {
        Console.WriteLine("🗂️ Memory-Mapped Files - Uso Básico");
        Console.WriteLine("===================================");

        // Criando arquivo temporário para demonstração
        string tempFile = Path.GetTempFileName();
        string data = "Hello, Memory-Mapped Files! Este é um teste de mapeamento de memória.";

        try
        {
            // Escrever dados iniciais no arquivo
            File.WriteAllText(tempFile, data);
            var fileInfo = new FileInfo(tempFile);
            
            Console.WriteLine($"Arquivo criado: {fileInfo.Name}");
            Console.WriteLine($"Tamanho: {fileInfo.Length} bytes");
            Console.WriteLine($"Conteúdo original: {data}");

            // Mapeando arquivo na memória (sem nome no macOS/Linux)
            using var mmf = MemoryMappedFile.CreateFromFile(tempFile, 
                FileMode.Open, null, fileInfo.Length);
            
            using var accessor = mmf.CreateViewAccessor(0, fileInfo.Length);
            
            Console.WriteLine("\n📖 Lendo via Memory-Mapped File:");
            
            // Lendo dados byte por byte
            var buffer = new byte[fileInfo.Length];
            accessor.ReadArray(0, buffer, 0, (int)fileInfo.Length);
            string readData = Encoding.UTF8.GetString(buffer);
            
            Console.WriteLine($"Dados lidos: {readData}");
            
            // Modificando dados diretamente na memória
            Console.WriteLine("\n✏️ Modificando dados na memória...");
            string newData = "MODIFIED! Memory-Mapped Files são incríveis para performance!";
            byte[] newBytes = Encoding.UTF8.GetBytes(newData);
            
            if (newBytes.Length <= fileInfo.Length)
            {
                accessor.WriteArray(0, newBytes, 0, newBytes.Length);
                Console.WriteLine($"Novos dados: {newData}");
                
                // Verificando se a modificação foi persistida
                string fileContent = File.ReadAllText(tempFile);
                Console.WriteLine($"Arquivo atualizado: {fileContent.Substring(0, Math.Min(newData.Length, fileContent.Length))}");
            }
        }
        finally
        {
            File.Delete(tempFile);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra mapeamento de arquivos existentes
    /// </summary>
    static async Task DemonstrateFileMapping()
    {
        Console.WriteLine("📁 Mapeamento de Arquivos");
        Console.WriteLine("========================");

        string testFile = "test_data.txt";
        
        try
        {
            // Criando arquivo de teste com conteúdo estruturado
            await CreateTestFile(testFile);
            
            var fileInfo = new FileInfo(testFile);
            Console.WriteLine($"Arquivo de teste: {fileInfo.Length:N0} bytes");

            // Mapeando arquivo para leitura
            using var mmf = MemoryMappedFile.CreateFromFile(testFile, FileMode.Open);
            using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

            Console.WriteLine("\n🔍 Análise do arquivo via MMF:");
            
            // Lendo header (primeiros 100 bytes)
            var headerBytes = new byte[100];
            accessor.ReadArray(0, headerBytes, 0, 100);
            string header = Encoding.UTF8.GetString(headerBytes).Trim('\0');
            
            Console.WriteLine($"Header: {header}");
            
            // Contando linhas de forma eficiente
            long lineCount = CountLinesUsingMMF(accessor, fileInfo.Length);
            Console.WriteLine($"Linhas encontradas: {lineCount:N0}");
            
            // Buscando padrão específico
            long matches = FindPatternUsingMMF(accessor, fileInfo.Length, "linha");
            Console.WriteLine($"Ocorrências de 'linha': {matches:N0}");
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra manipulação eficiente de arquivos grandes
    /// </summary>
    static async Task DemonstrateLargeFileHandling()
    {
        Console.WriteLine("📊 Manipulação de Arquivos Grandes");
        Console.WriteLine("===================================");

        string largeFile = "large_test.dat";
        const int fileSize = 10 * 1024 * 1024; // 10 MB
        
        try
        {
            Console.WriteLine($"Criando arquivo de {fileSize:N0} bytes...");
            
            // Criando arquivo grande usando MMF (mais eficiente)
            await CreateLargeFileUsingMMF(largeFile, fileSize);
            
            var fileInfo = new FileInfo(largeFile);
            Console.WriteLine($"Arquivo criado: {fileInfo.Length:N0} bytes");

            // Demonstrando acesso randômico eficiente
            using var mmf = MemoryMappedFile.CreateFromFile(largeFile, FileMode.Open);
            using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);

            Console.WriteLine("\n🎯 Acesso randômico:");
            
            var random = new Random();
            var sw = Stopwatch.StartNew();
            
            // Fazendo várias escritas/leituras randômicas
            for (int i = 0; i < 1000; i++)
            {
                long position = random.NextInt64(0, fileSize - 4);
                int value = random.Next();
                
                accessor.Write(position, value);
                int readValue = accessor.ReadInt32(position);
                
                if (i % 200 == 0)
                {
                    Console.WriteLine($"  Posição {position:N0}: escrito={value}, lido={readValue}");
                }
            }
            
            sw.Stop();
            Console.WriteLine($"1000 operações randômicas em {sw.ElapsedMilliseconds}ms");
            
            // Demonstrando views parciais para economizar memória
            DemonstratePartialViews(mmf, fileSize);
        }
        finally
        {
            if (File.Exists(largeFile))
                File.Delete(largeFile);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra compartilhamento de memória entre threads
    /// </summary>
    static async Task DemonstrateSharedMemory()
    {
        Console.WriteLine("🤝 Compartilhamento de Memória");
        Console.WriteLine("==============================");

        const int bufferSize = 1024;
        
        // No macOS/Linux, usar arquivo temporário para simular MMF compartilhado
        string tempFile = Path.GetTempFileName();

        try
        {
            // Criando arquivo para MMF
            using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
            {
                fs.SetLength(bufferSize);
            }

            // Criando MMF do arquivo
            using var mmf = MemoryMappedFile.CreateFromFile(tempFile, FileMode.Open, null, bufferSize);
            using var accessor = mmf.CreateViewAccessor(0, bufferSize);

            Console.WriteLine($"MMF criado ({bufferSize} bytes)");

            // Dados compartilhados
            var sharedData = new SharedData
            {
                Counter = 0,
                Message = "Dados compartilhados entre threads",
                Timestamp = DateTime.Now.Ticks
            };

            // Escrevendo estrutura na memória compartilhada
            WriteStructToMMF(accessor, 0, sharedData);
            Console.WriteLine($"Dados iniciais: Counter={sharedData.Counter}, Message='{sharedData.Message}'");

            // Criando múltiplas threads para acessar dados compartilhados
            var tasks = new Task[4];
            var cts = new CancellationTokenSource();

            for (int i = 0; i < tasks.Length; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() => WorkerThreadFile(tempFile, threadId, cts.Token));
            }

            // Deixa threads rodarem por um tempo
            await Task.Delay(3000);
            cts.Cancel();

            // Aguarda todas as threads terminarem
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Esperado quando cancellation é solicitado
            }

            // Lendo resultado final
            var finalData = ReadStructFromMMF<SharedData>(accessor, 0);
            Console.WriteLine($"\nResultado final: Counter={finalData.Counter}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Thread worker que acessa arquivo compartilhado (compatível com macOS/Linux)
    /// </summary>
    static void WorkerThreadFile(string fileName, int threadId, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
                using var accessor = mmf.CreateViewAccessor(0, 1024);

                // Lendo dados atuais
                var data = ReadStructFromMMF<SharedData>(accessor, 0);
                
                // Incrementando counter
                data.Counter++;
                data.Timestamp = DateTime.Now.Ticks;
                data.Message = $"Atualizado pela thread {threadId}";

                // Escrevendo de volta
                WriteStructToMMF(accessor, 0, data);

                if (data.Counter % 10 == 0)
                {
                    Console.WriteLine($"Thread {threadId}: Counter = {data.Counter}");
                }

                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thread {threadId}: Erro - {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstra comunicação entre processos usando MMF
    /// </summary>
    static async Task DemonstrateInterProcessCommunication()
    {
        Console.WriteLine("🔄 Comunicação Inter-Processo (IPC)");
        Console.WriteLine("===================================");
        Console.WriteLine("⚠️ Nota: MMF nomeados não são suportados no macOS/Linux");
        Console.WriteLine("Esta demonstração simula IPC usando arquivos temporários.");
        Console.WriteLine();

        // Demonstração simplificada de IPC
        await DemonstrateSimpleIPC();
    }

    /// <summary>
    /// Processo produtor para IPC
    /// </summary>
    static async Task RunProducer()
    {
        Console.WriteLine("🏭 Produtor - Comunicação Inter-Processo");
        Console.WriteLine("========================================");

        const string mmfName = "IPCDemo";
        const int bufferSize = 4096;

        try
        {
            using var mmf = MemoryMappedFile.CreateNew(mmfName, bufferSize);
            using var accessor = mmf.CreateViewAccessor(0, bufferSize);

            Console.WriteLine($"MMF criado: {mmfName}");
            Console.WriteLine("Produzindo mensagens... (Ctrl+C para parar)");

            var messageId = 0;
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            while (!cts.Token.IsCancellationRequested)
            {
                var message = new IPCMessage
                {
                    Id = ++messageId,
                    Timestamp = DateTime.Now.Ticks,
                    Type = MessageType.Data,
                    Content = $"Mensagem #{messageId} do Produtor - {DateTime.Now:HH:mm:ss.fff}"
                };

                WriteStructToMMF(accessor, 0, message);
                Console.WriteLine($"Enviado: {message.Content}");

                await Task.Delay(1000, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nProdutor encerrado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no produtor: {ex.Message}");
        }
    }

    /// <summary>
    /// Processo consumidor para IPC
    /// </summary>
    static async Task RunConsumer()
    {
        Console.WriteLine("🛒 Consumidor - Comunicação Inter-Processo");
        Console.WriteLine("==========================================");

        const string mmfName = "IPCDemo";

        try
        {
            Console.WriteLine("Aguardando MMF ser criado pelo produtor...");
            
            // Aguarda MMF estar disponível
            MemoryMappedFile? mmf = null;
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    mmf = MemoryMappedFile.OpenExisting(mmfName);
                    break;
                }
                catch (FileNotFoundException)
                {
                    await Task.Delay(1000);
                    Console.Write(".");
                }
            }

            if (mmf == null)
            {
                Console.WriteLine("\nMMF não encontrado. Execute o produtor primeiro.");
                return;
            }

            using (mmf)
            {
                using var accessor = mmf.CreateViewAccessor(0, 4096);

                Console.WriteLine($"\nConectado ao MMF: {mmfName}");
                Console.WriteLine("Consumindo mensagens... (Ctrl+C para parar)");

                var lastMessageId = 0;
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

                while (!cts.Token.IsCancellationRequested)
                {
                    var message = ReadStructFromMMF<IPCMessage>(accessor, 0);
                    
                    if (message.Id > lastMessageId)
                    {
                        lastMessageId = message.Id;
                        var timestamp = new DateTime(message.Timestamp);
                        Console.WriteLine($"Recebido: [{message.Id}] {message.Content} ({timestamp:HH:mm:ss.fff})");
                    }

                    await Task.Delay(500, cts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nConsumidor encerrado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no consumidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstração simplificada de IPC
    /// </summary>
    static async Task DemonstrateSimpleIPC()
    {
        Console.WriteLine("📝 Demonstração Simplificada de IPC:");

        string tempFile = Path.GetTempFileName();
        const int bufferSize = 1024;

        try
        {
            // Criando arquivo para MMF
            using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
            {
                fs.SetLength(bufferSize);
            }

            // Simulando dois "processos" com tasks
            using var mmf = MemoryMappedFile.CreateFromFile(tempFile, FileMode.Open);
            
            var cts = new CancellationTokenSource();
            
            // Task "Processo 1" - Escritor
            var writerTask = Task.Run(async () =>
            {
                using var accessor = mmf.CreateViewAccessor(0, bufferSize);
                for (int i = 1; i <= 5 && !cts.Token.IsCancellationRequested; i++)
                {
                    var data = $"Mensagem {i} do processo escritor";
                    var bytes = Encoding.UTF8.GetBytes(data);
                    accessor.WriteArray(0, bytes, 0, bytes.Length);
                    if (bytes.Length < bufferSize - 1)
                        accessor.Write(bytes.Length, (byte)0); // Null terminator
                    
                    Console.WriteLine($"  📤 Escritor: {data}");
                    await Task.Delay(800);
                }
            });

            // Task "Processo 2" - Leitor
            var readerTask = Task.Run(async () =>
            {
                using var accessor = mmf.CreateViewAccessor(0, bufferSize);
                await Task.Delay(400); // Offset para intercalar
                
                string lastMessage = "";
                for (int i = 0; i < 5 && !cts.Token.IsCancellationRequested; i++)
                {
                    var buffer = new byte[bufferSize];
                    accessor.ReadArray(0, buffer, 0, bufferSize);
                    
                    var nullIndex = Array.IndexOf(buffer, (byte)0);
                    if (nullIndex > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, nullIndex);
                        if (message != lastMessage)
                        {
                            Console.WriteLine($"  📥 Leitor: {message}");
                            lastMessage = message;
                        }
                    }
                    await Task.Delay(800);
                }
            });

            await Task.WhenAll(writerTask, readerTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na demonstração IPC: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Mostra comparação de performance
    /// </summary>
    static void ShowPerformanceComparison()
    {
        Console.WriteLine("⚡ Comparação de Performance");
        Console.WriteLine("============================");
        Console.WriteLine("MMF vs. File I/O tradicional:");
        Console.WriteLine();
        Console.WriteLine("📊 Resultados típicos:");
        Console.WriteLine("• Leitura sequencial: MMF ~2-3x mais rápido");
        Console.WriteLine("• Acesso randômico: MMF ~5-10x mais rápido");
        Console.WriteLine("• Arquivos grandes (>100MB): MMF ~3-5x mais rápido");
        Console.WriteLine("• Compartilhamento: MMF ~20-50x mais rápido que pipes");
        Console.WriteLine();
        Console.WriteLine("💡 Vantagens do MMF:");
        Console.WriteLine("• ✅ Lazy loading - só carrega páginas necessárias");
        Console.WriteLine("• ✅ Cache do SO - reutiliza páginas em memória");
        Console.WriteLine("• ✅ Zero-copy - acesso direto sem buffers intermediários");
        Console.WriteLine("• ✅ Compartilhamento eficiente entre processos");
        Console.WriteLine("• ✅ Sincronização automática com arquivo");
        Console.WriteLine();
        Console.WriteLine("⚠️ Desvantagens:");
        Console.WriteLine("• ❌ Uso de memória virtual (pode ser limitado)");
        Console.WriteLine("• ❌ Complexidade adicional para pequenos arquivos");
        Console.WriteLine("• ❌ Requer tratamento de exceções específicas");
        Console.WriteLine();
    }

    /// <summary>
    /// Mostra melhores práticas
    /// </summary>
    static void ShowBestPractices()
    {
        Console.WriteLine("🎯 Melhores Práticas");
        Console.WriteLine("====================");
        Console.WriteLine("1. 📏 Tamanho dos arquivos:");
        Console.WriteLine("   • Use MMF para arquivos > 1MB");
        Console.WriteLine("   • Para pequenos arquivos, I/O tradicional pode ser mais eficiente");
        Console.WriteLine();
        Console.WriteLine("2. 🔒 Sincronização:");
        Console.WriteLine("   • Use Mutex para coordenação entre processos");
        Console.WriteLine("   • Implemente timeouts para evitar deadlocks");
        Console.WriteLine("   • Considere usar semáforos para controle de acesso");
        Console.WriteLine();
        Console.WriteLine("3. 💾 Gerenciamento de memória:");
        Console.WriteLine("   • Dispose adequadamente de MMF e ViewAccessors");
        Console.WriteLine("   • Use using statements sempre que possível");
        Console.WriteLine("   • Monitore uso de memória virtual");
        Console.WriteLine();
        Console.WriteLine("4. 🛡️ Tratamento de erros:");
        Console.WriteLine("   • Trate UnauthorizedAccessException");
        Console.WriteLine("   • Handle FileNotFoundException para MMF nomeados");
        Console.WriteLine("   • Implemente retry logic para operações críticas");
        Console.WriteLine();
        Console.WriteLine("5. 🔧 Performance:");
        Console.WriteLine("   • Use views parciais para arquivos muito grandes");
        Console.WriteLine("   • Aligne acessos com boundaries de página (4KB)");
        Console.WriteLine("   • Considere usar unsafe code para performance máxima");
        Console.WriteLine();
    }

    #region Helper Methods

    /// <summary>
    /// Cria arquivo de teste com conteúdo estruturado
    /// </summary>
    static async Task CreateTestFile(string fileName)
    {
        using var writer = new StreamWriter(fileName);
        
        await writer.WriteLineAsync("# Arquivo de Teste para Memory-Mapped Files");
        await writer.WriteLineAsync("# Criado em: " + DateTime.Now);
        await writer.WriteLineAsync("");

        for (int i = 1; i <= 1000; i++)
        {
            await writer.WriteLineAsync($"Esta é a linha {i:D4} do arquivo de teste. Conteúdo: {Guid.NewGuid()}");
        }
    }

    /// <summary>
    /// Cria arquivo grande usando MMF para melhor performance
    /// </summary>
    static async Task CreateLargeFileUsingMMF(string fileName, int size)
    {
        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
        fs.SetLength(size);

        using var mmf = MemoryMappedFile.CreateFromFile(fs, null, size, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        using var accessor = mmf.CreateViewAccessor(0, size);

        // Preenchendo com padrão de dados
        var random = new Random(42); // Seed fixo para reproduzibilidade
        var buffer = new byte[4096]; // 4KB chunks
        
        for (int offset = 0; offset < size; offset += buffer.Length)
        {
            int chunkSize = Math.Min(buffer.Length, size - offset);
            random.NextBytes(buffer);
            accessor.WriteArray(offset, buffer, 0, chunkSize);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Conta linhas usando MMF
    /// </summary>
    static long CountLinesUsingMMF(MemoryMappedViewAccessor accessor, long fileSize)
    {
        long lineCount = 0;
        const int bufferSize = 4096;
        var buffer = new byte[bufferSize];

        for (long offset = 0; offset < fileSize; offset += bufferSize)
        {
            int bytesToRead = (int)Math.Min(bufferSize, fileSize - offset);
            accessor.ReadArray(offset, buffer, 0, bytesToRead);

            for (int i = 0; i < bytesToRead; i++)
            {
                if (buffer[i] == '\n')
                    lineCount++;
            }
        }

        return lineCount;
    }

    /// <summary>
    /// Encontra padrão usando MMF
    /// </summary>
    static long FindPatternUsingMMF(MemoryMappedViewAccessor accessor, long fileSize, string pattern)
    {
        long matches = 0;
        var patternBytes = Encoding.UTF8.GetBytes(pattern);
        const int bufferSize = 4096;
        var buffer = new byte[bufferSize + patternBytes.Length]; // Overlap para padrões que cruzam boundaries

        for (long offset = 0; offset < fileSize; offset += bufferSize)
        {
            int bytesToRead = (int)Math.Min(buffer.Length, fileSize - offset);
            accessor.ReadArray(offset, buffer, 0, bytesToRead);

            for (int i = 0; i <= bytesToRead - patternBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternBytes.Length; j++)
                {
                    if (buffer[i + j] != patternBytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) matches++;
            }
        }

        return matches;
    }

    /// <summary>
    /// Demonstra views parciais para economizar memória
    /// </summary>
    static void DemonstratePartialViews(MemoryMappedFile mmf, long fileSize)
    {
        Console.WriteLine("\n🔍 Views Parciais (economizando memória):");
        
        const int viewSize = 1024 * 1024; // 1MB views
        int viewCount = (int)Math.Ceiling((double)fileSize / viewSize);
        
        Console.WriteLine($"Arquivo: {fileSize:N0} bytes");
        Console.WriteLine($"Views: {viewCount} de {viewSize:N0} bytes cada");

        for (int i = 0; i < Math.Min(viewCount, 3); i++) // Mostra apenas 3 primeiras
        {
            long offset = (long)i * viewSize;
            long actualViewSize = Math.Min(viewSize, fileSize - offset);
            
            using var view = mmf.CreateViewAccessor(offset, actualViewSize, MemoryMappedFileAccess.Read);
            
            // Lê algumas amostras da view
            int sample1 = view.ReadInt32(0);
            int sample2 = view.ReadInt32(actualViewSize / 2);
            int sample3 = view.ReadInt32(actualViewSize - 4);
            
            Console.WriteLine($"  View {i}: offset={offset:N0}, size={actualViewSize:N0}, samples=[{sample1}, {sample2}, {sample3}]");
        }
    }

    /// <summary>
    /// Escreve struct para MMF usando bytes
    /// </summary>
    static void WriteStructToMMF<T>(MemoryMappedViewAccessor accessor, long offset, T value) where T : unmanaged
    {
        unsafe
        {
            int size = sizeof(T);
            byte* ptr = (byte*)&value;
            for (int i = 0; i < size; i++)
            {
                accessor.Write(offset + i, ptr[i]);
            }
        }
    }

    /// <summary>
    /// Lê struct de MMF usando bytes
    /// </summary>
    static T ReadStructFromMMF<T>(MemoryMappedViewAccessor accessor, long offset) where T : unmanaged
    {
        unsafe
        {
            T result = default;
            int size = sizeof(T);
            byte* ptr = (byte*)&result;
            for (int i = 0; i < size; i++)
            {
                ptr[i] = accessor.ReadByte(offset + i);
            }
            return result;
        }
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Estrutura para dados compartilhados
/// </summary>
public unsafe struct SharedData
{
    public int Counter;
    public long Timestamp;
    public char Placeholder1, Placeholder2, Placeholder3, Placeholder4; // Padding
    private fixed char _message[64];
    
    public string Message
    {
        get
        {
            fixed (char* ptr = _message)
            {
                return new string(ptr).TrimEnd('\0');
            }
        }
        set
        {
            fixed (char* ptr = _message)
            {
                var chars = value.AsSpan();
                int length = Math.Min(chars.Length, 63);
                
                for (int i = 0; i < length; i++)
                    ptr[i] = chars[i];
                
                for (int i = length; i < 64; i++)
                    ptr[i] = '\0';
            }
        }
    }
}

/// <summary>
/// Tipo de mensagem para IPC
/// </summary>
public enum MessageType : int
{
    Data = 1,
    Command = 2,
    Response = 3,
    Heartbeat = 4
}

/// <summary>
/// Estrutura para mensagens IPC
/// </summary>
public unsafe struct IPCMessage
{
    public int Id;
    public long Timestamp;
    public MessageType Type;
    private fixed char _content[128];
    
    public string Content
    {
        get
        {
            fixed (char* ptr = _content)
            {
                return new string(ptr).TrimEnd('\0');
            }
        }
        set
        {
            fixed (char* ptr = _content)
            {
                var chars = value.AsSpan();
                int length = Math.Min(chars.Length, 127);
                
                for (int i = 0; i < length; i++)
                    ptr[i] = chars[i];
                
                for (int i = length; i < 128; i++)
                    ptr[i] = '\0';
            }
        }
    }
}

#endregion

#region Benchmarks

/// <summary>
/// Benchmarks comparando MMF com I/O tradicional
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class MemoryMappedFilesBenchmarks
{
    private string _testFile = null!;
    private byte[] _testData = null!;
    private const int FileSize = 10 * 1024 * 1024; // 10MB

    [GlobalSetup]
    public void Setup()
    {
        _testFile = Path.GetTempFileName();
        _testData = new byte[FileSize];
        new Random(42).NextBytes(_testData);
        
        File.WriteAllBytes(_testFile, _testData);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testFile))
            File.Delete(_testFile);
    }

    [Benchmark]
    public long ReadFile_Traditional()
    {
        long sum = 0;
        using var fs = new FileStream(_testFile, FileMode.Open, FileAccess.Read);
        var buffer = new byte[4096];
        
        int bytesRead;
        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
                sum += buffer[i];
        }
        
        return sum;
    }

    [Benchmark]
    public long ReadFile_MemoryMapped()
    {
        long sum = 0;
        using var mmf = MemoryMappedFile.CreateFromFile(_testFile, FileMode.Open);
        using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        
        for (int i = 0; i < FileSize; i++)
        {
            sum += accessor.ReadByte(i);
        }
        
        return sum;
    }

    [Benchmark]
    public void RandomAccess_Traditional()
    {
        using var fs = new FileStream(_testFile, FileMode.Open, FileAccess.Read);
        var random = new Random(42);
        var buffer = new byte[4];
        
        for (int i = 0; i < 1000; i++)
        {
            long position = random.NextInt64(0, FileSize - 4);
            fs.Seek(position, SeekOrigin.Begin);
            fs.Read(buffer, 0, 4);
        }
    }

    [Benchmark]
    public void RandomAccess_MemoryMapped()
    {
        using var mmf = MemoryMappedFile.CreateFromFile(_testFile, FileMode.Open);
        using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        var random = new Random(42);
        
        for (int i = 0; i < 1000; i++)
        {
            long position = random.NextInt64(0, FileSize - 4);
            accessor.ReadInt32(position);
        }
    }
}

#endregion
