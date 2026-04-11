using GammarBE.Services;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace GammarBE.Tests
{
    public class CommonServicesTests
    {
        [Fact]
        public void Encrypt_And_Decrypt_Should_Return_Original_Text()
        {
            // Arrange
            string originalText = "Hello World";

            using var rsa = RSA.Create();
            string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            string privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());

            // Act
            string encrypted = CommonServices.Encrypt(originalText, publicKey);
            string decrypted = CommonServices.Decrypt(encrypted, privateKey);

            // Assert
            Assert.Equal(originalText, decrypted);
        }
    }
}
