using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        [Required]
        public int StopId { get; set; }
        [ForeignKey(nameof(StopId))]
        public Stop? Stop { get; set; }

        // Normal planlanan saat
        [Required]
        public TimeSpan Saat { get; set; }
    }
}