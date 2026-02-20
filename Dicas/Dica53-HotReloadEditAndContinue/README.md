# Dica 53: Hot Reload & Edit and Continue

## 📖 Descrição

Esta dica demonstra as funcionalidades **Hot Reload** e **Edit and Continue** do .NET, que permitem modificar código durante a execução da aplicação sem necessidade de reiniciar.

## 🎯 Objetivos

- Entender a diferença entre Hot Reload e Edit and Continue
- Configurar projetos para suportar essas funcionalidades
- Conhecer limitações e melhores práticas
- Comparar performance vs restart completo da aplicação

## 🔥 Hot Reload

### O que é
Hot Reload permite aplicar mudanças no código-fonte sem parar a aplicação em execução.

### Configuração
```xml
<PropertyGroup>
  <EnableHotReload>true</EnableHotReload>
  <UseAppHost>true</UseAppHost>
</PropertyGroup>
```

### Como usar
```bash
# Executar com Hot Reload
dotnet watch run

# Com argumentos
dotnet watch run -- demo

# Em testes
dotnet watch test
```

### Teclas durante execução
- `Ctrl+R` = Force reload
- `Ctrl+C` = Parar aplicação

## 🐛 Edit and Continue

### O que é
Edit and Continue permite editar código durante uma sessão de debug e continuar a execução.

### Configuração
```xml
<PropertyGroup>
  <DebugType>portable</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <Optimize>false</Optimize>
</PropertyGroup>
```

### Como usar
1. Execute no modo debug (F5)
2. Coloque breakpoint
3. Quando parar, modifique o código
4. Continue execução (F5)

## ✅ O que é suportado

### Hot Reload
- ✅ Modificar corpo de métodos
- ✅ Adicionar novos métodos
- ✅ Modificar valores de constantes
- ✅ Alterar strings e literals
- ✅ Adicionar novos campos

### Edit and Continue
- ✅ Modificar corpo de métodos
- ✅ Adicionar/modificar variáveis locais
- ✅ Alterar valores de constantes
- ✅ Modificar expressões

## ❌ Limitações

### Não suportado
- ❌ Mudanças em assinaturas de métodos
- ❌ Adicionar/remover tipos
- ❌ Mudanças em lambda expressions complexas
- ❌ Modificar estrutura de classes/interfaces
- ❌ Alterar herança ou implementações

## 🏃‍♂️ Como Executar

```bash
# Executar demonstração geral
dotnet run demo

# Executar servidor web (para Hot Reload)
dotnet watch run server

# Executar calculadora (para Edit and Continue)
dotnet run calculator
```

## 📊 Comparação de Performance

| Cenário | Hot Reload | Restart Completo |
|---------|------------|------------------|
| Pequenas mudanças | ~1-2s | ~5-10s |
| Aplicações grandes | ~2-5s | ~30-60s |
| Mudanças complexas | Não suportado | ~30-60s |

## 💡 Melhores Práticas

### Para Hot Reload
1. Use apenas durante desenvolvimento
2. Desabilite em produção
3. Reinicie periodicamente em sessões longas
4. Monitore uso de memória

### Para Edit and Continue
1. Configure projeto corretamente
2. Evite mudanças complexas durante debug
3. Use para ajustes rápidos de lógica
4. Reinicie debug se instável

## 🎯 Cenários de Uso

### Hot Reload é ideal para:
- Ajustes de UI em tempo real
- Modificação de algoritmos
- Testes rápidos de mudanças
- Desenvolvimento iterativo

### Edit and Continue é ideal para:
- Debug de lógica específica
- Testes de diferentes valores
- Correções rápidas durante debug
- Exploração de código

## ⚡ Dicas de Produtividade

1. **Combine ferramentas**: Use Hot Reload para desenvolvimento e Edit and Continue para debug
2. **Shortcuts úteis**: Aprenda teclas de atalho do seu IDE
3. **Configuração por ambiente**: Habilite apenas em Development
4. **Monitoramento**: Observe consumo de recursos

## 🔧 Suporte por IDE

| IDE | Hot Reload | Edit and Continue |
|-----|------------|-------------------|
| Visual Studio | ✅ Completo | ✅ Completo |
| VS Code | ✅ Limitado | ✅ Limitado |
| JetBrains Rider | ✅ Sim | ✅ Sim |

## 📚 Recursos Adicionais

- [Documentação Hot Reload](https://docs.microsoft.com/dotnet/core/tools/dotnet-watch)
- [Edit and Continue](https://docs.microsoft.com/visualstudio/debugger/edit-and-continue)
- [.NET CLI Watch](https://docs.microsoft.com/dotnet/core/tools/dotnet-watch)

---

**Nota**: Hot Reload e Edit and Continue revolucionam a produtividade do desenvolvimento, permitindo feedback instantâneo e debug mais eficiente!
