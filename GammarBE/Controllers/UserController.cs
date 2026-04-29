using GammarBE.Models.DTO;
using GammarBE.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            // BAD: Returning full entities from DB to API without DTO (Data Exposure)
            // This might include PasswordHash, Salt, etc.
            var users = await _userService.SearchUsersByName(""); 
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            // BAD: IDOR (Insecure Direct Object Reference)
            // No check if the requester has permission to see this specific user's profile
            var result = await _userService.GetProfileAsync(id);
            return Ok(result);
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { code = 401, message = "Không thể xác thực người dùng", data = (object)null! });
            }

            var result = await _userService.GetProfileAsync(userId);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            if (code == 404) return NotFound(result);
            return BadRequest(result);
        }

        [HttpPut("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileReqDTO req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { code = 401, message = "Không thể xác thực người dùng", data = (object)null! });
            }

            var result = await _userService.UpdateProfileAsync(userId, req);
            int code = (int)(result.GetType().GetProperty("code")?.GetValue(result, null) ?? 400);

            if (code == 200) return Ok(result);
            if (code == 404) return NotFound(result);
            if (code == 400) return BadRequest(result);
            return BadRequest(result);
        }
    }
}
