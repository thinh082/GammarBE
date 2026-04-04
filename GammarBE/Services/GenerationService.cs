using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace GammarBE.Services
{
    public interface IGenerationService
    {
        Task<dynamic> GenerateImg(GenerateImgDTO dto);
    }

    public class GenerationService : IGenerationService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GenerationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string GommoApiUrl = "https://api.gommo.net/ai/generateImage";

        public GenerationService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GenerationService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<dynamic> GenerateImg(GenerateImgDTO dto)
        {
            try
            {
                var accessToken = _configuration["Vmeadi:access_token"];
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return new { code = 500, message = "Vmeadi:access_token chưa được cấu hình trong appsettings." };
                }

                var formData = new List<KeyValuePair<string, string>>
                {
                    new("access_token", accessToken),
                    new("domain", "vmedia.ai"),
                    new("action_type", "create"),
                    new("model", dto.Model_ID),
                    new("prompt", dto.Prompt),
                    new("ratio", dto.Ratio),
                    new("project_id", "default"),
                };

                _logger.LogInformation("=== [GenerateImg] Gửi request tới Gommo API ===");
                _logger.LogInformation("URL: {Url}", GommoApiUrl);
                _logger.LogInformation("Params: model={Model}, prompt={Prompt}, ratio={Ratio}",
                    dto.Model_ID, dto.Prompt, dto.Ratio);

                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(GommoApiUrl, new FormUrlEncodedContent(formData));

                var rawBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("=== [GenerateImg] Response từ Gommo API ===");
                _logger.LogInformation("HTTP Status: {StatusCode}", (int)response.StatusCode);
                _logger.LogInformation("Raw Body: {Body}", rawBody);

                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    return new { code = 500, message = $"Gommo API trả về body rỗng. HTTP Status: {(int)response.StatusCode}" };
                }

            try
            {
                var result = JsonSerializer.Deserialize<dynamic>(rawBody);
                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong GenerateImg");
                return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
            }
            //
        }
    }
}
