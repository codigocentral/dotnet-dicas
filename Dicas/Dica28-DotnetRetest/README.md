# Dica 28: Retestando Testes Falhos (dotnet retest)

## 📋 Problema
Testes flaky (intermitentes) são aqueles que às vezes passam e às vezes falham, mesmo sem mudanças no código. Isso pode acontecer por:
- Condições de corrida
- Dependências externas instáveis
- Problemas de timing em testes assíncronos
- Estado compartilhado entre testes

## ✅ Solução
Use a ferramenta `dotnet retest` que automaticamente reexecuta testes que falharam, tentando fazê-los passar.

## 🔧 Instalação
```bash
dotnet tool install -g dotnet-retest
```

## 💡 Como Usar

### Uso Básico
```bash
# Executar testes com retest automático
dotnet retest

# Especificar número máximo de tentativas
dotnet retest --retry-count 3

# Retest apenas testes específicos
dotnet retest --filter "Category=Integration"
```

### Configuração Avançada
```bash
# Delay entre tentativas
dotnet retest --delay 1000

# Verbose output
dotnet retest --verbose

# Retest apenas quando há falhas específicas
dotnet retest --retry-count 5 --delay 500
```

## ⚠️ Importante
**Nota crítica**: `dotnet retest` é uma ferramenta de conveniência, **mas você deve realmente consertar seus testes flaky**. 

### Por que não confiar apenas no retest:
- Testes flaky indicam problemas reais no código
- Podem mascarar bugs importantes
- Reduzem a confiança na suite de testes
- Aumentam o tempo de build

## 🛠️ Melhores Práticas

### 1. Identifique a Causa Raiz
```csharp
// ❌ Teste flaky - condição de corrida
[Fact]
public async Task ProcessData_ShouldComplete()
{
    var processor = new DataProcessor();
    var task = processor.ProcessAsync();
    
    // Problema: não aguarda a conclusão
    Assert.True(processor.IsCompleted);
}

// ✅ Teste corrigido
[Fact]
public async Task ProcessData_ShouldComplete()
{
    var processor = new DataProcessor();
    await processor.ProcessAsync();
    
    Assert.True(processor.IsCompleted);
}
```

### 2. Use Timeouts Apropriados
```csharp
// ❌ Sem timeout - pode travar indefinidamente
[Fact]
public async Task ApiCall_ShouldReturnData()
{
    var result = await httpClient.GetAsync("/api/data");
    Assert.True(result.IsSuccessStatusCode);
}

// ✅ Com timeout
[Fact]
public async Task ApiCall_ShouldReturnData()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var result = await httpClient.GetAsync("/api/data", cts.Token);
    Assert.True(result.IsSuccessStatusCode);
}
```

### 3. Isole Estado Entre Testes
```csharp
// ❌ Estado compartilhado
public class UserServiceTests
{
    private static readonly List<User> _users = new();
    
    [Fact]
    public void AddUser_ShouldIncreaseCount()
    {
        _users.Add(new User("John"));
        Assert.Equal(1, _users.Count); // Falha se outro teste executou antes
    }
}

// ✅ Estado isolado
public class UserServiceTests
{
    [Fact]
    public void AddUser_ShouldIncreaseCount()
    {
        var users = new List<User>();
        users.Add(new User("John"));
        Assert.Equal(1, users.Count);
    }
}
```

## 📊 Exemplo Prático

### Cenário: Teste de Integração Flaky
```bash
# Executar com retest para identificar padrões
dotnet retest --retry-count 5 --verbose --filter "Category=Integration"

# Output exemplo:
# Test 'DatabaseConnectionTest' failed on attempt 1/5
# Test 'DatabaseConnectionTest' failed on attempt 2/5  
# Test 'DatabaseConnectionTest' passed on attempt 3/5
# 
# Summary: 1 test required retries
```

### Análise do Resultado
Se um teste precisa de múltiplas tentativas para passar:
1. Investigue logs detalhados
2. Adicione mais logging/debugging
3. Revise dependências externas
4. Considere usar TestContainers para isolamento

## 🎯 Quando Usar dotnet retest

### ✅ Cenários Apropriados:
- Durante investigação de testes flaky
- Builds de CI/CD temporariamente
- Desenvolvimento local para identificar padrões
- Validação de correções de testes flaky

### ❌ Não Use Como Solução Permanente:
- Em pipelines de produção sem investigação
- Como substituto para corrigir testes
- Para mascarar problemas conhecidos
- Em testes críticos de segurança

## 🔗 Recursos Adicionais
- [Documentação dotnet-retest](https://github.com/ViktorHofer/dotnet-retest)
- [Estratégias para testes determinísticos](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Debugging de testes flaky](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
