# Dica 85: P/Invoke - Platform Invocation Services

## 📋 Sobre

Este projeto demonstra como usar **P/Invoke (Platform Invocation Services)** em C# para chamar funções de bibliotecas nativas (DLLs no Windows, shared libraries no Linux/macOS).

## 🎯 Conceitos Demonstrados

### 1. **Conceitos Básicos de P/Invoke**
- Declaração de funções externas com `[DllImport]`
- Chamadas para Windows API e bibliotecas Unix
- Mapeamento de tipos entre managed e unmanaged code

### 2. **String Marshaling**
- `CharSet.Unicode` vs `CharSet.Ansi`
- `StringBuilder` para strings de saída
- Diferentes tipos de marshaling de string

### 3. **Struct Marshaling**
- `LayoutKind.Sequential`, `Explicit`, `Auto`
- Controle de alinhamento e padding
- Marshaling de estruturas complexas

### 4. **Funções de Callback**
- Delegates como function pointers
- Enumeração de recursos do sistema
- Gerenciamento de lifetime de callbacks

### 5. **Gerenciamento de Memória**
- Alocação e liberação de memória nativa
- `GCHandle` para pinning de objetos
- `Marshal.AllocHGlobal()` e equivalentes

### 6. **Tratamento de Erros**
- `SetLastError = true`
- `Marshal.GetLastWin32Error()`
- `Win32Exception` para códigos de erro

### 7. **Considerações de Performance**
- Overhead de P/Invoke vs operações managed
- Tipos blittable para melhor performance
- Otimizações de marshaling

### 8. **Melhores Práticas**
- Wrappers seguros para P/Invoke
- Validação de parâmetros
- Cleanup adequado de recursos

## 🚀 Como Executar

```bash
# Navegar para o diretório da dica
cd "Dicas/Dica85-PInvoke"

# Compilar o projeto
dotnet build

# Executar
dotnet run
```

## 📊 Exemplos de Saída

```
=== Dica 85: P/Invoke - Platform Invocation Services ===

1. Conceitos Básicos de P/Invoke

Thread ID atual: 12345
Handle do processo: 0xFFFFFFFF80000000
Nome do computador: MEUPC

2. Marshaling de Strings

Diretório Windows (Unicode): C:\Windows
Diretório Windows (ANSI): C:\Windows

Tipos de String Marshaling:
- MarshalAs(UnmanagedType.LPStr): ANSI string
- MarshalAs(UnmanagedType.LPWStr): Unicode string
- MarshalAs(UnmanagedType.BStr): BSTR string
- StringBuilder: Para strings de saída

3. Marshaling de Estruturas

Posição do cursor: X=1024, Y=768
Primeiro arquivo encontrado: System32
Tamanho: 4096 bytes

Layouts de Estrutura:
- LayoutKind.Sequential: Campos em ordem sequencial
- LayoutKind.Explicit: Controle manual do offset
- LayoutKind.Auto: Layout otimizado pelo CLR

Tamanho da estrutura POINT: 8 bytes
Tamanho da estrutura WIN32_FIND_DATA: 592 bytes

4. Funções de Callback

Janela visível #1: Calculator
Janela visível #2: Notepad
Janela visível #3: Visual Studio Code
Janela visível #4: Chrome
Janela visível #5: Explorer

Total de janelas: 156
Janelas visíveis: 23

5. Gerenciamento de Memória

Memória nativa alocada: 0x1A2B3C4D
Tamanho: 1024 bytes
Dados lidos: Hello from native memory!
Memória nativa liberada

Array pinned em: 0x2B3C4D5E
GCHandle liberado

6. Tratamento de Erros

Erro ao abrir arquivo:
Código de erro: 2
Mensagem: The system cannot find the file specified
Marshal.GetLastWin32Error(): 2

7. Considerações de Performance

Tempo para 100,000 chamadas P/Invoke: 45ms
Tempo médio por chamada: 0.450μs
Tempo para 100,000 operações managed: 12ms
Overhead do P/Invoke: 3.8x
```

## 🔍 Detalhes Técnicos

### DllImport Attributes
```csharp
[DllImport("kernel32.dll", 
    SetLastError = true,
    CharSet = CharSet.Unicode,
    EntryPoint = "CreateFileW")]
```

### Marshaling Types
- **Blittable Types**: `int`, `byte`, `IntPtr` (sem conversão)
- **Non-Blittable Types**: `string`, `bool`, arrays (requerem marshaling)

### Memory Layout
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MyStruct
{
    public byte Field1;
    public int Field2;  // Sem padding devido a Pack = 1
}
```

### SafeHandle Pattern
```csharp
public class SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);
    
    protected override bool ReleaseHandle()
    {
        return CloseHandle(handle);
    }
}
```

## ⚡ Performance Tips

1. **Use Blittable Types**: Evite conversões desnecessárias
2. **Cache Delegates**: Para callbacks frequentes
3. **StringBuilder**: Para strings de saída em vez de string
4. **Pinning**: Use `fixed` ou `GCHandle` para arrays grandes
5. **Batch Calls**: Minimize número de chamadas P/Invoke

## 🛡️ Segurança

- **Code Access Security**: P/Invoke requer `UnmanagedCode` permission
- **Input Validation**: Sempre valide parâmetros antes de chamadas nativas
- **Resource Cleanup**: Use `using` ou `try/finally` para cleanup
- **Trust Level**: Considere `SuppressUnmanagedCodeSecurity` apenas para código confiável

## 🔗 Casos de Uso Comuns

- **Windows API**: Funcionalidades não disponíveis no .NET
- **Legacy Libraries**: Integração com código C/C++ existente
- **Hardware Access**: Drivers e APIs de baixo nível
- **Performance**: Funções nativas otimizadas
- **Third-party Libraries**: SDKs nativos

## 🆚 Alternativas Modernas

- **C++/CLI**: Para interoperabilidade complexa
- **IJW (It Just Works)**: Mixed-mode assemblies
- **COM Interop**: Para componentes COM
- **Managed C++**: Wrappers managed para código nativo
- **Native Libraries**: Package com bibliotecas nativas

## 📚 Referências

- [Platform Invoke (P/Invoke)](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [Marshaling Data with Platform Invoke](https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-data-with-platform-invoke)
- [Calling Native Functions from Managed Code](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/)
