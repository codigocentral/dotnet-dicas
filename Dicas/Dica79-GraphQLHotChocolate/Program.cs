using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using FluentValidation;
using Dica79.GraphQLHotChocolate.Services;
using Dica79.GraphQLHotChocolate.Types;
using Dica79.GraphQLHotChocolate.Models;
using Dica79.GraphQLHotChocolate.Validators;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseInMemoryDatabase("BlogDatabase")
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// Register database seeder
builder.Services.AddScoped<DatabaseSeeder>();

// FluentValidation
builder.Services.AddScoped<IValidator<CreateUserInput>, CreateUserInputValidator>();
builder.Services.AddScoped<IValidator<UpdateUserInput>, UpdateUserInputValidator>();
builder.Services.AddScoped<IValidator<CreatePostInput>, CreatePostInputValidator>();
builder.Services.AddScoped<IValidator<UpdatePostInput>, UpdatePostInputValidator>();
builder.Services.AddScoped<IValidator<CreateCommentInput>, CreateCommentInputValidator>();

// JWT Authentication (simplified for demo)
var jwtKey = "super-secret-key-for-demo-purposes-only-do-not-use-in-production";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// GraphQL configuration
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddInMemorySubscriptions()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

// CORS for GraphQL Playground and external clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// WebSocket support for subscriptions
app.UseWebSockets();

// GraphQL endpoint
app.MapGraphQL();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    
    await context.Database.EnsureCreatedAsync();
    await seeder.SeedAsync();
}

// Demo routes
app.MapGet("/", () => Results.Redirect("/graphql"));

app.MapGet("/demo", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>GraphQL with HotChocolate Demo</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 40px; }
        .container { max-width: 800px; margin: 0 auto; }
        .query-box { background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }
        code { background: #e9ecef; padding: 2px 6px; border-radius: 4px; }
        h1 { color: #0066cc; }
        h2 { color: #333; border-bottom: 2px solid #eee; padding-bottom: 10px; }
        .feature { margin: 20px 0; padding: 15px; border-left: 4px solid #0066cc; background: #f8f9fa; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🚀 GraphQL with HotChocolate Demo</h1>
        
        <p>Bem-vindo à demonstração do GraphQL com HotChocolate! Esta aplicação mostra os recursos avançados do GraphQL em .NET.</p>
        
        <div class="feature">
            <h3>🎯 Recursos Demonstrados</h3>
            <ul>
                <li><strong>Queries</strong>: Consultas flexíveis com filtering, sorting e pagination</li>
                <li><strong>Mutations</strong>: Operações de criação, atualização e exclusão</li>
                <li><strong>Subscriptions</strong>: Atualizações em tempo real via WebSockets</li>
                <li><strong>Authentication</strong>: Integração com JWT</li>
                <li><strong>Validation</strong>: Validação de entrada com FluentValidation</li>
                <li><strong>Data Loader</strong>: Otimização automática de consultas</li>
            </ul>
        </div>
        
        <h2>🔗 Links Úteis</h2>
        <ul>
            <li><a href="/graphql" target="_blank">GraphQL Playground (Banana Cake Pop)</a></li>
            <li><a href="/graphql/schema.graphql" target="_blank">Schema GraphQL</a></li>
        </ul>
        
        <h2>📋 Queries de Exemplo</h2>
        
        <div class="query-box">
            <h3>1. Buscar Posts com Filtros</h3>
            <pre><code>query GetPosts {
  posts(first: 10, where: { isPublished: { eq: true } }) {
    nodes {
      id
      title
      content
      createdAt
      author {
        name
        profile {
          bio
        }
      }
      tags {
        name
        color
      }
      comments {
        totalCount
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
    totalCount
  }
}</code></pre>
        </div>
        
        <div class="query-box">
            <h3>2. Buscar Post Específico</h3>
            <pre><code>query GetPost($id: Int!) {
  post(id: $id) {
    id
    title
    content
    viewCount
    likeCount
    author {
      name
      email
      profile {
        bio
        avatarUrl
      }
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
}</code></pre>
        </div>
        
        <div class="query-box">
            <h3>3. Estatísticas do Blog</h3>
            <pre><code>query BlogStats {
  blogStats {
    totalUsers
    activeUsers
    totalPosts
    publishedPosts
    totalComments
    totalTags
    recentPosts
    engagementRate
  }
}</code></pre>
        </div>
        
        <h2>🔄 Mutations de Exemplo</h2>
        
        <div class="query-box">
            <h3>1. Criar Usuário</h3>
            <pre><code>mutation CreateUser($input: CreateUserInput!) {
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
}</code></pre>
            <p><strong>Variables:</strong></p>
            <pre><code>{
  "input": {
    "name": "João Silva",
    "email": "joao@example.com",
    "bio": "Desenvolvedor .NET apaixonado por GraphQL",
    "website": "https://joao.dev"
  }
}</code></pre>
        </div>
        
        <div class="query-box">
            <h3>2. Criar Post</h3>
            <pre><code>mutation CreatePost($input: CreatePostInput!) {
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
}</code></pre>
        </div>
        
        <h2>📡 Subscriptions de Exemplo</h2>
        
        <div class="query-box">
            <h3>1. Novos Posts</h3>
            <pre><code>subscription OnNewPost {
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
}</code></pre>
        </div>
        
        <div class="query-box">
            <h3>2. Comentários em um Post</h3>
            <pre><code>subscription OnPostComments($postId: Int!) {
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
}</code></pre>
        </div>
        
        <h2>⚙️ Funcionalidades Avançadas</h2>
        
        <div class="feature">
            <h3>🔍 Filtering e Sorting</h3>
            <p>Todos os queries suportam filtering complexo e sorting. Exemplo:</p>
            <pre><code>posts(
  where: { 
    and: [
      { title: { contains: "GraphQL" } }
      { createdAt: { gte: "2024-01-01" } }
      { isPublished: { eq: true } }
    ]
  }
  order: [{ createdAt: DESC }, { likeCount: DESC }]
)</code></pre>
        </div>
        
        <div class="feature">
            <h3>📄 Pagination</h3>
            <p>Suporte completo a Relay-style pagination:</p>
            <pre><code>posts(first: 10, after: "cursor") {
  nodes { ... }
  pageInfo {
    hasNextPage
    hasPreviousPage
    startCursor
    endCursor
  }
  totalCount
}</code></pre>
        </div>
        
        <div class="feature">
            <h3>🎯 Projections</h3>
            <p>Apenas os campos solicitados são carregados do banco de dados, otimizando performance automaticamente.</p>
        </div>
    </div>
</body>
</html>
""", "text/html"));

// Authentication endpoint (simplified for demo)
app.MapPost("/auth/login", (LoginRequest request) =>
{
    // Simplified authentication - in production, validate against database
    if (request.Email == "demo@example.com" && request.Password == "demo123")
    {
        var claims = new[]
        {
            new Claim("userId", "1"),
            new Claim(ClaimTypes.Email, request.Email),
            new Claim(ClaimTypes.Name, "Demo User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(1);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = tokenString, expires = expiry });
    }

    return Results.Unauthorized();
});

Console.WriteLine("🚀 GraphQL with HotChocolate Demo");
Console.WriteLine("=================================");
Console.WriteLine($"🌐 GraphQL Playground: {app.Urls.FirstOrDefault()}/graphql");
Console.WriteLine($"📋 Demo Page: {app.Urls.FirstOrDefault()}/demo");
Console.WriteLine($"🔑 Auth (demo): POST /auth/login {{ \"email\": \"demo@example.com\", \"password\": \"demo123\" }}");
Console.WriteLine();
Console.WriteLine("✅ Recursos Demonstrados:");
Console.WriteLine("  • Queries com filtering, sorting e pagination");
Console.WriteLine("  • Mutations com validação");
Console.WriteLine("  • Subscriptions em tempo real");
Console.WriteLine("  • Autenticação JWT");
Console.WriteLine("  • Entity Framework Integration");
Console.WriteLine("  • Data Loaders automáticos");
Console.WriteLine();

app.Run();

public record LoginRequest(string Email, string Password);
