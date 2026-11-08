using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class DelayUpdate
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

        // Örn: "14:20 → 14:30" veya "+10 dk"
        public string? Aciklama { get; set; }

        public int? GecikmeDakika { get; set; }

        public DateTime Zaman { get; set; } = DateTime.UtcNow;
    }
}