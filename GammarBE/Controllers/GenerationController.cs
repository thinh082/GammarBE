using GammarBE.Models.DTO;
using GammarBE.Services;
using Microsoft.AspNetCore.Mvc;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerationController : ControllerBase
    {
        private readonly GenerationService _generationService;

        public GenerationController(GenerationService generationService)
        {
            _generationService = generationService;
        }

      
        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] GenerateImgDTO dto)
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

            try
            {
                var result = await _generationService.GenerateImg(dto);
                return Ok(new
                {
                    code = 200,
                    message = "Đã gửi yêu cầu tạo ảnh, đang xử lý...",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = 500,
                    message = "Lỗi khi gọi API tạo ảnh",
                    error = ex.Message
                });
            }
        }
    }
}
