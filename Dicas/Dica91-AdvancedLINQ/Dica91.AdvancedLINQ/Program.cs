using System.Linq.Expressions;
using System.Reflection;

Console.WriteLine("🔍 Dica 91: Advanced LINQ Expressions (.NET 9)");
Console.WriteLine("===============================================");

// Dados de exemplo
var employees = new List<Employee>
{
    new(1, "Ana", "Engineering", 75000, DateTime.Parse("2020-01-15"), ["C#", "Azure", "SQL"]),
    new(2, "Bruno", "Marketing", 65000, DateTime.Parse("2019-03-20"), ["PowerPoint", "Analytics"]),
    new(3, "Carlos", "Engineering", 80000, DateTime.Parse("2021-07-10"), ["Python", "Docker", "Kubernetes"]),
    new(4, "Diana", "Sales", 70000, DateTime.Parse("2018-11-05"), ["CRM", "Negotiation"]),
    new(5, "Eduardo", "Engineering", 90000, DateTime.Parse("2017-05-15"), ["C#", "Azure", "React", "Node.js"]),
    new(6, "Fernanda", "HR", 60000, DateTime.Parse("2022-02-28"), ["Recruitment", "Training"]),
    new(7, "Gustavo", "Engineering", 85000, DateTime.Parse("2020-09-12"), ["Java", "Spring", "AWS"]),
    new(8, "Helena", "Marketing", 68000, DateTime.Parse("2021-01-30"), ["Digital Marketing", "SEO"])
};

// 1. Expression Trees Avançadas
Console.WriteLine("\n1. 🌳 Expression Trees Avançadas:");
Console.WriteLine("──────────────────────────────────");

DemonstrarExpressionTrees();

// 2. LINQ Dinâmico
Console.WriteLine("\n2. ⚡ LINQ Dinâmico:");
Console.WriteLine("────────────────────");

DemonstrarLINQDinamico(employees);

// 3. Custom LINQ Operators
Console.WriteLine("\n3. 🔧 Custom LINQ Operators:");
Console.WriteLine("────────────────────────────");

DemonstrarCustomOperators(employees);

// 4. Parallel LINQ Avançado
Console.WriteLine("\n4. 🚀 Parallel LINQ Avançado:");
Console.WriteLine("──────────────────────────────");

await DemonstrarPLINQAvancado();

// 5. LINQ com Memory e Span
Console.WriteLine("\n5. 💾 LINQ com Memory e Span:");
Console.WriteLine("──────────────────────────────");

DemonstrarLINQComMemory();

// 6. Query Optimization
Console.WriteLine("\n6. 📊 Query Optimization:");
Console.WriteLine("─────────────────────────");

DemonstrarQueryOptimization(employees);

Console.WriteLine("\n✅ Demonstração completa de Advanced LINQ!");

static void DemonstrarExpressionTrees()
{
    Console.WriteLine("🌳 Construindo Expression Trees dinamicamente:");
    
    // Expression Tree básica: x => x > 5
    var parameter = Expression.Parameter(typeof(int), "x");
    var constant = Expression.Constant(5);
    var greaterThan = Expression.GreaterThan(parameter, constant);
    var lambda = Expression.Lambda<Func<int, bool>>(greaterThan, parameter);
    
    var compiled = lambda.Compile();
    var numbers = new[] { 1, 6, 3, 8, 2, 9 };
    var filtered = numbers.Where(compiled).ToList();
    
    Console.WriteLine($"✅ Expressão: {lambda}");
    Console.WriteLine($"✅ Números > 5: [{string.Join(", ", filtered)}]");
    
    // Expression Tree complexa para Employee
    DemonstrarExpressionTreeComplexa();
}

static void DemonstrarExpressionTreeComplexa()
{
    Console.WriteLine("\n🌳 Expression Tree complexa para Employee:");
    
    // Construir: emp => emp.Department == "Engineering" && emp.Salary > 75000
    var empParam = Expression.Parameter(typeof(Employee), "emp");
    
    // emp.Department
    var deptProperty = Expression.Property(empParam, nameof(Employee.Department));
    var deptConstant = Expression.Constant("Engineering");
    var deptEquals = Expression.Equal(deptProperty, deptConstant);
    
    // emp.Salary
    var salaryProperty = Expression.Property(empParam, nameof(Employee.Salary));
    var salaryConstant = Expression.Constant(75000m);
    var salaryGreater = Expression.GreaterThan(salaryProperty, salaryConstant);
    
    // Combinar com AND
    var combined = Expression.AndAlso(deptEquals, salaryGreater);
    var lambdaExp = Expression.Lambda<Func<Employee, bool>>(combined, empParam);
    
    Console.WriteLine($"✅ Expression Tree: {lambdaExp}");
    
    // Aplicar aos dados
    var employees = GetSampleEmployees();
    var result = employees.Where(lambdaExp.Compile()).ToList();
    
    Console.WriteLine($"✅ Engenheiros com salário > $75k: {result.Count}");
    foreach (var emp in result)
    {
        Console.WriteLine($"   {emp.Name}: ${emp.Salary:N0}");
    }
}

static void DemonstrarLINQDinamico(List<Employee> employees)
{
    Console.WriteLine("⚡ LINQ Queries dinâmicas baseadas em critérios:");
    
    // Simular filtros do usuário
    var filters = new Dictionary<string, object>
    {
        ["MinSalary"] = 70000m,
        ["Department"] = "Engineering",
        ["MinExperience"] = 3
    };
    
    var query = employees.AsQueryable();
    
    // Aplicar filtros dinamicamente
    foreach (var filter in filters)
    {
        query = filter.Key switch
        {
            "MinSalary" => query.Where(CreateSalaryFilter((decimal)filter.Value)),
            "Department" => query.Where(CreateDepartmentFilter((string)filter.Value)),
            "MinExperience" => query.Where(CreateExperienceFilter((int)filter.Value)),
            _ => query
        };
    }
    
    var results = query.ToList();
    Console.WriteLine($"✅ Filtros aplicados: {filters.Count}");
    Console.WriteLine($"✅ Resultados encontrados: {results.Count}");
    
    foreach (var emp in results)
    {
        var experience = DateTime.Now.Year - emp.HireDate.Year;
        Console.WriteLine($"   {emp.Name} ({emp.Department}): ${emp.Salary:N0}, {experience} anos");
    }
    
    // Query dinâmica com ordenação
    Console.WriteLine("\n⚡ Ordenação dinâmica:");
    DemonstrarOrdenacaoDinamica(employees);
}

static Expression<Func<Employee, bool>> CreateSalaryFilter(decimal minSalary)
{
    return emp => emp.Salary >= minSalary;
}

static Expression<Func<Employee, bool>> CreateDepartmentFilter(string department)
{
    return emp => emp.Department == department;
}

static Expression<Func<Employee, bool>> CreateExperienceFilter(int minYears)
{
    return emp => DateTime.Now.Year - emp.HireDate.Year >= minYears;
}

static void DemonstrarOrdenacaoDinamica(List<Employee> employees)
{
    var sortCriteria = new[]
    {
        ("Salary", "desc"),
        ("Department", "asc"),
        ("Name", "asc")
    };
    
    IOrderedEnumerable<Employee>? orderedQuery = null;
    
    for (int i = 0; i < sortCriteria.Length; i++)
    {
        var (property, direction) = sortCriteria[i];
        
        if (i == 0)
        {
            orderedQuery = direction == "asc" 
                ? employees.OrderBy(GetPropertySelector<Employee>(property))
                : employees.OrderByDescending(GetPropertySelector<Employee>(property));
        }
        else
        {
            orderedQuery = direction == "asc"
                ? orderedQuery!.ThenBy(GetPropertySelector<Employee>(property))
                : orderedQuery!.ThenByDescending(GetPropertySelector<Employee>(property));
        }
    }
    
    var sorted = orderedQuery!.Take(5).ToList();
    Console.WriteLine("✅ Top 5 funcionários (Salary desc, Department asc, Name asc):");
    
    foreach (var emp in sorted)
    {
        Console.WriteLine($"   {emp.Name} ({emp.Department}): ${emp.Salary:N0}");
    }
}

static Func<T, object> GetPropertySelector<T>(string propertyName)
{
    var parameter = Expression.Parameter(typeof(T));
    var property = Expression.Property(parameter, propertyName);
    var conversion = Expression.Convert(property, typeof(object));
    var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);
    return lambda.Compile();
}

static void DemonstrarCustomOperators(List<Employee> employees)
{
    Console.WriteLine("🔧 Custom LINQ Operators:");
    
    // 1. Distinct by property
    var distinctDepartments = employees.DistinctBy(e => e.Department).ToList();
    Console.WriteLine($"✅ Departamentos únicos: {distinctDepartments.Count}");
    foreach (var emp in distinctDepartments)
    {
        Console.WriteLine($"   {emp.Department}: {emp.Name}");
    }
    
    // 2. Custom Batch operator
    Console.WriteLine("\n🔧 Batch Processing:");
    var batches = employees.Batch(3).ToList();
    Console.WriteLine($"✅ Total de lotes: {batches.Count}");
    
    for (int i = 0; i < batches.Count; i++)
    {
        var batch = batches[i].ToList();
        Console.WriteLine($"   Lote {i + 1}: {batch.Count} funcionários");
        foreach (var emp in batch)
        {
            Console.WriteLine($"     - {emp.Name}");
        }
    }
    
    // 3. Custom Window operator
    Console.WriteLine("\n🔧 Windowing Functions:");
    var windowed = employees
        .OrderBy(e => e.Salary)
        .Window(3)
        .Select((window, index) => new
        {
            Position = index + 1,
            Employees = window.ToList(),
            AvgSalary = window.Average(e => e.Salary)
        })
        .ToList();
    
    Console.WriteLine("✅ Janelas de 3 funcionários (ordenado por salário):");
    foreach (var w in windowed.Take(3))
    {
        Console.WriteLine($"   Janela {w.Position}: Média ${w.AvgSalary:N0}");
        foreach (var emp in w.Employees)
        {
            Console.WriteLine($"     - {emp.Name}: ${emp.Salary:N0}");
        }
    }
}

static async Task DemonstrarPLINQAvancado()
{
    Console.WriteLine("🚀 Parallel LINQ com configurações avançadas:");
    
    var largeDataSet = Enumerable.Range(1, 1_000_000).ToList();
    
    // 1. PLINQ com configuração de grau de paralelismo
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    var result1 = largeDataSet
        .AsParallel()
        .WithDegreeOfParallelism(Environment.ProcessorCount)
        .Where(x => IsPrime(x))
        .Count();
    
    sw.Stop();
    Console.WriteLine($"✅ Primos encontrados (PLINQ): {result1:N0} em {sw.ElapsedMilliseconds}ms");
    
    // 2. PLINQ com ordering
    sw.Restart();
    
    var result2 = largeDataSet
        .AsParallel()
        .AsOrdered()
        .Where(x => x % 2 == 0)
        .Take(1000)
        .Sum();
    
    sw.Stop();
    Console.WriteLine($"✅ Soma dos primeiros 1000 pares: {result2:N0} em {sw.ElapsedMilliseconds}ms");
    
    // 3. PLINQ com particionamento personalizado
    Console.WriteLine("\n🚀 Particionamento personalizado:");
    await DemonstrarParticionamento(largeDataSet);
    
    // 4. PLINQ com tratamento de exceções
    Console.WriteLine("\n🚀 Tratamento de exceções em PLINQ:");
    DemonstrarPLINQComExcecoes();
}

static bool IsPrime(int number)
{
    if (number < 2) return false;
    if (number == 2) return true;
    if (number % 2 == 0) return false;
    
    var sqrt = (int)Math.Sqrt(number);
    for (int i = 3; i <= sqrt; i += 2)
    {
        if (number % i == 0) return false;
    }
    return true;
}

static async Task DemonstrarParticionamento(List<int> data)
{
    // Usar um dataset menor para evitar overflow
    var smallerData = data.Take(10000).ToList();
    var partitioner = System.Collections.Concurrent.Partitioner.Create(smallerData, true);
    
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    var result = partitioner
        .AsParallel()
        .Where(x => x % 3 == 0)
        .Select(x => (long)x) // Usar long para evitar overflow
        .Sum();
    
    sw.Stop();
    Console.WriteLine($"✅ Soma múltiplos de 3 (particionamento): {result:N0} em {sw.ElapsedMilliseconds}ms");
    
    await Task.Delay(1); // Para evitar warning
}

static void DemonstrarPLINQComExcecoes()
{
    var numbers = Enumerable.Range(1, 100).ToList();
    
    try
    {
        var result = numbers
            .AsParallel()
            .Select(x =>
            {
                if (x == 50) throw new InvalidOperationException($"Erro simulado no número {x}");
                return x * x;
            })
            .Where(x => x > 1000)
            .ToList();
        
        Console.WriteLine($"✅ Processamento sem erros: {result.Count} resultados");
    }
    catch (AggregateException ex)
    {
        Console.WriteLine($"❌ Exceções capturadas: {ex.InnerExceptions.Count}");
        foreach (var inner in ex.InnerExceptions)
        {
            Console.WriteLine($"   - {inner.Message}");
        }
    }
}

static void DemonstrarLINQComMemory()
{
    Console.WriteLine("💾 LINQ otimizado com Memory<T> e Span<T>:");
    
    var data = Enumerable.Range(1, 1000).ToArray();
    Memory<int> memory = data.AsMemory();
    
    // Processamento em chunks usando Memory
    var chunkSize = 100;
    var results = new List<double>();
    
    for (int i = 0; i < memory.Length; i += chunkSize)
    {
        var chunk = memory.Slice(i, Math.Min(chunkSize, memory.Length - i));
        var span = chunk.Span;
        
        // Processar chunk como Span
        var average = CalculateAverage(span);
        results.Add(average);
    }
    
    Console.WriteLine($"✅ Processado {data.Length} elementos em {results.Count} chunks");
    Console.WriteLine($"✅ Média geral dos chunks: {results.Average():F2}");
    
    // LINQ com ReadOnlySpan
    DemonstrarSpanLINQ();
}

static double CalculateAverage(ReadOnlySpan<int> span)
{
    if (span.Length == 0) return 0;
    
    long sum = 0;
    foreach (var value in span)
    {
        sum += value;
    }
    
    return (double)sum / span.Length;
}

static void DemonstrarSpanLINQ()
{
    Console.WriteLine("\n💾 LINQ-like operations com Span:");
    
    Span<int> numbers = stackalloc int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    
    // Filtrar pares usando span
    var evenCount = 0;
    foreach (var num in numbers)
    {
        if (num % 2 == 0) evenCount++;
    }
    
    Console.WriteLine($"✅ Números pares no Span: {evenCount}");
    
    // Transformação in-place
    for (int i = 0; i < numbers.Length; i++)
    {
        numbers[i] *= numbers[i]; // Elevar ao quadrado
    }
    
    var sum = 0;
    foreach (var num in numbers)
    {
        sum += num;
    }
    
    Console.WriteLine($"✅ Soma dos quadrados: {sum}");
}

static void DemonstrarQueryOptimization(List<Employee> employees)
{
    Console.WriteLine("📊 Otimizações de Query:");
    
    // 1. Evitar múltiplas enumerações
    Console.WriteLine("\n📊 Evitando múltiplas enumerações:");
    
    var engineeringQuery = employees.Where(e => e.Department == "Engineering");
    
    // ❌ Má prática - múltiplas enumerações
    // var count = engineeringQuery.Count();
    // var list = engineeringQuery.ToList();
    
    // ✅ Boa prática - enumerar uma vez
    var engineeringList = engineeringQuery.ToList();
    var count = engineeringList.Count;
    
    Console.WriteLine($"✅ Engenheiros encontrados: {count}");
    
    // 2. Otimização com ToLookup
    Console.WriteLine("\n📊 Otimização com ToLookup:");
    
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var departmentLookup = employees.ToLookup(e => e.Department);
    sw.Stop();
    
    Console.WriteLine($"✅ Lookup criado em {sw.ElapsedTicks} ticks");
    
    sw.Restart();
    var engineeringEmps = departmentLookup["Engineering"].ToList();
    var marketingEmps = departmentLookup["Marketing"].ToList();
    sw.Stop();
    
    Console.WriteLine($"✅ Consultas ao lookup: {sw.ElapsedTicks} ticks");
    Console.WriteLine($"   Engineering: {engineeringEmps.Count}, Marketing: {marketingEmps.Count}");
    
    // 3. Lazy evaluation vs Eager evaluation
    Console.WriteLine("\n📊 Lazy vs Eager evaluation:");
    DemonstrarLazyVsEager(employees);
}

static void DemonstrarLazyVsEager(List<Employee> employees)
{
    // Lazy evaluation
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var lazyQuery = employees
        .Where(e => e.Salary > 60000)
        .Select(e => new { e.Name, e.Department, e.Salary })
        .OrderByDescending(e => e.Salary);
    sw.Stop();
    
    Console.WriteLine($"✅ Query lazy criada em {sw.ElapsedTicks} ticks");
    
    // Execution happens here
    sw.Restart();
    var lazyResults = lazyQuery.Take(3).ToList();
    sw.Stop();
    
    Console.WriteLine($"✅ Query lazy executada em {sw.ElapsedTicks} ticks");
    
    // Eager evaluation
    sw.Restart();
    var eagerResults = employees
        .Where(e => e.Salary > 60000)
        .Select(e => new { e.Name, e.Department, e.Salary })
        .OrderByDescending(e => e.Salary)
        .ToList() // Força execução imediata
        .Take(3)
        .ToList();
    sw.Stop();
    
    Console.WriteLine($"✅ Query eager executada em {sw.ElapsedTicks} ticks");
    Console.WriteLine($"✅ Ambas retornaram {lazyResults.Count} resultados");
}

static List<Employee> GetSampleEmployees()
{
    return new List<Employee>
    {
        new(1, "Ana", "Engineering", 75000, DateTime.Parse("2020-01-15"), ["C#", "Azure"]),
        new(2, "Bruno", "Marketing", 65000, DateTime.Parse("2019-03-20"), ["Analytics"]),
        new(3, "Carlos", "Engineering", 80000, DateTime.Parse("2021-07-10"), ["Python"])
    };
}

// Extension methods personalizados
public static class LINQExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);
        
        foreach (var item in source)
        {
            batch.Add(item);
            
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }
        
        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
    
    public static IEnumerable<IEnumerable<T>> Window<T>(this IEnumerable<T> source, int windowSize)
    {
        var window = new Queue<T>(windowSize);
        
        foreach (var item in source)
        {
            window.Enqueue(item);
            
            if (window.Count > windowSize)
            {
                window.Dequeue();
            }
            
            if (window.Count == windowSize)
            {
                yield return window.ToArray();
            }
        }
    }
}

// Record para Employee
public record Employee(
    int Id,
    string Name,
    string Department,
    decimal Salary,
    DateTime HireDate,
    string[] Skills
);
