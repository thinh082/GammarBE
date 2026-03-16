using System.ComponentModel.DataAnnotations;

namespace GammarBE.Models.DTO
{
    public class RegisterReqDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Fullname { get; set; } = null!;
    }

    public class LoginReqDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;
    }

    public class ForgotPasswordReqDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
    }

    public class VerifyOtpReqDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mã OTP không được để trống")]
        public string Otp { get; set; } = null!;
    }

    public class ResetPasswordReqDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mã OTP không được để trống")]
        public string Otp { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        public string NewPassword { get; set; } = null!;
    }

    public class UpdateProfileReqDTO
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Fullname { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
    }

    public class GoogleLoginReqDTO
    {
        [Required(ErrorMessage = "Mã xác thực không được để trống")]
        public string IdToken { get; set; } = null!;
    }
}
