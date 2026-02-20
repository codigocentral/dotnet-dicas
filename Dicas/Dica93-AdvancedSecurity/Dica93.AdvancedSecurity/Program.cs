using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

Console.WriteLine("🔒 Dica 93: Advanced Security & Cryptography (.NET 9)");
Console.WriteLine("======================================================");

// 1. Symmetric Encryption (AES-256-GCM)
Console.WriteLine("\n1. 🔐 Symmetric Encryption (AES-256-GCM):");
Console.WriteLine("─────────────────────────────────────────");

await DemonstrarCriptografiaSimetrica();

// 2. Asymmetric Encryption (RSA)
Console.WriteLine("\n2. 🗝️ Asymmetric Encryption (RSA):");
Console.WriteLine("──────────────────────────────────");

await DemonstrarCriptografiaAssimetrica();

// 3. Digital Signatures & Certificate Validation
Console.WriteLine("\n3. ✍️ Digital Signatures & Certificates:");
Console.WriteLine("─────────────────────────────────────");

await DemonstrarAssinaturasDigitais();

// 4. JWT Token Security
Console.WriteLine("\n4. 🎫 JWT Token Security:");
Console.WriteLine("─────────────────────────");

await DemonstrarJWTSecurity();

// 5. Hash Functions & Key Derivation
Console.WriteLine("\n5. #️⃣ Hash Functions & Key Derivation:");
Console.WriteLine("──────────────────────────────────────");

DemonstrarHashFunctions();

// 6. Secure Random Generation
Console.WriteLine("\n6. 🎲 Secure Random Generation:");
Console.WriteLine("──────────────────────────────");

DemonstrarSecureRandom();

// 7. Memory Protection & Secure Strings
Console.WriteLine("\n7. 🛡️ Memory Protection:");
Console.WriteLine("─────────────────────────");

DemonstrarMemoryProtection();

// 8. Rate Limiting & DDoS Protection
Console.WriteLine("\n8. 🚦 Rate Limiting & Protection:");
Console.WriteLine("─────────────────────────────────");

await DemonstrarRateLimiting();

Console.WriteLine("\n✅ Demonstração completa de Advanced Security!");

static async Task DemonstrarCriptografiaSimetrica()
{
    var dados = "Dados sensíveis para criptografar 🔒"u8.ToArray();
    
    // Gerar chave e nonce para AES-GCM
    using var aes = Aes.Create();
    aes.GenerateKey();
    var nonce = new byte[12]; // 96 bits para GCM
    RandomNumberGenerator.Fill(nonce);
    
    Console.WriteLine($"🔑 Chave AES: {Convert.ToHexString(aes.Key)[..16]}...");
    Console.WriteLine($"🔢 Nonce: {Convert.ToHexString(nonce)}");
    
    // Criptografar com AES-GCM
    var ciphertext = new byte[dados.Length];
    var tag = new byte[16]; // Authentication tag
    
    using var aesGcm = new AesGcm(aes.Key, 16);
    aesGcm.Encrypt(nonce, dados, ciphertext, tag);
    
    Console.WriteLine($"🔐 Dados originais: {Encoding.UTF8.GetString(dados)}");
    Console.WriteLine($"🔒 Dados criptografados: {Convert.ToHexString(ciphertext)}");
    Console.WriteLine($"🏷️ Tag de autenticação: {Convert.ToHexString(tag)}");
    
    // Descriptografar
    var dadosDescriptografados = new byte[ciphertext.Length];
    aesGcm.Decrypt(nonce, ciphertext, tag, dadosDescriptografados);
    
    Console.WriteLine($"🔓 Dados descriptografados: {Encoding.UTF8.GetString(dadosDescriptografados)}");
    Console.WriteLine($"✅ Integridade verificada: {dados.SequenceEqual(dadosDescriptografados)}");
    
    await Task.Delay(10); // Simular operação assíncrona
}

static async Task DemonstrarCriptografiaAssimetrica()
{
    // Gerar par de chaves RSA 2048 bits
    using var rsa = RSA.Create(2048);
    var publicKey = rsa.ExportRSAPublicKey();
    var privateKey = rsa.ExportRSAPrivateKey();
    
    Console.WriteLine($"🔑 Chave pública (primeiros 32 bytes): {Convert.ToHexString(publicKey)[..32]}...");
    Console.WriteLine($"🔐 Chave privada (primeiros 32 bytes): {Convert.ToHexString(privateKey)[..32]}...");
    
    var mensagem = "Mensagem confidencial para RSA"u8.ToArray();
    
    // Criptografar com chave pública
    var dadosCriptografados = rsa.Encrypt(mensagem, RSAEncryptionPadding.OaepSHA256);
    Console.WriteLine($"📝 Mensagem original: {Encoding.UTF8.GetString(mensagem)}");
    Console.WriteLine($"🔒 Dados criptografados: {Convert.ToHexString(dadosCriptografados)[..32]}...");
    
    // Descriptografar com chave privada
    var dadosDescriptografados = rsa.Decrypt(dadosCriptografados, RSAEncryptionPadding.OaepSHA256);
    Console.WriteLine($"🔓 Mensagem descriptografada: {Encoding.UTF8.GetString(dadosDescriptografados)}");
    
    // Assinatura digital
    var hash = SHA256.HashData(mensagem);
    var assinatura = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    
    Console.WriteLine($"✍️ Assinatura digital: {Convert.ToHexString(assinatura)[..32]}...");
    
    // Verificar assinatura
    var assinaturaValida = rsa.VerifyHash(hash, assinatura, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    Console.WriteLine($"✅ Assinatura válida: {assinaturaValida}");
    
    await Task.Delay(10);
}

static async Task DemonstrarAssinaturasDigitais()
{
    // Criar certificado auto-assinado para demonstração
    using var rsa = RSA.Create(2048);
    var certificateRequest = new CertificateRequest(
        "CN=Demo Certificate",
        rsa,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);
    
    var certificate = certificateRequest.CreateSelfSigned(
        DateTimeOffset.UtcNow.AddDays(-1),
        DateTimeOffset.UtcNow.AddDays(365));
    
    Console.WriteLine($"📜 Certificado criado:");
    Console.WriteLine($"   👤 Subject: {certificate.Subject}");
    Console.WriteLine($"   📅 Válido de: {certificate.NotBefore:yyyy-MM-dd}");
    Console.WriteLine($"   📅 Válido até: {certificate.NotAfter:yyyy-MM-dd}");
    Console.WriteLine($"   🔑 Thumbprint: {certificate.Thumbprint[..16]}...");
    
    // Demonstrar PKCS#7 (CMS) - Signed Data
    var dados = "Documento importante para assinar"u8.ToArray();
    var contentInfo = new ContentInfo(dados);
    var signedCms = new SignedCms(contentInfo);
    
    var cmsSigner = new CmsSigner(certificate)
    {
        DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1") // SHA-256
    };
    
    try
    {
        signedCms.ComputeSignature(cmsSigner);
        var signedData = signedCms.Encode();
        
        Console.WriteLine($"📄 Dados originais: {Encoding.UTF8.GetString(dados)}");
        Console.WriteLine($"✍️ Dados assinados (PKCS#7): {signedData.Length} bytes");
        Console.WriteLine($"📋 Assinantes: {signedCms.SignerInfos.Count}");
        
        // Verificar assinatura
        signedCms.CheckSignature(true);
        Console.WriteLine("✅ Assinatura PKCS#7 verificada com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro na assinatura PKCS#7: {ex.GetType().Name}");
    }
    
    certificate.Dispose();
    await Task.Delay(10);
}

static async Task DemonstrarJWTSecurity()
{
    // Criar chave HMAC para JWT
    var secretKey = new byte[32];
    RandomNumberGenerator.Fill(secretKey);
    var key = new SymmetricSecurityKey(secretKey);
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    Console.WriteLine($"🔑 JWT Secret Key: {Convert.ToHexString(secretKey)[..16]}...");
    
    // Criar claims do usuário
    var claims = new[]
    {
        new System.Security.Claims.Claim("sub", "user123"),
        new System.Security.Claims.Claim("name", "João Silva"),
        new System.Security.Claims.Claim("role", "admin"),
        new System.Security.Claims.Claim("email", "joao@exemplo.com"),
        new System.Security.Claims.Claim("scope", "read write delete")
    };
    
    // Criar JWT token
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(1),
        Issuer = "https://meuapp.com",
        Audience = "https://api.meuapp.com",
        SigningCredentials = credentials,
        NotBefore = DateTime.UtcNow,
        IssuedAt = DateTime.UtcNow
    };
    
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);
    
    Console.WriteLine($"🎫 JWT Token criado:");
    Console.WriteLine($"   📄 Header.Payload: {tokenString[..50]}...");
    Console.WriteLine($"   📝 Claims: {claims.Length} claims incluídos");
    Console.WriteLine($"   ⏰ Expira em: {tokenDescriptor.Expires:HH:mm:ss}");
    
    // Validar JWT token
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = "https://meuapp.com",
        ValidateAudience = true,
        ValidAudience = "https://api.meuapp.com",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    try
    {
        var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);
        var jwtToken = validatedToken as JwtSecurityToken;
        
        Console.WriteLine("✅ Token JWT válido!");
        Console.WriteLine($"   👤 Usuário: {principal.Identity?.Name}");
        Console.WriteLine($"   🏷️ Claims validados: {principal.Claims.Count()}");
        Console.WriteLine($"   🔐 Algoritmo: {jwtToken?.Header.Alg}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Token inválido: {ex.GetType().Name}");
    }
    
    await Task.Delay(10);
}

static void DemonstrarHashFunctions()
{
    var dados = "Dados para hash e derivação de chave"u8.ToArray();
    var salt = new byte[16];
    RandomNumberGenerator.Fill(salt);
    
    Console.WriteLine($"📄 Dados: {Encoding.UTF8.GetString(dados)}");
    Console.WriteLine($"🧂 Salt: {Convert.ToHexString(salt)}");
    
    // SHA-256
    var sha256Hash = SHA256.HashData(dados);
    Console.WriteLine($"#️⃣ SHA-256: {Convert.ToHexString(sha256Hash)}");
    
    // SHA-512
    var sha512Hash = SHA512.HashData(dados);
    Console.WriteLine($"#️⃣ SHA-512: {Convert.ToHexString(sha512Hash)[..32]}...");
    
    // PBKDF2 para derivação de chave
    using var pbkdf2 = new Rfc2898DeriveBytes(dados, salt, 100_000, HashAlgorithmName.SHA256);
    var derivedKey = pbkdf2.GetBytes(32);
    Console.WriteLine($"🔑 PBKDF2 (100k iterações): {Convert.ToHexString(derivedKey)}");
    
    // HMAC para autenticação de mensagem
    var hmacKey = new byte[32];
    RandomNumberGenerator.Fill(hmacKey);
    var hmacHash = HMACSHA256.HashData(hmacKey, dados);
    Console.WriteLine($"🔐 HMAC-SHA256: {Convert.ToHexString(hmacHash)}");
    
    // Argon2 (simulado com PBKDF2 mais forte)
    using var argon2Like = new Rfc2898DeriveBytes(dados, salt, 500_000, HashAlgorithmName.SHA256);
    var argon2Hash = argon2Like.GetBytes(32);
    Console.WriteLine($"💪 Argon2-like (500k iter): {Convert.ToHexString(argon2Hash)}");
}

static void DemonstrarSecureRandom()
{
    Console.WriteLine("🎲 Gerando números aleatórios seguros:");
    
    // Bytes aleatórios
    var randomBytes = new byte[16];
    RandomNumberGenerator.Fill(randomBytes);
    Console.WriteLine($"   📊 16 bytes aleatórios: {Convert.ToHexString(randomBytes)}");
    
    // Números inteiros seguros
    var randomNumbers = new int[5];
    for (int i = 0; i < randomNumbers.Length; i++)
    {
        randomNumbers[i] = RandomNumberGenerator.GetInt32(1, 1000);
    }
    Console.WriteLine($"   🔢 Números (1-1000): [{string.Join(", ", randomNumbers)}]");
    
    // UUID/GUID seguros
    var secureGuids = new Guid[3];
    for (int i = 0; i < secureGuids.Length; i++)
    {
        var guidBytes = new byte[16];
        RandomNumberGenerator.Fill(guidBytes);
        secureGuids[i] = new Guid(guidBytes);
    }
    Console.WriteLine($"   🆔 GUIDs seguros:");
    foreach (var guid in secureGuids)
    {
        Console.WriteLine($"      {guid}");
    }
    
    // Token de sessão seguro
    var sessionToken = new byte[32];
    RandomNumberGenerator.Fill(sessionToken);
    var sessionTokenString = Convert.ToBase64String(sessionToken);
    Console.WriteLine($"   🎫 Token de sessão: {sessionTokenString[..16]}...");
    
    // Senha temporária
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var password = new char[12];
    for (int i = 0; i < password.Length; i++)
    {
        password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }
    Console.WriteLine($"   🔑 Senha temporária: {new string(password)}");
}

static void DemonstrarMemoryProtection()
{
    // Demonstrar SecureString (obsoleto no .NET 9, mas ainda útil para entender)
    Console.WriteLine("🛡️ Proteção de memória sensível:");
    
    // Usando arrays que são zerados após uso
    var sensiveData = "senha123"u8.ToArray();
    Console.WriteLine($"   📝 Dados sensíveis: {Encoding.UTF8.GetString(sensiveData)}");
    
    // Processar dados sensíveis
    var hash = SHA256.HashData(sensiveData);
    Console.WriteLine($"   #️⃣ Hash processado: {Convert.ToHexString(hash)[..16]}...");
    
    // Limpar dados da memória
    Array.Clear(sensiveData);
    Console.WriteLine($"   🧹 Dados limpos da memória: {Encoding.UTF8.GetString(sensiveData)}");
    
    // Demonstrar proteção de chaves
    using var aes = Aes.Create();
    var originalKey = aes.Key.ToArray();
    Console.WriteLine($"   🔑 Chave AES original: {Convert.ToHexString(originalKey)[..16]}...");
    
    // Limpar chave após uso
    Array.Clear(originalKey);
    aes.Clear(); // Limpa todas as propriedades
    Console.WriteLine("   🗑️ Chave AES limpa da memória");
    
    // GC para demonstrar limpeza
    GC.Collect();
    GC.WaitForPendingFinalizers();
    Console.WriteLine("   🧽 Garbage Collection executado");
}

static async Task DemonstrarRateLimiting()
{
    var rateLimiter = new SimpleRateLimiter(5, TimeSpan.FromSeconds(10)); // 5 requests per 10 seconds
    
    Console.WriteLine("🚦 Testando Rate Limiting (5 requests/10s):");
    
    for (int i = 1; i <= 8; i++)
    {
        var allowed = await rateLimiter.TryAcquireAsync($"user123");
        var status = allowed ? "✅ PERMITIDO" : "❌ BLOQUEADO";
        Console.WriteLine($"   Request {i}: {status}");
        
        if (allowed)
        {
            await Task.Delay(50); // Simular processamento
        }
    }
    
    Console.WriteLine("\n   ⏰ Aguardando reset do rate limiter...");
    await Task.Delay(2000);
    
    var afterReset = await rateLimiter.TryAcquireAsync($"user123");
    Console.WriteLine($"   Request após reset: {(afterReset ? "✅ PERMITIDO" : "❌ BLOQUEADO")}");
}

// Classe simples para Rate Limiting
public class SimpleRateLimiter
{
    private readonly Dictionary<string, Queue<DateTime>> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly object _lock = new();

    public SimpleRateLimiter(int maxRequests, TimeSpan timeWindow)
    {
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
    }

    public async Task<bool> TryAcquireAsync(string key)
    {
        await Task.Yield(); // Simular operação assíncrona
        
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            if (!_requests.ContainsKey(key))
            {
                _requests[key] = new Queue<DateTime>();
            }
            
            var userRequests = _requests[key];
            
            // Remove requests antigas
            while (userRequests.Count > 0 && now - userRequests.Peek() > _timeWindow)
            {
                userRequests.Dequeue();
            }
            
            // Verifica se pode fazer nova request
            if (userRequests.Count < _maxRequests)
            {
                userRequests.Enqueue(now);
                return true;
            }
            
            return false;
        }
    }
}

// Classe para demonstrar criptografia de dados estruturados
public record UserData(string Name, string Email, string Phone);

public static class SecureDataProcessor
{
    public static string EncryptUserData(UserData userData, byte[] key)
    {
        var json = JsonSerializer.Serialize(userData);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(jsonBytes, 0, jsonBytes.Length);
        
        // Combinar IV + dados criptografados
        var result = new byte[aes.IV.Length + encrypted.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        
        return Convert.ToBase64String(result);
    }
    
    public static UserData DecryptUserData(string encryptedData, byte[] key)
    {
        var data = Convert.FromBase64String(encryptedData);
        
        using var aes = Aes.Create();
        aes.Key = key;
        
        // Extrair IV
        var iv = new byte[16];
        var encrypted = new byte[data.Length - 16];
        Array.Copy(data, 0, iv, 0, 16);
        Array.Copy(data, 16, encrypted, 0, encrypted.Length);
        
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        
        var json = Encoding.UTF8.GetString(decrypted);
        return JsonSerializer.Deserialize<UserData>(json)!;
    }
}

// Interface para auditoria de segurança
public interface ISecurityAuditor
{
    void LogSecurityEvent(string eventType, string details);
    Task<bool> IsUserAuthorizedAsync(string userId, string action);
}

public class ConsoleSecurityAuditor : ISecurityAuditor
{
    public void LogSecurityEvent(string eventType, string details)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        Console.WriteLine($"🔍 [{timestamp}] {eventType}: {details}");
    }
    
    public async Task<bool> IsUserAuthorizedAsync(string userId, string action)
    {
        await Task.Delay(10); // Simular verificação de banco
        
        // Simulação simples de autorização
        var authorized = userId == "admin" || (userId == "user123" && action != "delete");
        
        LogSecurityEvent("AUTHORIZATION", 
            $"User {userId} {(authorized ? "GRANTED" : "DENIED")} for action {action}");
        
        return authorized;
    }
}
