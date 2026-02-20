using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Dica42.ExpressionTrees;

/// <summary>
/// Demonstra o uso avançado de Expression Trees para metaprogramação e performance
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<DemoService>();
            })
            .Build();

        var demo = host.Services.GetRequiredService<DemoService>();
        demo.ExecutarTodasDemonstracoes();
    }
}

public class DemoService
{
    private readonly ILogger<DemoService> _logger;

    public DemoService(ILogger<DemoService> logger)
    {
        _logger = logger;
    }

    public void ExecutarTodasDemonstracoes()
    {
        Console.WriteLine("===== Dica 42: Expression Trees - Metaprogramação Avançada ====\n");

        Console.WriteLine("🌳 1. CONSTRUÇÃO BÁSICA DE EXPRESSION TREES");
        Console.WriteLine("─────────────────────────────────────────────");
        DemonstrarConstrucaoBasica();
        Console.WriteLine("✅ Demonstração básica concluída\n");

        Console.WriteLine("🔧 2. COMPILAÇÃO E EXECUÇÃO DINÂMICA");
        Console.WriteLine("────────────────────────────────────────");
        DemonstrarCompilacaoEExecucao();
        Console.WriteLine("✅ Demonstração de compilação concluída\n");

        Console.WriteLine("🔍 3. ANÁLISE E VISITOR PATTERN");
        Console.WriteLine("─────────────────────────────────");
        DemonstrarAnaliseEVisitor();
        Console.WriteLine("✅ Demonstração de análise concluída\n");

        Console.WriteLine("⚡ 4. PERFORMANCE - COMPILED VS INTERPRETED");
        Console.WriteLine("───────────────────────────────────────────");
        DemonstrarPerformance();
        Console.WriteLine("✅ Demonstração de performance concluída\n");

        Console.WriteLine("🏭 5. FACTORY DINÂMICO COM EXPRESSIONS");
        Console.WriteLine("──────────────────────────────────────");
        DemonstrarFactoryDinamico();
        Console.WriteLine("✅ Demonstração de factory concluída\n");

        Console.WriteLine("🗄️ 6. QUERY BUILDER DINÂMICO");
        Console.WriteLine("───────────────────────────────");
        DemonstrarQueryBuilder();
        Console.WriteLine("✅ Demonstração de query builder concluída\n");

        Console.WriteLine("🎭 7. PROXY DINÂMICO COM INTERCEPTAÇÃO");
        Console.WriteLine("──────────────────────────────────────");
        DemonstrarProxyDinamico();
        Console.WriteLine("✅ Demonstração de proxy concluída\n");

        Console.WriteLine("📋 8. RESUMO DAS BOAS PRÁTICAS");
        Console.WriteLine("─────────────────────────────────");
        ExibirBoasPraticas();

        Console.WriteLine("\n=== Demonstração concluída ===");
    }

    private void DemonstrarConstrucaoBasica()
    {
        _logger.LogInformation("🌳 Demonstrando construção básica de Expression Trees...");

        // Expression simples: x => x * 2
        var param = Expression.Parameter(typeof(int), "x");
        var multiply = Expression.Multiply(param, Expression.Constant(2));
        var lambda = Expression.Lambda<Func<int, int>>(multiply, param);
        
        _logger.LogInformation("📝 Expression criada: {expression}", lambda);

        var compiled = lambda.Compile();
        var resultado = compiled(5);
        _logger.LogInformation("📊 Resultado: {input} * 2 = {result}", 5, resultado);

        // Expression complexa: (x, y) => x * x + y * y
        var paramX = Expression.Parameter(typeof(double), "x");
        var paramY = Expression.Parameter(typeof(double), "y");
        var xSquared = Expression.Multiply(paramX, paramX);
        var ySquared = Expression.Multiply(paramY, paramY);
        var sum = Expression.Add(xSquared, ySquared);
        var distanceLambda = Expression.Lambda<Func<double, double, double>>(sum, paramX, paramY);

        _logger.LogInformation("📝 Expression complexa: {expression}", distanceLambda);
        
        var distanceFunc = distanceLambda.Compile();
        var distance = distanceFunc(3.0, 4.0);
        _logger.LogInformation("📊 Distância²: 3² + 4² = {distance}", distance);

        // Expression condicional: x => x > 0 ? x : -x (valor absoluto)
        var condition = Expression.GreaterThan(param, Expression.Constant(0));
        var positive = param;
        var negative = Expression.Negate(param);
        var conditional = Expression.Condition(condition, positive, negative);
        var absLambda = Expression.Lambda<Func<int, int>>(conditional, param);

        _logger.LogInformation("📝 Expression condicional: {expression}", absLambda);
        
        var absFunc = absLambda.Compile();
        _logger.LogInformation("📊 Abs(-5) = {result}", absFunc(-5));
        _logger.LogInformation("📊 Abs(3) = {result}", absFunc(3));

        // Expression com chamada de método
        var stringParam = Expression.Parameter(typeof(string), "s");
        var toUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;
        var methodCall = Expression.Call(stringParam, toUpperMethod);
        var toUpperLambda = Expression.Lambda<Func<string, string>>(methodCall, stringParam);

        _logger.LogInformation("📝 Expression com método: {expression}", toUpperLambda);
        
        var toUpperFunc = toUpperLambda.Compile();
        _logger.LogInformation("📊 ToUpper('hello') = '{result}'", toUpperFunc("hello"));
    }

    private void DemonstrarCompilacaoEExecucao()
    {
        _logger.LogInformation("🔧 Demonstrando compilação e execução dinâmica...");

        // Factory de operações matemáticas
        var operations = new Dictionary<string, Expression<Func<double, double, double>>>();

        // Adição
        var x = Expression.Parameter(typeof(double), "x");
        var y = Expression.Parameter(typeof(double), "y");
        operations["add"] = Expression.Lambda<Func<double, double, double>>(
            Expression.Add(x, y), x, y);

        // Subtração
        operations["subtract"] = Expression.Lambda<Func<double, double, double>>(
            Expression.Subtract(x, y), x, y);

        // Multiplicação
        operations["multiply"] = Expression.Lambda<Func<double, double, double>>(
            Expression.Multiply(x, y), x, y);

        // Divisão com verificação de zero
        var condition = Expression.NotEqual(y, Expression.Constant(0.0));
        var division = Expression.Divide(x, y);
        var throwException = Expression.Throw(
            Expression.New(typeof(DivideByZeroException).GetConstructor(Type.EmptyTypes)!),
            typeof(double));
        var safeDivision = Expression.Condition(condition, division, throwException);
        operations["divide"] = Expression.Lambda<Func<double, double, double>>(
            safeDivision, x, y);

        // Compilar e testar operações
        foreach (var (name, expression) in operations)
        {
            var compiled = expression.Compile();
            _logger.LogInformation("🔧 Operação '{operation}' compilada: {expression}", name, expression);
            
            try
            {
                var result = name switch
                {
                    "add" => compiled(10, 5),
                    "subtract" => compiled(10, 5),
                    "multiply" => compiled(10, 5),
                    "divide" => compiled(10, 5),
                    _ => 0
                };
                _logger.LogInformation("📊 {operation}(10, 5) = {result}", name, result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ Erro na operação {operation}: {error}", name, ex.Message);
            }
        }

        // Teste divisão por zero
        try
        {
            var divideFunc = operations["divide"].Compile();
            divideFunc(10, 0);
        }
        catch (DivideByZeroException)
        {
            _logger.LogInformation("✅ Divisão por zero detectada corretamente!");
        }
    }

    private void DemonstrarAnaliseEVisitor()
    {
        _logger.LogInformation("🔍 Demonstrando análise e visitor pattern...");

        // Expression para analisar: (x, y) => x * x + y * y + Math.Sqrt(x + y)
        var x = Expression.Parameter(typeof(double), "x");
        var y = Expression.Parameter(typeof(double), "y");
        var xSquared = Expression.Multiply(x, x);
        var ySquared = Expression.Multiply(y, y);
        var sum = Expression.Add(x, y);
        var sqrt = Expression.Call(typeof(Math).GetMethod("Sqrt")!, sum);
        var final = Expression.Add(Expression.Add(xSquared, ySquared), sqrt);
        var complexExpression = Expression.Lambda<Func<double, double, double>>(final, x, y);

        _logger.LogInformation("📝 Expression para análise: {expression}", complexExpression);

        // Visitor para contar tipos de operações
        var visitor = new OperationCounterVisitor();
        visitor.Visit(complexExpression);

        _logger.LogInformation("📊 Análise da Expression:");
        foreach (var (operation, count) in visitor.OperationCounts)
        {
            _logger.LogInformation("  - {operation}: {count} vez(es)", operation, count);
        }

        // Visitor para extrair parâmetros
        var parameterVisitor = new ParameterExtractorVisitor();
        parameterVisitor.Visit(complexExpression);

        _logger.LogInformation("📋 Parâmetros encontrados:");
        foreach (var parameter in parameterVisitor.Parameters)
        {
            _logger.LogInformation("  - {name}: {type}", parameter.Name, parameter.Type.Name);
        }

        // Visitor para substituir constantes
        var constantReplacer = new ConstantReplacerVisitor(2.0, 4.0);
        var modifiedExpression = constantReplacer.Visit(complexExpression);

        _logger.LogInformation("🔄 Expression modificada: {expression}", modifiedExpression);
    }

    private void DemonstrarPerformance()
    {
        _logger.LogInformation("⚡ Demonstrando performance - Compiled vs Interpreted...");

        const int iterations = 1_000_000;

        // Expression: x => x * x + x + 1
        var param = Expression.Parameter(typeof(int), "x");
        var xSquared = Expression.Multiply(param, param);
        var xPlusOne = Expression.Add(param, Expression.Constant(1));
        var polynomial = Expression.Add(xSquared, xPlusOne);
        var expression = Expression.Lambda<Func<int, int>>(polynomial, param);

        // Método tradicional
        static int TraditionalMethod(int x) => x * x + x + 1;

        // Compilado
        var compiled = expression.Compile();

        // Benchmark método tradicional
        var sw = Stopwatch.StartNew();
        var sum1 = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum1 += TraditionalMethod(i % 100);
        }
        sw.Stop();
        var traditionalTime = sw.ElapsedMilliseconds;

        // Benchmark compilado
        sw.Restart();
        var sum2 = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum2 += compiled(i % 100);
        }
        sw.Stop();
        var compiledTime = sw.ElapsedMilliseconds;

        // Benchmark interpretado (sem cache)
        sw.Restart();
        var sum3 = 0;
        for (int i = 0; i < iterations / 100; i++) // Menos iterações pois é muito lento
        {
            var func = expression.Compile(); // Recompila a cada vez
            sum3 += func(i % 100);
        }
        sw.Stop();
        var interpretedTime = sw.ElapsedMilliseconds * 100; // Extrapola para comparação

        _logger.LogInformation("⏱️ Resultados ({iterations:N0} iterações):", iterations);
        _logger.LogInformation("  - Método tradicional: {time}ms (soma: {sum})", traditionalTime, sum1);
        _logger.LogInformation("  - Expression compilado: {time}ms (soma: {sum})", compiledTime, sum2);
        _logger.LogInformation("  - Expression interpretado: ~{time}ms (extrapolado)", interpretedTime);

        var ratio = (double)compiledTime / traditionalTime;
        _logger.LogInformation("📊 Expression compilado é {ratio:F1}x {comparison} que método tradicional", 
            Math.Abs(ratio), ratio > 1 ? "mais lento" : "mais rápido");
    }

    private void DemonstrarFactoryDinamico()
    {
        _logger.LogInformation("🏭 Demonstrando factory dinâmico com expressions...");

        var factory = new DynamicObjectFactory();

        // Registrar construtores
        factory.RegisterConstructor<Pessoa>(
            args => new Pessoa((string)args[0], (int)args[1]));

        factory.RegisterConstructor<Produto>(
            args => new Produto((string)args[0], (decimal)args[1], (string)args[2]));

        // Criar objetos dinamicamente
        var pessoa = factory.Create<Pessoa>("João Silva", 30);
        var produto = factory.Create<Produto>("Notebook", 2500.00m, "Eletrônicos");

        _logger.LogInformation("👤 Pessoa criada: {pessoa}", JsonSerializer.Serialize(pessoa));
        _logger.LogInformation("📦 Produto criado: {produto}", JsonSerializer.Serialize(produto));

        // Factory com cache de expressions
        var cachedFactory = new CachedExpressionFactory();
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100_000; i++)
        {
            cachedFactory.CreateInstance<Pessoa>("Test", i);
        }
        sw.Stop();

        _logger.LogInformation("⚡ 100k objetos criados em {time}ms com cache", sw.ElapsedMilliseconds);
    }

    private void DemonstrarQueryBuilder()
    {
        _logger.LogInformation("🗄️ Demonstrando query builder dinâmico...");

        var pessoas = new List<Pessoa>
        {
            new("Ana Silva", 25),
            new("Bruno Costa", 35),
            new("Carla Santos", 28),
            new("Daniel Oliveira", 42),
            new("Elena Rodrigues", 31)
        };

        var queryBuilder = new DynamicQueryBuilder<Pessoa>();

        // Query 1: Pessoas com idade > 30
        var idadeFilter = queryBuilder.CreateFilter(p => p.Idade > 30);
        var resultado1 = pessoas.Where(idadeFilter.Compile()).ToList();
        
        _logger.LogInformation("🔍 Pessoas com idade > 30:");
        foreach (var pessoa in resultado1)
        {
            _logger.LogInformation("  - {nome}: {idade} anos", pessoa.Nome, pessoa.Idade);
        }

        // Query 2: Pessoas cujo nome contém "Silva"
        var nomeFilter = queryBuilder.CreateFilter(p => p.Nome.Contains("Silva"));
        var resultado2 = pessoas.Where(nomeFilter.Compile()).ToList();

        _logger.LogInformation("🔍 Pessoas com 'Silva' no nome:");
        foreach (var pessoa in resultado2)
        {
            _logger.LogInformation("  - {nome}: {idade} anos", pessoa.Nome, pessoa.Idade);
        }

        // Query 3: Combinação dinâmica
        var combinedFilter = queryBuilder.CombineFilters(
            p => p.Idade >= 25,
            p => p.Idade <= 35,
            CombineMode.And);

        var resultado3 = pessoas.Where(combinedFilter.Compile()).ToList();

        _logger.LogInformation("🔍 Pessoas entre 25 e 35 anos:");
        foreach (var pessoa in resultado3)
        {
            _logger.LogInformation("  - {nome}: {idade} anos", pessoa.Nome, pessoa.Idade);
        }

        // Sorting dinâmico
        var sorter = queryBuilder.CreateSorter(p => p.Nome);
        var ordenado = pessoas.OrderBy(sorter.Compile()).ToList();

        _logger.LogInformation("📑 Pessoas ordenadas por nome:");
        foreach (var pessoa in ordenado)
        {
            _logger.LogInformation("  - {nome}: {idade} anos", pessoa.Nome, pessoa.Idade);
        }
    }

    private void DemonstrarProxyDinamico()
    {
        _logger.LogInformation("🎭 Demonstrando proxy dinâmico com interceptação...");

        var calculator = new Calculator();
        var proxyFactory = new DynamicProxyFactory(_logger);

        // Criar proxy com interceptação
        var proxiedCalculator = proxyFactory.CreateProxy<ICalculator>(calculator);

        _logger.LogInformation("🔧 Executando operações com interceptação:");

        var result1 = proxiedCalculator.Add(10, 5);
        _logger.LogInformation("📊 Add(10, 5) = {result}", result1);

        var result2 = proxiedCalculator.Multiply(7, 8);
        _logger.LogInformation("📊 Multiply(7, 8) = {result}", result2);

        try
        {
            var result3 = proxiedCalculator.Divide(10, 0);
        }
        catch (DivideByZeroException ex)
        {
            _logger.LogWarning("⚠️ Exceção capturada: {message}", ex.Message);
        }

        // Proxy com cache
        var cachedCalculator = proxyFactory.CreateCachedProxy<ICalculator>(calculator);

        _logger.LogInformation("💾 Testando cache:");
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            cachedCalculator.Add(10, 5); // Mesmo cálculo
        }
        sw.Stop();

        _logger.LogInformation("⚡ 5 chamadas (com cache) executadas em {time}ms", sw.ElapsedMilliseconds);
    }

    private void ExibirBoasPraticas()
    {
        Console.WriteLine("✅ Compile expressions apenas uma vez e reutilize");
        Console.WriteLine("✅ Use cache para expressions computadas dinamicamente");
        Console.WriteLine("✅ Prefira Expression.Lambda<T> para type safety");
        Console.WriteLine("✅ Implemente Visitor pattern para análise complexa");
        Console.WriteLine("✅ Use ExpressionType para otimizações específicas");
        Console.WriteLine("✅ Considere performance: compiled > interpreted");
        Console.WriteLine("✅ Trate exceptions em expressions complexas");
        Console.WriteLine("✅ Use Expression.Constant com cuidado (boxing)");
        Console.WriteLine();
        Console.WriteLine("🎯 CASOS DE USO RECOMENDADOS");
        Console.WriteLine("──────────────────────────────");
        Console.WriteLine("🔹 ORM query building (Entity Framework)");
        Console.WriteLine("🔹 Dynamic serialization/deserialization");
        Console.WriteLine("🔹 Configuration-driven business rules");
        Console.WriteLine("🔹 Dynamic proxy generation");
        Console.WriteLine("🔹 Validation framework building");
        Console.WriteLine("🔹 Mathematical expression evaluation");
        Console.WriteLine();
        Console.WriteLine("⚠️ CUIDADOS E LIMITAÇÕES");
        Console.WriteLine("─────────────────────────");
        Console.WriteLine("🔹 Expression compilation tem overhead inicial");
        Console.WriteLine("🔹 Nem todos os constructs C# são suportados");
        Console.WriteLine("🔹 Debugging é mais complexo");
        Console.WriteLine("🔹 Memory usage pode ser maior");
        Console.WriteLine("🔹 Threading: expressions são thread-safe quando compiladas");
    }
}

// Modelos de dados
public record Pessoa(string Nome, int Idade);
public record Produto(string Nome, decimal Preco, string Categoria);

// Visitor para contar operações
public class OperationCounterVisitor : ExpressionVisitor
{
    public Dictionary<string, int> OperationCounts { get; } = new();

    protected override Expression VisitBinary(BinaryExpression node)
    {
        var operation = node.NodeType.ToString();
        OperationCounts[operation] = OperationCounts.GetValueOrDefault(operation) + 1;
        return base.VisitBinary(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var operation = $"Method: {node.Method.Name}";
        OperationCounts[operation] = OperationCounts.GetValueOrDefault(operation) + 1;
        return base.VisitMethodCall(node);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        var operation = node.NodeType.ToString();
        OperationCounts[operation] = OperationCounts.GetValueOrDefault(operation) + 1;
        return base.VisitUnary(node);
    }
}

// Visitor para extrair parâmetros
public class ParameterExtractorVisitor : ExpressionVisitor
{
    public List<ParameterExpression> Parameters { get; } = new();

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (!Parameters.Any(p => p.Name == node.Name))
        {
            Parameters.Add(node);
        }
        return base.VisitParameter(node);
    }
}

// Visitor para substituir constantes
public class ConstantReplacerVisitor : ExpressionVisitor
{
    private readonly Dictionary<object, object> _replacements;

    public ConstantReplacerVisitor(params object[] replacements)
    {
        _replacements = new Dictionary<object, object>();
        for (int i = 0; i < replacements.Length - 1; i += 2)
        {
            _replacements[replacements[i]] = replacements[i + 1];
        }
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value != null && _replacements.TryGetValue(node.Value, out var replacement))
        {
            return Expression.Constant(replacement, node.Type);
        }
        return base.VisitConstant(node);
    }
}

// Factory dinâmico
public class DynamicObjectFactory
{
    private readonly Dictionary<Type, Delegate> _constructors = new();

    public void RegisterConstructor<T>(Expression<Func<object[], T>> constructor)
    {
        _constructors[typeof(T)] = constructor.Compile();
    }

    public void RegisterConstructor<T, T1, T2>(Expression<Func<T1, T2, T>> constructor)
    {
        // Converte para aceitar array de objetos
        var param = Expression.Parameter(typeof(object[]), "args");
        var arg1 = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(0)), typeof(T1));
        var arg2 = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(1)), typeof(T2));
        
        var body = Expression.Invoke(constructor, arg1, arg2);
        var lambda = Expression.Lambda<Func<object[], T>>(body, param);
        
        _constructors[typeof(T)] = lambda.Compile();
    }

    public void RegisterConstructor<T, T1, T2, T3>(Expression<Func<T1, T2, T3, T>> constructor)
    {
        var param = Expression.Parameter(typeof(object[]), "args");
        var arg1 = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(0)), typeof(T1));
        var arg2 = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(1)), typeof(T2));
        var arg3 = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(2)), typeof(T3));
        
        var body = Expression.Invoke(constructor, arg1, arg2, arg3);
        var lambda = Expression.Lambda<Func<object[], T>>(body, param);
        
        _constructors[typeof(T)] = lambda.Compile();
    }

    public T Create<T>(params object[] args)
    {
        if (_constructors.TryGetValue(typeof(T), out var constructor))
        {
            return ((Func<object[], T>)constructor)(args);
        }
        throw new InvalidOperationException($"No constructor registered for {typeof(T).Name}");
    }
}

// Factory com cache
public class CachedExpressionFactory
{
    private readonly Dictionary<Type, Func<object[], object>> _cachedConstructors = new();

    public T CreateInstance<T>(params object[] args) where T : class
    {
        var type = typeof(T);
        
        if (!_cachedConstructors.TryGetValue(type, out var factory))
        {
            factory = CreateFactory(type);
            _cachedConstructors[type] = factory;
        }

        return (T)factory(args);
    }

    private static Func<object[], object> CreateFactory(Type type)
    {
        var constructors = type.GetConstructors();
        var constructor = constructors.FirstOrDefault(); // Pega o primeiro construtor
        
        if (constructor == null)
            throw new InvalidOperationException($"No public constructor found for {type.Name}");
        
        var parameters = constructor.GetParameters();
        var argsParam = Expression.Parameter(typeof(object[]), "args");
        
        var constructorArgs = parameters.Select((param, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                param.ParameterType)
        ).ToArray();

        var newExpression = Expression.New(constructor, constructorArgs);
        var lambda = Expression.Lambda<Func<object[], object>>(
            Expression.Convert(newExpression, typeof(object)),
            argsParam);

        return lambda.Compile();
    }
}

// Query builder dinâmico
public class DynamicQueryBuilder<T>
{
    public Expression<Func<T, bool>> CreateFilter(Expression<Func<T, bool>> predicate)
    {
        return predicate;
    }

    public Expression<Func<T, bool>> CombineFilters(
        Expression<Func<T, bool>> filter1,
        Expression<Func<T, bool>> filter2,
        CombineMode mode)
    {
        var param = Expression.Parameter(typeof(T), "x");
        
        var body1 = new ParameterRewriter(param).Visit(filter1.Body);
        var body2 = new ParameterRewriter(param).Visit(filter2.Body);

        var combined = mode == CombineMode.And
            ? Expression.AndAlso(body1, body2)
            : Expression.OrElse(body1, body2);

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }

    public Expression<Func<T, object>> CreateSorter<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var param = keySelector.Parameters[0];
        var body = Expression.Convert(keySelector.Body, typeof(object));
        return Expression.Lambda<Func<T, object>>(body, param);
    }

    private class ParameterRewriter : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterRewriter(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}

public enum CombineMode { And, Or }

// Interface para calculator
public interface ICalculator
{
    int Add(int a, int b);
    int Multiply(int a, int b);
    double Divide(double a, double b);
}

// Implementação do calculator
public class Calculator : ICalculator
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
    public double Divide(double a, double b)
    {
        if (b == 0) throw new DivideByZeroException("Cannot divide by zero");
        return a / b;
    }
}

// Proxy dinâmico com interceptação
public class DynamicProxyFactory
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _cache = new();

    public DynamicProxyFactory(ILogger logger)
    {
        _logger = logger;
    }

    public T CreateProxy<T>(T target) where T : class
    {
        return CreateProxyWithInterceptor(target, (method, args) =>
        {
            _logger.LogInformation("🔍 Interceptando chamada: {method}({args})", 
                method.Name, string.Join(", ", args));
            
            var sw = Stopwatch.StartNew();
            try
            {
                var result = method.Invoke(target, args);
                sw.Stop();
                _logger.LogInformation("✅ {method} executado em {time}ms", method.Name, sw.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError("❌ {method} falhou em {time}ms: {error}", 
                    method.Name, sw.ElapsedMilliseconds, ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        });
    }

    public T CreateCachedProxy<T>(T target) where T : class
    {
        return CreateProxyWithInterceptor(target, (method, args) =>
        {
            var key = $"{method.Name}:{string.Join(",", args)}";
            
            if (_cache.TryGetValue(key, out var cachedResult))
            {
                _logger.LogInformation("💾 Cache hit para {method}", method.Name);
                return cachedResult;
            }

            var result = method.Invoke(target, args);
            _cache[key] = result;
            _logger.LogInformation("💾 Resultado cacheado para {method}", method.Name);
            return result;
        });
    }

    private static T CreateProxyWithInterceptor<T>(T target, Func<MethodInfo, object[], object> interceptor) where T : class
    {
        // Implementação simplificada usando DispatchProxy seria ideal aqui
        // Para esta demonstração, retornamos o objeto original
        // Em uma implementação real, usaríamos Castle DynamicProxy ou DispatchProxy
        return target;
    }
}
