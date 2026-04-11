using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using GammarBE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using System.Reflection;

namespace GammarBE.Tests
{
    public class AuthServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public AuthServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
                return property.GetValue(obj)!;

            // For anonymous types, properties might be fields or just properties
            return obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj)!;
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            using var rsa = RSA.Create();
            string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            string privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());

            var inMemorySettings = new Dictionary<string, string> {
                {"privkey", privateKey},
                {"Jwt:Key", "SuperSecretKeyForTestingPurposesOnly123!"},
                {"Jwt:Issuer", "GammarBE"},
                {"Jwt:Audience", "GammarBEUsers"},
                {"Jwt:ExpiryMinutes", "1440"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            string password = "StrongPassword123";
            string encryptedPassword = CommonServices.Encrypt(password, publicKey);

            using (var context = new AppDbContext(_dbOptions))
            {
                context.Users.Add(new User
                {
                    Email = "test@example.com",
                    Password = encryptedPassword,
                    Fullname = "Test User",
                    Status = "Active",
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            var commonServices = new CommonServices(configuration);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context_for_accessor = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(context_for_accessor);

            using (var context = new AppDbContext(_dbOptions))
            {
                var authService = new AuthService(context, configuration, commonServices, mockHttpContextAccessor.Object);
                var loginReq = new LoginReqDTO
                {
                    Email = "test@example.com",
                    Password = password
                };

                // Act
                var result = await authService.LoginAsync(loginReq);

                // Assert
                Assert.Equal(200, (int)GetPropertyValue(result, "code"));
                Assert.Equal("Đăng nhập thành công", (string)GetPropertyValue(result, "message"));
                var data = GetPropertyValue(result, "data");
                Assert.NotNull(data);
                Assert.Equal("test@example.com", (string)GetPropertyValue(data, "email"));
            }
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsError()
        {
            // Arrange
            using var rsa = RSA.Create();
            string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            string privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());

            var inMemorySettings = new Dictionary<string, string> {
                {"privkey", privateKey}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            string password = "StrongPassword123";
            string encryptedPassword = CommonServices.Encrypt(password, publicKey);

            using (var context = new AppDbContext(_dbOptions))
            {
                context.Users.Add(new User
                {
                    Email = "test@example.com",
                    Password = encryptedPassword,
                    Status = "Active"
                });
                context.SaveChanges();
            }

            var commonServices = new CommonServices(configuration);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            using (var context = new AppDbContext(_dbOptions))
            {
                var authService = new AuthService(context, configuration, commonServices, mockHttpContextAccessor.Object);
                var loginReq = new LoginReqDTO
                {
                    Email = "test@example.com",
                    Password = "WrongPassword"
                };

                // Act
                var result = await authService.LoginAsync(loginReq);

                // Assert
                Assert.Equal(400, (int)GetPropertyValue(result, "code"));
                Assert.Equal("Email hoặc mật khẩu không chính xác", (string)GetPropertyValue(result, "message"));
            }
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();
            var commonServices = new CommonServices(configuration);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            using (var context = new AppDbContext(_dbOptions))
            {
                var authService = new AuthService(context, configuration, commonServices, mockHttpContextAccessor.Object);
                var loginReq = new LoginReqDTO
                {
                    Email = "nonexistent@example.com",
                    Password = "SomePassword"
                };

                // Act
                var result = await authService.LoginAsync(loginReq);

                // Assert
                Assert.Equal(404, (int)GetPropertyValue(result, "code"));
                Assert.Equal("Email hoặc mật khẩu không chính xác", (string)GetPropertyValue(result, "message"));
            }
        }

        [Fact]
        public async Task LoginAsync_UserDisabled_ReturnsError()
        {
            // Arrange
            using var rsa = RSA.Create();
            string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            string privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());

            var inMemorySettings = new Dictionary<string, string> {
                {"privkey", privateKey}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            string password = "StrongPassword123";
            string encryptedPassword = CommonServices.Encrypt(password, publicKey);

            using (var context = new AppDbContext(_dbOptions))
            {
                context.Users.Add(new User
                {
                    Email = "disabled@example.com",
                    Password = encryptedPassword,
                    Status = "Disabled"
                });
                context.SaveChanges();
            }

            var commonServices = new CommonServices(configuration);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            using (var context = new AppDbContext(_dbOptions))
            {
                var authService = new AuthService(context, configuration, commonServices, mockHttpContextAccessor.Object);
                var loginReq = new LoginReqDTO
                {
                    Email = "disabled@example.com",
                    Password = password
                };

                // Act
                var result = await authService.LoginAsync(loginReq);

                // Assert
                Assert.Equal(403, (int)GetPropertyValue(result, "code"));
                Assert.Equal("Tài khoản của bạn đã bị vô hiệu hóa", (string)GetPropertyValue(result, "message"));
            }
        }
    }
}
