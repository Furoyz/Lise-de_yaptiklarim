using System.ComponentModel.DataAnnotations;

namespace MinibusTakip.Models
{
    public class Stop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Durak adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        public int Sira { get; set; }

        public double? Enlem { get; set; }
        public double? Boylam { get; set; }

        public string? ResimUrl { get; set; }
    }
}