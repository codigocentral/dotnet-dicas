using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Dica67_APIVersioning.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar API Versioning
builder.Services.AddApiVersioning(options =>
{
    // Versão padrão quando nenhuma é especificada
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // Estratégias de leitura de versão
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),           // /api/v1.0/products
        new HeaderApiVersionReader("X-API-Version"), // Header: X-API-Version: 2.0
        new QueryStringApiVersionReader("api-version") // ?api-version=3.0
    );
    
    // Como reportar versões suportadas
    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
    
}).AddApiExplorer(setup =>
{
    // Configurar API Explorer para Swagger
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Configurar Swagger para múltiplas versões
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerDefaultValues>();
    
    // Incluir comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Registrar serviços
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductServiceV2, ProductService>();
builder.Services.AddScoped<IProductServiceV3, ProductService>();

// Health Checks
builder.Services.AddHealthChecks();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        // Criar UI do Swagger para cada versão
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"API Versioning Demo {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = string.Empty; // Swagger na raiz
        c.DefaultModelExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.DisplayOperationId();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

// Endpoint raiz que redireciona para Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Endpoint para demonstrar versionamento dinâmico
app.MapGet("/api/version", (HttpContext context) =>
{
    var requestedVersion = context.GetRequestedApiVersion();
    return new
    {
        RequestedVersion = requestedVersion?.ToString() ?? "None",
        SupportedVersions = new[] { "1.0", "2.0", "3.0" },
        VersioningMethods = new[]
        {
            "URL: /api/v{version}/endpoint",
            "Header: X-API-Version: {version}",
            "Query: ?api-version={version}"
        },
        Examples = new
        {
            UrlPath = "/api/v2.0/products",
            Header = "curl -H 'X-API-Version: 2.0' /api/orders",
            Query = "/api/customers?api-version=3.0"
        }
    };
}).WithName("GetVersionInfo")
  .WithTags("Version Info");

Console.WriteLine("🚀 API Versioning Demo iniciada!");
Console.WriteLine("📊 Swagger UI: http://localhost:5000");
Console.WriteLine("🏥 Health Check: http://localhost:5000/health");
Console.WriteLine("📋 Versões suportadas: 1.0, 2.0, 3.0");
Console.WriteLine("🔄 Estratégias: URL Path, Header, Query Parameter");

app.Run();

// Classes de configuração do Swagger
public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = "API Versioning Demo",
            Version = description.ApiVersion.ToString(),
            Description = "Demonstração de diferentes estratégias de versionamento de APIs em ASP.NET Core",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "api-support@example.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " Esta versão da API foi depreciada.";
        }

        return info;
    }
}

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        // Marcar como deprecated se aplicável
        if (apiDescription.ActionDescriptor.EndpointMetadata.Any(m => m is ObsoleteAttribute))
        {
            operation.Deprecated = true;
        }

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            if (operation.Responses.ContainsKey(responseKey))
            {
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys.ToList())
                {
                    if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }
        }

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);
            if (description != null)
            {
                parameter.Description ??= description.ModelMetadata?.Description;
                parameter.Required |= description.IsRequired;
            }
        }
    }
}
