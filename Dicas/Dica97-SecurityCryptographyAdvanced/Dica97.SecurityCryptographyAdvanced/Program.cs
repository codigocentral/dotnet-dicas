using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

Console.WriteLine("🔐 Dica 97: Security & Cryptography Advanced (.NET 9)");
Console.WriteLine("======================================================");

// 1. Advanced Encryption (AES-GCM)
Console.WriteLine("\n1. 🔒 Advanced Encryption (AES-GCM):");
Console.WriteLine("──────────────────────────────────────");

DemonstrarAESGCM();

// 2. Digital Signatures (RSA)
Console.WriteLine("\n2. ✍️ Digital Signatures (RSA):");
Console.WriteLine("───────────────────────────────");

DemonstrarAssinaturasDigitais();

// 3. Password Hashing & Key Derivation
Console.WriteLine("\n3. 🔑 Password Hashing & Key Derivation:");
Console.WriteLine("────────────────────────────────────────");

DemonstrarPasswordHashing();

// 4. JWT Token Security
Console.WriteLine("\n4. 🎫 JWT Token Security:");
Console.WriteLine("────────────────────────");

DemonstrarJWTSecurity();

// 5. Cryptographic Random Generation
Console.WriteLine("\n5. 🎲 Cryptographic Random Generation:");
Console.WriteLine("─────────────────────────────────────");

DemonstrarCryptographicRandom();

// 6. X.509 Certificate Handling
Console.WriteLine("\n6. 📜 X.509 Certificate Handling:");
Console.WriteLine("─────────────────────────────────");

DemonstrarCertificados();

// 7. Secure Hash Algorithms
Console.WriteLine("\n7. 🔍 Secure Hash Algorithms:");
Console.WriteLine("────────────────────────────");

DemonstrarHashAlgorithms();

// 8. Message Authentication (HMAC)
Console.WriteLine("\n8. 🔏 Message Authentication (HMAC):");
Console.WriteLine("───────────────────────────────────");

DemonstrarHMAC();

Console.WriteLine("\n✅ Demonstração completa de Security & Cryptography!");

static void DemonstrarAESGCM()
{
    // AES-GCM é o padrão ouro para criptografia simétrica autenticada
    using var aes = Aes.Create();
    aes.GenerateKey();
    
    var plaintext = "Dados confidenciais para demonstração"u8.ToArray();
    var nonce = new byte[12]; // 96 bits recomendado para GCM
    var tag = new byte[16];   // 128 bits para autenticação
    var ciphertext = new byte[plaintext.Length];
    
    RandomNumberGenerator.Fill(nonce);
    
    Console.WriteLine($"📝 Texto original: {Encoding.UTF8.GetString(plaintext)}");
    Console.WriteLine($"🔑 Chave AES: {Convert.ToHexString(aes.Key)}");
    Console.WriteLine($"🎲 Nonce: {Convert.ToHexString(nonce)}");
    
    // Encryption
    using (var gcm = new AesGcm(aes.Key, 16))
    {
        gcm.Encrypt(nonce, plaintext, ciphertext, tag);
    }
    
    Console.WriteLine($"🔒 Texto cifrado: {Convert.ToHexString(ciphertext)}");
    Console.WriteLine($"🏷️ Tag autenticação: {Convert.ToHexString(tag)}");
    
    // Decryption
    var decrypted = new byte[plaintext.Length];
    using (var gcm = new AesGcm(aes.Key, 16))
    {
        gcm.Decrypt(nonce, ciphertext, tag, decrypted);
    }
    
    Console.WriteLine($"🔓 Texto decifrado: {Encoding.UTF8.GetString(decrypted)}");
    Console.WriteLine($"✅ Integridade verificada: {plaintext.SequenceEqual(decrypted)}");
}

static void DemonstrarAssinaturasDigitais()
{
    // Geração de chaves RSA
    using var rsa = RSA.Create(2048);
    var publicKey = rsa.ExportRSAPublicKey();
    var privateKey = rsa.ExportRSAPrivateKey();
    
    Console.WriteLine($"🔑 Chave pública (tamanho): {publicKey.Length} bytes");
    Console.WriteLine($"🔐 Chave privada (tamanho): {privateKey.Length} bytes");
    
    var message = "Documento importante que precisa ser assinado"u8.ToArray();
    
    // Assinatura com PSS (mais seguro que PKCS#1 v1.5)
    var signature = rsa.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    
    Console.WriteLine($"📝 Mensagem: {Encoding.UTF8.GetString(message)}");
    Console.WriteLine($"✍️ Assinatura: {Convert.ToHexString(signature)}");
    Console.WriteLine($"📏 Tamanho assinatura: {signature.Length} bytes");
    
    // Verificação da assinatura
    var isValid = rsa.VerifyData(message, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    Console.WriteLine($"✅ Assinatura válida: {isValid}");
    
    // Teste com mensagem alterada
    var tamperedMessage = "Documento importante que foi alterado"u8.ToArray();
    var isTamperedValid = rsa.VerifyData(tamperedMessage, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    Console.WriteLine($"🚫 Mensagem alterada válida: {isTamperedValid}");
    
    // Informações do algoritmo
    Console.WriteLine($"🔧 Tamanho da chave: {rsa.KeySize} bits");
    Console.WriteLine($"📊 Algoritmo hash: SHA256");
    Console.WriteLine($"🛡️ Padding: PSS (mais seguro)");
}

static void DemonstrarPasswordHashing()
{
    var password = "MinhaSupErSenh@123!";
    var salt = new byte[32];
    RandomNumberGenerator.Fill(salt);
    
    Console.WriteLine($"🔑 Senha: {password}");
    Console.WriteLine($"🧂 Salt: {Convert.ToHexString(salt)}");
    
    // PBKDF2 com diferentes iterações
    var iterations = new[] { 10000, 50000, 100000 };
    
    foreach (var iteration in iterations)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iteration,
            numBytesRequested: 32);
        
        stopwatch.Stop();
        
        Console.WriteLine($"🔄 {iteration:N0} iterações:");
        Console.WriteLine($"   🔒 Hash: {Convert.ToHexString(hash)}");
        Console.WriteLine($"   ⏱️ Tempo: {stopwatch.ElapsedMilliseconds}ms");
    }
    
    // Demonstrar diferentes algoritmos
    Console.WriteLine("\n🔬 Comparação de algoritmos:");
    
    // Argon2 seria preferível, mas não está no .NET standard
    // Demonstrando PBKDF2 vs BCrypt concept
    var bcryptLike = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 100000, 64);
    Console.WriteLine($"🔐 PBKDF2-SHA512: {Convert.ToHexString(bcryptLike)}");
    
    // Key stretching demonstration
    Console.WriteLine($"\n💡 Key Stretching Benefits:");
    Console.WriteLine($"   ⏱️ Força tempo de computação no atacante");
    Console.WriteLine($"   🧂 Salt previne rainbow table attacks");
    Console.WriteLine($"   🔄 Iterações ajustáveis conforme poder computacional");
}

static void DemonstrarJWTSecurity()
{
    // Geração de chave secreta para HMAC
    var secretKey = new byte[64]; // 512 bits
    RandomNumberGenerator.Fill(secretKey);
    var securityKey = new SymmetricSecurityKey(secretKey);
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    
    // Claims do usuário
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "12345"),
        new Claim(ClaimTypes.Name, "usuario@exemplo.com"),
        new Claim(ClaimTypes.Role, "Admin"),
        new Claim("scope", "read write"),
        new Claim("aud", "minha-api"),
        new Claim("iss", "meu-servidor")
    };
    
    // Criação do token
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = credentials,
        Issuer = "meu-servidor",
        Audience = "minha-api"
    };
    
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);
    
    Console.WriteLine($"🎫 JWT Token criado:");
    Console.WriteLine($"   📏 Tamanho: {tokenString.Length} chars");
    
    // Parsing do token para mostrar estrutura
    var jwtToken = tokenHandler.ReadJwtToken(tokenString);
    Console.WriteLine($"\n📋 Header:");
    Console.WriteLine($"   🔧 Algoritmo: {jwtToken.Header.Alg}");
    Console.WriteLine($"   🏷️ Tipo: {jwtToken.Header.Typ}");
    
    Console.WriteLine($"\n📄 Payload:");
    foreach (var claim in jwtToken.Claims.Take(5))
    {
        Console.WriteLine($"   {claim.Type}: {claim.Value}");
    }
    
    Console.WriteLine($"\n⏰ Validade:");
    Console.WriteLine($"   📅 Emitido: {jwtToken.IssuedAt:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"   ⏳ Expira: {jwtToken.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
    
    // Validação do token
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = securityKey,
        ValidateIssuer = true,
        ValidIssuer = "meu-servidor",
        ValidateAudience = true,
        ValidAudience = "minha-api",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    try
    {
        var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out _);
        Console.WriteLine($"✅ Token válido para: {principal.Identity?.Name}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Token inválido: {ex.Message}");
    }
}

static void DemonstrarCryptographicRandom()
{
    Console.WriteLine("🎲 Geração de números aleatórios criptograficamente seguros:");
    
    // Bytes aleatórios
    var randomBytes = new byte[32];
    RandomNumberGenerator.Fill(randomBytes);
    Console.WriteLine($"🔢 32 bytes aleatórios: {Convert.ToHexString(randomBytes)}");
    
    // Números inteiros aleatórios
    using var rng = RandomNumberGenerator.Create();
    var randomInts = new int[5];
    for (int i = 0; i < randomInts.Length; i++)
    {
        randomInts[i] = RandomNumberGenerator.GetInt32(1, 1000000);
    }
    Console.WriteLine($"🎯 Números aleatórios: [{string.Join(", ", randomInts)}]");
    
    // Senha aleatória segura
    var passwordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
    var passwordLength = 16;
    var password = new char[passwordLength];
    
    for (int i = 0; i < passwordLength; i++)
    {
        password[i] = passwordChars[RandomNumberGenerator.GetInt32(passwordChars.Length)];
    }
    
    Console.WriteLine($"🔐 Senha aleatória: {new string(password)}");
    
    // API Key aleatória
    var apiKeyBytes = new byte[32];
    RandomNumberGenerator.Fill(apiKeyBytes);
    var apiKey = Convert.ToBase64String(apiKeyBytes);
    Console.WriteLine($"🗝️ API Key: {apiKey}");
    
    // Session ID
    var sessionBytes = new byte[16];
    RandomNumberGenerator.Fill(sessionBytes);
    var sessionId = Convert.ToHexString(sessionBytes).ToLower();
    Console.WriteLine($"🆔 Session ID: {sessionId}");
    
    // Entropia analysis
    Console.WriteLine($"\n📊 Análise de entropia:");
    Console.WriteLine($"   🔢 32 bytes = 256 bits de entropia");
    Console.WriteLine($"   🎯 2^256 ≈ 1.16 × 10^77 combinações possíveis");
    Console.WriteLine($"   🔒 Praticamente impossível de quebrar por força bruta");
}

static void DemonstrarCertificados()
{
    // Demonstração conceitual de certificados X.509
    Console.WriteLine("📜 Conceitos de Certificados X.509:");
    Console.WriteLine("   🏢 Autoridade Certificadora (CA)");
    Console.WriteLine("   🔗 Cadeia de confiança");
    Console.WriteLine("   📅 Período de validade");
    Console.WriteLine("   🔑 Chave pública + Assinatura da CA");
    
    // Simular informações de certificado
    using var rsa = RSA.Create(2048);
    var publicKey = rsa.ExportRSAPublicKey();
    
    var certInfo = new
    {
        Subject = "CN=exemplo.com, O=Minha Empresa, C=BR",
        Issuer = "CN=CA Raiz, O=Autoridade Certificadora, C=BR",
        ValidFrom = DateTime.UtcNow,
        ValidTo = DateTime.UtcNow.AddYears(1),
        SerialNumber = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
        KeySize = rsa.KeySize,
        SignatureAlgorithm = "RSA-SHA256",
        KeyUsage = "Digital Signature, Key Encipherment"
    };
    
    Console.WriteLine($"\n📋 Informações do Certificado Simulado:");
    Console.WriteLine($"   👤 Subject: {certInfo.Subject}");
    Console.WriteLine($"   🏢 Issuer: {certInfo.Issuer}");
    Console.WriteLine($"   📅 Válido de: {certInfo.ValidFrom:yyyy-MM-dd}");
    Console.WriteLine($"   📅 Válido até: {certInfo.ValidTo:yyyy-MM-dd}");
    Console.WriteLine($"   🔢 Serial: {certInfo.SerialNumber}");
    Console.WriteLine($"   🔑 Tamanho chave: {certInfo.KeySize} bits");
    Console.WriteLine($"   ✍️ Algoritmo: {certInfo.SignatureAlgorithm}");
    Console.WriteLine($"   🎯 Uso: {certInfo.KeyUsage}");
    
    Console.WriteLine($"\n🔒 Casos de uso:");
    Console.WriteLine($"   🌐 TLS/SSL para HTTPS");
    Console.WriteLine($"   📧 Assinatura de email (S/MIME)");
    Console.WriteLine($"   📝 Assinatura de código");
    Console.WriteLine($"   🔐 Autenticação de cliente");
}

static void DemonstrarHashAlgorithms()
{
    var data = "Dados para demonstração de hash"u8.ToArray();
    
    Console.WriteLine($"📝 Dados originais: {Encoding.UTF8.GetString(data)}");
    Console.WriteLine($"📏 Tamanho: {data.Length} bytes");
    
    // SHA-256
    using (var sha256 = SHA256.Create())
    {
        var hash = sha256.ComputeHash(data);
        Console.WriteLine($"\n🔍 SHA-256:");
        Console.WriteLine($"   Hash: {Convert.ToHexString(hash)}");
        Console.WriteLine($"   Tamanho: {hash.Length * 8} bits");
    }
    
    // SHA-384
    using (var sha384 = SHA384.Create())
    {
        var hash = sha384.ComputeHash(data);
        Console.WriteLine($"\n🔍 SHA-384:");
        Console.WriteLine($"   Hash: {Convert.ToHexString(hash)}");
        Console.WriteLine($"   Tamanho: {hash.Length * 8} bits");
    }
    
    // SHA-512
    using (var sha512 = SHA512.Create())
    {
        var hash = sha512.ComputeHash(data);
        Console.WriteLine($"\n🔍 SHA-512:");
        Console.WriteLine($"   Hash: {Convert.ToHexString(hash)}");
        Console.WriteLine($"   Tamanho: {hash.Length * 8} bits");
    }
    
    // Demonstrar propriedades do hash
    Console.WriteLine($"\n✨ Propriedades dos hashes criptográficos:");
    Console.WriteLine($"   🔄 Determinístico: mesmo input = mesmo hash");
    Console.WriteLine($"   ⚡ Rápido de calcular");
    Console.WriteLine($"   🔒 Difícil de reverter (one-way)");
    Console.WriteLine($"   🎯 Pequena mudança = hash completamente diferente");
    Console.WriteLine($"   🚫 Resistente a colisões");
    
    // Demonstrar avalanche effect
    var modifiedData = "Dados para demonstração de Hash"u8.ToArray(); // H maiúsculo
    using var sha256_modified = SHA256.Create();
    var originalHash = SHA256.HashData(data);
    var modifiedHash = sha256_modified.ComputeHash(modifiedData);
    
    Console.WriteLine($"\n🌊 Efeito Avalanche (1 caractere diferente):");
    Console.WriteLine($"   Original: {Convert.ToHexString(originalHash)}");
    Console.WriteLine($"   Modificado: {Convert.ToHexString(modifiedHash)}");
    
    var differentBits = 0;
    for (int i = 0; i < originalHash.Length; i++)
    {
        differentBits += System.Numerics.BitOperations.PopCount((uint)(originalHash[i] ^ modifiedHash[i]));
    }
    Console.WriteLine($"   📊 Bits diferentes: {differentBits}/256 ({differentBits / 256.0:P1})");
}

static void DemonstrarHMAC()
{
    var key = RandomNumberGenerator.GetBytes(32); // 256-bit key
    var message = "Mensagem que precisa ser autenticada"u8.ToArray();
    
    Console.WriteLine($"🔑 Chave HMAC: {Convert.ToHexString(key)}");
    Console.WriteLine($"📝 Mensagem: {Encoding.UTF8.GetString(message)}");
    
    // HMAC-SHA256
    using (var hmac = new HMACSHA256(key))
    {
        var mac = hmac.ComputeHash(message);
        Console.WriteLine($"\n🔏 HMAC-SHA256:");
        Console.WriteLine($"   MAC: {Convert.ToHexString(mac)}");
        Console.WriteLine($"   Tamanho: {mac.Length * 8} bits");
        
        // Verificação
        var mac2 = hmac.ComputeHash(message);
        var isValid = mac.SequenceEqual(mac2);
        Console.WriteLine($"   ✅ Verificação: {isValid}");
    }
    
    // HMAC-SHA512
    using (var hmac512 = new HMACSHA512(key))
    {
        var mac = hmac512.ComputeHash(message);
        Console.WriteLine($"\n🔏 HMAC-SHA512:");
        Console.WriteLine($"   MAC: {Convert.ToHexString(mac)}");
        Console.WriteLine($"   Tamanho: {mac.Length * 8} bits");
    }
    
    // Demonstrar proteção contra tampering
    var tamperedMessage = "Mensagem que foi modificada maliciosamente"u8.ToArray();
    using (var hmacVerify = new HMACSHA256(key))
    {
        var originalMac = hmacVerify.ComputeHash(message);
        var tamperedMac = hmacVerify.ComputeHash(tamperedMessage);
        
        Console.WriteLine($"\n🔍 Detecção de tampering:");
        Console.WriteLine($"   Original MAC: {Convert.ToHexString(originalMac)}");
        Console.WriteLine($"   Tampered MAC: {Convert.ToHexString(tamperedMac)}");
        Console.WriteLine($"   🚫 Mensagem foi alterada: {!originalMac.SequenceEqual(tamperedMac)}");
    }
    
    Console.WriteLine($"\n💡 Usos do HMAC:");
    Console.WriteLine($"   🔐 Autenticação de API requests");
    Console.WriteLine($"   📧 Verificação de integridade de mensagens");
    Console.WriteLine($"   🎫 Assinatura de tokens (JWT)");
    Console.WriteLine($"   🔒 Protocolos criptográficos (TLS)");
}
