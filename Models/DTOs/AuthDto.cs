using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs
{
    public class AuthDto
    {
        [Required]
        public string Login { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
