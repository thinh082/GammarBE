using System.ComponentModel.DataAnnotations;

namespace GammarBE.Models.DTO
{
    public class GenerateImgDTO
    {
        [Required(ErrorMessage = "Prompt không được để trống")]
        public string Prompt { get; set; } = null!;

        [Required(ErrorMessage = "Phải chọn loại ảnh (9_16 | 16_9 | 1_1)")]
        public string Ratio { get; set; } = null!;

        /// <summary>Model tạo ảnh. Mặc định: google_image_gen_3_5</summary>
        public string Model_ID { get; set; } = "google_image_gen_3_5";
    }
}
