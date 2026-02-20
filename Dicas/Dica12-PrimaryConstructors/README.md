# Dica 12: Primary Constructors no C# 12

## 📋 Descrição

Esta dica demonstra a poderosa funcionalidade de **Primary Constructors** introduzida no C# 12, que permite definir construtores diretamente na declaração da classe, struct ou record, resultando em código mais conciso e legível.

## 🎯 Objetivo de Aprendizado

Entender como utilizar Primary Constructors para simplificar drasticamente a criação de classes, reduzir código boilerplate e melhorar a legibilidade, especialmente em cenários como DTOs, Value Objects, Dependency Injection e validação de dados.

## ⚡ Nova Sintaxe do C# 12

### Antes (Sintaxe Tradicional)

```csharp
public class PessoaTradicional
{
    public string Nome { get; }
    public int Idade { get; }
    
    public PessoaTradicional(string nome, int idade)
    {
        Nome = nome;
        Idade = idade;
    }
}
```

### Depois (Primary Constructors C# 12)

```csharp
public class PessoaModerna(string nome, int idade)
{
    public string Nome { get; } = nome;
    public int Idade { get; } = idade;
}
```

## 🔧 Casos de Uso Práticos

### 1. **DTOs e Records**

```csharp
public record Endereco(string Rua, string Numero, string Cidade, string Estado);

public record Coordenada(double Latitude, double Longitude)
{
    public override string ToString() => $"({Latitude:F4}, {Longitude:F4})";
}
```

### 2. **Value Objects com Validação**

```csharp
public class Email(string email)
{
    public string Valor { get; } = IsValidEmail(email) ? email 
        : throw new ArgumentException("Email inválido", nameof(email));
    
    private static bool IsValidEmail(string email) => email.Contains('@') && email.Contains('.');
}
```

### 3. **Dependency Injection**

```csharp
public class UsuarioService(IUsuarioRepository repository, ILogger logger)
{
    public void CriarUsuario(string nome, string email)
    {
        logger.Log($"Criando usuário: {nome}");
        repository.Salvar(nome, email);
        logger.Log("Usuário criado com sucesso");
    }
}
```

### 4. **Herança com Primary Constructors**

```csharp
public class Pessoa(string nome, int idade)
{
    public string Nome { get; } = nome;
    public int Idade { get; } = idade;
}

public class Funcionario(string nome, int idade, string cargo, decimal salario) 
    : Pessoa(nome, idade)
{
    public string Cargo { get; } = cargo;
    public decimal Salario { get; } = salario;
}
```

## 📊 Demonstrações Incluídas

1. **Comparação de Sintaxe** - Tradicional vs. Primary Constructors
2. **Validação e Transformação** - Email e CPF com validação automática
3. **Dependency Injection** - Serviços com injeção de dependência limpa
4. **Herança** - Funcionário herdando de Pessoa
5. **Imutabilidade** - Structs e records imutáveis
6. **Propriedades Computadas** - Cálculos automáticos (Área, MegaPixels)
7. **Interfaces** - Implementação com Primary Constructors
8. **Análise de Redução de Código** - Comparação quantitativa

## ✅ Vantagens dos Primary Constructors

- **🎨 Sintaxe Concisa**: Até 70% menos código que sintaxe tradicional
- **📖 Legibilidade**: Declaração e inicialização em uma linha
- **🔧 Flexibilidade**: Funciona com classes, structs, records e interfaces
- **⚡ Validação**: Suporte completo a validação no construtor
- **🏗️ Herança**: Integração perfeita com herança e polimorfismo
- **🛠️ DI-Friendly**: Ideal para injeção de dependência
- **🔒 Imutabilidade**: Facilita criação de tipos imutáveis

## 🎯 Quando Usar

- **DTOs e Records** para transferência de dados
- **Value Objects** com validação integrada
- **Services com DI** para injeção de dependência limpa
- **Configuration Objects** para configurações tipadas
- **Immutable Types** para tipos imutáveis thread-safe
- **Domain Models** para modelos de domínio concisos

## 🏗️ Estrutura do Projeto

```bash
Dica12-PrimaryConstructors/
├── Dica12.PrimaryConstructors/
│   ├── Program.cs                    # Demonstração completa
│   └── Dica12.PrimaryConstructors.csproj
└── README.md                         # Esta documentação
```

## 🚀 Como Executar

```bash
cd "Dica12-PrimaryConstructors/Dica12.PrimaryConstructors"
dotnet run
```

## 🔍 Pontos de Aprendizado

1. **Sintaxe**: Use `(parâmetros)` após o nome da classe/struct/record
2. **Inicialização**: Parâmetros ficam disponíveis em toda a classe
3. **Validação**: Pode incluir lógica de validação nas propriedades
4. **Herança**: Chame base constructor com `: BaseClass(params)`
5. **Imutabilidade**: Combine com `readonly` para tipos imutáveis
6. **DI**: Perfeito para serviços com dependências injetadas

## 💡 Dicas Importantes

- Parâmetros do Primary Constructor são "captured" automaticamente
- Pode usar tanto com propriedades automáticas quanto com backing fields
- Validação pode ser feita nas propriedades ou em métodos
- Funciona perfeitamente com records para máxima concisão
- Mantém compatibilidade total com construtores adicionais
- Ideal para cenários onde construtor é simples e direto

## 🎓 Conceitos Relacionados

- **DTOs**: Data Transfer Objects com menos boilerplate
- **Value Objects**: Objetos de valor com validação integrada
- **Dependency Injection**: Injeção de dependência simplificada
- **Immutable Types**: Tipos imutáveis thread-safe
- **Builder Pattern**: Alternativa ao builder para objetos simples
- **Record Types**: Combinação perfeita com records do C# 9+

## 📈 Impacto na Produtividade

- **Redução de Código**: 50-70% menos linhas para classes simples
- **Menos Erros**: Menos código significa menos bugs
- **Manutenibilidade**: Código mais limpo e fácil de manter
- **Legibilidade**: Intenção mais clara e código autodocumentado
