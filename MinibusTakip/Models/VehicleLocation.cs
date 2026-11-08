using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class VehicleLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        public double Enlem { get; set; }
        public double Boylam { get; set; }
        public DateTime Zaman { get; set; } = DateTime.UtcNow;
    }
}