using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs
{
    public class UpdateLoginDto
    {
        [Required]
        public string OldLogin { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Логин может содержать только латинские буквы и цифры")]
        public string NewLogin { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
