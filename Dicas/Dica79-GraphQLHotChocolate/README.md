# 🚀 Dica 79: GraphQL with HotChocolate

## 📋 Visão Geral

Esta dica demonstra a implementação de uma API GraphQL completa usando HotChocolate, a biblioteca GraphQL mais avançada para .NET. O projeto inclui um blog system com funcionalidades modernas como filtering, pagination, subscriptions em tempo real e autenticação.

## 🎯 Conceitos Demonstrados

### 1. 🔍 GraphQL Query Types
- **Queries complexas** com filtering, sorting e pagination
- **Projections automáticas** para otimização de performance
- **Data Loaders** para resolver o problema N+1
- **Consultas aninhadas** com relacionamentos complexos

### 2. 🔄 GraphQL Mutations
- **Operações CRUD** completas (Create, Read, Update, Delete)
- **Validação de entrada** com FluentValidation
- **Error handling** estruturado
- **Autorização** baseada em claims

### 3. 📡 GraphQL Subscriptions
- **Real-time updates** via WebSockets
- **Event-driven notifications** para posts e comentários
- **Filtered subscriptions** por usuário ou post específico
- **Topic-based messaging** para escalabilidade

### 4. 🔐 Autenticação e Autorização
- **JWT Authentication** integrado
- **Claims-based authorization** em mutations
- **User context** em resolvers
- **Secure endpoints** para operações sensíveis

### 5. 📊 Data Management
- **Entity Framework Core** integration
- **In-memory database** para demonstração
- **Database seeding** com dados realistas usando Bogus
- **Relacionamentos complexos** (User, Post, Comment, Tag)

## 🛠️ Tecnologias e Bibliotecas

- **.NET 9.0** - Framework principal
- **HotChocolate 14.1.0** - GraphQL server para .NET
- **Entity Framework Core 9.0** - ORM e data access
- **FluentValidation 11.11.0** - Validação de entrada
- **JWT Bearer Authentication** - Autenticação segura
- **Bogus 35.6.1** - Geração de dados fake para seeding

## 🏃‍♂️ Como Executar

### Iniciar o Servidor
```bash
cd Dica79-GraphQLHotChocolate
dotnet run
```

### Acessar Interfaces
- **GraphQL Playground**: http://localhost:5000/graphql
- **Demo Page**: http://localhost:5000/demo
- **Schema**: http://localhost:5000/graphql/schema.graphql

### Autenticação de Demonstração
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "demo@example.com", "password": "demo123"}'
```

## 📋 Exemplos de Uso

### Queries Básicas

#### Buscar Posts com Paginação
```graphql
query GetPosts {
  posts(first: 10, where: { isPublished: { eq: true } }) {
    nodes {
      id
      title
      content
      createdAt
      viewCount
      likeCount
      author {
        name
        profile {
          bio
          avatarUrl
        }
      }
      tags {
        name
        color
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    totalCount
  }
}
```

#### Buscar Post Específico
```graphql
query GetPost($id: Int!) {
  post(id: $id) {
    id
    title
    content
    author {
      name
      email
    }
    comments {
      id
      content
      author {
        name
      }
      replies {
        id
        content
        author {
          name
        }
      }
    }
    tags {
      name
      color
    }
  }
}
```

#### Busca Avançada com Filtros
```graphql
query SearchPosts {
  posts(
    where: { 
      and: [
        { title: { contains: "GraphQL" } }
        { createdAt: { gte: "2024-01-01" } }
        { author: { status: { eq: ACTIVE } } }
        { tags: { some: { name: { in: ["CSharp", "DotNet"] } } } }
      ]
    }
    order: [{ createdAt: DESC }, { likeCount: DESC }]
    first: 20
  ) {
    nodes {
      title
      author { name }
      tags { name }
      likeCount
    }
  }
}
```

### Mutations

#### Criar Usuário
```graphql
mutation CreateUser($input: CreateUserInput!) {
  createUser(input: $input) {
    user {
      id
      name
      email
      profile {
        bio
        website
      }
    }
    errors
  }
}
```

**Variables:**
```json
{
  "input": {
    "name": "João Silva",
    "email": "joao@example.com",
    "bio": "Desenvolvedor .NET especializado em GraphQL",
    "website": "https://joao.dev"
  }
}
```

#### Criar Post
```graphql
mutation CreatePost($input: CreatePostInput!) {
  createPost(input: $input) {
    post {
      id
      title
      content
      isPublished
      tags {
        name
        color
      }
    }
    errors
  }
}
```

#### Atualizar Post
```graphql
mutation UpdatePost($input: UpdatePostInput!) {
  updatePost(input: $input) {
    post {
      id
      title
      content
      updatedAt
    }
    errors
  }
}
```

### Subscriptions

#### Novos Posts
```graphql
subscription OnNewPost {
  onPostCreated {
    post {
      id
      title
      author {
        name
      }
    }
    action
  }
}
```

#### Comentários em Post Específico
```graphql
subscription OnPostComments($postId: Int!) {
  onCommentAdded(postId: $postId) {
    comment {
      id
      content
      author {
        name
      }
    }
    action
    postId
  }
}
```

## 🔧 Configurações Avançadas

### Filtering
HotChocolate oferece filtering automático em todos os campos:

```graphql
posts(where: {
  or: [
    { title: { contains: "GraphQL" } }
    { content: { contains: "HotChocolate" } }
  ]
  and: [
    { isPublished: { eq: true } }
    { createdAt: { gte: "2024-01-01" } }
  ]
  author: {
    status: { neq: SUSPENDED }
    profile: {
      bio: { contains: "developer" }
    }
  }
})
```

### Sorting
Sorting em múltiplos campos:

```graphql
posts(order: [
  { createdAt: DESC }
  { likeCount: DESC }
  { title: ASC }
])
```

### Pagination
Relay-style cursor pagination:

```graphql
posts(first: 10, after: "cursor") {
  nodes { ... }
  pageInfo {
    hasNextPage
    hasPreviousPage
    startCursor
    endCursor
  }
  totalCount
}
```

## 📊 Modelos de Dados

### User
- **Campos**: ID, Name, Email, CreatedAt, LastLoginAt, Status
- **Relacionamentos**: Profile (1:1), Posts (1:N), Comments (1:N)

### Post
- **Campos**: ID, Title, Content, CreatedAt, UpdatedAt, IsPublished, ViewCount, LikeCount
- **Relacionamentos**: Author (N:1), Comments (1:N), Tags (N:N)

### Comment
- **Campos**: ID, Content, CreatedAt, UpdatedAt
- **Relacionamentos**: Author (N:1), Post (N:1), ParentComment (N:1), Replies (1:N)

### Tag
- **Campos**: ID, Name, Color, CreatedAt
- **Relacionamentos**: Posts (N:N)

## 🎯 Funcionalidades Especiais

### 1. Data Loaders Automáticos
HotChocolate resolve automaticamente o problema N+1 usando data loaders:

```graphql
# Esta query executa apenas 3 consultas SQL, não N+1
posts {
  author { name }    # Batch loaded
  comments {
    author { name }  # Batch loaded
  }
}
```

### 2. Projections
Apenas campos solicitados são carregados:

```graphql
# SQL gerado inclui apenas: SELECT id, title FROM posts
posts {
  id
  title
}
```

### 3. Error Handling
Errors estruturados nos mutations:

```json
{
  "data": {
    "createUser": {
      "user": null,
      "errors": [
        "Email already exists",
        "Name is required"
      ]
    }
  }
}
```

## 🚀 Casos de Uso

### APIs Modernas
- **Single endpoint** para todas as operações
- **Type-safe** queries no frontend
- **Optimized data fetching** automático
- **Real-time updates** via subscriptions

### Sistemas de Conteúdo
- **Blog platforms** com comentários aninhados
- **CMS systems** com relacionamentos complexos
- **Social networks** com feeds personalizados
- **E-commerce** com catálogos dinâmicos

### Microservices
- **GraphQL Gateway** para múltiplos serviços
- **Schema federation** entre domínios
- **Type-safe** integration entre serviços

## 🎓 Conceitos Aprendidos

1. **GraphQL Fundamentals**: Schema-first development, type system
2. **HotChocolate Features**: Filtering, sorting, pagination automáticos
3. **Real-time Communication**: WebSocket subscriptions
4. **Performance Optimization**: Data loaders, projections
5. **Authentication Integration**: JWT com GraphQL
6. **Validation Patterns**: FluentValidation integration
7. **Database Integration**: Entity Framework com GraphQL

## 🔗 Recursos Adicionais

- [HotChocolate Documentation](https://chillicream.com/docs/hotchocolate)
- [GraphQL Specification](https://spec.graphql.org/)
- [GraphQL Best Practices](https://graphql.org/learn/best-practices/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://fluentvalidation.net/)

---

**Objetivo**: Demonstrar a implementação completa de uma API GraphQL moderna usando HotChocolate, incluindo queries avançadas, mutations com validação, subscriptions em tempo real e integração com Entity Framework Core.
