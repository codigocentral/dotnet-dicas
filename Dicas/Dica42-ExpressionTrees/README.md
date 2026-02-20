# Dica 42: Expression Trees - Metaprogramação Avançada

Esta demonstração apresenta o uso avançado de **Expression Trees** em C# para metaprogramação, compilação dinâmica de código e otimização de performance.

## 🎯 Objetivos

- Demonstrar construção e manipulação de Expression Trees
- Mostrar compilação dinâmica vs interpretação
- Implementar análise de expressions usando Visitor Pattern
- Comparar performance entre diferentes abordagens
- Criar factories dinâmicos e query builders
- Implementar proxy dinâmico com interceptação

## 🌟 Principais Conceitos

### 1. Construção Básica de Expression Trees

```csharp
// Expression simples: x => x * 2
var param = Expression.Parameter(typeof(int), "x");
var multiply = Expression.Multiply(param, Expression.Constant(2));
var lambda = Expression.Lambda<Func<int, int>>(multiply, param);
```

### 2. Compilação vs Interpretação

- **Compilado**: ~3ms para 1 milhão de operações
- **Interpretado**: ~15,300ms (5,100x mais lento)
- **Conclusão**: Sempre compile expressions para performance

### 3. Visitor Pattern para Análise

```csharp
public class OperationCounterVisitor : ExpressionVisitor
{
    public Dictionary<string, int> OperationCounts { get; } = new();

    protected override Expression VisitBinary(BinaryExpression node)
    {
        var operation = node.NodeType.ToString();
        OperationCounts[operation] = OperationCounts.GetValueOrDefault(operation) + 1;
        return base.VisitBinary(node);
    }
}
```

### 4. Factory Dinâmico

- Criação de objetos baseada em expressions compiladas
- Cache de constructors para performance
- 100k objetos criados em ~7ms

### 5. Query Builder Dinâmico

- Construção de predicados em tempo de execução
- Combinação de filtros com AND/OR
- Sorting dinâmico

## 📊 Resultados de Performance

| Abordagem | Tempo (1M iterações) | Multiplicador |
|-----------|---------------------|---------------|
| Método tradicional | 3ms | 1x (baseline) |
| Expression compilado | 3ms | 1x |
| Expression interpretado | ~15,300ms | 5,100x mais lento |

## ✅ Boas Práticas

1. **Compile apenas uma vez** e reutilize expressions
2. **Use cache** para expressions computadas dinamicamente
3. **Prefira Expression.Lambda\<T\>** para type safety
4. **Implemente Visitor pattern** para análise complexa
5. **Considere performance**: compiled >> interpreted
6. **Trate exceptions** em expressions complexas
7. **Use Expression.Constant com cuidado** (boxing)

## 🎯 Casos de Uso Recomendados

- **ORM query building** (Entity Framework)
- **Dynamic serialization/deserialization**
- **Configuration-driven business rules**
- **Dynamic proxy generation**
- **Validation framework building**
- **Mathematical expression evaluation**

## ⚠️ Cuidados e Limitações

- Expression compilation tem **overhead inicial**
- **Nem todos os constructs C#** são suportados
- **Debugging é mais complexo**
- **Memory usage** pode ser maior
- **Threading**: expressions são thread-safe quando compiladas

## 🚀 Como Executar

```bash
dotnet run
```

## 📋 Funcionalidades Demonstradas

1. ✅ Construção básica de expressions
2. ✅ Compilação e execução dinâmica
3. ✅ Análise com Visitor Pattern
4. ✅ Comparação de performance
5. ✅ Factory dinâmico com cache
6. ✅ Query builder flexível
7. ✅ Proxy dinâmico com interceptação

Expression Trees são uma ferramenta poderosa para metaprogramação em C#, oferecendo flexibilidade única para criar código que gera e manipula código em tempo de execução com excelente performance quando usado corretamente.
