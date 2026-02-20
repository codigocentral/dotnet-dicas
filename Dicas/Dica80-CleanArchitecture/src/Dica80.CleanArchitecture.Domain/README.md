# 🏗️ Domain Layer - Clean Architecture

## 📋 Visão Geral

Esta camada representa o **núcleo** da Clean Architecture, contendo:
- ✅ **Entidades de domínio** com regras de negócio
- ✅ **Value Objects** para conceitos importantes
- ✅ **Domain Events** para comunicação assíncrona
- ✅ **Interfaces de repositórios** (contratos)
- ✅ **Exceções específicas do domínio**

## 🎯 Princípios Aplicados

### **1. Dependency Inversion**
- A camada de domínio **não depende de nada**
- Define interfaces que outras camadas implementam
- Inversão de controle através de abstrações

### **2. Single Responsibility**
- Cada entidade tem uma responsabilidade única
- Separação clara entre diferentes conceitos
- Coesão alta dentro de cada classe

### **3. Domain Events**
- Comunicação assíncrona entre agregados
- Desacoplamento entre operações
- Auditoria e rastreabilidade

### **4. Value Objects**
- Encapsulamento de conceitos importantes
- Validação centralizada
- Imutabilidade por design

## 📁 Estrutura

```
Domain/
├── Common/
│   └── BaseEntity.cs           # Classe base para entidades
├── Entities/
│   ├── User.cs                 # Entidade usuário
│   ├── Project.cs              # Entidade projeto
│   ├── TaskItem.cs             # Entidade tarefa
│   └── Comment.cs              # Entidade comentário
├── ValueObjects/
│   └── CommonValueObjects.cs   # Email, Money, Priority
├── Events/
│   ├── UserAndProjectEvents.cs # Eventos de usuário/projeto
│   └── TaskAndCommentEvents.cs # Eventos de tarefa/comentário
├── Enums/
│   └── DomainEnums.cs          # Enumeradores do domínio
├── Repositories/
│   └── IRepositories.cs        # Interfaces dos repositórios
└── Exceptions/
    └── DomainExceptions.cs     # Exceções específicas
```

## ⚡ Principais Características

### **Entidades com Comportamento Rico**
```csharp
public class TaskItem : BaseEntity
{
    public void Complete()
    {
        if (Status == TaskStatus.Done)
            throw new InvalidOperationException("Task is already completed");

        Status = TaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new TaskCompletedEvent(this));
    }
}
```

### **Value Objects Validados**
```csharp
public class Email : ValueObject
{
    public static Email Create(string email)
    {
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");
        
        return new Email(email.ToLowerInvariant());
    }
}
```

### **Domain Events**
```csharp
public class TaskCompletedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskCompletedEvent(TaskItem task)
    {
        Task = task;
    }
}
```

## 🔄 Fluxo de Domain Events

1. **Entidade** gera evento durante operação
2. **Infrastructure** coleta eventos após SaveChanges
3. **MediatR** publica eventos para handlers
4. **Application** processa eventos assíncronos

## 🛡️ Validações e Regras

- **Validação de entrada** nos métodos de criação
- **Regras de negócio** dentro das entidades
- **Estado consistente** sempre mantido
- **Exceções específicas** para diferentes cenários

## 📊 Benefícios

✅ **Testabilidade**: Lógica isolada e sem dependências  
✅ **Manutenibilidade**: Regras centralizadas  
✅ **Flexibilidade**: Mudanças não afetam outras camadas  
✅ **Clareza**: Código expressa intenção do negócio  
✅ **Consistência**: Estado sempre válido  

## 🎨 Design Patterns Utilizados

- **Domain Events** - Comunicação assíncrona
- **Value Objects** - Encapsulamento de conceitos
- **Repository Pattern** - Abstração de persistência
- **Unit of Work** - Transações e consistência
- **Specification Pattern** - Regras de negócio complexas

---

💡 **Esta camada é o coração da aplicação - toda lógica de negócio importante deve estar aqui!**
