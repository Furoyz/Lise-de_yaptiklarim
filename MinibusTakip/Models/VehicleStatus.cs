using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class VehicleStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        // Şoförün son bildirdiği durak
        public int? SonDurakId { get; set; }
        [ForeignKey(nameof(SonDurakId))]
        public Stop? SonDurak { get; set; }

        // Şoförün gitmekte olduğu durak
        public int? HedefDurakId { get; set; }
        [ForeignKey(nameof(HedefDurakId))]
        public Stop? HedefDurak { get; set; }

        // Örn: "Yolda", "Molada", "Arıza"
        public string? Durum { get; set; }

        public DateTime SonGuncellemeZamani { get; set; } = DateTime.UtcNow;
    }
}