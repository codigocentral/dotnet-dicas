using System.Diagnostics;
using System.Runtime.CompilerServices;

Console.WriteLine("=== Dica 46: Palavra-chave 'in' (Passagem de Structs por Referência Readonly) ===\n");

// Demonstração 1: Comparando passagem por valor vs 'in'
Console.WriteLine("1. Comparando passagem por valor vs 'in':");
DemonstrarDiferencasPassagem();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 2: Performance com structs grandes
Console.WriteLine("2. Performance com structs grandes:");
DemonstrarPerformanceStructsGrandes();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 3: Readonly structs para máxima segurança
Console.WriteLine("3. Readonly structs para máxima segurança:");
DemonstrarReadonlyStructs();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 4: Cópias defensivas (armadilha de performance)
Console.WriteLine("4. Cópias defensivas (armadilha de performance):");
DemonstrarCopiasDefensivas();

Console.WriteLine("\n" + new string('=', 60) + "\n");

// Demonstração 5: Casos de uso práticos
Console.WriteLine("5. Casos de uso práticos:");
DemonstrarCasosDeUsoPraticos();

Console.WriteLine("\n=== Resumo das Vantagens da Palavra-chave 'in' ===");
Console.WriteLine("✅ Evita cópias desnecessárias de structs grandes");
Console.WriteLine("✅ Mantém a semântica de readonly (método não pode modificar)");
Console.WriteLine("✅ Performance superior para structs > 16 bytes");
Console.WriteLine("✅ Segurança de tipos em tempo de compilação");
Console.WriteLine("✅ Ideal para cálculos matemáticos intensivos");
Console.WriteLine("⚠️  Combine com readonly structs para evitar cópias defensivas");

static void DemonstrarDiferencasPassagem()
{
    Console.WriteLine("  📊 Diferenças na passagem de parâmetros:");
    
    var pequenaStruct = new PequenaStruct(42, 3.14f);
    var grandeStruct = new GrandeStruct(
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 10.10
    );
    
    Console.WriteLine("\n  Passagem por valor (cria cópia):");
    var resultadoPorValor = ProcessarPorValor(grandeStruct);
    Console.WriteLine($"     Resultado: {resultadoPorValor:F2}");
    
    Console.WriteLine("\n  Passagem por 'in' (referência readonly):");
    var resultadoPorIn = ProcessarPorIn(in grandeStruct);
    Console.WriteLine($"     Resultado: {resultadoPorIn:F2}");
    
    Console.WriteLine("\n  📝 Observações:");
    Console.WriteLine("     • 'in' evita cópia da struct (melhor performance)");
    Console.WriteLine("     • Método não pode modificar a struct original");
    Console.WriteLine("     • Compilador garante imutabilidade do parâmetro");
}

static void DemonstrarPerformanceStructsGrandes()
{
    Console.WriteLine("  ⚡ Impacto de performance com structs grandes:");
    
    var matriz = new MatrizGrande();
    matriz.InicializarComValoresAleatorios();
    
    const int iteracoes = 100_000;
    
    // Teste por valor (com cópias)
    var sw1 = Stopwatch.StartNew();
    double somaPorValor = 0;
    for (int i = 0; i < iteracoes; i++)
    {
        somaPorValor += CalcularSomaPorValor(matriz);
    }
    sw1.Stop();
    
    // Teste por 'in' (sem cópias)
    var sw2 = Stopwatch.StartNew();
    double somaPorIn = 0;
    for (int i = 0; i < iteracoes; i++)
    {
        somaPorIn += CalcularSomaPorIn(in matriz);
    }
    sw2.Stop();
    
    Console.WriteLine($"     Struct size: {Unsafe.SizeOf<MatrizGrande>()} bytes");
    Console.WriteLine($"     Iterações: {iteracoes:N0}");
    Console.WriteLine($"     Por valor: {sw1.ElapsedMilliseconds} ms (soma: {somaPorValor:F0})");
    Console.WriteLine($"     Por 'in':  {sw2.ElapsedMilliseconds} ms (soma: {somaPorIn:F0})");
    Console.WriteLine($"     ⚡ Speedup: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x mais rápido");
}

static void DemonstrarReadonlyStructs()
{
    Console.WriteLine("  🔒 Readonly structs para eliminar cópias defensivas:");
    
    var pontoMutavel = new PontoMutavel(10, 20);
    var pontoReadonly = new PontoReadonly(10, 20);
    
    Console.WriteLine("\n  Struct mutável (pode gerar cópias defensivas):");
    var distanciaMutavel = CalcularDistanciaOrigemMutavel(in pontoMutavel);
    Console.WriteLine($"     Distância da origem: {distanciaMutavel:F2}");
    
    Console.WriteLine("\n  Readonly struct (sem cópias defensivas):");
    var distanciaReadonly = CalcularDistanciaOrigemReadonly(in pontoReadonly);
    Console.WriteLine($"     Distância da origem: {distanciaReadonly:F2}");
    
    Console.WriteLine("\n  📝 Readonly structs são fundamentais com 'in':");
    Console.WriteLine("     • Evitam cópias defensivas desnecessárias");
    Console.WriteLine("     • Garantem que métodos/propriedades não modificam estado");
    Console.WriteLine("     • Resultado: Performance real do 'in' parameter");
}

static void DemonstrarCopiasDefensivas()
{
    Console.WriteLine("  ⚠️  Armadilha: Cópias defensivas com structs mutáveis:");
    
    var estruturaMutavel = new EstruturaMutavel(100, 200);
    
    Console.WriteLine("\n  Quando usar 'in' com struct mutável:");
    Console.WriteLine("     • C# pode criar cópias defensivas");
    Console.WriteLine("     • Isso acontece ao acessar membros que PODEM modificar");
    Console.WriteLine("     • Resultado: Perda de performance do 'in'");
    
    // Demonstrar acesso que pode gerar cópia defensiva
    ProcessarEstruturaMutavel(in estruturaMutavel);
    
    Console.WriteLine("\n  ✅ Solução: Use readonly structs sempre que possível");
    Console.WriteLine("  ✅ Alternativa: readonly members em structs mutáveis");
}

static void DemonstrarCasosDeUsoPraticos()
{
    Console.WriteLine("  🎯 Casos de uso práticos para 'in':");
    
    Console.WriteLine("\n  1. Cálculos matemáticos com Vector3:");
    var vetor1 = new Vector3D(1.0, 2.0, 3.0);
    var vetor2 = new Vector3D(4.0, 5.0, 6.0);
    
    var produtoEscalar = CalcularProdutoEscalar(in vetor1, in vetor2);
    var distancia = CalcularDistancia(in vetor1, in vetor2);
    
    Console.WriteLine($"     Produto escalar: {produtoEscalar:F2}");
    Console.WriteLine($"     Distância: {distancia:F2}");
    
    Console.WriteLine("\n  2. Processamento de matrizes:");
    var matrizA = new Matriz4x4();
    var matrizB = new Matriz4x4();
    matrizA.InicializarIdentidade();
    matrizB.InicializarComValores(2.0f);
    
    var determinante = CalcularDeterminante(in matrizA);
    Console.WriteLine($"     Determinante matriz identidade: {determinante:F2}");
    
    Console.WriteLine("\n  3. Validação de estruturas grandes:");
    var configuracao = new ConfiguracaoCompleta
    {
        ServidorPrincipal = "localhost",
        Porta = 8080,
        TimeoutConexao = 30000,
        MaximoConexoes = 100
    };
    
    var valida = ValidarConfiguracao(in configuracao);
    Console.WriteLine($"     Configuração válida: {valida}");
}

// ===== MÉTODOS DE DEMONSTRAÇÃO =====

static double ProcessarPorValor(GrandeStruct estrutura)
{
    // Recebe uma CÓPIA da struct (custo de performance)
    return estrutura.Campo1 + estrutura.Campo11 + estrutura.Campo20;
}

static double ProcessarPorIn(in GrandeStruct estrutura)
{
    // Recebe uma REFERÊNCIA readonly (zero custo de cópia)
    return estrutura.Campo1 + estrutura.Campo11 + estrutura.Campo20;
}

static double CalcularSomaPorValor(MatrizGrande matriz)
{
    // Cópia cara da matriz inteira
    double soma = 0;
    for (int i = 0; i < 100; i++)
    {
        soma += matriz.Valores[i];
    }
    return soma;
}

static double CalcularSomaPorIn(in MatrizGrande matriz)
{
    // Sem cópia - apenas referência readonly
    double soma = 0;
    for (int i = 0; i < 100; i++)
    {
        soma += matriz.Valores[i];
    }
    return soma;
}

static double CalcularDistanciaOrigemMutavel(in PontoMutavel ponto)
{
    // PODE gerar cópia defensiva ao acessar propriedades
    return Math.Sqrt(ponto.X * ponto.X + ponto.Y * ponto.Y);
}

static double CalcularDistanciaOrigemReadonly(in PontoReadonly ponto)
{
    // Sem cópia defensiva - struct é readonly
    return Math.Sqrt(ponto.X * ponto.X + ponto.Y * ponto.Y);
}

static void ProcessarEstruturaMutavel(in EstruturaMutavel estrutura)
{
    // Ao acessar propriedades/métodos, C# pode criar cópias defensivas
    var valor = estrutura.Valor; // Pode gerar cópia
    var resultado = estrutura.CalcularAlgumaCoisa(); // Pode gerar cópia
    
    Console.WriteLine($"     Valor processado: {valor}, Resultado: {resultado}");
}

static double CalcularProdutoEscalar(in Vector3D a, in Vector3D b)
{
    return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
}

static double CalcularDistancia(in Vector3D a, in Vector3D b)
{
    var dx = a.X - b.X;
    var dy = a.Y - b.Y;
    var dz = a.Z - b.Z;
    return Math.Sqrt(dx * dx + dy * dy + dz * dz);
}

static float CalcularDeterminante(in Matriz4x4 matriz)
{
    // Cálculo simplificado para demonstração
    return matriz.M11 * matriz.M22 * matriz.M33 * matriz.M44;
}

static bool ValidarConfiguracao(in ConfiguracaoCompleta config)
{
    return !string.IsNullOrEmpty(config.ServidorPrincipal) &&
           config.Porta > 0 &&
           config.TimeoutConexao > 0 &&
           config.MaximoConexoes > 0;
}

// ===== DEFINIÇÃO DAS STRUCTS =====

public struct PequenaStruct
{
    public int Inteiro { get; }
    public float Float { get; }
    
    public PequenaStruct(int inteiro, float @float)
    {
        Inteiro = inteiro;
        Float = @float;
    }
}

public struct GrandeStruct
{
    public int Campo1, Campo2, Campo3, Campo4, Campo5;
    public int Campo6, Campo7, Campo8, Campo9, Campo10;
    public double Campo11, Campo12, Campo13, Campo14, Campo15;
    public double Campo16, Campo17, Campo18, Campo19, Campo20;
    
    public GrandeStruct(int c1, int c2, int c3, int c4, int c5,
                       int c6, int c7, int c8, int c9, int c10,
                       double c11, double c12, double c13, double c14, double c15,
                       double c16, double c17, double c18, double c19, double c20)
    {
        Campo1 = c1; Campo2 = c2; Campo3 = c3; Campo4 = c4; Campo5 = c5;
        Campo6 = c6; Campo7 = c7; Campo8 = c8; Campo9 = c9; Campo10 = c10;
        Campo11 = c11; Campo12 = c12; Campo13 = c13; Campo14 = c14; Campo15 = c15;
        Campo16 = c16; Campo17 = c17; Campo18 = c18; Campo19 = c19; Campo20 = c20;
    }
}

public struct MatrizGrande
{
    public readonly double[] Valores;
    
    public MatrizGrande()
    {
        Valores = new double[100];
    }
    
    public void InicializarComValoresAleatorios()
    {
        for (int i = 0; i < Valores.Length; i++)
        {
            Valores[i] = Random.Shared.NextDouble() * 100;
        }
    }
}

public struct PontoMutavel
{
    public double X { get; set; }
    public double Y { get; set; }
    
    public PontoMutavel(double x, double y)
    {
        X = x;
        Y = y;
    }
}

public readonly struct PontoReadonly
{
    public double X { get; }
    public double Y { get; }
    
    public PontoReadonly(double x, double y)
    {
        X = x;
        Y = y;
    }
}

public struct EstruturaMutavel
{
    public int Valor { get; set; }
    public int OutroValor { get; set; }
    
    public EstruturaMutavel(int valor, int outroValor)
    {
        Valor = valor;
        OutroValor = outroValor;
    }
    
    public int CalcularAlgumaCoisa()
    {
        return Valor * OutroValor;
    }
}

public readonly struct Vector3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public struct Matriz4x4
{
    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;
    public float M41, M42, M43, M44;
    
    public void InicializarIdentidade()
    {
        M11 = M22 = M33 = M44 = 1.0f;
        M12 = M13 = M14 = M21 = M23 = M24 = 0.0f;
        M31 = M32 = M34 = M41 = M42 = M43 = 0.0f;
    }
    
    public void InicializarComValores(float valor)
    {
        M11 = M12 = M13 = M14 = valor;
        M21 = M22 = M23 = M24 = valor;
        M31 = M32 = M33 = M34 = valor;
        M41 = M42 = M43 = M44 = valor;
    }
}

public struct ConfiguracaoCompleta
{
    public string ServidorPrincipal { get; set; }
    public string ServidorSecundario { get; set; }
    public int Porta { get; set; }
    public int TimeoutConexao { get; set; }
    public int MaximoConexoes { get; set; }
    public bool UsarSSL { get; set; }
    public string CertificadoPath { get; set; }
    public int RetryCount { get; set; }
    public TimeSpan RetryDelay { get; set; }
    public bool LogarDetalhes { get; set; }
}
