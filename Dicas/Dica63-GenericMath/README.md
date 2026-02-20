# Dica 63: Generic Math - C# 11+

## 📋 Sobre

Esta dica demonstra o uso do **Generic Math** introduzido no C# 11 e .NET 7, que permite criar algoritmos matemáticos genéricos que funcionam com qualquer tipo numérico, utilizando as interfaces do `System.Numerics`.

## 🎯 Objetivo

Mostrar como usar interfaces matemáticas genéricas para:
- Criar algoritmos matemáticos reutilizáveis
- Trabalhar com diferentes tipos numéricos de forma unificada
- Implementar operações matemáticas type-safe
- Otimizar performance com especialização de tipos

## ✨ Funcionalidades Demonstradas

### 1. **Interfaces Matemáticas Fundamentais**
- `INumber<T>` - Interface base para todos os tipos numéricos
- `IAdditionOperators<T>` - Operadores de soma
- `IBinaryNumber<T>` - Operações binárias
- `IPowerFunctions<T>` - Funções de potência
- `IRootFunctions<T>` - Funções de raiz
- `ITrigonometricFunctions<T>` - Funções trigonométricas

### 2. **Algoritmos Matemáticos Genéricos**
- **Soma/Multiplicação genérica** para qualquer tipo numérico
- **Potência genérica** com otimização exponencial
- **Fatorial genérico** para int, long, BigInteger
- **MDC (Máximo Divisor Comum)** genérico

### 3. **Estatísticas Genéricas**
- **Média aritmética** para arrays de qualquer tipo numérico
- **Variância** com cálculo genérico
- **Desvio padrão** usando interfaces matemáticas

### 4. **Geometria Computacional**
- **Pontos 2D genéricos** com cálculo de distância
- **Círculos genéricos** com área e perímetro
- **Matrizes 2x2** com operações matriciais
- **Números complexos** com módulo e argumento

### 5. **Casos Práticos**
- **Juros compostos** com tipos de precisão diferentes
- **Interpolação linear** genérica
- **Conversão de unidades** (Fahrenheit para Celsius)
- **Cálculos financeiros** com decimal para precisão

## 🚀 Como Executar

### Projeto Principal
```bash
cd Dica63.GenericMath
dotnet run
```

### Benchmarks
```bash
cd Dica63.GenericMath.Benchmark
dotnet run -c Release
```

## 📊 Resultados de Performance

O Generic Math oferece:
- **Zero overhead** após otimização JIT
- **Type safety** em tempo de compilação
- **Reutilização** de algoritmos entre tipos
- **Performance** equivalente a implementações específicas

### Comparações Típicas:
- **Soma genérica vs tradicional**: ~0-5% overhead
- **Algoritmos complexos**: Performance idêntica
- **Tipos especializados**: Otimização automática pelo JIT

## 🎯 Casos de Uso Reais

### Bibliotecas Matemáticas
```csharp
// Algoritmo único funciona com int, double, decimal, BigInteger
public static T Fatorial<T>(T n) where T : INumber<T>
{
    // Implementação única para todos os tipos
}
```

### Computação Científica
```csharp
// Estatísticas genéricas para qualquer tipo numérico
public static T Media<T>(T[] dados) where T : INumber<T>
{
    // Uma implementação, múltiplos tipos
}
```

### Sistemas Financeiros
```csharp
// Cálculos precisos com decimal ou aproximados com double
public static T JurosCompostos<T>(T capital, T taxa, int periodo) 
    where T : INumber<T>, IPowerFunctions<T>
{
    // Flexibilidade de tipo baseada em necessidade de precisão
}
```

## ⚡ Vantagens

- **Reutilização**: Um algoritmo para todos os tipos numéricos
- **Type Safety**: Verificação em tempo de compilação
- **Performance**: Zero overhead após otimização JIT
- **Flexibilidade**: Suporte a tipos customizados
- **Consistência**: API unificada para operações matemáticas

## ⚠️ Considerações

- **Versão**: Requer C# 11+ e .NET 7+
- **Curva de Aprendizado**: Conhecimento das interfaces necessário
- **Debugging**: Stacktraces podem ser mais complexos
- **Compatibilidade**: Nem todos os tipos implementam todas as interfaces

## 🔗 Conceitos Relacionados

- **Static Abstract Members** (Dica 49)
- **Nullable Reference Types** (Dica 24)
- **Pattern Matching** (Dica 20)
- **Performance Optimization** (Dica 25)

## 📚 Referências

- [Generic Math no .NET 7](https://devblogs.microsoft.com/dotnet/dotnet-7-generic-math/)
- [System.Numerics Interfaces](https://docs.microsoft.com/en-us/dotnet/api/system.numerics)
- [C# 11 Features](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11)
