using System.Runtime.InteropServices;
using System.Text;

namespace Dica85.PInvoke
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Dica 85: P/Invoke - Platform Invocation Services ===\n");

            DemonstrateBasicPInvoke();
            DemonstrateStringMarshaling();
            DemonstrateStructMarshaling();
            DemonstrateCallbackFunctions();
            DemonstrateMemoryManagement();
            DemonstrateErrorHandling();
            DemonstratePerformanceConsiderations();
            DemonstrateBestPractices();
        }

        #region 1. Basic P/Invoke Concepts

        // Windows API examples
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetComputerNameA(
            StringBuilder lpBuffer,
            ref uint nSize);

        // Cross-platform examples using libc
        [DllImport("libc", EntryPoint = "getpid")]
        public static extern int GetProcessId();

        [DllImport("libc", EntryPoint = "time")]
        public static extern long GetUnixTime(IntPtr timer);

        private static void DemonstrateBasicPInvoke()
        {
            Console.WriteLine("1. Conceitos Básicos de P/Invoke\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Chamadas para Windows API
                    var threadId = GetCurrentThreadId();
                    var processHandle = GetCurrentProcess();

                    Console.WriteLine($"Thread ID atual: {threadId}");
                    Console.WriteLine($"Handle do processo: 0x{processHandle:X}");

                    // Obtendo nome do computador
                    var buffer = new StringBuilder(256);
                    uint size = (uint)buffer.Capacity;
                    
                    if (GetComputerNameA(buffer, ref size))
                    {
                        Console.WriteLine($"Nome do computador: {buffer.ToString(0, (int)size)}");
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Chamadas para bibliotecas Unix/Linux
                    var pid = GetProcessId();
                    var unixTime = GetUnixTime(IntPtr.Zero);

                    Console.WriteLine($"Process ID: {pid}");
                    Console.WriteLine($"Unix Time: {unixTime}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em P/Invoke básico: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 2. String Marshaling

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowsDirectoryW(
            [Out] StringBuilder lpBuffer,
            uint nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern int GetWindowsDirectoryA(
            [Out] StringBuilder lpBuffer,
            uint nSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int MessageBox(
            IntPtr hWnd,
            string lpText,
            string lpCaption,
            uint uType);

        private static void DemonstrateStringMarshaling()
        {
            Console.WriteLine("2. Marshaling de Strings\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Exemplo com CharSet.Unicode
                    var bufferW = new StringBuilder(260);
                    var resultW = GetWindowsDirectoryW(bufferW, (uint)bufferW.Capacity);
                    
                    if (resultW > 0)
                    {
                        Console.WriteLine($"Diretório Windows (Unicode): {bufferW}");
                    }

                    // Exemplo com CharSet.Ansi
                    var bufferA = new StringBuilder(260);
                    var resultA = GetWindowsDirectoryA(bufferA, (uint)bufferA.Capacity);
                    
                    if (resultA > 0)
                    {
                        Console.WriteLine($"Diretório Windows (ANSI): {bufferA}");
                    }

                    // Demonstração de diferentes tipos de string marshaling
                    Console.WriteLine("\nTipos de String Marshaling:");
                    Console.WriteLine("- MarshalAs(UnmanagedType.LPStr): ANSI string");
                    Console.WriteLine("- MarshalAs(UnmanagedType.LPWStr): Unicode string");
                    Console.WriteLine("- MarshalAs(UnmanagedType.BStr): BSTR string");
                    Console.WriteLine("- StringBuilder: Para strings de saída");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em string marshaling: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 3. Struct Marshaling

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindFirstFile(
            string lpFileName,
            out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        private static void DemonstrateStructMarshaling()
        {
            Console.WriteLine("3. Marshaling de Estruturas\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Exemplo com estrutura simples
                    if (GetCursorPos(out POINT cursorPos))
                    {
                        Console.WriteLine($"Posição do cursor: X={cursorPos.X}, Y={cursorPos.Y}");
                    }

                    // Exemplo com estrutura complexa
                    var searchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "*.*");
                    var handle = FindFirstFile(searchPath, out WIN32_FIND_DATA findData);
                    
                    if (handle != IntPtr.Zero && handle.ToInt64() != -1)
                    {
                        Console.WriteLine($"Primeiro arquivo encontrado: {findData.cFileName}");
                        Console.WriteLine($"Tamanho: {findData.nFileSizeLow} bytes");
                        FindClose(handle);
                    }
                }

                // Demonstração de diferentes layouts de estrutura
                Console.WriteLine("\nLayouts de Estrutura:");
                Console.WriteLine("- LayoutKind.Sequential: Campos em ordem sequencial");
                Console.WriteLine("- LayoutKind.Explicit: Controle manual do offset");
                Console.WriteLine("- LayoutKind.Auto: Layout otimizado pelo CLR");
                
                Console.WriteLine($"\nTamanho da estrutura POINT: {Marshal.SizeOf<POINT>()} bytes");
                Console.WriteLine($"Tamanho da estrutura WIN32_FIND_DATA: {Marshal.SizeOf<WIN32_FIND_DATA>()} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em struct marshaling: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 4. Callback Functions

        // Definindo um delegate para callback
        public delegate bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsCallback lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        private static void DemonstrateCallbackFunctions()
        {
            Console.WriteLine("4. Funções de Callback\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var windowCount = 0;
                    var visibleWindowCount = 0;

                    // Callback function que será chamada para cada janela
                    bool WindowEnumCallback(IntPtr hWnd, IntPtr lParam)
                    {
                        windowCount++;
                        
                        if (IsWindowVisible(hWnd))
                        {
                            visibleWindowCount++;
                            
                            // Obter título da janela (apenas as primeiras 5 visíveis)
                            if (visibleWindowCount <= 5)
                            {
                                var title = new StringBuilder(256);
                                var length = GetWindowText(hWnd, title, title.Capacity);
                                
                                if (length > 0)
                                {
                                    Console.WriteLine($"Janela visível #{visibleWindowCount}: {title}");
                                }
                            }
                        }
                        
                        return true; // Continue enumerating
                    }

                    // Chamando a função que aceita callback
                    EnumWindows(WindowEnumCallback, IntPtr.Zero);
                    
                    Console.WriteLine($"\nTotal de janelas: {windowCount}");
                    Console.WriteLine($"Janelas visíveis: {visibleWindowCount}");
                }

                Console.WriteLine("\nConceitos de Callback:");
                Console.WriteLine("- Delegates são automaticamente marshaled como function pointers");
                Console.WriteLine("- GC.KeepAlive() pode ser necessário para evitar collection");
                Console.WriteLine("- Cuidado com exceptions em callbacks nativos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em callback functions: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 5. Memory Management

        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalAlloc(uint uFlags, UIntPtr uBytes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private static void DemonstrateMemoryManagement()
        {
            Console.WriteLine("5. Gerenciamento de Memória\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Alocando memória nativa
                    const uint LMEM_FIXED = 0x0000;
                    const int bufferSize = 1024;
                    
                    var nativeMemory = LocalAlloc(LMEM_FIXED, new UIntPtr(bufferSize));
                    
                    if (nativeMemory != IntPtr.Zero)
                    {
                        Console.WriteLine($"Memória nativa alocada: 0x{nativeMemory:X}");
                        Console.WriteLine($"Tamanho: {bufferSize} bytes");

                        // Escrevendo dados na memória nativa
                        var testData = Encoding.UTF8.GetBytes("Hello from native memory!");
                        Marshal.Copy(testData, 0, nativeMemory, testData.Length);

                        // Lendo dados da memória nativa
                        var readData = new byte[testData.Length];
                        Marshal.Copy(nativeMemory, readData, 0, testData.Length);
                        var readString = Encoding.UTF8.GetString(readData);
                        
                        Console.WriteLine($"Dados lidos: {readString}");

                        // Liberando memória nativa
                        LocalFree(nativeMemory);
                        Console.WriteLine("Memória nativa liberada");
                    }
                }

                // Demonstração de diferentes métodos de alocação
                Console.WriteLine("\nMétodos de Gerenciamento de Memória:");
                Console.WriteLine("- Marshal.AllocHGlobal(): Heap global");
                Console.WriteLine("- Marshal.AllocCoTaskMem(): COM task memory");
                Console.WriteLine("- GCHandle.Alloc(): Pinning de objetos managed");
                Console.WriteLine("- fixed statement: Pinning temporário");

                // Exemplo com GCHandle
                var managedArray = new byte[] { 1, 2, 3, 4, 5 };
                var handle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
                
                try
                {
                    var pinnedAddress = handle.AddrOfPinnedObject();
                    Console.WriteLine($"\nArray pinned em: 0x{pinnedAddress:X}");
                    
                    // O array agora está fixo na memória e pode ser usado por código nativo
                }
                finally
                {
                    handle.Free();
                    Console.WriteLine("GCHandle liberado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em memory management: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 6. Error Handling

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        private static void DemonstrateErrorHandling()
        {
            Console.WriteLine("6. Tratamento de Erros\n");

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Tentativa de abrir um arquivo que não existe
                    const uint GENERIC_READ = 0x80000000;
                    const uint OPEN_EXISTING = 3;
                    
                    var handle = CreateFile(
                        "arquivo_inexistente.txt",
                        GENERIC_READ,
                        0,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0,
                        IntPtr.Zero);

                    if (handle == IntPtr.Zero || handle.ToInt64() == -1)
                    {
                        var errorCode = GetLastError();
                        var win32Exception = new System.ComponentModel.Win32Exception((int)errorCode);
                        
                        Console.WriteLine($"Erro ao abrir arquivo:");
                        Console.WriteLine($"Código de erro: {errorCode}");
                        Console.WriteLine($"Mensagem: {win32Exception.Message}");
                        
                        // Usando Marshal.GetLastWin32Error()
                        var marshalError = Marshal.GetLastWin32Error();
                        Console.WriteLine($"Marshal.GetLastWin32Error(): {marshalError}");
                    }
                    else
                    {
                        CloseHandle(handle);
                    }
                }

                Console.WriteLine("\nEstatégias de Tratamento de Erros:");
                Console.WriteLine("- SetLastError = true no DllImport");
                Console.WriteLine("- Marshal.GetLastWin32Error()");
                Console.WriteLine("- Win32Exception para códigos de erro Windows");
                Console.WriteLine("- Verificação de valores de retorno (IntPtr.Zero, -1, etc.)");
                Console.WriteLine("- HRESULT para APIs COM");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em error handling: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 7. Performance Considerations

        // Diferentes formas de chamar a mesma função
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

        [DllImport("kernel32.dll", EntryPoint = "GetTickCount")]
        public static extern uint GetTickCountCached();

        private static void DemonstratePerformanceConsiderations()
        {
            Console.WriteLine("7. Considerações de Performance\n");

            try
            {
                // Medindo overhead de P/Invoke
                const int iterations = 100000;
                var sw = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 0; i < iterations; i++)
                {
                    var tick = GetTickCount();
                }

                sw.Stop();
                var pinvokeTime = sw.ElapsedMilliseconds;

                Console.WriteLine($"Tempo para {iterations:N0} chamadas P/Invoke: {pinvokeTime}ms");
                Console.WriteLine($"Tempo médio por chamada: {(double)pinvokeTime / iterations * 1000:F3}μs");

                // Comparando com operação managed equivalente
                sw.Restart();
                
                for (int i = 0; i < iterations; i++)
                {
                    var ticks = Environment.TickCount;
                }
                
                sw.Stop();
                var managedTime = sw.ElapsedMilliseconds;
                
                Console.WriteLine($"Tempo para {iterations:N0} operações managed: {managedTime}ms");
                Console.WriteLine($"Overhead do P/Invoke: {(double)pinvokeTime / managedTime:F1}x");

                Console.WriteLine("\nDicas de Performance:");
                Console.WriteLine("- Minimize chamadas P/Invoke em loops");
                Console.WriteLine("- Use blittable types quando possível");
                Console.WriteLine("- Cache delegates para callbacks");
                Console.WriteLine("- Considere SuppressUnmanagedCodeSecurity para trusted code");
                Console.WriteLine("- Use CharSet correto para evitar conversões");
                Console.WriteLine("- PreserveSig para evitar marshaling de HRESULT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em performance considerations: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion

        #region 8. Best Practices

        // Exemplo de classe wrapper bem estruturada
        public static class SafeNativeMethods
        {
            // Constantes bem documentadas
            private const uint GENERIC_READ = 0x80000000;
            private const uint FILE_SHARE_READ = 0x00000001;
            private const uint OPEN_EXISTING = 3;
            private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr CreateFileW(
                [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr hObject);

            // Wrapper seguro que encapsula a lógica de P/Invoke
            public static bool FileExists(string filePath)
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var handle = CreateFileW(
                    filePath,
                    GENERIC_READ,
                    FILE_SHARE_READ,
                    IntPtr.Zero,
                    OPEN_EXISTING,
                    0,
                    IntPtr.Zero);

                if (handle == IntPtr.Zero || handle == INVALID_HANDLE_VALUE)
                {
                    return false;
                }

                CloseHandle(handle);
                return true;
            }
        }

        private static void DemonstrateBestPractices()
        {
            Console.WriteLine("8. Melhores Práticas\n");

            try
            {
                // Demonstração do wrapper seguro
                var tempFile = Path.GetTempFileName();
                
                Console.WriteLine($"Verificando se arquivo existe: {tempFile}");
                var exists = SafeNativeMethods.FileExists(tempFile);
                Console.WriteLine($"Arquivo existe: {exists}");

                File.Delete(tempFile);

                Console.WriteLine("\n✅ Melhores Práticas P/Invoke:");
                Console.WriteLine();
                Console.WriteLine("🔒 **Segurança:**");
                Console.WriteLine("   • Use classes internas ou privadas para P/Invoke");
                Console.WriteLine("   • Valide parâmetros antes de chamadas nativas");
                Console.WriteLine("   • Use SuppressUnmanagedCodeSecurity com cuidado");
                Console.WriteLine();
                Console.WriteLine("📝 **Assinaturas:**");
                Console.WriteLine("   • Use tipos corretos (IntPtr para handles)");
                Console.WriteLine("   • Especifique CharSet explicitamente");
                Console.WriteLine("   • Use [return: MarshalAs] quando necessário");
                Console.WriteLine();
                Console.WriteLine("🎯 **Performance:**");
                Console.WriteLine("   • Prefira tipos blittable");
                Console.WriteLine("   • Minimize marshaling de strings");
                Console.WriteLine("   • Cache delegates de callback");
                Console.WriteLine();
                Console.WriteLine("🛡️ **Robustez:**");
                Console.WriteLine("   • Sempre use SetLastError = true quando apropriado");
                Console.WriteLine("   • Implemente wrappers seguros");
                Console.WriteLine("   • Libere recursos nativos adequadamente");
                Console.WriteLine("   • Trate erros de P/Invoke apropriadamente");
                Console.WriteLine();
                Console.WriteLine("🏗️ **Arquitetura:**");
                Console.WriteLine("   • Separe P/Invoke em classes dedicadas");
                Console.WriteLine("   • Use factory patterns para recursos nativos");
                Console.WriteLine("   • Implemente IDisposable para cleanup");
                Console.WriteLine("   • Considere usar SafeHandle para handles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em best practices: {ex.Message}");
            }

            Console.WriteLine();
        }

        #endregion
    }
}
