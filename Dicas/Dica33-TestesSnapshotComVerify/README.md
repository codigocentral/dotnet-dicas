# Dica 33: Testes de Snapshot com Verify

## 📋 O que são Testes de Snapshot?

Snapshot testing é uma técnica onde você captura o resultado de uma operação (classe, JSON, texto, imagem, UI) e compara com uma versão "aprovada" previamente salva. É uma maneira robusta de detectar mudanças inesperadas.

## 🔧 Biblioteca Verify

A biblioteca **Verify** é uma das melhores ferramentas para snapshot testing em .NET, oferecendo:

- ✅ Suporte a múltiplos formatos (JSON, XML, HTML, imagens, etc.)
- ✅ Integração com frameworks de teste (xUnit, NUnit, MSTest)
- ✅ Comparação inteligente de diferenças
- ✅ Aprovação automática de mudanças
- ✅ Suporte a Entity Framework e outras bibliotecas

## 💡 Vantagens

### ✅ Quando Usar:
- Validação de saídas complexas (JSON, XML, HTML)
- Testes de regressão visual
- Projetos legados sem testes (low-invasive testing)
- APIs com respostas complexas
- Relatórios e documentos gerados
- Resultados de transformações de dados

### ❌ Limitações:
- Dados dinâmicos (timestamps, IDs aleatórios)
- Testes que dependem de estado externo
- Quando você precisa de validações específicas de negócio

## 🎯 Casos de Uso Comuns

1. **Validação de APIs**: Verificar estrutura de JSON
2. **Testes de UI**: Comparar screenshots
3. **Relatórios**: Validar documentos PDF/HTML
4. **Serializadores**: Verificar saídas XML/JSON
5. **Entity Framework**: Comparar consultas SQL geradas

## 🏃‍♂️ Como Executar

```bash
cd Dicas/Dica33-TestesSnapshotComVerify/Dica33.TestesSnapshotComVerify
dotnet run

# Executar testes
cd ../Dica33.TestesSnapshotComVerify.Tests
dotnet test
```

## 📊 Workflow de Aprovação

1. **Primeira execução**: Gera arquivo .received.txt
2. **Revisar**: Verificar se o conteúdo está correto
3. **Aprovar**: Renomear para .verified.txt ou usar ferramenta
4. **Futuras execuções**: Compara com versão aprovada
5. **Mudanças**: Gera novo .received.txt para revisão

## 🛠️ Ferramentas Complementares

- **DiffEngineTray**: Aprovação visual de mudanças
- **Rider/VS Extensions**: Integração com IDE
- **CI/CD**: Configuração para builds automatizados
