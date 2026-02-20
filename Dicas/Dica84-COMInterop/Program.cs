// Dica 84: COM Interop em C#
// Demonstra como interagir com componentes COM (Component Object Model)

using System.Runtime.InteropServices;
using static System.Console;

namespace Dica84.COMInterop;

class Program
{
    static void Main()
    {
        WriteLine("=== 🔗 Dica 84: COM Interop em C# ===\n");

        // 1. DEMONSTRAÇÃO: Conceitos básicos de COM Interop
        WriteLine("1. 📚 Conceitos Básicos de COM Interop:");
        DemonstrarConceitosBasicos();

        // 2. DEMONSTRAÇÃO: Atributos COM Interop
        WriteLine("\n2. 🏷️ Atributos COM Interop:");
        DemonstrarAtributosCOM();

        // 3. DEMONSTRAÇÃO: Criação de objetos COM dinamicamente
        WriteLine("\n3. 🔧 Criação Dinâmica de Objetos COM:");
        DemonstrarCriacaoDinamica();

        // 4. DEMONSTRAÇÃO: Interop com Office (simulado)
        WriteLine("\n4. 📊 Interop com Microsoft Office (Simulado):");
        DemonstrarOfficeInterop();

        // 5. DEMONSTRAÇÃO: Gerenciamento de recursos COM
        WriteLine("\n5. 🗑️ Gerenciamento de Recursos COM:");
        DemonstrarGerenciamentoRecursos();

        // 6. DEMONSTRAÇÃO: Marshaling de dados
        WriteLine("\n6. 🔄 Marshaling de Dados:");
        DemonstrarMarshaling();

        // 7. DEMONSTRAÇÃO: Tratamento de erros COM
        WriteLine("\n7. ❌ Tratamento de Erros COM:");
        DemonstrarTratamentoErros();

        // 8. DEMONSTRAÇÃO: Performance e otimizações
        WriteLine("\n8. ⚡ Performance e Otimizações:");
        DemonstrarPerformance();

        WriteLine("\n🎉 Demonstração concluída!");
        WriteLine("📝 Principais takeaways:");
        WriteLine("   • COM Interop permite integrar com componentes Windows legados");
        WriteLine("   • Sempre gerencie recursos COM adequadamente");
        WriteLine("   • Use atributos de marshaling para controle fino");
        WriteLine("   • Trate erros COM através de COMException");
        WriteLine("   • Otimize performance evitando MDN e usando caching");
        WriteLine("   • Considere alternativas modernas quando possível");
    }

    static void DemonstrarConceitosBasicos()
    {
        WriteLine("💡 COM (Component Object Model) é uma tecnologia Microsoft para:");
        WriteLine("   • Comunicação entre componentes de software");
        WriteLine("   • Reutilização de código entre linguagens");
        WriteLine("   • Automação de aplicações (Office, Windows Shell, etc.)");
        WriteLine();

        WriteLine("🔍 C# fornece várias formas de interagir com COM:");
        WriteLine("   • Interop Assemblies (Type Libraries)");
        WriteLine("   • Dynamic COM (late binding)");
        WriteLine("   • Primary Interop Assemblies (PIAs)");
        WriteLine("   • Embedded Interop Types");
        WriteLine();

        WriteLine("📋 Principais namespace para COM Interop:");
        WriteLine("   • System.Runtime.InteropServices");
        WriteLine("   • System.Runtime.InteropServices.ComTypes");
        WriteLine("   • Microsoft.Office.Interop.*");
    }

    static void DemonstrarAtributosCOM()
    {
        WriteLine("🏷️ Principais atributos para COM Interop:");
        WriteLine();

        WriteLine("📌 [ComVisible(true/false)]");
        WriteLine("   • Controla se o tipo/membro é visível para COM");
        WriteLine("   • Padrão: false para assemblies .NET");
        WriteLine();

        WriteLine("📌 [Guid(\"guid-string\")]");
        WriteLine("   • Define GUID único para interfaces/classes COM");
        WriteLine("   • Necessário para componentes COM personalizados");
        WriteLine();

        WriteLine("📌 [ClassInterface(ClassInterfaceType)]");
        WriteLine("   • AutoDual: Interface dual (IDispatch + custom)");
        WriteLine("   • AutoDispatch: Apenas IDispatch");
        WriteLine("   • None: Nenhuma interface automática");
        WriteLine();

        WriteLine("📌 [ComImport]");
        WriteLine("   • Marca interface como sendo importada de COM");
        WriteLine("   • Usada com [Guid] para definir interface COM");
        WriteLine();

        WriteLine("📌 [MarshalAs(UnmanagedType)]");
        WriteLine("   • Controla como dados são convertidos entre .NET e COM");
        WriteLine("   • Importante para strings, arrays, estruturas");

        try
        {
            var comClass = new ExampleCOMClass();
            comClass.Data = "Teste COM";
            WriteLine($"\n✅ Exemplo COM Class criada: Data = '{comClass.Data}'");
            WriteLine($"✅ Cálculo: 5 + 3 = {comClass.Calculate(5, 3)}");
        }
        catch (Exception ex)
        {
            WriteLine($"❌ Erro: {ex.Message}");
        }
    }

    static void DemonstrarCriacaoDinamica()
    {
        WriteLine("🔧 Criação dinâmica permite acessar COM sem PIAs:");
        WriteLine();

        WriteLine("📝 Exemplo conceitual de criação dinâmica:");
        WriteLine("   Type comType = Type.GetTypeFromProgID(\"Excel.Application\");");
        WriteLine("   dynamic excel = Activator.CreateInstance(comType);");
        WriteLine("   excel.Visible = true;");
        WriteLine("   var workbook = excel.Workbooks.Add();");
        WriteLine();

        WriteLine("💡 Vantagens da criação dinâmica:");
        WriteLine("   • Não precisa de PIAs em tempo de compilação");
        WriteLine("   • Flexibilidade para diferentes versões");
        WriteLine("   • Menor acoplamento");
        WriteLine();

        WriteLine("⚠️ Desvantagens:");
        WriteLine("   • Sem IntelliSense/type safety");
        WriteLine("   • Erros apenas em tempo de execução");
        WriteLine("   • Performance ligeiramente menor");
    }

    static void DemonstrarOfficeInterop()
    {
        WriteLine("📊 Interop com Microsoft Office (Conceitual):");
        WriteLine();

        WriteLine("📈 Excel Automation:");
        WriteLine("   var excel = new Excel.Application();");
        WriteLine("   excel.Visible = true;");
        WriteLine("   var workbook = excel.Workbooks.Add();");
        WriteLine("   var worksheet = workbook.ActiveSheet;");
        WriteLine("   worksheet.Cells[1, 1] = \"Hello, COM!\";");
        WriteLine();

        WriteLine("📝 Word Automation:");
        WriteLine("   var word = new Word.Application();");
        WriteLine("   word.Visible = true;");
        WriteLine("   var document = word.Documents.Add();");
        WriteLine("   document.Content.Text = \"Documento criado via COM\";");
        WriteLine();

        WriteLine("🔧 Melhores práticas Office Interop:");
        WriteLine("   • Sempre libere objetos COM explicitamente");
        WriteLine("   • Use using statements quando possível");
        WriteLine("   • Evite Multiple Dot Notation (MDN)");
        WriteLine("   • Configure CultureInfo para consistência");
    }

    static void DemonstrarGerenciamentoRecursos()
    {
        WriteLine("🗑️ Gerenciamento adequado de recursos COM é crucial:");
        WriteLine();

        WriteLine("⚠️ Problemas comuns:");
        WriteLine("   • Memory leaks por não liberar objetos COM");
        WriteLine("   • Processos Excel/Word 'fantasma'");
        WriteLine("   • Handles de arquivo não liberados");
        WriteLine();

        WriteLine("✅ Soluções:");
        WriteLine("1. 🔄 Marshal.ReleaseComObject(comObject)");
        WriteLine("2. 🗑️ Marshal.FinalReleaseComObject(comObject)");
        WriteLine("3. 🧹 GC.Collect() e GC.WaitForPendingFinalizers()");
        WriteLine("4. 💡 Wrapper classes com IDisposable");

        try
        {
            var exampleObject = new ExampleCOMClass();
            using var wrapper = new COMWrapper(exampleObject);
            WriteLine("\n✅ Wrapper criado com sucesso");
            WriteLine("✅ Objeto será liberado automaticamente");
        }
        catch (Exception ex)
        {
            WriteLine($"❌ Erro: {ex.Message}");
        }
    }

    static void DemonstrarMarshaling()
    {
        WriteLine("🔄 Marshaling controla conversão de dados entre .NET e COM:");
        WriteLine();

        WriteLine("📝 Tipos de marshaling comuns:");
        WriteLine("🔤 Strings: [MarshalAs(UnmanagedType.BStr)]");
        WriteLine("🔢 Arrays: [MarshalAs(UnmanagedType.SafeArray)]");
        WriteLine("📅 Dates: [MarshalAs(UnmanagedType.Struct)]");
        WriteLine("✅ Boolean: [MarshalAs(UnmanagedType.VariantBool)]");
        WriteLine();

        try
        {
            var comStruct = new COMStruct
            {
                Name = "Exemplo",
                Value = 42,
                IsActive = true,
                Numbers = [1, 2, 3, 4, 5]
            };

            WriteLine($"✅ Estrutura criada:");
            WriteLine($"   Name: {comStruct.Name}");
            WriteLine($"   Value: {comStruct.Value}");
            WriteLine($"   IsActive: {comStruct.IsActive}");
            WriteLine($"   Numbers: [{string.Join(", ", comStruct.Numbers ?? [])}]");
            WriteLine($"📏 Tamanho: {Marshal.SizeOf<COMStruct>()} bytes");
        }
        catch (Exception ex)
        {
            WriteLine($"❌ Erro: {ex.Message}");
        }
    }

    static void DemonstrarTratamentoErros()
    {
        WriteLine("❌ COM usa HRESULTs para indicar erros:");
        WriteLine();

        WriteLine("📋 Códigos HRESULT comuns:");
        WriteLine("   S_OK (0x00000000)           - Sucesso");
        WriteLine("   E_FAIL (0x80004005)         - Falha não especificada");
        WriteLine("   E_INVALIDARG (0x80070057)   - Argumento inválido");
        WriteLine("   E_OUTOFMEMORY (0x8007000E)  - Memória insuficiente");
        WriteLine();

        WriteLine("🔄 .NET converte HRESULTs em exceções:");
        WriteLine("   HRESULT -> COMException");
        WriteLine("   ErrorCode property contém HRESULT original");
    }

    static void DemonstrarPerformance()
    {
        WriteLine("⚡ Otimizações de performance para COM Interop:");
        WriteLine();

        WriteLine("🚀 1. Evite Multiple Dot Notation (MDN)");
        WriteLine("   ❌ Lento: excel.Workbooks.Add().Worksheets[1].Cells[1, 1].Value");
        WriteLine("   ✅ Rápido: Cache referencias intermediárias");
        WriteLine();

        WriteLine("🚀 2. Use arrays para operações em lote");
        WriteLine("🚀 3. Desabilite atualizações durante operações");
        WriteLine("🚀 4. Use Interop Types embedding");
        WriteLine("🚀 5. Considere alternativas modernas:");
        WriteLine("   • OpenXML SDK para Office");
        WriteLine("   • EPPlus para Excel");
        WriteLine("   • ClosedXML para Excel");
    }
}

// Declarações de tipos COM

[ComVisible(true)]
[Guid("12345678-1234-1234-1234-123456789012")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class ExampleCOMClass
{
    private string _data = "";

    [ComVisible(true)]
    public string Data
    {
        get => _data;
        set => _data = value ?? "";
    }

    [ComVisible(true)]
    public int Calculate(int a, int b)
    {
        return a + b;
    }
}

public class COMWrapper : IDisposable
{
    private object? _comObject;
    private bool _disposed = false;

    public COMWrapper(object comObject)
    {
        _comObject = comObject;
    }

    public object ComObject => _comObject ?? throw new ObjectDisposedException(nameof(COMWrapper));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_comObject != null)
            {
                try
                {
                    if (Marshal.IsComObject(_comObject))
                    {
                        Marshal.FinalReleaseComObject(_comObject);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine($"Erro ao liberar objeto COM: {ex.Message}");
                }
                finally
                {
                    _comObject = null;
                }
            }
            _disposed = true;
        }
    }

    ~COMWrapper()
    {
        Dispose(false);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct COMStruct
{
    [MarshalAs(UnmanagedType.BStr)]
    public string Name;

    [MarshalAs(UnmanagedType.I4)]
    public int Value;

    [MarshalAs(UnmanagedType.VariantBool)]
    public bool IsActive;

    [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    public int[]? Numbers;
}
