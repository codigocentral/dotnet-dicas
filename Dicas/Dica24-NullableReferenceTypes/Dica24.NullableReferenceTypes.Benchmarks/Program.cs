using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Diagnostics.CodeAnalysis;

BenchmarkRunner.Run<NullableBenchmarks>();

[MemoryDiagnoser]
[SimpleJob]
public class NullableBenchmarks
{
    private readonly string?[] _dadosComNull;
    private readonly string[] _dadosSemNull;
    private readonly Usuario?[] _usuariosComNull;
    private readonly Usuario[] _usuariosSemNull;

    public NullableBenchmarks()
    {
        // Dados com valores null misturados
        _dadosComNull = new string?[1000];
        for (int i = 0; i < 1000; i++)
        {
            _dadosComNull[i] = i % 3 == 0 ? null : $"Valor_{i}";
        }

        // Dados sem valores null
        _dadosSemNull = new string[1000];
        for (int i = 0; i < 1000; i++)
        {
            _dadosSemNull[i] = $"Valor_{i}";
        }

        // Usuários com valores null misturados
        _usuariosComNull = new Usuario?[1000];
        for (int i = 0; i < 1000; i++)
        {
            _usuariosComNull[i] = i % 4 == 0 ? null : 
                new Usuario($"Usuario_{i}", i % 2 == 0 ? null : $"user{i}@email.com");
        }

        // Usuários sem valores null
        _usuariosSemNull = new Usuario[1000];
        for (int i = 0; i < 1000; i++)
        {
            _usuariosSemNull[i] = new Usuario($"Usuario_{i}", $"user{i}@email.com");
        }
    }

    [Benchmark(Baseline = true)]
    public int ValidacaoTradicionalComTryCatch()
    {
        int contador = 0;
        foreach (var item in _dadosComNull)
        {
            try
            {
                if (item!.Length > 5) // Null-forgiving operator para demonstração
                    contador++;
            }
            catch (NullReferenceException)
            {
                // Ignorar nulls
            }
        }
        return contador;
    }

    [Benchmark]
    public int ValidacaoComNullConditional()
    {
        int contador = 0;
        foreach (var item in _dadosComNull)
        {
            if (item?.Length > 5)
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public int ValidacaoComNullCheck()
    {
        int contador = 0;
        foreach (var item in _dadosComNull)
        {
            if (item != null && item.Length > 5)
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public int ValidacaoComIsNotNull()
    {
        int contador = 0;
        foreach (var item in _dadosComNull)
        {
            if (item is not null && item.Length > 5)
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public string ProcessamentoComNullCoalescing()
    {
        var resultado = new System.Text.StringBuilder();
        foreach (var item in _dadosComNull)
        {
            resultado.Append(item ?? "VAZIO");
            resultado.Append("|");
        }
        return resultado.ToString();
    }

    [Benchmark]
    public string ProcessamentoComTernario()
    {
        var resultado = new System.Text.StringBuilder();
        foreach (var item in _dadosComNull)
        {
            resultado.Append(item != null ? item : "VAZIO");
            resultado.Append("|");
        }
        return resultado.ToString();
    }

    [Benchmark]
    public int ProcessamentoUsuariosComNullConditional()
    {
        int contador = 0;
        foreach (var usuario in _usuariosComNull)
        {
            if (usuario?.Email?.Contains("@") == true)
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public int ProcessamentoUsuariosComNullChecks()
    {
        int contador = 0;
        foreach (var usuario in _usuariosComNull)
        {
            if (usuario != null && usuario.Email != null && usuario.Email.Contains("@"))
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public int ProcessamentoUsuariosComPatternMatching()
    {
        int contador = 0;
        foreach (var usuario in _usuariosComNull)
        {
            if (usuario is { Email: not null } && usuario.Email.Contains("@"))
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public List<string> FiltrarDadosValidosComLinq()
    {
        return _dadosComNull
            .Where(item => item != null)
            .Select(item => item!)
            .Where(item => item.Length > 3)
            .ToList();
    }

    [Benchmark]
    public List<string> FiltrarDadosValidosComLoop()
    {
        var resultado = new List<string>();
        foreach (var item in _dadosComNull)
        {
            if (item != null && item.Length > 3)
                resultado.Add(item);
        }
        return resultado;
    }

    [Benchmark]
    public List<string> FiltrarDadosValidosComNullConditional()
    {
        var resultado = new List<string>();
        foreach (var item in _dadosComNull)
        {
            if (item?.Length > 3)
                resultado.Add(item!); // Null-forgiving operator para evitar warning
        }
        return resultado;
    }

    [Benchmark]
    public int ContarEmailsValidosComNullPropagation()
    {
        return _usuariosComNull.Count(u => u?.Email?.Contains("@") == true);
    }

    [Benchmark]
    public int ContarEmailsValidosComNullChecks()
    {
        return _usuariosComNull.Count(u => u != null && u.Email != null && u.Email.Contains("@"));
    }

    [Benchmark]
    public string ConcatenarNomesComNullCoalescing()
    {
        return string.Join(", ", _usuariosComNull.Select(u => u?.Nome ?? "DESCONHECIDO"));
    }

    [Benchmark]
    public string ConcatenarNomesComTernario()
    {
        return string.Join(", ", _usuariosComNull.Select(u => u != null ? u.Nome : "DESCONHECIDO"));
    }

    // Benchmark para dados sem null (baseline de performance)
    [Benchmark]
    public int ProcessamentoDadosSemNull()
    {
        int contador = 0;
        foreach (var item in _dadosSemNull)
        {
            if (item.Length > 5)
                contador++;
        }
        return contador;
    }

    [Benchmark]
    public int ProcessamentoUsuariosSemNull()
    {
        int contador = 0;
        foreach (var usuario in _usuariosSemNull)
        {
            if (usuario.Email!.Contains("@")) // Null-forgiving operator necessário
                contador++;
        }
        return contador;
    }
}

// Classes de suporte para benchmark
public class Usuario
{
    public string Nome { get; }
    public string? Email { get; }

    public Usuario(string nome, string? email)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email;
    }
}

// Métodos utilitários para demonstrar diferentes abordagens
public static class UtilitariosNullable
{
    // Método com validação tradicional
    public static string ProcessarTextoTradicional(string? texto)
    {
        if (texto == null)
            return "VAZIO";
        
        if (texto.Length == 0)
            return "VAZIO";
            
        return texto.ToUpper();
    }

    // Método com nullable operators
    public static string ProcessarTextoModerno(string? texto)
    {
        return texto?.Length > 0 ? texto.ToUpper() : "VAZIO";
    }

    // Método com pattern matching
    public static string ProcessarTextoComPattern(string? texto) => texto switch
    {
        null => "VAZIO",
        "" => "VAZIO", 
        var t => t.ToUpper()
    };

    // Validação robusta
    public static bool EmailValido([NotNullWhen(true)] string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains("@") && 
               email.Contains(".");
    }

    // Processamento de lista com nullable
    public static List<T> FiltrarNulls<T>(IEnumerable<T?> fonte) where T : class
    {
        var resultado = new List<T>();
        foreach (var item in fonte)
        {
            if (item is not null)
                resultado.Add(item);
        }
        return resultado;
    }
}
