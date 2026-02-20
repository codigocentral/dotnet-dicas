# Dica 11: StringSyntax Attribute

## 📋 Visão Geral

Esta dica demonstra o uso do **StringSyntax Attribute**, uma funcionalidade introduzida no **.NET 7** que fornece dicas de sintaxe para o IntelliSense, melhorando a experiência de desenvolvimento ao trabalhar com strings que representam diferentes tipos de conteúdo.

## 🎯 Objetivo

Mostrar como o StringSyntax attribute melhora o suporte do IDE para diferentes tipos de strings como Regex, JSON, SQL, URIs e strings de formatação.

## ✨ Características do StringSyntax

### 1. **Suporte a Regex**
```csharp
public class RegexService
{
    public void ValidarEmail([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
        var regex = new Regex(pattern);
        // IDE oferece syntax highlighting e validação para regex
    }
}
```

### 2. **Suporte a JSON**
```csharp
public class JsonService
{
    public void ProcessarJson([StringSyntax(StringSyntaxAttribute.Json)] string jsonString)
    {
        // IDE oferece syntax highlighting para JSON
        var document = JsonDocument.Parse(jsonString);
    }
}
```

### 3. **Suporte a URI**
```csharp
public class UrlService
{
    public void ProcessarUrl([StringSyntax(StringSyntaxAttribute.Uri)] string url)
    {
        var uri = new Uri(url);
        // IDE oferece validação de URI
    }
}
```

### 4. **Suporte a SQL**
```csharp
public class SqlService
{
    public void ExecutarQuery([StringSyntax("sql")] string query)
    {
        // IDE oferece syntax highlighting para SQL
        Console.WriteLine($"Executando: {query}");
    }
}
```

### 5. **Strings de Formatação**
```csharp
public class FormattingService
{
    public void FormatarTexto([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object[] args)
    {
        var resultado = string.Format(format, args);
        // IDE oferece validação de formato
        Console.WriteLine(resultado);
    }
}
```

## 🔧 Exemplos Práticos

### Validação de Email com Regex
```csharp
var regexService = new RegexService();
// O IDE oferece syntax highlighting para o padrão regex
regexService.ValidarEmail(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
```

### Processamento de JSON
```csharp
var jsonService = new JsonService();
// O IDE oferece syntax highlighting para JSON
jsonService.ProcessarJson("""
{
    "nome": "João Silva",
    "idade": 30,
    "ativo": true
}
""");
```

### Validação de URLs
```csharp
var urlService = new UrlService();
// O IDE valida a sintaxe da URI
urlService.ProcessarUrl("https://www.exemplo.com/api/usuarios");
```

## 🚀 Como Executar

1. **Clone o repositório**
2. **Navegue até o diretório da Dica 11**
3. **Execute o projeto**

```bash
cd Dicas/Dica11-StringSyntax
dotnet run
```

## 📊 Saída Esperada

```
=== Demonstração do StringSyntax Attribute ===

1. Validação de Email com Regex:
Validando email com padrão: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$

2. Processamento de JSON:
Processando JSON: {
  "nome": "João Silva",
  "idade": 30,
  "ativo": true
}

3. Processamento de URL:
Processando URL: https://www.exemplo.com/api/usuarios
Host: www.exemplo.com
Caminho: /api/usuarios

4. Execução de Query SQL:
Executando: SELECT * FROM usuarios WHERE ativo = 1

5. Formatação de String:
Olá João, você tem 30 anos e está ativo: True
```

## 🔍 Conceitos Demonstrados

- **StringSyntax Attribute**: Fornece dicas de sintaxe para diferentes tipos de string
- **Regex Support**: Syntax highlighting e validação para expressões regulares
- **JSON Support**: Syntax highlighting para strings JSON
- **URI Support**: Validação de sintaxe para URIs
- **SQL Support**: Syntax highlighting para queries SQL
- **Composite Format**: Validação para strings de formatação

## 💡 Benefícios

- **Melhor Experiência de Desenvolvimento**: IDE oferece syntax highlighting apropriado
- **Detecção Precoce de Erros**: Validação em tempo de design
- **Maior Produtividade**: IntelliSense aprimorado para diferentes tipos de string
- **Código Mais Legível**: Intenção clara sobre o tipo de conteúdo esperado
- **Suporte a Múltiplos Formatos**: Regex, JSON, SQL, URI, formatação

## 📚 Referências

- [StringSyntax Attribute - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.stringsyntaxattribute)
- [.NET 7 New Features](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-7)
- [Code Analysis Attributes](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis)

