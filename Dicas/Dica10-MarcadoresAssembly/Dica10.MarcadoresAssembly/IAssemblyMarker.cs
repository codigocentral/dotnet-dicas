namespace Dica10.MarcadoresAssembly;

/// <summary>
/// Interface marcadora para o assembly Dica10.MarcadoresAssembly
/// 
/// Esta interface vazia serve como um marcador claro e inequívoco para identificar
/// este assembly específico ao registrar bibliotecas como MediatR, AutoMapper,
/// FluentValidation, etc. na Injeção de Dependência.
/// 
/// Vantagens sobre usar Program.cs:
/// - Remove ambiguidade sobre qual assembly está sendo referenciado
/// - Torna o código mais legível e auto-documentado
/// - Evita dependências acidentais do assembly de entrada
/// - Facilita refatoração e manutenção
/// </summary>
public interface IAssemblyMarker
{
    // Esta interface é intencionalmente vazia - serve apenas como marcador
}
