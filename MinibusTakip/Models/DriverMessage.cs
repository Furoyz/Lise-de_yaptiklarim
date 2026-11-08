using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class DriverMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        public int? StopId { get; set; }
        [ForeignKey(nameof(StopId))]
        public Stop? Stop { get; set; }

        [Required]
        [StringLength(500)]
        public string Mesaj { get; set; } = string.Empty;

        public DateTime Zaman { get; set; } = DateTime.UtcNow;
    }
}