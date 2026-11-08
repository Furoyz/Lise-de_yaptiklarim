using System.ComponentModel.DataAnnotations;

namespace MinibusTakip.Models
{
    public class UserPushSubscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Endpoint { get; set; } = string.Empty;
        [Required]
        public string P256dh { get; set; } = string.Empty;
        [Required]
        public string Auth { get; set; } = string.Empty;
    }
}