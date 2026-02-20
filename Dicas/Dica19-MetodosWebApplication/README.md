# Dica 19: Métodos WebApplication (Run, Use, Map)

## 📋 Problema

A classe `WebApplication` no .NET tem métodos fundamentais que são frequentemente mal compreendidos:

- **Run**: Middleware terminal (de finalização)
- **Use**: Middleware geral no pipeline
- **Map**: Middleware mapeado para rotas específicas

A **ordem** de chamada desses métodos é crucial para o funcionamento correto da aplicação.

## 💡 Solução

Entender a diferença entre cada método e sua ordem de execução no pipeline de middleware.

## 🛠️ Como Funcionam

### 1. **Use() - Middleware Geral**

```csharp
app.Use(async (context, next) =>
{
    // Código executado ANTES do próximo middleware
    await next(); // Chama o próximo middleware
    // Código executado DEPOIS do próximo middleware
});
```

- Executa em **todas** as requisições
- Pode chamar `next()` para continuar o pipeline
- Pode interromper o pipeline não chamando `next()`

### 2. **Map() - Middleware Condicional**

```csharp
app.Map("/api/info", infoApp =>
{
    // Middleware específico para /api/info
    infoApp.Use(async (context, next) => { /* ... */ });
    infoApp.Run(async context => { /* ... */ });
});
```

- Executa **apenas** se o path corresponder
- Cria um sub-pipeline para a rota específica
- Útil para organizar middlewares por funcionalidade

### 3. **Run() - Middleware Terminal**

```csharp
app.Run(async context =>
{
    // Este middleware NUNCA chama next()
    // É sempre o último a executar
});
```

- **Terminal**: Nunca chama `next()`
- Sempre deve ser o **último** middleware
- Usado como fallback ou handler final

## 🎯 Exemplo Prático

### Pipeline Completo

```csharp
var app = WebApplication.Create();

// 1. USE - Logging (sempre executa)
app.Use(async (context, next) =>
{
    Console.WriteLine($"Requisição: {context.Request.Path}");
    await next();
    Console.WriteLine($"Resposta: {context.Response.StatusCode}");
});

// 2. USE - Autenticação (pode interromper)
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/secure") && !HasAuth())
    {
        context.Response.StatusCode = 401;
        return; // Não chama next() - pipeline interrompido
    }
    await next();
});

// 3. MAP - Rota específica
app.Map("/api/info", infoApp =>
{
    infoApp.Run(async context =>
    {
        await context.Response.WriteAsJsonAsync(new { Info = "API Info" });
    });
});

// 4. Endpoints normais
app.MapGet("/", () => "Hello World");

// 5. RUN - Fallback (sempre por último)
app.Run(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Not Found");
});

app.Run();
```

## 📊 Ordem de Execução

| Ordem | Método | Quando Executa | Pode Interromper |
|-------|--------|----------------|------------------|
| 1 | **Use** | Sempre | ✅ Sim |
| 2 | **Use** | Sempre | ✅ Sim |
| 3 | **Map** | Se path corresponder | ✅ Sim |
| 4 | **MapGet/Post/etc** | Se rota corresponder | ✅ Sim |
| 5 | **Run** | Se nada anterior respondeu | ❌ Não (terminal) |

## 🎮 Como Executar

```bash
cd Dicas/Dica19-MetodosWebApplication/Dica19.MetodosWebApplication
dotnet run
```

### Testando o Pipeline

```bash
# Endpoint normal
curl http://localhost:5000/

# Rota mapeada com Map
curl http://localhost:5000/api/info

# Área protegida (sem autenticação)
curl http://localhost:5000/api/secure

# Área protegida (com autenticação)
curl -H "Authorization: Bearer token123" http://localhost:5000/api/secure

# Rota inexistente (fallback)
curl http://localhost:5000/rota-inexistente
```

## 🔍 O que a Demonstração Mostra

1. **Middleware de Logging**: Sempre executa, mostra entrada e saída
2. **Middleware de Autenticação**: Pode bloquear acesso a rotas protegidas
3. **Map para /api/info**: Executa apenas para esta rota específica
4. **Map para /api/secure**: Rota protegida que requer autenticação
5. **Endpoints da API**: Rotas normais da aplicação
6. **Run Fallback**: Captura requisições não atendidas (404)

## ⚠️ Cuidados Importantes

### 1. **Ordem Importa**

```csharp
// ❌ ERRADO - Run antes dos endpoints
app.Run(async context => { /* ... */ });
app.MapGet("/", () => "Never reached");

// ✅ CORRETO - Run por último
app.MapGet("/", () => "This works");
app.Run(async context => { /* ... */ });
```

### 2. **Use sem next() Para Pipeline**

```csharp
// ❌ ERRADO - Não chama next()
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync("Response");
    // next() nunca é chamado - pipeline parado
});

// ✅ CORRETO - Chama next() quando apropriado
app.Use(async (context, next) =>
{
    // Fazer alguma verificação
    if (shouldContinue)
        await next();
    else
        await context.Response.WriteAsync("Stopped here");
});
```

### 3. **Map vs MapGet/MapPost**

```csharp
// Map - para sub-pipelines complexos
app.Map("/api", apiApp =>
{
    apiApp.Use(/* middleware específico da API */);
    apiApp.MapGet("/users", /* ... */);
    apiApp.MapPost("/users", /* ... */);
});

// MapGet - para endpoints simples
app.MapGet("/health", () => "OK");
```

## ✅ Benefícios

- ✅ **Controle Total**: Pipeline flexível e customizável
- ✅ **Organização**: Map ajuda a organizar funcionalidades
- ✅ **Performance**: Middleware condicional (Map) executa apenas quando necessário
- ✅ **Segurança**: Use para implementar autenticação/autorização
- ✅ **Debugging**: Use para logging e monitoramento

## 📚 Recursos Adicionais

- [ASP.NET Core Middleware](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/)
- [WebApplication Documentation](https://docs.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Middleware Pipeline](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/index#middleware-pipeline)

---

**Dica**: A ordem dos métodos Use, Map e Run define o comportamento da aplicação. Sempre coloque Run por último e use Map para organizar funcionalidades específicas.
