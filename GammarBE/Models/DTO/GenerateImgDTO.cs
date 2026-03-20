using System.ComponentModel.DataAnnotations;

namespace GammarBE.Models.DTO
{
    public class GenerateImgDTO
    {
        [Required(ErrorMessage ="Prompt không được để trống")]
        public string Prompt { get; set; } = null!;
        [Required(ErrorMessage = "Phải chọn loại ảnh")]
        public string Ratio { get; set; } = null!;
    }
}
