# Dica 59: Target-Typed New Expressions

## 📋 Resumo
Demonstra o uso de **Target-Typed New Expressions**, uma funcionalidade do C# 9+ que simplifica a criação de objetos eliminando a necessidade de repetir o tipo quando este pode ser inferido do contexto.

## 🎯 Conceitos Abordados

### 1. **Sintaxe Básica**
- Substituição de `new TipoCompleto()` por `new()`
- Inferência de tipo pelo compilador
- Compatibilidade com collections e objetos

### 2. **Collections e Arrays**
- Lists, Dictionaries, HashSets com sintaxe simplificada
- Arrays unidimensionais e multidimensionais
- Collections imutáveis (ImmutableList, ImmutableDictionary)

### 3. **Objetos Complexos**
- Classes com init-only properties
- Records e record structs
- Objetos aninhados e configurações

### 4. **Expressões Avançadas**
- Operadores ternários e condicionais
- Switch expressions
- Pattern matching

### 5. **Factory Methods e Builders**
- APIs fluentes com target-typed new
- Builder patterns simplificados
- Dependency injection scenarios

## 🔧 Funcionalidades Demonstradas

### Sintaxe Básica
```csharp
// Tradicional
List<string> traditional = new List<string> { "a", "b", "c" };

// Target-Typed (C# 9+)
List<string> targetTyped = new() { "a", "b", "c" };
```

### Collections Complexas
```csharp
Dictionary<string, List<Person>> groups = new()
{
    ["developers"] = new()
    {
        new("Alice", 25, "Senior Dev"),
        new("Bob", 30, "Tech Lead")
    },
    ["designers"] = new()
    {
        new("Carol", 28, "UX Designer")
    }
};
```

### Expressões Condicionais
```csharp
ResponseData result = input.IsValid 
    ? new() { Status = "Success", Message = "Valid input" }
    : new() { Status = "Error", Message = "Invalid input" };
```

### Switch Expressions
```csharp
HttpResponse response = contentType switch
{
    "json" => new() { ContentType = "application/json", Body = jsonData },
    "xml" => new() { ContentType = "application/xml", Body = xmlData },
    _ => new() { ContentType = "text/plain", Body = plainData }
};
```

## ⚡ Vantagens do Target-Typed New

### Redução de Código
- **Menos repetição**: Elimina duplicação de tipos longos
- **DRY Principle**: Don't Repeat Yourself aplicado
- **Manutenibilidade**: Mudanças de tipo em um local apenas

### Legibilidade
- **Foco no conteúdo**: Menos ruído sintático
- **Tipos complexos**: Especialmente útil com generics aninhados
- **Expressões claras**: Intenção mais evidente

### Robustez
- **Type Safety**: Mesma segurança de tipos
- **Compile-time**: Erros detectados em tempo de compilação
- **Refactoring**: IDEs podem renomear tipos automaticamente

## 📊 Performance

### Zero Overhead
- **IL idêntico**: Compilador gera o mesmo código
- **Runtime igual**: Nenhuma diferença de performance
- **Memory layout**: Mesma organização de memória

### Benchmarks Demonstram
- Criação de objetos: **0% diferença**
- Collections: **0% overhead**
- Expressões complexas: **Performance idêntica**

## 🎯 Casos de Uso Ideais

### Collections com Generics
```csharp
// Muito útil com tipos longos
Dictionary<string, List<CustomObject<ComplexType>>> data = new()
{
    ["key"] = new() { new() { /* ... */ } }
};
```

### Configurações e DTOs
```csharp
ApiConfiguration config = new()
{
    BaseUrl = "https://api.example.com",
    Timeout = TimeSpan.FromSeconds(30),
    Headers = new() { ["Authorization"] = "Bearer token" }
};
```

### Factory Methods
```csharp
public static IApiClient CreateClient(ClientOptions options = null)
{
    return new ApiClient(options ?? new()
    {
        Timeout = TimeSpan.FromMinutes(5),
        RetryPolicy = new() { MaxRetries = 3 }
    });
}
```

## ⚠️ Limitações e Considerações

### Requisitos
- **C# 9.0+**: Funcionalidade disponível apenas em versões recentes
- **.NET 5+**: Runtime support necessário
- **Tipo inferível**: Contexto deve permitir inferência

### Limitações
- **Tipos anônimos**: Ainda requer `new { ... }`
- **Apenas var**: Não funciona com `var` sozinho
- **Ambiguidade**: Tipo deve ser claro do contexto

### Quando NÃO usar
- **Tipos curtos**: `int`, `string` - ganho mínimo
- **Ambiguidade**: Quando o tipo não é óbvio
- **Compatibilidade**: Projetos que precisam suportar C# < 9

## 🔍 Comparação com Alternativas

| Abordagem | Legibilidade | Manutenibilidade | Performance | Compatibilidade |
|-----------|--------------|------------------|-------------|-----------------|
| **Target-Typed New** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Traditional New | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| var + new | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Factory Methods | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

## 🚀 Práticas Recomendadas

### Use Target-Typed New quando:
1. **Tipos longos**: Generics complexos ou namespaces longos
2. **Collections**: Especialmente com múltiplos níveis
3. **Expressões**: Ternários, switch expressions
4. **Factory methods**: Parâmetros opcionais e builders
5. **Configurações**: DTOs e option objects

### Evite quando:
1. **Tipos simples**: `int`, `string`, `bool`
2. **Ambiguidade**: Quando o tipo não é claro
3. **Compatibilidade**: Projetos legacy
4. **Overloads**: Múltiplos construtores ambíguos

## 🔗 Recursos Relacionados

- **C# 10 Features**: Global using, file-scoped namespaces
- **C# 11 Features**: Required members, generic attributes
- **Collections**: Span<T>, Memory<T>, ImmutableCollections
- **Pattern Matching**: Switch expressions, property patterns

## 📚 Próximos Passos

1. **Adote gradualmente**: Comece com casos óbvios
2. **Configure IDE**: Rules para sugerir target-typed new
3. **Code reviews**: Inclua na checklist de revisão
4. **Team guidelines**: Estabeleça padrões da equipe
5. **Combine com records**: Sintaxe ainda mais concisa
