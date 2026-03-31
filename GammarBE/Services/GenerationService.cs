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
                var result = JsonSerializer.Deserialize<JsonElement>(rawBody);

                // Kiểm tra kết quả từ Gommo API
                if (result.TryGetProperty("code", out var codeProp) && codeProp.GetInt32() == 200)
                {
                    // Lấy URL ảnh từ response (Giả sử cấu trúc là result.data.url hoặc result.data)
                    // Cần kiểm tra thực tế cấu trúc JSON của Gommo API.
                    // Ở đây tôi sẽ cố gắng lấy URL từ data property.
                    string imageUrl = "";
                    if (result.TryGetProperty("data", out var dataProp))
                    {
                        if (dataProp.ValueKind == JsonValueKind.String)
                        {
                            imageUrl = dataProp.GetString() ?? "";
                        }
                        else if (dataProp.ValueKind == JsonValueKind.Object && dataProp.TryGetProperty("url", out var urlProp))
                        {
                            imageUrl = urlProp.GetString() ?? "";
                        }
                    }

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Lưu vào Database
                        await SaveGenerationToDb(dto, imageUrl);
                    }
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Không parse được JSON. Raw: {Body}", rawBody);
                throw new Exception($"Gommo API trả về response không hợp lệ. HTTP {(int)response.StatusCode}. Raw: {rawBody}", ex);
            }
        }

        private string GetFileExtension(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                return Path.GetExtension(path).Replace(".", "");
            }
            catch
            {
                return "png"; // Mặc định nếu lỗi parse URL
            }
        }

        private async Task SaveGenerationToDb(GenerateImgDTO dto, string imageUrl)
        {
            // Lấy UserId từ Claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out Guid parsedGuid))
            {
                userId = parsedGuid;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo bản ghi Generation
                var generation = new Generation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Prompt = dto.Prompt,
                    Model = dto.Model_ID,
                    Params = JsonSerializer.Serialize(new { ratio = dto.Ratio }),
                    CreatedAt = DateTime.UtcNow,
                    Url = imageUrl // Lưu URL trực tiếp vào Generation nếu cần hoặc chỉ lưu trong MediaAsset
                };

                _context.Generations.Add(generation);

                // 2. Tạo bản ghi MediaAsset
                var mediaAsset = new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    GenId = generation.Id,
                    UserId = userId,
                    FileUrl = imageUrl,
                    AssetType = "Image",
                    Extension = GetFileExtension(imageUrl),
                    Dimension = dto.Ratio
                };

                _context.MediaAssets.Add(mediaAsset);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Đã lưu lịch sử tạo ảnh cho user {UserId}, GenerationId: {GenId}", userId, generation.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi lưu lịch sử tạo ảnh vào Database");
                // Không throw exception ở đây để tránh làm gián đoạn phản hồi cho client nếu chỉ là lỗi lưu history
            }
        }
    }
}
