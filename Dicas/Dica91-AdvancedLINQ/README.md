# 🔍 Dica 91: Advanced LINQ Expressions (.NET 9)

## 📋 Sobre

Esta dica demonstra técnicas avançadas de LINQ em .NET 9, incluindo Expression Trees, LINQ dinâmico, operadores customizados, Parallel LINQ, otimizações com Memory/Span e técnicas de otimização de queries.

## 🎯 Conceitos Demonstrados

### 1. 🌳 Expression Trees Avançadas

- Construção dinâmica de Expression Trees
- Composição de expressões complexas
- Compilação e execução de expressões
- Análise de propriedades com reflection

### 2. ⚡ LINQ Dinâmico

- Construção de queries baseadas em critérios
- Filtros dinâmicos configuráveis
- Ordenação multi-critério dinâmica
- Seletores de propriedades genéricos

### 3. 🔧 Custom LINQ Operators

- Extension methods personalizados
- Operador Batch para processamento em lotes
- Operador Window para análise de janelas
- Integração com LINQ padrão

### 4. 🚀 Parallel LINQ Avançado

- Configuração de grau de paralelismo
- Particionamento personalizado
- Tratamento de exceções em PLINQ
- Comparação de performance

### 5. 💾 LINQ com Memory e Span

- Processamento otimizado com Memory<T>
- Operações com Span<T> e ReadOnlySpan<T>
- LINQ-like operations sem alocações
- Integração com stack allocation

### 6. 📊 Query Optimization

- Evitar múltiplas enumerações
- Uso de ToLookup para performance
- Lazy vs Eager evaluation
- Métricas de performance

## 🚀 Como Executar

```bash
dotnet run
```

## 📊 Saída Esperada

```
🔍 Dica 91: Advanced LINQ Expressions (.NET 9)
===============================================

1. 🌳 Expression Trees Avançadas:
──────────────────────────────────
🌳 Construindo Expression Trees dinamicamente:
✅ Expressão: x => (x > 5)
✅ Números > 5: [6, 8, 9]

🌳 Expression Tree complexa para Employee:
✅ Expression Tree: emp => ((emp.Department == "Engineering") AndAlso (emp.Salary > 75000))
✅ Engenheiros com salário > $75k: 1
   Carlos: $80,000

2. ⚡ LINQ Dinâmico:
────────────────────
⚡ LINQ Queries dinâmicas baseadas em critérios:
✅ Filtros aplicados: 3
✅ Resultados encontrados: 4
...

3. 🔧 Custom LINQ Operators:
────────────────────────────
🔧 Batch Processing:
✅ Total de lotes: 3
   Lote 1: 3 funcionários
...

4. 🚀 Parallel LINQ Avançado:
──────────────────────────────
✅ Primos encontrados (PLINQ): 78,498 em 31ms
✅ Soma dos primeiros 1000 pares: 1,001,000 em 2ms
...

5. 💾 LINQ com Memory e Span:
──────────────────────────────
✅ Processado 1000 elementos em 10 chunks
✅ Média geral dos chunks: 500.50
...

6. 📊 Query Optimization:
─────────────────────────
✅ Lookup criado em 60583 ticks
✅ Query lazy executada em 407542 ticks
✅ Query eager executada em 148792 ticks
```

## 🔧 Funcionalidades

### Expression Trees

- ✅ Construção dinâmica de expressões
- ✅ Compilação e execução em runtime
- ✅ Análise de propriedades com reflection
- ✅ Composição de expressões complexas

### LINQ Dinâmico

- ✅ Filtros configuráveis em runtime
- ✅ Ordenação multi-critério
- ✅ Seletores de propriedades genéricos
- ✅ Queries baseadas em dicionários

### Custom Operators

- ✅ Extension methods para LINQ
- ✅ Processamento em lotes (Batch)
- ✅ Análise de janelas (Window)
- ✅ Integração transparente

### Parallel LINQ

- ✅ Configuração de paralelismo
- ✅ Particionamento otimizado
- ✅ Tratamento robusto de exceções
- ✅ Métricas de performance

### Memory/Span Integration

- ✅ Processamento sem alocações
- ✅ Operações em chunks
- ✅ Stack allocation integration
- ✅ Performance otimizada

### Query Optimization

- ✅ Prevenção de múltiplas enumerações
- ✅ Lookup tables para performance
- ✅ Análise lazy vs eager
- ✅ Profiling detalhado

## 🎓 Conceitos Aprendidos

- **Expression Trees**: Metaprogramação e análise de código
- **Dynamic Queries**: Construção de queries em runtime
- **Custom Extensions**: Extensão do LINQ com operadores próprios
- **Parallel Processing**: Processamento paralelo eficiente
- **Memory Management**: Otimizações de memória e performance
- **Query Optimization**: Técnicas de otimização de consultas

## 📚 Referências

- [Expression Trees in C#](https://docs.microsoft.com/en-us/dotnet/csharp/expression-trees/)
- [PLINQ Documentation](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/parallel-linq-plinq)
- [Memory and Span](https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/)
- [LINQ Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/linq/)
