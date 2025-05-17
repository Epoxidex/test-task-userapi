using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs
{
    public class UpdatePasswordDto
    {
        [Required]
        public string Login { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль может содержать только латинские буквы и цифры")]
        public string OldPassword { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль может содержать только латинские буквы и цифры")]
        public string NewPassword { get; set; } = null!;
    }
}
