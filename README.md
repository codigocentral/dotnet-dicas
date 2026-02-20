# 100 Dicas de C# - Repositorio Educacional

Bem-vindo! Este repositorio contem mais de 100 dicas praticas de C#, cada uma implementada como um projeto independente que voce pode executar, estudar e modificar.

## Indice Rapido

- [Como Comecar](#como-comecar)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Lista de Dicas](#lista-de-dicas)
- [Como Executar](#como-executar)
- [Perguntas Frequentes](#perguntas-frequentes)

---

## Como Comecar

### O que voce precisa

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** - Versao mais recente
- Um editor de codigo:
  - [Visual Studio Code](https://code.visualstudio.com/) (gratuito)
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (gratuito para uso individual)
  - Ou qualquer editor de sua preferencia

### Verificando a instalacao

```bash
dotnet --version
# Deve mostrar 10.0.x ou superior
```

### Primeiros passos

1. **Clone o repositorio**
   ```bash
   git clone https://github.com/seu-usuario/didaticos-dicas-csharp.git
   cd didaticos-dicas-csharp
   ```

2. **Restaure os pacotes NuGet**
   ```bash
   dotnet restore
   ```

3. **Execute sua primeira dica**
   ```bash
   dotnet run --project "Dicas/Dica01-RetornandoColecoesVazias/Dica01"
   ```

---

## Estrutura do Projeto

```
didaticos-dicas-csharp/
├── DicasCSharp.sln              # Solution principal
├── README.md                    # Este arquivo
├── Dicas/                       # Todas as dicas
│   ├── Dica01-RetornandoColecoesVazias/
│   │   ├── Dica01/              # Projeto de demonstracao
│   │   │   ├── Program.cs       # Codigo principal
│   │   │   └── Dica01.csproj    # Arquivo de projeto
│   │   └── Dica01.Benchmark/    # Benchmark de performance (opcional)
│   ├── Dica02-RelancandoExcecoesCorretamente/
│   └── ...
```

### Tipos de projetos

| Tipo | Descricao | Quando executar |
|------|-----------|-----------------|
| **Demo** (`DicaXX/`) | Demonstracao didatica do conceito | `dotnet run` |
| **Benchmark** (`DicaXX.Benchmark/`) | Medicao de performance | `dotnet run -c Release` |
| **Tests** (`DicaXX.Tests/`) | Testes automatizados | `dotnet test` |

---

## Como Executar

### Executar uma demonstracao

```bash
# Formato basico
dotnet run --project "Dicas/DicaXX-Nome/DicaXX"

# Exemplo: Dica 01
dotnet run --project "Dicas/Dica01-RetornandoColecoesVazias/Dica01"
```

### Executar um benchmark

Benchmarks medem performance e **devem** ser executados em Release mode:

```bash
# Formato basico
dotnet run -c Release --project "Dicas/DicaXX-Nome/DicaXX.Benchmark"

# Exemplo: Dica 01
dotnet run -c Release --project "Dicas/Dica01-RetornandoColecoesVazias/Dica01.Benchmark"
```

### Executar testes

```bash
# Todos os testes
dotnet test

# Testes de uma dica especifica
dotnet test "Dicas/Dica33-TestesSnapshotComVerify/Dica33.TestesSnapshotComVerify.Tests"

# Um teste especifico
dotnet test --filter "FullyQualifiedName~NomeDoMetodoDeTeste"
```

### Compilar a solution completa

```bash
# Debug (desenvolvimento)
dotnet build

# Release (producao/benchmarks)
dotnet build -c Release
```

---

## Lista de Dicas

### Performance e Otimizacao

| Dica | Nome | O que voce vai aprender |
|------|------|-------------------------|
| 01 | Retornando Colecoes Vazias | Evitar alocacoes desnecessarias com `Array.Empty<T>()` |
| 04 | Armadilhas de Performance no LINQ | Evitar multiplas enumeracoes |
| 06 | Acessando Span de Lista | Performance com `CollectionsMarshal.AsSpan()` |
| 09 | ToList vs ToArray | Diferencas e quando usar cada um |
| 25 | Performance de Strings | Interpolacao vs StringBuilder vs Concat |
| 48 | Usando stackalloc | Alocacao na stack para alta performance |
| 51 | ArrayPool | Reutilizacao de arrays |
| 73 | ValueTask vs Task | Quando usar ValueTask |
| 99 | Method Inlining | Otimizacao com AggressiveInlining |
| 100 | Compiled Regex | Regex.CompileToAssembly |

### Boas Praticas

| Dica | Nome | O que voce vai aprender |
|------|------|-------------------------|
| 02 | Relancando Excecoes | Preservar stack trace com `throw;` |
| 03 | Travamento com Async/Await | SemaphoreSlim vs lock |
| 05 | C# REPL | Prototipacao rapida com dotnet-script |
| 07 | Logging Correto | Structured logging com templates |
| 15 | CancellationTokens | Cancelamento cooperativo em APIs |
| 24 | Nullable Reference Types | Nulabilidade explicita |
| 35 | ConfigureAwait(false) | Quando e por que usar |
| 42 | Atribuicao Condicional Nula | Operador `??=` |
| 62 | Operador nameof | Refatoracao segura |
| 76 | Excecoes para Casos Excepcionais | Result Pattern |

### C# Moderno

| Dica | Nome | O que voce vai aprender |
|------|------|-------------------------|
| 08 | Tipos Vazios | Empty types e sua utilidade |
| 11 | StringSyntax Attribute | Syntax highlighting em strings |
| 12 | Primary Constructors | Construtores primarios em classes |
| 13 | UUIDv7 | Identificadores ordenaveis |
| 21 | Interpolated Parser | Parsing reverso de strings |
| 22 | Alias para Qualquer Tipo | using aliases avancados |
| 23 | DateTimeOffset vs DateTime | Trabalhando com timezones |
| 39 | Pattern Matching Switch | Switch expressions avancados |
| 58 | Record Types | Tipos imutaveis |
| 102 | Inicializadores C# 12 | Collection expressions `[]` |

### Arquitetura e Padroes

| Dica | Nome | O que voce vai aprender |
|------|------|-------------------------|
| 10 | Marcadores de Assembly | DI com interfaces marcadoras |
| 16 | Minimal APIs | APIs minimalistas do ASP.NET Core |
| 19 | Metodos WebApplication | Run, Use, Map middleware |
| 33 | Snapshot Testing | Verify.Xunit |
| 34 | Refit | Cliente HTTP tipado |
| 44 | MediatR | Padrao Mediator |
| 82 | nameof vs Reflexao | Performance em tempo de compilacao |

---

## Perguntas Frequentes

### Qual versao do .NET preciso?

**.NET 10.0 SDK** ou superior. Todos os projetos deste repositorio usam `net10.0`.

```bash
# Verifique sua versao
dotnet --version
```

### O que sao Benchmarks?

Benchmarks sao testes de performance que medem quanto tempo e memoria uma operacao consome. Usamos a biblioteca [BenchmarkDotNet](https://benchmarkdotnet.org/).

**Sempre execute benchmarks em Release mode** para resultados precisos:

```bash
dotnet run -c Release --project "Dicas/Dica01-RetornandoColecoesVazias/Dica01.Benchmark"
```

### Como escolher qual dica estudar?

1. **Iniciante**: Comece pelas Dicas 01, 02, 05, 12, 13
2. **Performance**: Dicas 01, 04, 06, 09, 25, 48, 51, 73, 99, 100
3. **Async/Await**: Dicas 03, 15, 27, 35, 73
4. **C# Moderno**: Dicas 08, 11, 12, 21, 22, 24, 39, 58, 102

### Posso contribuir?

Sim! Veja como:

1. Faca um fork do repositorio
2. Crie uma branch para sua contribuicao
3. Siga o padrao de codigo existente
4. Abra um Pull Request

### Encontrei um bug ou erro

Abra uma [Issue](../../issues) descrevendo:
- Qual dica apresenta o problema
- O que voce esperava que acontecesse
- O que realmente aconteceu
- Mensagens de erro (se houver)

---

## Padrao de Codigo

Cada dica segue este padrao:

```csharp
/*
 * Dica XX: Titulo da Dica
 * 
 * Explicacao breve do conceito e por que e importante.
 * Inclua contexto sobre quando usar e os beneficios.
 */

Console.WriteLine("=== Dica XX: Titulo ===\n");

// Demonstracao da abordagem recomendada
var resultado = MetodoRecomendado();
Console.WriteLine($"Resultado: {resultado}");

// Comparacao com abordagem problematica (quando aplicavel)
var resultadoRuim = MetodoProblematico();

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

// Implementacoes...
```

---

## Tecnologias e Bibliotecas

| Tecnologia | Versao | Uso |
|------------|--------|-----|
| .NET | 10.0 | Framework principal |
| C# | 14 | Linguagem |
| BenchmarkDotNet | 0.15.x | Benchmarks |
| xUnit | 2.8.x | Testes |
| Moq | 4.x | Mocks |
| Verify.Xunit | 24.x | Snapshot testing |

---

## Recursos Adicionais

### Documentacao Oficial
- [Microsoft C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET API Browser](https://docs.microsoft.com/en-us/dotnet/api/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/)

### Aprendizado
- [Microsoft Learn - C#](https://docs.microsoft.com/learn/dotnet/)
- [C# Fundamentals for Absolute Beginners](https://channel9.msdn.com/Series/C-Fundamentals-for-Absolute-Beginners)

### Ferramentas
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [NuGet Package Explorer](https://www.nuget.org/packages/NuGet.PackageExplorer.GUI)

---

## Licenca

Este projeto esta licenciado sob a [MIT License](LICENSE).

---

Feito com proposito educacional. Cada dica foi implementada para demonstrar conceitos especificos de C# e .NET que voce encontrara no dia a dia de desenvolvimento.

**Bons estudos!**
