# 🔒 Dica 93: Advanced Security & Cryptography (.NET 9)

## 📋 Sobre

Esta dica demonstra técnicas avançadas de segurança e criptografia em .NET 9, incluindo criptografia simétrica e assimétrica, assinaturas digitais, JWT tokens, funções hash, geração de números aleatórios seguros, proteção de memória e rate limiting.

## 🎯 Conceitos Demonstrados

### 1. 🔐 Symmetric Encryption (AES-256-GCM)

- AES-GCM para criptografia autenticada
- Geração segura de chaves e nonces
- Tag de autenticação para integridade
- Proteção contra ataques de modificação

### 2. 🗝️ Asymmetric Encryption (RSA)

- Criptografia RSA 2048 bits
- OAEP-SHA256 padding para segurança
- Assinaturas digitais PKCS#1
- Verificação de integridade e autenticidade

### 3. ✍️ Digital Signatures & Certificates

- Certificados X.509 auto-assinados
- PKCS#7 (CMS) para assinatura de documentos
- Validação de certificados
- Metadados de certificados

### 4. 🎫 JWT Token Security

- Criação e validação de tokens JWT
- HMAC-SHA256 para assinatura
- Claims personalizados
- Validação de tempo e audience

### 5. #️⃣ Hash Functions & Key Derivation

- SHA-256 e SHA-512
- PBKDF2 para derivação de chaves
- HMAC para autenticação de mensagem
- Simulação de Argon2

### 6. 🎲 Secure Random Generation

- RandomNumberGenerator criptograficamente seguro
- Geração de GUIDs seguros
- Tokens de sessão
- Senhas temporárias

### 7. 🛡️ Memory Protection

- Limpeza segura de dados sensíveis
- Proteção de chaves criptográficas
- Garbage Collection controlado
- Boas práticas de memória

### 8. 🚦 Rate Limiting & Protection

- Rate limiting simples
- Proteção contra DDoS
- Janela deslizante de tempo
- Controle de acesso por usuário

## 🚀 Como Executar

```bash
dotnet run
```

## 📊 Saída Esperada

```
🔒 Dica 93: Advanced Security & Cryptography (.NET 9)
======================================================

1. 🔐 Symmetric Encryption (AES-256-GCM):
─────────────────────────────────────────
🔑 Chave AES: 597CB8688826C492...
🔢 Nonce: 4B83738CBF479E4D5DEB3964
🔐 Dados originais: Dados sensíveis para criptografar 🔒
🔒 Dados criptografados: FDA881317B7181BB35E79BB3...
🏷️ Tag de autenticação: 84015A44CDA3429F90E11AD971F54D8B
🔓 Dados descriptografados: Dados sensíveis para criptografar 🔒
✅ Integridade verificada: True

2. 🗝️ Asymmetric Encryption (RSA):
──────────────────────────────────
🔑 Chave pública (primeiros 32 bytes): 3082010A0282010100B168E31A...
🔐 Chave privada (primeiros 32 bytes): 308204A20201000282010100B168E31A...
📝 Mensagem original: Mensagem confidencial para RSA
🔒 Dados criptografados: 18DE479DEA68D0DB443941B4502DE5A0...
🔓 Mensagem descriptografada: Mensagem confidencial para RSA
✍️ Assinatura digital: 985F2097FF2CCA5633E3235D9DB33541...
✅ Assinatura válida: True

3. ✍️ Digital Signatures & Certificates:
─────────────────────────────────────
📜 Certificado criado:
   👤 Subject: CN=Demo Certificate
   📅 Válido de: 2025-07-10
   📅 Válido até: 2026-07-11
   🔑 Thumbprint: 23DC900D853C8081...
📄 Dados originais: Documento importante para assinar
✍️ Dados assinados (PKCS#7): 1133 bytes
📋 Assinantes: 1
✅ Assinatura PKCS#7 verificada com sucesso!

4. 🎫 JWT Token Security:
─────────────────────────
🔑 JWT Secret Key: D054A5F648A8C9C5...
🎫 JWT Token criado:
   📄 Header.Payload: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   📝 Claims: 5 claims incluídos
   ⏰ Expira em: 15:41:31
✅ Token JWT válido!
   👤 Usuário: 
   🏷️ Claims validados: 10
   🔐 Algoritmo: HS256

5. #️⃣ Hash Functions & Key Derivation:
──────────────────────────────────────
📄 Dados: Dados para hash e derivação de chave
🧂 Salt: 60EBC63613FA00B854A7CAAF0599AFF9
#️⃣ SHA-256: 47DE3CD2A22EAC1114CE75F827505C59...
#️⃣ SHA-512: ACF67ED8BB9E66CEFA5C617B9B62F498...
🔑 PBKDF2 (100k iterações): 842B54355B279F38AE918AD2C53FEEE7...
🔐 HMAC-SHA256: E68144860D6D9AF255B9BA96CE92029A...
💪 Argon2-like (500k iter): 51A9EBDAE3417F9C15ACBDBA3A26FD1C...

6. 🎲 Secure Random Generation:
──────────────────────────────
🎲 Gerando números aleatórios seguros:
   📊 16 bytes aleatórios: 2B42FEA5582431200634DAF5DF377231
   🔢 Números (1-1000): [610, 489, 742, 897, 873]
   🆔 GUIDs seguros:
      9c75076c-fe9b-8585-4f6a-e01526e26233
      add15c14-96c6-3625-dfd4-81658d6575a5
      f8fb5acd-9611-e6bd-45c1-c837b2af9ac1
   🎫 Token de sessão: bMhI3NhW2TEIh9bQ...
   🔑 Senha temporária: hIFewfNUcNcw

7. 🛡️ Memory Protection:
─────────────────────────
🛡️ Proteção de memória sensível:
   📝 Dados sensíveis: senha123
   #️⃣ Hash processado: 55A5E9E78207B4DF...
   🧹 Dados limpos da memória:         
   🔑 Chave AES original: 70BCD8B193283E5A...
   🗑️ Chave AES limpa da memória
   🧽 Garbage Collection executado

8. 🚦 Rate Limiting & Protection:
─────────────────────────────────
🚦 Testando Rate Limiting (5 requests/10s):
   Request 1: ✅ PERMITIDO
   Request 2: ✅ PERMITIDO
   Request 3: ✅ PERMITIDO
   Request 4: ✅ PERMITIDO
   Request 5: ✅ PERMITIDO
   Request 6: ❌ BLOQUEADO
   Request 7: ❌ BLOQUEADO
   Request 8: ❌ BLOQUEADO

   ⏰ Aguardando reset do rate limiter...
   Request após reset: ❌ BLOQUEADO

✅ Demonstração completa de Advanced Security!
```

## 🔧 Funcionalidades

### Symmetric Encryption

- ✅ AES-256-GCM com autenticação
- ✅ Geração segura de chaves e nonces
- ✅ Tag de autenticação integrada
- ✅ Proteção contra tampering

### Asymmetric Encryption

- ✅ RSA 2048 bits
- ✅ OAEP-SHA256 padding
- ✅ Assinatura digital PKCS#1
- ✅ Verificação de integridade

### Digital Signatures

- ✅ Certificados X.509
- ✅ PKCS#7/CMS signing
- ✅ Validação automática
- ✅ Metadados completos

### JWT Security

- ✅ Criação e validação
- ✅ HMAC-SHA256 signing
- ✅ Claims customizados
- ✅ Validação temporal

### Hash & Key Derivation

- ✅ SHA-256/SHA-512
- ✅ PBKDF2 com salt
- ✅ HMAC authentication
- ✅ Key strengthening

### Secure Random

- ✅ Cryptographically secure
- ✅ Multiple data types
- ✅ Session tokens
- ✅ Password generation

### Memory Protection

- ✅ Secure data clearing
- ✅ Key protection
- ✅ Memory cleanup
- ✅ GC management

### Rate Limiting

- ✅ Time-window based
- ✅ Per-user limits
- ✅ DDoS protection
- ✅ Configurable thresholds

## 🎓 Conceitos Aprendidos

- **AES-GCM**: Criptografia autenticada moderna
- **RSA**: Criptografia de chave pública robusta
- **PKCS#7**: Padrão para assinatura digital
- **JWT**: Tokens seguros para autenticação
- **PBKDF2**: Derivação segura de chaves
- **Memory Security**: Proteção de dados sensíveis
- **Rate Limiting**: Proteção contra ataques

## 📚 Referências

- [.NET Cryptography](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
- [AES-GCM](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm)
- [JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [PKCS#7 CMS](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.pkcs.signedcms)
