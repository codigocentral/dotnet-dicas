# 🏷️ Dica 31: Convenção de Underscore para Campos Privados

## 📋 Descrição
A convenção padrão para campos privados no .NET é iniciar com um underscore (`_`). O motivo é que o underscore define o escopo e facilita a distinção entre campos da classe e variáveis locais.

## 🎯 Problema Resolvido
- **Confusão de Escopo**: Sem o underscore, é difícil distinguir campos da classe de variáveis locais
- **Legibilidade**: Código mais claro e autodocumentado
- **Convenções .NET**: Seguir as diretrizes oficiais da Microsoft

## ✅ Solução
- Use underscore (`_`) no início de todos os campos privados
- Mantenha consistência em todo o projeto
- Evite usar `this.` quando o underscore já clarifica o escopo

## 🔧 Implementação

### ❌ Ruim (Sem Convenção)
```csharp
public class ContadorRuim
{
    private int contador;
    private string nome;
    
    public void Incrementar()
    {
        int contador = 5; // Confuso! Qual contador?
        this.contador += contador; // Precisa usar 'this.'
    }
}
```

### ✅ Bom (Com Underscore)
```csharp
public class ContadorBom
{
    private int _contador;
    private string _nome;
    
    public void Incrementar()
    {
        int contador = 5; // Claramente variável local
        _contador += contador; // Claramente campo da classe
    }
}
```

## 🎓 Benefícios
1. **Clareza de Código**: Distinção imediata entre campos e variáveis
2. **Padrão .NET**: Segue as convenções oficiais da Microsoft
3. **Menos Digitação**: Evita uso desnecessário de `this.`
4. **Manutenibilidade**: Facilita leitura e manutenção do código

## 📊 Comparação com Outras Convenções

| Convenção | Exemplo | Uso |
|-----------|---------|-----|
| Underscore | `_campo` | **Recomendado** - Padrão .NET |
| PascalCase | `Campo` | ❌ Conflita com propriedades |
| camelCase | `campo` | ❌ Confunde com variáveis locais |
| Hungarian | `intCampo` | ❌ Obsoleto |

## 🛠️ Como Executar
```bash
cd Dicas/Dica31-ConvencaoUnderscore/Dica31.ConvencaoUnderscore
dotnet run
```

## 📚 Referências
- [Microsoft Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
