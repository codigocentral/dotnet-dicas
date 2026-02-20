# Dica 67: API Versioning em ASP.NET Core

## 📋 Sobre

Esta dica demonstra como implementar **versionamento de APIs** em ASP.NET Core, uma prática essencial para manter compatibilidade e evoluir APIs em produção sem quebrar clientes existentes.

## 🎯 Conceitos Demonstrados

### 1. **Estratégias de Versionamento**
- **URL Path**: `/api/v1.0/products` - Versão na URL
- **Header**: `X-API-Version: 2.0` - Versão no header HTTP
- **Query Parameter**: `?api-version=3.0` - Versão como parâmetro

### 2. **Tipos de Mudanças**
- **Backward Compatible**: Adicionar campos, novos endpoints
- **Breaking Changes**: Remover campos, alterar estrutura, mudar comportamento
- **Deprecation**: Marcar versões antigas como obsoletas

### 3. **Casos de Uso**
- Evolução gradual de APIs em produção
- Suporte a múltiplas versões simultaneamente
- Migração controlada de clientes
- A/B testing de novas funcionalidades

## 🏗️ Estrutura do Projeto

```
Dica67-APIVersioning/
├── Controllers/
│   ├── V1/
│   │   └── ProductsController.cs     # API V1 - Básica
│   ├── V2/
│   │   └── ProductsController.cs     # API V2 - Com categorias
│   ├── V3/
│   │   └── ProductsController.cs     # API V3 - Breaking changes
│   ├── Alternative/
│   │   └── VersioningExamplesController.cs  # Header/Query examples
│   └── ApiInfoController.cs         # Informações sobre versões
├── Models/
│   └── Product.cs                   # Modelos para cada versão
├── Services/
│   └── ProductService.cs            # Serviços implementando todas as versões
└── Program.cs                       # Configuração de versionamento
```

## 🚀 Funcionalidades

### **1. Products API - Evolução Completa**

#### **V1.0 - Versão Básica**
```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    // CRUD básico com modelo simples
    // GET /api/v1.0/products
    // GET /api/v1.0/products/{id}
    // POST /api/v1.0/products
}
```

**Modelo V1:**
```json
{
  "id": 1,
  "name": "Laptop",
  "price": 999.99,
  "description": "High-performance laptop",
  "createdAt": "2025-07-10T23:55:00Z"
}
```

#### **V2.0 - Recursos Expandidos**
```csharp
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    // Mantém compatibilidade + novos recursos
    // GET /api/v2.0/products/category/{category} (novo)
}
```

**Modelo V2 (Backward Compatible):**
```json
{
  "id": 1,
  "name": "Laptop",
  "price": 999.99,
  "description": "High-performance laptop",
  "createdAt": "2025-07-10T23:55:00Z",
  "category": "Electronics",
  "tags": ["computer", "portable"],
  "rating": { "average": 4.5, "count": 123 },
  "inventory": { "stock": 50, "reserved": 5, "available": 45 }
}
```

#### **V3.0 - Breaking Changes**
```csharp
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    // Mudanças incompatíveis
    // GET /api/v3.0/products/{sku} (era {id})
    // GET /api/v3.0/products/search?q={term} (novo)
    // PATCH /api/v3.0/products/{sku}/stock (novo)
}
```

**Modelo V3 (Breaking Changes):**
```json
{
  "sku": "LAPTOP-001",
  "title": "MacBook Pro 16\"",
  "price": { "amount": 2499.99, "currency": "USD" },
  "summary": "Professional laptop with M3 chip",
  "metadata": {
    "brand": "Apple",
    "model": "MacBook Pro 16\" M3",
    "createdAt": "2025-07-10T23:55:00Z",
    "updatedAt": "2025-07-10T23:55:00Z"
  },
  "availability": {
    "inStock": true,
    "quantity": 25,
    "restockDate": null
  }
}
```

### **2. Estratégias de Versionamento**

#### **URL Path Versioning**
```bash
# Vantagens: Explícito, cacheable, SEO-friendly
# Desvantagens: URLs diferentes, pode gerar confusão

GET /api/v1.0/products
GET /api/v2.0/products  
GET /api/v3.0/products
```

#### **Header Versioning**
```bash
# Vantagens: URLs limpos, flexível
# Desvantagens: Menos óbvio, pode ser esquecido

curl -H "X-API-Version: 2.0" /api/orders
```

#### **Query Parameter Versioning**
```bash
# Vantagens: Visível na URL, fácil de testar
# Desvantagens: URLs mais longas, pode conflitar

GET /api/customers?api-version=3.0
```

## ⚙️ Configuração

### **Program.cs - Configuração Completa**
```csharp
builder.Services.AddApiVersioning(options =>
{
    // Versão padrão
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // Múltiplas estratégias combinadas
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),           // /api/v1.0/products
        new HeaderApiVersionReader("X-API-Version"), // Header
        new QueryStringApiVersionReader("api-version") // Query
    );
    
}).AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
```

### **Swagger Multi-Version**
```csharp
// Configuração automática para múltiplas versões no Swagger
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// Swagger UI com dropdown de versões
app.UseSwaggerUI(c =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
    {
        c.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"API Versioning Demo {description.GroupName.ToUpperInvariant()}");
    }
});
```

## 🧪 Como Testar

### **1. Executar a Aplicação**
```bash
dotnet run
```
Acesse: http://localhost:5000

### **2. Testar Versionamento por URL**
```bash
# V1 - Modelo básico
curl http://localhost:5000/api/v1.0/products

# V2 - Modelo expandido
curl http://localhost:5000/api/v2.0/products

# V3 - Breaking changes
curl http://localhost:5000/api/v3.0/products
```

### **3. Testar Versionamento por Header**
```bash
# V1 (default)
curl http://localhost:5000/api/orders

# V2 com header
curl -H "X-API-Version: 2.0" http://localhost:5000/api/orders
```

### **4. Testar Versionamento por Query**
```bash
# V1 (default)
curl http://localhost:5000/api/customers

# V2 com query parameter
curl "http://localhost:5000/api/customers?api-version=2.0"

# V3 com query parameter
curl "http://localhost:5000/api/customers?api-version=3.0"
```

### **5. Endpoints de Informação**
```bash
# Informações sobre versões disponíveis
curl http://localhost:5000/api/info/versions | jq

# Comparação entre versões
curl http://localhost:5000/api/info/comparison | jq

# Detecção de versão
curl http://localhost:5000/api/info/version-detection | jq
```

## 📊 Endpoints Disponíveis

### **Products API (URL Versioning)**
- `GET /api/v1.0/products` - Lista produtos V1
- `GET /api/v1.0/products/{id}` - Produto específico V1
- `POST /api/v1.0/products` - Criar produto V1
- `GET /api/v2.0/products` - Lista produtos V2 (com categorias)
- `GET /api/v2.0/products/category/{category}` - Produtos por categoria (novo V2)
- `GET /api/v3.0/products` - Lista produtos V3 (modelo novo)
- `GET /api/v3.0/products/{sku}` - Produto por SKU (breaking: era ID)
- `GET /api/v3.0/products/search?q={term}` - Busca produtos (novo V3)
- `PATCH /api/v3.0/products/{sku}/stock` - Atualizar estoque (novo V3)

### **Orders API (Header Versioning)**
- `GET /api/orders` - Lista pedidos (use header X-API-Version)

### **Customers API (Query Versioning)**
- `GET /api/customers?api-version=1.0` - Clientes básicos
- `GET /api/customers?api-version=2.0` - Clientes com endereço
- `GET /api/customers?api-version=3.0` - Clientes completos

### **Info APIs**
- `GET /api/info/versions` - Informações sobre versões
- `GET /api/info/comparison` - Comparação entre versões
- `GET /api/info/version-detection` - Como a versão foi detectada
- `GET /api/version` - Endpoint de demonstração rápida

## 🔧 Evolução da API

### **V1 → V2: Mudanças Compatíveis**
- ✅ Adição de campos opcionais
- ✅ Novos endpoints
- ✅ Melhoria de funcionalidades existentes
- ❌ Não quebra clientes V1

### **V2 → V3: Breaking Changes**
- 🔄 `id` → `sku` (mudança de identificador)
- 🔄 `name` → `title` (renomeação de campo)
- 🔄 `price` → `Money object` (mudança de estrutura)
- 🔄 `description` → `summary` (renomeação)
- ❌ Quebra compatibilidade com V1/V2

### **Estratégia de Migração**
```json
{
  "migrationGuide": {
    "v1_to_v2": {
      "difficulty": "Low",
      "changes": ["Add category field", "Handle new response fields"],
      "timeline": "1-2 days"
    },
    "v2_to_v3": {
      "difficulty": "High", 
      "changes": ["Update ID references to SKU", "Handle name->title mapping"],
      "timeline": "1-2 weeks"
    }
  }
}
```

## 📈 Detecção de Versão

### **Ordem de Precedência**
1. **URL Path**: `/api/v2.0/products` (maior prioridade)
2. **Header**: `X-API-Version: 2.0`
3. **Query Parameter**: `?api-version=2.0`
4. **Default**: `1.0` (menor prioridade)

### **Exemplo de Detecção**
```csharp
var requestedVersion = HttpContext.GetRequestedApiVersion();
// Retorna a versão detectada baseada na precedência
```

## 🎯 Cenários Reais

### **1. E-commerce em Evolução**
- V1: Produtos básicos
- V2: Adicionar categorias e ratings
- V3: Sistema completo com SKU e inventário

### **2. API Pública com Clientes Diversos**
- Clientes legados usando V1
- Aplicações modernas usando V3
- Suporte simultâneo durante migração

### **3. Microserviços**
- Cada serviço com versionamento independente
- Compatibilidade entre versões de serviços
- Deploys independentes

### **4. Mobile Apps**
- Apps antigos não podem ser forçados a atualizar
- Suporte a múltiplas versões simultaneamente
- Funcionalidades progressivas

## 📚 Melhores Práticas

### **1. Semantic Versioning**
- **Major**: Breaking changes (1.0 → 2.0)
- **Minor**: Novos recursos compatíveis (1.0 → 1.1)
- **Patch**: Bug fixes (1.0.0 → 1.0.1)

### **2. Deprecation Strategy**
```csharp
[ApiVersion("1.0", Deprecated = true)]
public class OldProductsController : ControllerBase
{
    // Marcar versões antigas como deprecated
}
```

### **3. Documentation**
- Documentar mudanças entre versões
- Guias de migração claros
- Exemplos de antes/depois

### **4. Testing**
- Testes para todas as versões suportadas
- Testes de compatibilidade
- Validação de contratos de API

## ⚠️ Considerações de Produção

### **1. Suporte a Versões**
- Quantas versões manter ativas?
- Política de deprecation clara
- Comunicação com clientes

### **2. Performance**
- Múltiplas versões = múltiplos códigos
- Otimização de recursos compartilhados
- Monitoring por versão

### **3. Deploy e CI/CD**
- Testes automatizados para todas as versões
- Deploy sem afetar versões existentes
- Rollback strategies

### **4. Monitoring**
- Métricas por versão da API
- Uso e adoção de versões
- Detecção de problemas específicos

## 🔗 Recursos Adicionais

- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [Microsoft API Versioning Guidelines](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [REST API Versioning Strategies](https://restfulapi.net/versioning/)

## 🎉 Conclusão

API Versioning é essencial para:

- **Evolução Controlada**: Permitir mudanças sem quebrar clientes
- **Backward Compatibility**: Manter suporte a versões antigas
- **Flexibilidade**: Múltiplas estratégias para diferentes cenários
- **Profissionalismo**: APIs robustas e confiáveis

Esta implementação demonstra as principais estratégias e fornece uma base sólida para APIs versionadas em produção.
