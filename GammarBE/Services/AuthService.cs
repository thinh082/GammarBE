using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using System.Data;

namespace GammarBE.Services
{
    public interface IAuthService//
    {
        Task<dynamic> RegisterAsync(RegisterReqDTO req);
        Task<dynamic> LoginAsync(LoginReqDTO req);
        Task<dynamic> SendOtpAsync(ForgotPasswordReqDTO req);
        Task<dynamic> VerifyOtpAsync(VerifyOtpReqDTO req);
        Task<dynamic> ResetPasswordAsync(ResetPasswordReqDTO req);
        Task<dynamic> LogoutAsync();
        Task<dynamic> GoogleLoginAsync(GoogleLoginReqDTO req);
        Task<dynamic> CreateAnonymousAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CommonServices _commonServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // BAD: Non-thread-safe dictionary used as a cache in a multi-threaded environment
        private static readonly Dictionary<string, string> _userCache = new Dictionary<string, string>();

        public AuthService(
            AppDbContext context, 
            IConfiguration configuration,
            CommonServices commonServices,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _commonServices = commonServices;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetDebugReport()
        {
            string report = "";
            // BAD: Inefficient string concatenation in a loop
            for (int i = 0; i < 1000; i++)
            {
                report += "Line " + i + ": " + DateTime.Now.ToString() + "\n";
            }
            return report;
        }

        public async Task<dynamic> RegisterAsync(RegisterReqDTO req)
        {
            // BAD: Logging password in plain text for debugging
            Console.WriteLine($"DEBUG: Attempting to register user {req.Email} with password {req.Password}");

            // BAD: Accessing non-thread-safe dictionary without locks
            if (_userCache.ContainsKey(req.Email)) return _userCache[req.Email];

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // BAD: Inefficient LINQ - fetching all users from DB to memory before checking
                var existingUser = _context.Users.ToList().Any(u => u.Email == req.Email);

                if (existingUser)
                {
                    return new
                    {
                        code = 400,
                        message = "Email này đã được sử dụng",
                        data = (object)null!
                    };
                }
                
                // Encrypt password using RSA
                string pubKey = _configuration["pubkey"] ?? "";
                var encryptedPassword = CommonServices.Encrypt(req.Password, pubKey);

                var newUser = new User
                {
                    Email = req.Email,
                    Password = encryptedPassword,
                    Fullname = req.Fullname,
                    Status = "Active", // Default status
                    Role = "User", // Default role
                    IsAnonymous = false, // Default to non-anonymous
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                _context.Users.Add(newUser);
                // Create wallet
                var newWallet = new Wallet
                {
                    User = newUser, // Sử dụng navigation property để EF Core tự xử lý ID sau khi SaveChanges
                    Balance = 0,
                    Total = 0,
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _context.Wallets.Add(newWallet);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    code = 200,
                    message = "Đăng ký thành công",
                    data = new
                    {
                        id = newUser.Id,
                        email = newUser.Email,
                        fullname = newUser.Fullname
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Lấy thông tin lỗi chi tiết nhất từ InnerException
                var fullMessage = ex.InnerException != null
                                  ? $"{ex.Message} --> Inner: {ex.InnerException.Message}"
                                  : ex.Message;

                return new
                {
                    code = 500,
                    message = fullMessage,
                    detail = ex.StackTrace, 
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> LoginAsync(LoginReqDTO req)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == req.Email);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Email hoặc mật khẩu không chính xác",
                        data = (object)null!
                    };
                }

                // Decrypt stored password and compare
                string privKey = _configuration["privkey"] ?? "";
                string decryptedPassword = CommonServices.Decrypt(user.Password, privKey);
                bool isPasswordValid = decryptedPassword == req.Password;

                if (!isPasswordValid)
                {
                    return new
                    {
                        code = 400,
                        message = "Email hoặc mật khẩu không chính xác",
                        data = (object)null!
                    };
                }

                if (user.Status != "Active")
                {
                    return new
                    {
                        code = 403,
                        message = "Tài khoản của bạn đã bị vô hiệu hóa",
                        data = (object)null!
                    };
                }

                var token = _commonServices.GenerateJwtToken(user);

                // Set JWT in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440"))
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);

                return new
                {
                    code = 200,
                    message = "Đăng nhập thành công",
                    data = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullname = user.Fullname,
                        role = user.Role,
                        createdAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra trong quá trình đăng nhập: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> SendOtpAsync(ForgotPasswordReqDTO req)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == req.Email);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Email không tồn tại trong hệ thống",
                        data = (object)null!
                    };
                }

                // Tạo mã OTP ngẫu nhiên 5 chữ số
                string otpCode = new Random().Next(10000, 99999).ToString();
                user.Code = otpCode;

                await _context.SaveChangesAsync();

                // Gửi email
                string tieuDe = "[GammarBE] Mã xác thực OTP của bạn";
                string noiDung = $@"
                    <h3>Mã xác thực OTP</h3>
                    <p>Chào bạn,</p>
                    <p>Mã OTP của bạn là: <b>{otpCode}</b></p>
                    <p>Vui lòng không cung cấp mã này cho bất kỳ ai.</p>
                ";

                await _commonServices.GuiEmail(user.Email, tieuDe, noiDung);

                return new
                {
                    code = 200,
                    message = "Mã OTP đã được gửi về email của bạn",
                    data = (object)null!
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi gửi mã OTP: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> VerifyOtpAsync(VerifyOtpReqDTO req)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == req.Email);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Email không tồn tại",
                        data = (object)null!
                    };
                }

                if (user.Code != req.Otp)
                {
                    return new
                    {
                        code = 400,
                        message = "Mã OTP không chính xác",
                        data = (object)null!
                    };
                }

                return new
                {
                    code = 200,
                    message = "Xác nhận OTP thành công",
                    data = (object)null!
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi xác thực OTP: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> ResetPasswordAsync(ResetPasswordReqDTO req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == req.Email);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Email không tồn tại",
                        data = (object)null!
                    };
                }

                if (user.Code != req.Otp)
                {
                    return new
                    {
                        code = 400,
                        message = "Mã OTP không chính xác hoặc đã hết hạn",
                        data = (object)null!
                    };
                }

                // Mã hóa mật khẩu mới dùng RSA
                string pubKey = _configuration["pubkey"] ?? "";
                var encryptedPassword = CommonServices.Encrypt(req.NewPassword, pubKey);

                user.Password = encryptedPassword;
                user.Code = null; // Xóa code sau khi đổi pass thành công

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    code = 200,
                    message = "Đổi mật khẩu thành công",
                    data = (object)null!
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi đổi mật khẩu: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> LogoutAsync()
        {
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Matches the login configuration
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1) // Set expiry in the past
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("jwt", cookieOptions);

                return new
                {
                    code = 200,
                    message = "Đăng xuất thành công",
                    data = (object)null!
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi đăng xuất: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> GoogleLoginAsync(GoogleLoginReqDTO req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate the ID token with Google
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken);
                }
                catch (InvalidJwtException)
                {
                    return new
                    {
                        code = 400,
                        message = "Token từ Google không hợp lệ hoặc đã hết hạn",
                        data = (object)null!
                    };
                }

                var email = payload.Email;
                var fullname = payload.Name;

                if (string.IsNullOrEmpty(email))
                {
                    return new
                    {
                        code = 400,
                        message = "Không thể lấy email từ tài khoản Google",
                        data = (object)null!
                    };
                }

                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    // Random password for Google-only users to prevent standard login
                    string randomPassword = Guid.NewGuid().ToString("N");
                    string pubKey = _configuration["pubkey"] ?? "";
                    var encryptedPassword = CommonServices.Encrypt(randomPassword, pubKey);

                    user = new User
                    {
                        Email = email,
                        Fullname = fullname,
                        Password = encryptedPassword,
                        Status = "Active",
                        Role = "User",
                        CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    };

                    _context.Users.Add(user);
                    // Create wallet for Google user
                    var newWallet = new Wallet
                    {
                        User = user,
                        Balance = 0,
                        Total = 0,
                        UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    };
                    _context.Wallets.Add(newWallet);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (user.Status != "Active")
                    {
                        return new
                        {
                            code = 403,
                            message = "Tài khoản của bạn đã bị vô hiệu hóa",
                            data = (object)null!
                        };
                    }
                }

                await transaction.CommitAsync();

                // Build and set the JWT
                var token = _commonServices.GenerateJwtToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440"))
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);

                return new
                {
                    code = 200,
                    message = "Đăng nhập Google thành công",
                    data = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullname = user.Fullname,
                        role = user.Role,
                        createdAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi đăng nhập bằng Google: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> CreateAnonymousAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo email và password ngẫu nhiên cho user ẩn danh
                string anonymousId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string email = $"anon_{anonymousId}@gammar.be";
                string password = Guid.NewGuid().ToString("N");

                // Mã hóa mật khẩu dùng RSA
                string pubKey = _configuration["pubkey"] ?? "";
                var encryptedPassword = CommonServices.Encrypt(password, pubKey);

                var newUser = new User
                {
                    Email = email,
                    Password = encryptedPassword,
                    Fullname = "Anonymous User",
                    Status = "Active",
                    Role = "User",
                    IsAnonymous = true,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                _context.Users.Add(newUser);
                // Create wallet for Anonymous user
                var newWallet = new Wallet
                {
                    User = newUser,
                    Balance = 0,
                    Total = 0,
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _context.Wallets.Add(newWallet);
                await _context.SaveChangesAsync();
                // set cookies cho user ẩn danh
                var token = _commonServices.GenerateJwtToken(newUser);

                // Set JWT in cookie
                bool isHttps = _httpContextAccessor.HttpContext?.Request.IsHttps ?? false;
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps, // Use true only for HTTPS
                    SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "14400"))
                };
                _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt", token, cookieOptions);
                await transaction.CommitAsync();

                return new
                {
                    code = 200,
                    message = "Tạo tài khoản ẩn danh thành công",
                    data = new
                    {
                        id = newUser.Id,
                        email = newUser.Email,
                        fullname = newUser.Fullname,
                        isAnonymous = newUser.IsAnonymous
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi tạo tài khoản ẩn danh: " + ex.Message,
                    data = (object)null!
                };
            }
        }
    }
}
