# Dica 72: Memory-Mapped Files

## 📋 Sobre
Demonstra o uso de Memory-Mapped Files (MMF) para acesso eficiente a grandes arquivos, compartilhamento de memória entre processos e comunicação inter-processo (IPC) de alta performance.

Memory-Mapped Files mapeiam arquivos diretamente na memória virtual, permitindo acesso como se fossem arrays na memória, com performance superior ao I/O tradicional.

## 🎯 Conceitos Abordados

### 1. Uso Básico de MMF
- Criação e mapeamento de arquivos
- Leitura e escrita direta na memória
- ViewAccessors para acesso aos dados

### 2. Mapeamento de Arquivos
- Arquivos existentes e novos
- Diferentes modos de acesso
- Análise eficiente de conteúdo

### 3. Manipulação de Arquivos Grandes
- Views parciais para economizar memória
- Acesso randômico otimizado
- Processamento eficiente de grandes volumes

### 4. Compartilhamento de Memória
- MMF nomeados para IPC
- Sincronização entre threads
- Estruturas de dados compartilhadas

### 5. Comunicação Inter-Processo
- Padrão produtor-consumidor
- Troca de mensagens estruturadas
- Coordenação entre processos

## 🚀 Como Executar

```bash
# Demonstração completa
dotnet run

# Benchmarks de performance
dotnet run benchmark

# Teste de IPC - Produtor
dotnet run producer

# Teste de IPC - Consumidor (em outro terminal)
dotnet run consumer
```

## 💡 Exemplos de Uso

### Mapeamento Básico
```csharp
using var mmf = MemoryMappedFile.CreateFromFile("data.txt");
using var accessor = mmf.CreateViewAccessor(0, 0);

// Leitura direta
string data = accessor.ReadString(0);

// Escrita direta
accessor.Write(0, "novo conteudo");
```

### Compartilhamento entre Processos
```csharp
// Processo 1 - Criador
using var mmf = MemoryMappedFile.CreateNew("SharedData", 1024);
using var accessor = mmf.CreateViewAccessor(0, 1024);
accessor.Write(0, "dados compartilhados");

// Processo 2 - Consumidor
using var mmf = MemoryMappedFile.OpenExisting("SharedData");
using var accessor = mmf.CreateViewAccessor(0, 1024);
string data = accessor.ReadString(0);
```

### Views Parciais para Arquivos Grandes
```csharp
using var mmf = MemoryMappedFile.CreateFromFile("large_file.dat");

// Processa arquivo em chunks de 1MB
const int chunkSize = 1024 * 1024;
for (long offset = 0; offset < fileSize; offset += chunkSize)
{
    using var view = mmf.CreateViewAccessor(offset, chunkSize);
    // Processa esta parte do arquivo
}
```

## ⚡ Performance

### Vantagens do MMF
- **Lazy Loading**: Só carrega páginas necessárias
- **Cache do SO**: Reutiliza páginas já em memória
- **Zero-Copy**: Acesso direto sem buffers intermediários
- **Compartilhamento**: Eficiente entre processos

### Benchmarks Típicos
- **Leitura sequencial**: 2-3x mais rápido que FileStream
- **Acesso randômico**: 5-10x mais rápido
- **Arquivos grandes (>100MB)**: 3-5x mais rápido
- **IPC**: 20-50x mais rápido que named pipes

## 🎯 Quando Usar

### Cenários Ideais
- **Arquivos grandes**: > 1MB para obter benefícios significativos
- **Acesso randômico**: Navegação não-sequencial em dados
- **Compartilhamento**: Dados entre múltiplos processos
- **Performance crítica**: Aplicações que precisam de I/O rápido
- **Análise de dados**: Processing de grandes datasets

### Casos de Uso Comuns
- **Bancos de dados**: Sistemas de storage custom
- **Game engines**: Loading de assets e worlds
- **Análise de logs**: Processing de arquivos gigantes
- **Cache distribuído**: Compartilhamento de dados
- **IPC de alta performance**: Comunicação entre serviços

## ⚠️ Considerações

### Limitações
- **Memória virtual**: Limitada pelo espaço de endereçamento
- **Overhead**: Para arquivos pequenos pode ser ineficiente
- **Complexidade**: Requer tratamento de exceções específicas
- **Portabilidade**: Comportamento pode variar entre SOs

### Melhores Práticas

#### 1. Tamanho dos Arquivos
```csharp
// Use MMF para arquivos > 1MB
if (fileSize > 1024 * 1024)
{
    // Use Memory-Mapped File
}
else
{
    // Use File I/O tradicional
}
```

#### 2. Gerenciamento de Recursos
```csharp
// Sempre use using statements
using var mmf = MemoryMappedFile.CreateFromFile(file);
using var accessor = mmf.CreateViewAccessor(0, size);
// Recursos são liberados automaticamente
```

#### 3. Sincronização
```csharp
// Para IPC, use synchronization primitives
using var mutex = new Mutex(false, "MyAppMutex");
try
{
    if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
    {
        // Acesso seguro aos dados compartilhados
    }
}
finally
{
    mutex.ReleaseMutex();
}
```

#### 4. Tratamento de Erros
```csharp
try
{
    using var mmf = MemoryMappedFile.OpenExisting("SharedData");
    // Use MMF
}
catch (FileNotFoundException)
{
    // MMF não existe, trate apropriadamente
}
catch (UnauthorizedAccessException)
{
    // Sem permissão de acesso
}
```

## 🔧 Configurações

### Dependências
```xml
<PackageReference Include="System.IO.MemoryMappedFiles" Version="4.3.0" />
```

### Permissões
- Windows: Requer privilégios de "Create global objects"
- Linux: Requer acesso a `/dev/shm` para MMF nomeados
- macOS: Funciona com limitações de tamanho

## 📚 Recursos Adicionais

- [Memory-Mapped Files (Microsoft Docs)](https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files)
- [System.IO.MemoryMappedFiles Namespace](https://docs.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles)
- [Inter-Process Communication in .NET](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)
- [Virtual Memory Management](https://docs.microsoft.com/en-us/windows/win32/memory/virtual-memory-management)

## 🏷️ Tags
`memory-mapped-files`, `performance`, `ipc`, `file-io`, `shared-memory`, `large-files`, `optimization`, `system-programming`
