// Dica 71: Comparando Tuplas em C#
// Demonstra como C# compara tuplas elemento por elemento, operadores de igualdade,
// e as nuances entre diferentes tipos de tuplas (ValueTuple vs Tuple)

using System.Diagnostics;
using static System.Console;

WriteLine("=== 🔄 Dica 71: Comparando Tuplas em C# ===\n");

// 1. DEMONSTRAÇÃO: Comparação básica de ValueTuples
WriteLine("1. 📋 Comparação Básica de ValueTuples:");
DemonstrarComparacaoBasica();

// 2. DEMONSTRAÇÃO: Nomes não afetam a igualdade
WriteLine("\n2. 🏷️ Nomes de Campos Não Afetam Igualdade:");
DemonstrarNomesNaoAfetamIgualdade();

// 3. DEMONSTRAÇÃO: Comparação elemento por elemento
WriteLine("\n3. 🔍 Comparação Elemento por Elemento:");
DemonstrarComparacaoElementoPorElemento();

// 4. DEMONSTRAÇÃO: Diferentes tipos de tuplas
WriteLine("\n4. 📦 Diferentes Tipos de Tuplas:");
DemonstrarDiferentesTiposTuplas();

// 5. DEMONSTRAÇÃO: Tuplas aninhadas
WriteLine("\n5. 🎯 Tuplas Aninhadas:");
DemonstrarTuplasAninhadas();

// 6. DEMONSTRAÇÃO: Performance de comparação
WriteLine("\n6. ⚡ Performance de Comparação:");
DemonstrarPerformanceComparacao();

// 7. DEMONSTRAÇÃO: Métodos de comparação disponíveis
WriteLine("\n7. 🛠️ Métodos de Comparação Disponíveis:");
DemonstrarMetodosComparacao();

// 8. DEMONSTRAÇÃO: Comparação com null
WriteLine("\n8. ❓ Comparação com Null:");
DemonstrarComparacaoComNull();

// 9. DEMONSTRAÇÃO: Casos práticos de uso
WriteLine("\n9. 💼 Casos Práticos de Uso:");
DemonstrarCasosPraticos();

// 10. DEMONSTRAÇÃO: Melhores práticas
WriteLine("\n10. ✅ Melhores Práticas:");
DemonstrarMelhoresPraticas();

static void DemonstrarComparacaoBasica()
{
    // ValueTuples com mesmos valores
    var tupla1 = (1, "hello", true);
    var tupla2 = (1, "hello", true);
    var tupla3 = (1, "hello", false);

    WriteLine($"tupla1: {tupla1}");
    WriteLine($"tupla2: {tupla2}");
    WriteLine($"tupla3: {tupla3}");
    WriteLine();

    // Operador ==
    WriteLine($"tupla1 == tupla2: {tupla1 == tupla2}"); // True
    WriteLine($"tupla1 == tupla3: {tupla1 == tupla3}"); // False
    WriteLine();

    // Método Equals
    WriteLine($"tupla1.Equals(tupla2): {tupla1.Equals(tupla2)}"); // True
    WriteLine($"tupla1.Equals(tupla3): {tupla1.Equals(tupla3)}"); // False
    WriteLine();

    // Operador !=
    WriteLine($"tupla1 != tupla3: {tupla1 != tupla3}"); // True
}

static void DemonstrarNomesNaoAfetamIgualdade()
{
    // Tuplas com nomes diferentes mas valores iguais
    var pessoa1 = (Id: 1, Nome: "João", Ativo: true);
    var pessoa2 = (Codigo: 1, NomeCompleto: "João", Status: true);
    var registro = (1, "João", true); // Sem nomes

    WriteLine($"pessoa1: {pessoa1}");
    WriteLine($"pessoa2: {pessoa2}"); 
    WriteLine($"registro: {registro}");
    WriteLine();

    // Todas são consideradas iguais porque os VALORES são iguais
    WriteLine($"pessoa1 == pessoa2: {pessoa1 == pessoa2}"); // True
    WriteLine($"pessoa1 == registro: {pessoa1 == registro}"); // True
    WriteLine($"pessoa2 == registro: {pessoa2 == registro}"); // True
    WriteLine();

    WriteLine("💡 IMPORTANTE: Apenas os valores importam, não os nomes dos campos!");
}

static void DemonstrarComparacaoElementoPorElemento()
{
    // Demonstra como a comparação é feita elemento por elemento, na ordem
    var coordenada1 = (X: 10, Y: 20);
    var coordenada2 = (X: 10, Y: 20);
    var coordenada3 = (X: 20, Y: 10); // Valores trocados
    
    WriteLine($"coordenada1: {coordenada1}");
    WriteLine($"coordenada2: {coordenada2}");
    WriteLine($"coordenada3: {coordenada3}");
    WriteLine();

    WriteLine($"coordenada1 == coordenada2: {coordenada1 == coordenada2}"); // True
    WriteLine($"coordenada1 == coordenada3: {coordenada1 == coordenada3}"); // False
    WriteLine();

    // Tuplas com diferentes números de elementos não podem ser comparadas
    var ponto2D = (10, 20);
    var ponto3D = (10, 20, 30);
    
    WriteLine($"ponto2D: {ponto2D}");
    WriteLine($"ponto3D: {ponto3D}");
    WriteLine("❌ ponto2D == ponto3D: ERRO DE COMPILAÇÃO - diferentes aridades");
}

static void DemonstrarDiferentesTiposTuplas()
{
    // ValueTuple (sintaxe moderna)
    var valueTuple = (1, "teste");
    
    // Tuple (classe de referência - legado)
    var referenceTuple = Tuple.Create(1, "teste");
    
    WriteLine($"ValueTuple: {valueTuple} (Tipo: {valueTuple.GetType().Name})");
    WriteLine($"Tuple: {referenceTuple} (Tipo: {referenceTuple.GetType().Name})");
    WriteLine();

    // ValueTuple é struct (valor), Tuple é class (referência)
    WriteLine($"ValueTuple é struct: {valueTuple.GetType().IsValueType}");
    WriteLine($"Tuple é class: {!referenceTuple.GetType().IsValueType}");
    WriteLine();

    // Comparação entre diferentes tipos não compila diretamente
    WriteLine("❌ valueTuple == referenceTuple: ERRO DE COMPILAÇÃO - tipos diferentes");
    
    // Mas podemos comparar os valores manualmente
    bool saoIguais = valueTuple.Item1 == referenceTuple.Item1 && 
                     valueTuple.Item2 == referenceTuple.Item2;
    WriteLine($"✅ Comparação manual: {saoIguais}");
}

static void DemonstrarTuplasAninhadas()
{
    // Tuplas podem conter outras tuplas
    var tupla1 = ((1, 2), (3, 4));
    var tupla2 = ((1, 2), (3, 4));
    var tupla3 = ((1, 2), (3, 5)); // Último elemento diferente

    WriteLine($"tupla1: {tupla1}");
    WriteLine($"tupla2: {tupla2}");
    WriteLine($"tupla3: {tupla3}");
    WriteLine();

    WriteLine($"tupla1 == tupla2: {tupla1 == tupla2}"); // True
    WriteLine($"tupla1 == tupla3: {tupla1 == tupla3}"); // False
    WriteLine();

    // Tupla mais complexa
    var pessoa1 = (
        Info: (Nome: "João", Idade: 30),
        Endereço: (Rua: "Main St", Numero: 123),
        Ativo: true
    );
    
    var pessoa2 = (
        Dados: (NomeCompleto: "João", Anos: 30),
        Local: (Logradouro: "Main St", NumeroEndereco: 123),
        Status: true
    );

    WriteLine($"pessoa1: {pessoa1}");
    WriteLine($"pessoa2: {pessoa2}");
    WriteLine($"pessoa1 == pessoa2: {pessoa1 == pessoa2}"); // True - valores iguais
}

static void DemonstrarPerformanceComparacao()
{
    // Comparação de performance entre ValueTuple e Tuple
    const int iterations = 1_000_000;

    // Preparar dados
    var valueTuples = new (int, string)[iterations];
    var referenceTuples = new Tuple<int, string>[iterations];

    for (int i = 0; i < iterations; i++)
    {
        valueTuples[i] = (i, $"item{i}");
        referenceTuples[i] = Tuple.Create(i, $"item{i}");
    }

    // Benchmark ValueTuple
    var sw = Stopwatch.StartNew();
    int equalCount1 = 0;
    
    for (int i = 0; i < iterations - 1; i++)
    {
        if (valueTuples[i] == valueTuples[i + 1])
            equalCount1++;
    }
    
    sw.Stop();
    var valueTime = sw.ElapsedMilliseconds;

    // Benchmark Tuple (classe)
    sw.Restart();
    int equalCount2 = 0;
    
    for (int i = 0; i < iterations - 1; i++)
    {
        if (referenceTuples[i].Equals(referenceTuples[i + 1]))
            equalCount2++;
    }
    
    sw.Stop();
    var referenceTime = sw.ElapsedMilliseconds;

    WriteLine($"ValueTuple comparisons: {valueTime}ms ({equalCount1} equals)");
    WriteLine($"Tuple comparisons: {referenceTime}ms ({equalCount2} equals)");
    WriteLine($"⚡ ValueTuple é ~{(double)referenceTime / valueTime:F1}x mais rápido");
}

static void DemonstrarMetodosComparacao()
{
    var tupla1 = (42, "answer", true);
    var tupla2 = (42, "answer", true);
    var tupla3 = (42, "question", false);

    WriteLine($"tupla1: {tupla1}");
    WriteLine($"tupla2: {tupla2}");
    WriteLine($"tupla3: {tupla3}");
    WriteLine();

    // Diferentes formas de comparar
    WriteLine("🔍 Diferentes métodos de comparação:");
    WriteLine($"== operator: tupla1 == tupla2 = {tupla1 == tupla2}");
    WriteLine($"!= operator: tupla1 != tupla3 = {tupla1 != tupla3}");
    WriteLine($"Equals method: tupla1.Equals(tupla2) = {tupla1.Equals(tupla2)}");
    WriteLine($"Object.Equals: Equals(tupla1, tupla2) = {Equals(tupla1, tupla2)}");
    WriteLine($"ReferenceEquals: ReferenceEquals(tupla1, tupla2) = {ReferenceEquals(tupla1, tupla2)}"); // False (structs)
    WriteLine();

    // GetHashCode
    WriteLine("🔨 Hash codes:");
    WriteLine($"tupla1.GetHashCode(): {tupla1.GetHashCode()}");
    WriteLine($"tupla2.GetHashCode(): {tupla2.GetHashCode()}");
    WriteLine($"tupla3.GetHashCode(): {tupla3.GetHashCode()}");
    WriteLine($"tupla1 e tupla2 têm hash igual: {tupla1.GetHashCode() == tupla2.GetHashCode()}");
}

static void DemonstrarComparacaoComNull()
{
    // ValueTuples não podem ser null (são structs)
    var tupla = (1, "test");
    WriteLine($"tupla: {tupla}");
    WriteLine($"tupla pode ser null? {(tupla.GetType().IsValueType ? "Não" : "Sim")}");
    WriteLine();

    // Mas podem conter elementos null
    var tuplaComNull = (1, (string?)null, true);
    var tuplaComNull2 = (1, (string?)null, true);
    var tuplaComTexto = (1, "texto", true);

    WriteLine($"tuplaComNull: {tuplaComNull}");
    WriteLine($"tuplaComNull2: {tuplaComNull2}");
    WriteLine($"tuplaComTexto: {tuplaComTexto}");
    WriteLine();

    WriteLine($"tuplaComNull == tuplaComNull2: {tuplaComNull == tuplaComNull2}"); // True
    WriteLine($"tuplaComNull == tuplaComTexto: {tuplaComNull == tuplaComTexto}"); // False
    WriteLine();

    // Tuplas nullable
    (int, string)? tuplaNull = null;
    (int, string)? tuplaValida = (1, "test");

    WriteLine($"tuplaNull: {tuplaNull}");
    WriteLine($"tuplaValida: {tuplaValida}");
    WriteLine($"tuplaNull == null: {tuplaNull == null}"); // True
    WriteLine($"tuplaValida == null: {tuplaValida == null}"); // False
}

static void DemonstrarCasosPraticos()
{
    WriteLine("💼 Casos práticos onde comparação de tuplas é útil:");
    WriteLine();

    // 1. Coordenadas em jogos/gráficos
    var posicaoJogador = (X: 10, Y: 20);
    var posicaoInimigo = (X: 10, Y: 20);
    
    if (posicaoJogador == posicaoInimigo)
    {
        WriteLine("🎮 Colisão detectada entre jogador e inimigo!");
    }

    // 2. Chaves compostas para dicionários
    var vendas = new Dictionary<(int ano, int mes), decimal>
    {
        [(2024, 1)] = 10000m,
        [(2024, 2)] = 15000m,
        [(2024, 3)] = 12000m
    };

    var chave = (2024, 2);
    if (vendas.ContainsKey(chave))
    {
        WriteLine($"💰 Vendas em {chave.Item1}/{chave.Item2}: {vendas[chave]:C}");
    }

    // 3. Comparação de resultados
    var resultado1 = ObterResultadoOperacao();
    var resultado2 = ObterResultadoOperacao();

    if (resultado1 == resultado2)
    {
        WriteLine("✅ Resultados são consistentes");
    }

    // 4. Estado de componentes
    var estadoAtual = (Online: true, Conectado: true, Sincronizado: false);
    var estadoEsperado = (Online: true, Conectado: true, Sincronizado: true);

    if (estadoAtual != estadoEsperado)
    {
        WriteLine("⚠️ Sistema não está no estado esperado");
    }
}

static (bool sucesso, string mensagem, int codigo) ObterResultadoOperacao()
{
    return (true, "Operação concluída", 200);
}

static void DemonstrarMelhoresPraticas()
{
    WriteLine("✅ Melhores práticas para comparação de tuplas:");
    WriteLine();

    WriteLine("1. 🎯 Use ValueTuples para melhor performance");
    WriteLine("   ✅ var ponto = (10, 20);");
    WriteLine("   ❌ var ponto = Tuple.Create(10, 20);");
    WriteLine();

    WriteLine("2. 🏷️ Use nomes descritivos mesmo que não afetem igualdade");
    WriteLine("   ✅ var pessoa = (Id: 1, Nome: \"João\");");
    WriteLine("   🤔 var pessoa = (1, \"João\");");
    WriteLine();

    WriteLine("3. 📊 Para performance crítica, considere structs customizados");
    WriteLine("   struct Ponto { int X, Y; } // Com IEquatable<T>");
    WriteLine();

    WriteLine("4. 🔍 Use em chaves de dicionário quando fizer sentido");
    WriteLine("   Dictionary<(int, int), string> coordenadas;");
    WriteLine();

    WriteLine("5. ⚠️ Cuidado com tuplas muito grandes (> 7 elementos)");
    WriteLine("   Consider usar classes/structs para melhor legibilidade");
    WriteLine();

    WriteLine("6. 🎮 Ideal para retornos múltiplos simples");
    WriteLine("   (bool success, string error) TryParse(...)");
}

WriteLine("\n🎉 Demonstração concluída!");
WriteLine("📝 Principais takeaways:");
WriteLine("   • Tuplas são comparadas elemento por elemento");
WriteLine("   • Nomes dos campos NÃO afetam igualdade");
WriteLine("   • ValueTuples são mais rápidas que Tuple");
WriteLine("   • Funcionam bem como chaves de dicionário");
WriteLine("   • Ideais para dados temporários e retornos múltiplos");
