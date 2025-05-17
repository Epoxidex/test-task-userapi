namespace UserAPI.Models.DTOs
{
    public class UserDto
    {
        public string Login { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
    }
}
