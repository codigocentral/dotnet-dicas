# Dica 101: Record Types - Tipos Imutáveis e Funcionais

## 📖 Descrição

Esta dica demonstra o uso de **Record Types** em C# 9+ para criar tipos imutáveis, comparação por valor, e funcionalidades modernas que facilitam a programação funcional e reduzem boilerplate code.

## 🎯 Problema

Criar classes imutáveis tradicionais requer muito código repetitivo (boilerplate):
- Propriedades somente leitura
- Construtor para todas as propriedades  
- Implementar `IEquatable<T>`
- Override de `GetHashCode()` e `Equals()`
- Override de `ToString()`

## ✅ Solução

**Record Types** oferecem:
- **Imutabilidade por padrão** - propriedades init-only
- **Comparação por valor** - automática
- **Desestruturação** - extrair valores facilmente  
- **Expressões `with`** - criar cópias modificadas
- **ToString() automático** - representação legível
- **Herança de records** - suporte completo

## 🔧 Implementação

### Record Classes vs Record Structs
- **`record class`** (referência) - para objetos maiores, herança
- **`record struct`** (valor) - para dados pequenos, alta performance
- **`readonly record struct`** - completamente imutável

### Casos de Uso Ideais
1. **DTOs e Models** - transferência de dados
2. **Value Objects** - representar conceitos de domínio
3. **Configurações** - objetos de configuração imutáveis
4. **Estados** - representar estados em máquinas de estado

## 📊 Benefícios

- **Menos código** - reduz boilerplate significativamente
- **Imutabilidade** - thread-safe por padrão
- **Comparação por valor** - semântica mais natural
- **Performance** - otimizações automáticas do compilador

## 🚀 Como Executar

```bash
dotnet run
```

## 📈 Benchmark

Execute o benchmark para ver a diferença de performance:

```bash
dotnet run --configuration Release
```
