using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaAPI.Models
{
    public class ReservationMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        [Required]
        [ForeignKey("Material")]
        public int MaterialId { get; set; }
        public virtual Material Material { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int ReturnedQuantity { get; set; } = 0;

        [Required]
        public ReservationMaterialStatus Status { get; set; } = ReservationMaterialStatus.Reserved;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
    public enum ReservationMaterialStatus
    {
        Reserved,  
        Returned, 
        Damaged  
    }
    public class CreateReservationMaterialDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
    public class UpdateReservationMaterialDto
    {
        [Range(0, int.MaxValue)]
        public int? ReturnedQuantity { get; set; }

        public string? Status { get; set; }
    }
    public class ReservationMaterialResponseDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public int ReturnedQuantity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}