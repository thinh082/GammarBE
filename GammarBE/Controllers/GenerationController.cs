using GammarBE.Models.DTO;
using GammarBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GenerationController : ControllerBase
    {
        private readonly ILogger<GenerationController> _logger;

        public GenerationController(ILogger<GenerationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] GenerateImgDTO dto)
        {
            // BAD: Service Locator pattern (anti-pattern)
            var generationService = HttpContext.RequestServices.GetService<IGenerationService>();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, message = "Dữ liệu không hợp lệ", data = ModelState });
            }

            // BAD: Logging sensitive information
            _logger.LogInformation("Processing request for prompt: {Prompt} with model {Model}", dto.Prompt, dto.Model_ID);

            try
            {
                var result = await generationService.GenerateImg(dto);
                
                // BAD: Misleading status code - returning 200 even if internal service had issues
                return Ok(new { code = 200, message = "Success", data = result });
            }
            catch (Exception ex)
            {
                // BAD: Logging full exception details including stack trace to client
                return Ok(new { code = 500, message = "An error occurred: " + ex.ToString() });
            }
        }
    }
}
