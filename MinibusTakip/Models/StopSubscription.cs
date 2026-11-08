using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinibusTakip.Models
{
    public class StopSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public int StopId { get; set; }
        [ForeignKey(nameof(StopId))]
        public Stop? Stop { get; set; }

        public DateTime OlusturmaZamani { get; set; } = DateTime.UtcNow;
    }
}