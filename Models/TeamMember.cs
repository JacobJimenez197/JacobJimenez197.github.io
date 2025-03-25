using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaAPI.Models
{
    public class TeamMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
    public class CreateTeamMemberDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
    public class TeamMemberResponseDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}