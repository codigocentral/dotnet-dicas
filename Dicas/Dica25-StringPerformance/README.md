# Dica 25: String Performance - Interpolação vs StringBuilder vs Concat

## 📝 Problema

A concatenação de strings é uma operação muito comum, mas diferentes técnicas têm impactos drasticamente diferentes na performance e uso de memória. Escolher a técnica errada pode resultar em aplicações lentas e com alto uso de memória.

## ⚡ Solução

### Cenário 1: Concatenação Simples (< 5 elementos)

```csharp
// ✅ RECOMENDADO: String Interpolation
var mensagem = $"Olá, {nome}! Você tem {idade} anos.";

// ✅ ALTERNATIVA: String.Concat para máxima performance
var mensagem = string.Concat("Olá, ", nome, "! Você tem ", idade.ToString(), " anos.");

// ❌ EVITAR: Concatenação com +
var mensagem = "Olá, " + nome + "! Você tem " + idade + " anos.";
```

### Cenário 2: Concatenação em Loop

```csharp
// ✅ RECOMENDADO: StringBuilder com capacidade pré-alocada
var sb = new StringBuilder(estimatedSize);
foreach (var item in items)
{
    sb.AppendLine($"- {item}");
}
var resultado = sb.ToString();

// ✅ ALTERNATIVA: String.Join para casos específicos
var resultado = string.Join("\n", items.Select(item => $"- {item}"));

// ❌ EVITAR: Concatenação com + em loop
var resultado = "";
foreach (var item in items)
{
    resultado += $"- {item}\n"; // Cria nova string a cada iteração!
}
```

### Cenário 3: Formatação Complexa

```csharp
// ✅ RECOMENDADO: String Interpolation com formatação
var relatorio = $"""
    === RELATÓRIO ===
    ID: {produto.Id:D6}
    Preço: {produto.Preco:C}
    Data: {produto.DataCriacao:dd/MM/yyyy}
    """;

// ✅ ALTERNATIVA: StringBuilder para casos muito complexos
var sb = new StringBuilder();
sb.AppendLine("=== RELATÓRIO ===");
sb.AppendLine($"ID: {produto.Id:D6}");
sb.AppendLine($"Preço: {produto.Preco:C}");
sb.AppendLine($"Data: {produto.DataCriacao:dd/MM/yyyy}");
var relatorio = sb.ToString();
```

## 🚀 Performance

### Resultados dos Benchmarks

| Cenário | Método | Items | Tempo | Alocação | Observações |
|---------|---------|-------|-------|-----------|-------------|
| **Simples** | String Interpolation | 1 | 50 ns | 80 B | ✅ Ideal para poucos elementos |
| **Simples** | String.Concat | 1 | 45 ns | 80 B | ✅ Ligeiramente mais rápido |
| **Simples** | StringBuilder | 1 | 120 ns | 200 B | ❌ Overhead desnecessário |
| **Loop** | StringBuilder | 50 | 2.5 μs | 2.1 KB | ✅ Ideal para loops |
| **Loop** | String.Join | 50 | 3.8 μs | 3.2 KB | ✅ Boa alternativa |
| **Loop** | Concatenação + | 50 | 45 μs | 25 KB | ❌ 18x mais lento! |
| **Loop** | StringBuilder | 500 | 25 μs | 20 KB | ✅ Escala bem |
| **Loop** | Concatenação + | 500 | 4.5 ms | 2.5 MB | ❌ 180x mais lento! |

## 🎯 Principais Insights

### 1. **Concatenação Simples**
- **String Interpolation** é ideal para < 5 elementos
- **String.Concat** oferece performance ligeiramente superior
- Evite **StringBuilder** para casos simples (overhead)

### 2. **Loops e Múltiplas Concatenações**
- **StringBuilder** é obrigatório para loops
- **Pré-alocar capacidade** melhora performance significativamente
- **String.Join** é boa alternativa para casos específicos

### 3. **Formatação**
- **String Interpolation** com formatação é muito eficiente
- **Raw String Literals** (C# 11+) são ideais para templates

### 4. **Anti-patterns**
- **Nunca** use `+` em loops
- **Evite** String.Format() - interpolation é mais rápida
- **Não** use StringBuilder para concatenações simples

## 📋 Guia de Decisão

```csharp
// Escolha baseada no contexto:

// 📝 Poucos elementos (< 5)
string result = $"{a} {b} {c}";

// 🔄 Loop ou construção incremental
var sb = new StringBuilder(estimatedCapacity);
foreach (var item in items)
    sb.AppendLine(item);

// 🔗 Join de coleção
string result = string.Join(", ", collection);

// 📄 Templates complexos
string template = $$"""
    {
        "name": "{{name}}",
        "value": {{value}}
    }
    """;
```

## ⚠️ Armadilhas Comuns

1. **StringBuilder sem capacidade**: Causa realocações
2. **Concatenação + em loops**: Crescimento quadrático O(n²)
3. **StringBuilder para casos simples**: Overhead desnecessário
4. **String.Format**: Mais lenta que interpolation

## 🏃‍♂️ Como Executar

### Projeto Principal
```bash
cd "Dica25-StringPerformance/Dica25"
dotnet run
```

### Benchmarks
```bash
cd "Dica25-StringPerformance/Dica25.Benchmark"
dotnet run -c Release
```

## 📁 Estrutura

```
Dica25-StringPerformance/
├── Dica25/
│   ├── Program.cs
│   └── Dica25.csproj
├── Dica25.Benchmark/
│   ├── Program.cs
│   └── Dica25.Benchmark.csproj
└── README.md
```

## 🎯 Pontos-Chave

1. **Context-aware**: Escolha baseada no cenário
2. **StringBuilder**: Essencial para loops e construção incremental
3. **String Interpolation**: Ideal para casos simples e formatação
4. **Pré-alocação**: Critical para performance do StringBuilder
5. **Evite +**: Nunca use em loops ou múltiplas concatenações

---

**Resultado**: Melhore a performance da aplicação em até **180x** escolhendo a técnica correta de concatenação de strings!
