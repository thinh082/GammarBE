using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using System.Text.Json;

namespace GammarBE.Services
{
    public class GenerationService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GenerationService> _logger;

        private const string GommoApiUrl = "https://api.gommo.net/ai/generateImage";

        public GenerationService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GenerationService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<dynamic> GenerateImg(GenerateImgDTO dto)
        {
            var accessToken = _configuration["Vmeadi:access_token"]
                ?? throw new InvalidOperationException("Vmeadi:access_token chưa được cấu hình trong appsettings.");

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
                throw new Exception($"Gommo API trả về body rỗng. HTTP Status: {(int)response.StatusCode}");

            try
            {
                var result = JsonSerializer.Deserialize<dynamic>(rawBody);
                return result!;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Không parse được JSON. Raw: {Body}", rawBody);
                throw new Exception($"Gommo API trả về response không hợp lệ. HTTP {(int)response.StatusCode}. Raw: {rawBody}", ex);
            }
            //
        }
    }
}
