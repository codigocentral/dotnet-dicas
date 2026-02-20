#!/bin/bash

echo "🧪 Dica 28: Demonstração do dotnet retest"
echo "=========================================="

# Verificar se dotnet retest está instalado
if ! command -v dotnet-retest &> /dev/null; then
    echo "📦 Instalando dotnet-retest..."
    dotnet tool install -g dotnet-retest
    echo ""
fi

echo "📋 Executando diferentes cenários de teste:"
echo ""

# Navegar para o diretório do projeto
cd "$(dirname "$0")/Dica28.DotnetRetest"

echo "1️⃣ Executar apenas testes estáveis (devem passar sempre):"
echo "dotnet test --filter \"Category=Stable\""
dotnet test --filter "Category=Stable"
echo ""

echo "2️⃣ Executar testes flaky normalmente (alguns podem falhar):"
echo "dotnet test --filter \"Category=Flaky\""
dotnet test --filter "Category=Flaky"
echo ""

echo "3️⃣ Executar testes flaky com dotnet retest (retry automático):"
echo "dotnet retest --retry-count 3 --filter \"Category=Flaky\""
dotnet retest --retry-count 3 --filter "Category=Flaky"
echo ""

echo "4️⃣ Executar todos os testes com retest e verbose:"
echo "dotnet retest --retry-count 5 --verbose"
dotnet retest --retry-count 5 --verbose
echo ""

echo "✅ Demonstração concluída!"
echo ""
echo "💡 Pontos importantes:"
echo "- dotnet retest ajuda a identificar testes flaky"
echo "- Use apenas para investigação, não como solução permanente"
echo "- Sempre corrija a causa raiz dos testes flaky"
echo "- Prefira timeout, isolamento de estado e async/await correto"
