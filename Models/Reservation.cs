using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaAPI.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ReservationMaterial>? ReservationMaterials { get; set; }
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }


        [ForeignKey("Subject")]
        public int? SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        [ForeignKey("Group")]
        public int? GroupId { get; set; }
        public virtual Group? Group { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
    public enum ReservationStatus
    {
        Pending,   
        Confirmed,  
        Cancelled, 
        Completed 
    }
    public class CreateReservationDto
    {
        [Required]
        public int UserId { get; set; }

        public int? SubjectId { get; set; }

        public int? GroupId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; }
    }
    public class UpdateReservationDto
    {
        public int? SubjectId { get; set; }

        public int? GroupId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Status { get; set; }

        [StringLength(500)]
        public string? Purpose { get; set; }
    }
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}