using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs
{
    public class UserFullInfoDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? RevokedOn { get; set; }
        public string? RevokedBy { get; set; }
        public bool IsActive { get; set; }
    }
}