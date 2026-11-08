using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka zorunludur.")]
        [StringLength(20)]
        public string Plaka { get; set; } = string.Empty;

        // Atanan şoför (Identity kullanıcı Id'si)
        public string? SoforId { get; set; }

        [ForeignKey(nameof(SoforId))]
        public ApplicationUser? Sofor { get; set; }

        public bool AktifMi { get; set; } = true;

        public ICollection<Schedule> Tarifeler { get; set; } = new List<Schedule>();
    }
}