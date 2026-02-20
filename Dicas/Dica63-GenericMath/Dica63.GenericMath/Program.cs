using System.Numerics;
using System.Diagnostics;

namespace Dica63.GenericMath;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("==== Dica 63: Generic Math - C# 11+ ====\n");
        
        Console.WriteLine("🧮 Generic Math permite algoritmos matemáticos genéricos!");
        Console.WriteLine("Sistema unificado para operações com qualquer tipo numérico...\n");

        // ===== DEMONSTRAÇÃO 1: INTERFACES MATEMÁTICAS BÁSICAS =====
        Console.WriteLine("1. Interfaces Matemáticas Fundamentais");
        Console.WriteLine("--------------------------------------");

        // INumber<T> - Interface base para todos os tipos numéricos
        Console.WriteLine("✅ Soma genérica com INumber<T>:");
        Console.WriteLine($"   Soma int: {SomaGenerica(10, 20)}");
        Console.WriteLine($"   Soma double: {SomaGenerica(3.14, 2.86)}");
        Console.WriteLine($"   Soma decimal: {SomaGenerica(100.50m, 99.50m)}");
        Console.WriteLine($"   Soma float: {SomaGenerica(1.5f, 2.5f)}");

        // IAdditionOperators<T, T, T> - Interface para operação de soma
        Console.WriteLine("\n✅ Operações aritméticas genéricas:");
        Console.WriteLine($"   Multiplicação int: {MultiplicaGenerica(7, 8)}");
        Console.WriteLine($"   Multiplicação double: {MultiplicaGenerica(2.5, 4.0)}");

        // IBinaryNumber<T> - Para números binários
        Console.WriteLine("\n✅ Operações binárias genéricas:");
        Console.WriteLine($"   XOR int: {XorGenerico(15, 10)} (binário: {Convert.ToString(XorGenerico(15, 10), 2)})");
        Console.WriteLine($"   XOR uint: {XorGenerico(255u, 85u)} (binário: {Convert.ToString(XorGenerico(255u, 85u), 2)})");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 2: ALGORITMOS MATEMÁTICOS GENÉRICOS =====
        Console.WriteLine("2. Algoritmos Matemáticos Genéricos");
        Console.WriteLine("------------------------------------");

        // Algoritmo de potência genérico
        Console.WriteLine("✅ Potência genérica:");
        Console.WriteLine($"   2^8 (int): {PotenciaGenerica(2, 8)}");
        Console.WriteLine($"   1.5^3 (double): {PotenciaGenerica(1.5, 3)}");
        Console.WriteLine($"   2.0^10 (decimal): {PotenciaGenerica(2.0m, 10)}");

        // Algoritmo de fatorial genérico
        Console.WriteLine("\n✅ Fatorial genérico:");
        Console.WriteLine($"   5! (int): {FatorialGenerico(5)}");
        Console.WriteLine($"   7! (long): {FatorialGenerico(7L)}");
        Console.WriteLine($"   10! (BigInteger): {FatorialGenerico(new BigInteger(10))}");

        // Algoritmo de máximo divisor comum
        Console.WriteLine("\n✅ MDC (Máximo Divisor Comum) genérico:");
        Console.WriteLine($"   MDC(48, 18) (int): {MdcGenerico(48, 18)}");
        Console.WriteLine($"   MDC(100, 75) (long): {MdcGenerico(100L, 75L)}");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 3: ESTATÍSTICAS GENÉRICAS =====
        Console.WriteLine("3. Estatísticas com Generic Math");
        Console.WriteLine("--------------------------------");

        var numerosInt = new[] { 10, 20, 30, 40, 50 };
        var numerosDouble = new[] { 1.5, 2.7, 3.9, 4.1, 5.3 };
        var numerosDecimal = new[] { 10.5m, 20.3m, 30.7m, 40.1m, 50.9m };

        Console.WriteLine("✅ Média genérica:");
        Console.WriteLine($"   Média int: {MediaGenerica(numerosInt):F2}");
        Console.WriteLine($"   Média double: {MediaGenerica(numerosDouble):F2}");
        Console.WriteLine($"   Média decimal: {MediaGenerica(numerosDecimal):F2}");

        Console.WriteLine("\n✅ Variância genérica:");
        Console.WriteLine($"   Variância int: {VarianciaGenerica(numerosInt):F2}");
        Console.WriteLine($"   Variância double: {VarianciaGenerica(numerosDouble):F2}");
        Console.WriteLine($"   Variância decimal: {VarianciaGenerica(numerosDecimal):F2}");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 4: GEOMETRIA GENÉRICA =====
        Console.WriteLine("4. Geometria com Generic Math");
        Console.WriteLine("-----------------------------");

        // Pontos genéricos (removendo int por não implementar IRootFunctions)
        var pontoDouble = new Ponto2D<double>(3.0, 4.0);
        var pontoFloat = new Ponto2D<float>(3.0f, 4.0f);

        Console.WriteLine("✅ Distância euclidiana genérica:");
        Console.WriteLine($"   Ponto double (3,4) distância da origem: {pontoDouble.DistanciaOrigem():F2}");
        Console.WriteLine($"   Ponto float (3,4) distância da origem: {pontoFloat.DistanciaOrigem():F2}");

        // Círculos genéricos
        var circuloInt = new Circulo<int>(5);
        var circuloDouble = new Circulo<double>(5.5);

        Console.WriteLine("\n✅ Cálculos geométricos genéricos:");
        Console.WriteLine($"   Área círculo int (r=5): {circuloInt.Area():F2}");
        Console.WriteLine($"   Área círculo double (r=5.5): {circuloDouble.Area():F2}");
        Console.WriteLine($"   Perímetro círculo int (r=5): {circuloInt.Perimetro():F2}");
        Console.WriteLine($"   Perímetro círculo double (r=5.5): {circuloDouble.Perimetro():F2}");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 5: MATRIZES GENÉRICAS =====
        Console.WriteLine("5. Matrizes com Generic Math");
        Console.WriteLine("----------------------------");

        // Matrizes de diferentes tipos
        var matrizInt = new Matriz2x2<int>(1, 2, 3, 4);
        var matrizDouble = new Matriz2x2<double>(1.5, 2.5, 3.5, 4.5);

        Console.WriteLine("✅ Operações com matrizes genéricas:");
        Console.WriteLine($"   Determinante matriz int: {matrizInt.Determinante()}");
        Console.WriteLine($"   Determinante matriz double: {matrizDouble.Determinante():F2}");

        var matrizSoma = matrizInt + new Matriz2x2<int>(5, 6, 7, 8);
        Console.WriteLine($"   Soma de matrizes int: [{matrizSoma.A11}, {matrizSoma.A12}; {matrizSoma.A21}, {matrizSoma.A22}]");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 6: NÚMEROS COMPLEXOS GENÉRICOS =====
        Console.WriteLine("6. Números Complexos com Generic Math");
        Console.WriteLine("--------------------------------------");

        var complexoFloat = new NumeroComplexo<float>(3.0f, 4.0f);
        var complexoDouble = new NumeroComplexo<double>(3.0, 4.0);

        Console.WriteLine("✅ Operações com números complexos genéricos:");
        Console.WriteLine($"   Módulo complexo float (3+4i): {complexoFloat.Modulo():F2}");
        Console.WriteLine($"   Módulo complexo double (3+4i): {complexoDouble.Modulo():F2}");
        Console.WriteLine($"   Argumento complexo float: {complexoFloat.Argumento():F4} radianos");

        var complexoSoma = complexoFloat + new NumeroComplexo<float>(1.0f, 2.0f);
        Console.WriteLine($"   Soma (3+4i) + (1+2i): {complexoSoma.Real:F1} + {complexoSoma.Imaginario:F1}i");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 7: PERFORMANCE COMPARISONS =====
        Console.WriteLine("7. Comparação de Performance");
        Console.WriteLine("----------------------------");

        const int iteracoes = 1_000_000;
        
        // Teste com soma tradicional vs genérica
        var sw = Stopwatch.StartNew();
        int somaTradicional = 0;
        for (int i = 0; i < iteracoes; i++)
        {
            somaTradicional += i;
        }
        sw.Stop();
        var tempoTradicional = sw.ElapsedMilliseconds;

        sw.Restart();
        int somaGenerica = 0;
        for (int i = 0; i < iteracoes; i++)
        {
            somaGenerica = SomaGenerica(somaGenerica, i);
        }
        sw.Stop();
        var tempoGenerico = sw.ElapsedMilliseconds;

        Console.WriteLine("✅ Performance soma (1M iterações):");
        Console.WriteLine($"   Tradicional: {tempoTradicional} ms");
        Console.WriteLine($"   Genérica: {tempoGenerico} ms");
        Console.WriteLine($"   Overhead: {((double)tempoGenerico / tempoTradicional - 1) * 100:F1}%");

        Console.WriteLine();

        // ===== DEMONSTRAÇÃO 8: CASOS PRÁTICOS =====
        Console.WriteLine("8. Casos Práticos de Uso");
        Console.WriteLine("------------------------");

        // Calculadora de juros compostos genérica (apenas tipos com IPowerFunctions)
        Console.WriteLine("✅ Juros compostos genéricos:");
        var montanteDouble = JurosCompostosGenerico(1000.0, 0.05, 12, 5);
        var montanteFloat = JurosCompostosGenerico(1000.0f, 0.05f, 12, 5);
        Console.WriteLine($"   Capital 1000, taxa 5% a.a., 12x ao ano, 5 anos:");
        Console.WriteLine($"   Montante (double): R$ {montanteDouble:F2}");
        Console.WriteLine($"   Montante (float): R$ {montanteFloat:F2}");

        // Interpolação linear genérica
        Console.WriteLine("\n✅ Interpolação linear genérica:");
        var interpInt = InterpolacaoLinearGenerica(0, 10, 0, 100, 5);
        var interpDouble = InterpolacaoLinearGenerica(0.0, 10.0, 0.0, 100.0, 5.0);
        Console.WriteLine($"   Interpolação int [0,10] -> [0,100] em x=5: {interpInt}");
        Console.WriteLine($"   Interpolação double [0,10] -> [0,100] em x=5: {interpDouble:F1}");

        // Conversão de unidades genérica
        Console.WriteLine("\n✅ Conversão de unidades genérica:");
        var celsiusInt = ConverterFahrenheitParaCelsius(100);
        var celsiusDouble = ConverterFahrenheitParaCelsius(100.0);
        var celsiusDecimal = ConverterFahrenheitParaCelsius(100.0m);
        Console.WriteLine($"   100°F em Celsius (int): {celsiusInt}°C");
        Console.WriteLine($"   100°F em Celsius (double): {celsiusDouble:F1}°C");
        Console.WriteLine($"   100°F em Celsius (decimal): {celsiusDecimal:F1}°C");

        Console.WriteLine();

        Console.WriteLine("=== RESUMO: Generic Math ===");
        Console.WriteLine("✅ VANTAGENS:");
        Console.WriteLine("   • Algoritmos únicos para todos os tipos numéricos");
        Console.WriteLine("   • Type safety com performance otimizada");
        Console.WriteLine("   • Reutilização de código matemático");
        Console.WriteLine("   • Suporte nativo a operadores");
        Console.WriteLine("   • Integração com System.Numerics");
        Console.WriteLine();
        Console.WriteLine("🎯 CASOS DE USO:");
        Console.WriteLine("   • Bibliotecas matemáticas genéricas");
        Console.WriteLine("   • Algoritmos de computação científica");
        Console.WriteLine("   • Sistemas de geometria computacional");
        Console.WriteLine("   • Cálculos financeiros precisos");
        Console.WriteLine("   • Processamento de dados numéricos");
        Console.WriteLine();
        Console.WriteLine("⚠️  CONSIDERAÇÕES:");
        Console.WriteLine("   • C# 11+ e .NET 7+ necessários");
        Console.WriteLine("   • Overhead mínimo com JIT otimization");
        Console.WriteLine("   • Requer understanding de interfaces matemáticas");
    }

    // ===== OPERAÇÕES BÁSICAS GENÉRICAS =====
    
    static T SomaGenerica<T>(T a, T b) where T : INumber<T>
        => a + b;

    static T MultiplicaGenerica<T>(T a, T b) where T : INumber<T>
        => a * b;

    static T XorGenerico<T>(T a, T b) where T : IBinaryNumber<T>
        => a ^ b;

    // ===== ALGORITMOS MATEMÁTICOS GENÉRICOS =====
    
    static T PotenciaGenerica<T>(T baseNum, int expoente) where T : INumber<T>
    {
        if (expoente == 0) return T.One;
        if (expoente == 1) return baseNum;
        
        T resultado = T.One;
        T baseAtual = baseNum;
        int exp = expoente;
        
        while (exp > 0)
        {
            if ((exp & 1) == 1)
                resultado *= baseAtual;
            baseAtual *= baseAtual;
            exp >>= 1;
        }
        
        return resultado;
    }

    static T FatorialGenerico<T>(T n) where T : INumber<T>
    {
        if (n <= T.One) return T.One;
        
        T resultado = T.One;
        T contador = T.One;
        
        while (contador <= n)
        {
            resultado *= contador;
            contador += T.One;
        }
        
        return resultado;
    }

    static T MdcGenerico<T>(T a, T b) where T : INumber<T>
    {
        while (b != T.Zero)
        {
            T temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    // ===== ESTATÍSTICAS GENÉRICAS =====
    
    static T MediaGenerica<T>(T[] valores) where T : INumber<T>
    {
        if (valores.Length == 0) return T.Zero;
        
        T soma = T.Zero;
        foreach (var valor in valores)
            soma += valor;
            
        return soma / T.CreateChecked(valores.Length);
    }

    static T VarianciaGenerica<T>(T[] valores) where T : INumber<T>
    {
        if (valores.Length <= 1) return T.Zero;
        
        T media = MediaGenerica(valores);
        T somaQuadrados = T.Zero;
        
        foreach (var valor in valores)
        {
            T diferenca = valor - media;
            somaQuadrados += diferenca * diferenca;
        }
        
        return somaQuadrados / T.CreateChecked(valores.Length - 1);
    }

    // ===== CASOS PRÁTICOS =====
    
    static T JurosCompostosGenerico<T>(T capital, T taxa, int frequencia, int anos) 
        where T : INumber<T>, IPowerFunctions<T>
    {
        var n = T.CreateChecked(frequencia);
        var t = T.CreateChecked(anos);
        var um = T.One;
        
        // M = C * (1 + i/n)^(n*t)
        var fator = um + (taxa / n);
        var expoente = n * t;
        
        return capital * T.Pow(fator, expoente);
    }

    static T InterpolacaoLinearGenerica<T>(T x0, T x1, T y0, T y1, T x) where T : INumber<T>
    {
        // y = y0 + (y1 - y0) * (x - x0) / (x1 - x0)
        return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
    }

    static T ConverterFahrenheitParaCelsius<T>(T fahrenheit) where T : INumber<T>
    {
        var cinco = T.CreateChecked(5);
        var nove = T.CreateChecked(9);
        var trintaDois = T.CreateChecked(32);
        
        // C = (F - 32) * 5/9
        return (fahrenheit - trintaDois) * cinco / nove;
    }
}

// ===== CLASSES AUXILIARES =====

public struct Ponto2D<T> where T : INumber<T>, IRootFunctions<T>
{
    public T X { get; }
    public T Y { get; }

    public Ponto2D(T x, T y)
    {
        X = x;
        Y = y;
    }

    public T DistanciaOrigem()
    {
        return T.Sqrt(X * X + Y * Y);
    }

    public T DistanciaDe(Ponto2D<T> outro)
    {
        var deltaX = X - outro.X;
        var deltaY = Y - outro.Y;
        return T.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}

public struct Circulo<T> where T : INumber<T>
{
    public T Raio { get; }

    public Circulo(T raio)
    {
        Raio = raio;
    }

    public T Area()
    {
        // π * r²
        var pi = T.CreateChecked(Math.PI);
        return pi * Raio * Raio;
    }

    public T Perimetro()
    {
        // 2 * π * r
        var dois = T.CreateChecked(2);
        var pi = T.CreateChecked(Math.PI);
        return dois * pi * Raio;
    }
}

public struct Matriz2x2<T> where T : INumber<T>
{
    public T A11 { get; }
    public T A12 { get; }
    public T A21 { get; }
    public T A22 { get; }

    public Matriz2x2(T a11, T a12, T a21, T a22)
    {
        A11 = a11;
        A12 = a12;
        A21 = a21;
        A22 = a22;
    }

    public T Determinante()
    {
        return A11 * A22 - A12 * A21;
    }

    public static Matriz2x2<T> operator +(Matriz2x2<T> a, Matriz2x2<T> b)
    {
        return new Matriz2x2<T>(
            a.A11 + b.A11,
            a.A12 + b.A12,
            a.A21 + b.A21,
            a.A22 + b.A22
        );
    }
}

public struct NumeroComplexo<T> where T : INumber<T>, ITrigonometricFunctions<T>, IRootFunctions<T>
{
    public T Real { get; }
    public T Imaginario { get; }

    public NumeroComplexo(T real, T imaginario)
    {
        Real = real;
        Imaginario = imaginario;
    }

    public T Modulo()
    {
        return T.Sqrt(Real * Real + Imaginario * Imaginario);
    }

    public T Argumento()
    {
        // Simplificação - usar apenas para demonstração
        if (Real == T.Zero && Imaginario == T.Zero) return T.Zero;
        return T.Atan(Imaginario / Real);
    }

    public static NumeroComplexo<T> operator +(NumeroComplexo<T> a, NumeroComplexo<T> b)
    {
        return new NumeroComplexo<T>(a.Real + b.Real, a.Imaginario + b.Imaginario);
    }

    public static NumeroComplexo<T> operator *(NumeroComplexo<T> a, NumeroComplexo<T> b)
    {
        // (a + bi) * (c + di) = (ac - bd) + (ad + bc)i
        var real = a.Real * b.Real - a.Imaginario * b.Imaginario;
        var imaginario = a.Real * b.Imaginario + a.Imaginario * b.Real;
        return new NumeroComplexo<T>(real, imaginario);
    }
}
