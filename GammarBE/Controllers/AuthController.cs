using GammarBE.Models.DTO;
using GammarBE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    code = 400,
                    message = "Dữ liệu không hợp lệ",
                    data = ModelState
                });
            }

            var result = await _authService.RegisterAsync(req);
            
            if (result.GetType().GetProperty("code")?.GetValue(result, null)?.ToString() == "200")
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDTO req)
        {
            

            var result = await _authService.LoginAsync(req);

            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200)
            {
                return Ok(result);
            }
            else if (code == 404)
            {
                return NotFound(result);
            }
            else if (code == 403)
            {
                return StatusCode(403, result);
            }

            return BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            var result = await _authService.SendOtpAsync(req);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            if (code == 404) return NotFound(result);
            return BadRequest(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            var result = await _authService.VerifyOtpAsync(req);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            var result = await _authService.ResetPasswordAsync(req);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync();
            return Ok(result);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            var result = await _authService.GoogleLoginAsync(req);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            if (code == 403) return StatusCode(403, result);
            return BadRequest(result);
        }

        [HttpPost("create-anonymous")]
        public async Task<IActionResult> CreateAnonymous()
        {
            var result = await _authService.CreateAnonymousAsync();
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            return BadRequest(result);
        }
    }
}
