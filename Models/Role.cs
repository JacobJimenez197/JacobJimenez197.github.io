using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PlataformaAPI.Models
{
    public class Role : IdentityRole<int>
    {
        [Required]
        [StringLength(20)]
        public override string Name { get; set; } = string.Empty;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class RoleCreateDto
    {
        [Required]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleUpdateDto
    {
        [Required]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty;
    }
}