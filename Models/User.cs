using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PlataformaAPI.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }

        [StringLength(20)]
        public string? Matricula { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    public class UserCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        [StringLength(20)]
        public string? Matricula { get; set; }
    }

    public class UserUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int RoleId { get; set; }

        [StringLength(20)]
        public string? Matricula { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}