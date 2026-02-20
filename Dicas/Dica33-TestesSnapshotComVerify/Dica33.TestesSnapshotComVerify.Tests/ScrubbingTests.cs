using Dica33.TestesSnapshotComVerify;
using Xunit;

namespace Dica33.TestesSnapshotComVerify.Tests;

public class ScrubbingTests
{
    [Fact]
    public Task ShouldScrubDynamicTimestamps()
    {
        // Arrange
        var logEntry = new
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            Level = "INFO",
            Message = "User logged in successfully",
            UserId = 123,
            SessionId = Guid.NewGuid().ToString(),
            RequestId = $"req_{DateTime.Now.Ticks}",
            Properties = new Dictionary<string, object>
            {
                ["UserAgent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                ["IpAddress"] = "192.168.1.100",
                ["LoginAttempt"] = 1,
                ["PreviousLogin"] = DateTime.Now.AddDays(-1),
                ["DeviceFingerprint"] = Guid.NewGuid().ToString()
            }
        };

        // Act & Assert
        return Verify(logEntry)
            .ScrubMembers("Id", "Timestamp", "SessionId", "RequestId", "PreviousLogin", "DeviceFingerprint")
            .ScrubMembersWithType<Guid>();
    }

    [Fact]
    public Task ShouldScrubSensitiveData()
    {
        // Arrange
        var userProfile = new
        {
            Id = 123,
            Username = "joao.silva",
            Email = "joao.silva@email.com",
            PasswordHash = "d1e8a70b5ccab1dc2f56bbf7e99f064a660c08e361a35751b9c483c88943d082",
            Salt = "a1b2c3d4e5f6",
            PersonalInfo = new
            {
                FullName = "João da Silva Santos",
                CPF = "123.456.789-00",
                Phone = "+55 11 99999-9999",
                BirthDate = new DateTime(1990, 5, 15),
                Address = new
                {
                    Street = "Rua das Flores, 123",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            },
            PaymentInfo = new
            {
                CreditCards = new[]
                {
                    new
                    {
                        Id = 1,
                        LastFourDigits = "1234",
                        Brand = "Visa",
                        ExpiryMonth = 12,
                        ExpiryYear = 2025,
                        Token = "tok_1234567890abcdef"
                    }
                },
                DefaultPaymentMethod = "credit_card_1"
            },
            ApiKeys = new[]
            {
                new { Name = "main_api_key", Key = "sk_live_1234567890abcdef", CreatedAt = DateTime.Now }
            }
        };

        // Act & Assert
        return Verify(userProfile)
            .ScrubMembers("PasswordHash", "Salt", "CPF", "Phone", "Token", "Key")
            .ScrubMembersWithType<DateTime>();
    }

    [Fact]
    public Task ShouldScrubEnvironmentSpecificData()
    {
        // Arrange
        var systemInfo = new
        {
            Environment = "Production",
            Server = new
            {
                Hostname = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                WorkingDirectory = Environment.CurrentDirectory,
                UserName = Environment.UserName,
                OSVersion = Environment.OSVersion.ToString(),
                MemoryUsage = GC.GetTotalMemory(false),
                UpTime = TimeSpan.FromMilliseconds(Environment.TickCount64)
            },
            Database = new
            {
                ConnectionString = "Server=prod-db.internal.com;Database=ecommerce;User=app_user;Password=secret123",
                ActiveConnections = 45,
                LastBackup = DateTime.Now.AddHours(-6),
                Version = "PostgreSQL 15.2"
            },
            Cache = new
            {
                RedisHost = "redis-cluster.internal.com:6379",
                HitRate = 89.5,
                UsedMemory = "2.1GB",
                LastFlush = DateTime.Now.AddDays(-1)
            },
            Monitoring = new
            {
                ApplicationInsightsKey = "abc123def456ghi789",
                DatadogApiKey = "dd_api_key_987654321",
                NewRelicLicense = "nr_license_abcdef123456"
            }
        };

        // Act & Assert
        return Verify(systemInfo)
            .ScrubMembers(
                "Hostname", "ProcessId", "WorkingDirectory", "UserName", 
                "MemoryUsage", "UpTime", "ActiveConnections", "LastBackup", 
                "LastFlush", "ApplicationInsightsKey", "DatadogApiKey", "NewRelicLicense"
            )
            .ScrubLinesContaining("Password=", "ConnectionString");
    }

    [Fact]
    public Task ShouldScrubUsingCustomScrubber()
    {
        // Arrange
        var apiResponse = new
        {
            RequestId = "req_abc123def456",
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                Users = new[]
                {
                    new
                    {
                        Id = 1,
                        Email = "user1@domain.com",
                        Profile = new
                        {
                            IpAddress = "192.168.1.100",
                            LastSeen = DateTime.Now.AddMinutes(-30),
                            DeviceId = "device_xyz789"
                        }
                    },
                    new
                    {
                        Id = 2,
                        Email = "user2@domain.com", 
                        Profile = new
                        {
                            IpAddress = "10.0.0.50",
                            LastSeen = DateTime.Now.AddHours(-2),
                            DeviceId = "device_abc123"
                        }
                    }
                }
            },
            Debug = new
            {
                ExecutionTime = TimeSpan.FromMilliseconds(147),
                SqlQueries = new[]
                {
                    "SELECT * FROM users WHERE created_at > '2024-01-15 10:30:00'",
                    "UPDATE user_sessions SET last_activity = '2024-01-15 11:45:22' WHERE user_id = 1"
                },
                CacheHits = new Dictionary<string, object>
                {
                    ["user:1"] = $"hit_at_{DateTime.Now.Ticks}",
                    ["user:2"] = $"miss_at_{DateTime.Now.Ticks}"
                }
            }
        };

        // Act & Assert
        return Verify(apiResponse)
            .ScrubMembers("RequestId", "ExecutionTime")
            .ScrubLinesContaining("'2024-", "hit_at_", "miss_at_")
            .ScrubMembersWithType<DateTime>()
            .ScrubMembersWithType<TimeSpan>();
    }

    [Fact] 
    public Task ShouldScrubRegexPatterns()
    {
        // Arrange
        var securityLog = new
        {
            Event = "failed_login_attempt",
            Details = new
            {
                Username = "admin",
                IpAddress = "203.0.113.42",
                UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36",
                Timestamp = DateTime.Now,
                SessionId = "sess_1a2b3c4d5e6f7g8h9i0j",
                AttemptNumber = 3,
                MaxAttempts = 5,
                LockoutUntil = DateTime.Now.AddMinutes(15),
                PreviousAttempts = new[]
                {
                    new { Time = DateTime.Now.AddMinutes(-5), Ip = "203.0.113.42" },
                    new { Time = DateTime.Now.AddMinutes(-10), Ip = "203.0.113.42" },
                    new { Time = DateTime.Now.AddMinutes(-15), Ip = "198.51.100.123" }
                }
            },
            SecurityHeaders = new Dictionary<string, string>
            {
                ["X-Forwarded-For"] = "203.0.113.42, 198.51.100.1",
                ["X-Real-IP"] = "203.0.113.42",
                ["X-Request-ID"] = "req_abcdef123456789",
                ["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            },
            GeoLocation = new
            {
                Country = "BR",
                Region = "SP", 
                City = "São Paulo",
                Latitude = -23.5505,
                Longitude = -46.6333,
                ISP = "Example ISP Ltd"
            }
        };

        // Act & Assert
        return Verify(securityLog)
            .ScrubMembersWithType<DateTime>()
            .ScrubLinesContaining("192.168", "10.0", "172.16", "sess_", "req_", "Bearer ", "40.74", "-73.98");
    }

    [Fact]
    public Task ShouldScrubNestedComplexData()
    {
        // Arrange
        var auditTrail = new
        {
            AuditId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Action = "user_data_export",
            Actor = new
            {
                UserId = 123,
                Email = "admin@company.com",
                Role = "DataProtectionOfficer",
                SessionId = Guid.NewGuid(),
                IpAddress = "10.0.1.50"
            },
            Target = new
            {
                UserId = 456,
                Email = "customer@email.com",
                DataRequested = new[]
                {
                    "personal_information",
                    "order_history", 
                    "payment_methods",
                    "browsing_history"
                }
            },
            Result = new
            {
                Success = true,
                ExportId = Guid.NewGuid(),
                FilePath = $"/exports/user_456_export_{DateTime.Now:yyyyMMdd_HHmmss}.zip",
                FileSize = 2547836,
                ExpiresAt = DateTime.Now.AddDays(30),
                DownloadToken = $"dl_{Guid.NewGuid().ToString("N")[..16]}"
            },
            Compliance = new
            {
                GDPRCompliant = true,
                LGPDCompliant = true,
                RetentionPeriod = TimeSpan.FromDays(30),
                EncryptionUsed = "AES-256-GCM",
                AccessLog = new[]
                {
                    new { Time = DateTime.Now, Action = "file_created", User = "system" },
                    new { Time = DateTime.Now.AddSeconds(5), Action = "encryption_applied", User = "system" },
                    new { Time = DateTime.Now.AddSeconds(10), Action = "token_generated", User = "system" }
                }
            },
            Metadata = new
            {
                ProcessingTime = TimeSpan.FromSeconds(12.5),
                ServerInstance = Environment.MachineName,
                RequestCorrelationId = Guid.NewGuid(),
                BatchJobId = $"batch_{DateTime.Now.Ticks}"
            }
        };

        // Act & Assert
        return Verify(auditTrail)
            .ScrubMembersWithType<Guid>()
            .ScrubMembersWithType<DateTime>()
            .ScrubMembersWithType<TimeSpan>()
            .ScrubMembers("FilePath", "DownloadToken", "ServerInstance", "BatchJobId")
            .ScrubLinesContaining("dl_", "batch_", "exports/");
    }
}
