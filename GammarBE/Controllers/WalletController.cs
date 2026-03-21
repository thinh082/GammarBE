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

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
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
    }
}
