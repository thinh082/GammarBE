using BCrypt.Net;
using GammarBE.Models.Entities;
using GammarBE.Models.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GammarBE.Services
{
    public class CommonServices
    {
        private readonly IConfiguration _configuration;
        public CommonServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<dynamic> GuiEmail(string Email, string TieuDe, string NoiDung)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return new
                {
                    statusCode = 400,
                    message = "Email không được để trống!"
                };
            }
            var emailSetting = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
            if (emailSetting == null) 
            {
                return new
                {
                    statusCode = 400,
                    message = "Lỗi hệ thống!"
                };
            }
            try
            {
                // lấy thông tin email gửi
                var email = emailSetting?.SmtpUsername;
                var password = emailSetting?.SmtpPassword;
                var host = "smtp.gmail.com";
                var port = 587;

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email, password)
                };

                var message = new MailMessage(email!, Email, TieuDe, NoiDung);
                message.IsBodyHtml = true;
                await smtpClient.SendMailAsync(message);

                return new
                {
                    statusCode = 200,
                    message = "Gửi đường dẫn thay đổi mật khẩu vui lòng kiểm tra Email để thực hiện thay đổi mật khẩu!"
                };
            }
            catch (Exception ex)
            {
                // Log ra console (có thể thay bằng logger hoặc lưu DB)
                Console.WriteLine($"[GuiEmail] Lỗi gửi email: {ex}");

                return new
                {
                    statusCode = 500,
                    message = $"Gửi thất bại! Lý do: {ex.Message}"
                };
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "1440")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string Encrypt(string plainText, string base64PublicKey)
        {
            byte[] publicKeyBytes = Convert.FromBase64String(base64PublicKey);
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                byte[] encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Decrypt(string encryptedData, string base64PrivateKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] privateKeyBytes = Convert.FromBase64String(base64PrivateKey);
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                byte[] decrypted = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decrypted);
            }
        }
    }
}
