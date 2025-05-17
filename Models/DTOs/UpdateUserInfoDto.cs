using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs
{
    public class UpdateUserInfoDto
    {
        [Required]
        public string Login { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Имя может содержать только русские и латинские буквы")]
        public string Name { get; set; } = null!;

        [Range(0, 2, ErrorMessage = "Пол должен быть: 0 - женщина, 1 - мужчина, 2 - неизвестно")]
        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }
    }
}
