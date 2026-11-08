using Microsoft.AspNetCore.Identity;

namespace MinibusTakip.Models
{
    // IdentityUser'dan türetiyoruz; Email, PhoneNumber, PasswordHash gibi alanlar hazır gelir.
    // Biz sadece uygulamaya özel Ad ve Soyad ekliyoruz.
    public class ApplicationUser : IdentityUser
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
    }
}