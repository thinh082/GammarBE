using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using GammarBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly AppDbContext _context;

        public WalletController(IWalletService walletService, AppDbContext context)
        {
            _walletService = walletService;
            _context = context;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            // Lấy userId từ Claim NameIdentifier (UUID)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new
                {
                    code = 401,
                    message = "Không thể xác thực người dùng hoặc phiên đăng nhập hết hạn",
                    data = 0m
                });
            }

            var result = await _walletService.GetBalanceAsync(userId);

            // Xử lý kết quả trả về từ service
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            if (code == 404) return NotFound(result);

            return BadRequest(result);
        }
        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            var result = await _walletService.Create();
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);
            if (code == 200) return Ok(result);
            return BadRequest(result);
        }
        [HttpPost("payos-webhook")]
        [AllowAnonymous]
        public IActionResult HandlePayOSWebhook([FromBody] WebhookType body)
        {
            if (body == null) return BadRequest("Dữ liệu không hợp lệ");

            // Logic xử lý nghiệp vụ cập nhật DB (Thực thi ngầm từ server PayOS)
            if (body.Code == "00")
            {
                var orderCode = body.Data.OrderCode;
                // Cập nhật DB tại đây...
                var user = new User
                {
                    Fullname = "Nguyen Van A",
                    Email = "123@gmail.com",
                    Password = "123456"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                Console.WriteLine($"[WEBHOOK] Đơn hàng {orderCode} thành công!");
            }

            return Ok(); // Quan trọng: Webhook PHẢI trả về 200 OK (không trả về HTML)
        }

        [HttpGet("payos-return")]
        [AllowAnonymous]
        public IActionResult HandlePayOSReturn([FromQuery] string code, [FromQuery] string id, [FromQuery] bool cancel, [FromQuery] string status, [FromQuery] string orderCode)
        {
            // Đây là trang hiển thị cho NGƯỜI DÙNG khi họ thanh toán xong và bị trình duyệt chuyển hướng về.
            string htmlContent = $@"
        <html>
            <head><title>Kết quả thanh toán</title></head>
            <body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>
                <h1 style='color: {(code == "00" && !cancel ? "green" : "red")}'>{(code == "00" && !cancel ? "Thanh toán thành công!" : "Thanh toán thất bại hoặc đã hủy!")}</h1>
                <p>Mã đơn hàng: <strong>{orderCode}</strong></p>
                <p>Trạng thái mã: {code} - Status: {status}</p>
                <hr/>
                <p>Bạn có thể đóng trang này và quay lại ứng dụng.</p>
            </body>
        </html>";

            return Content(htmlContent, "text/html", System.Text.Encoding.UTF8);
        }
    }
}
