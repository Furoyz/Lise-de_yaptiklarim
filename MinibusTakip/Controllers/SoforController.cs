using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinibusTakip.Data;
using MinibusTakip.Models;
using MinibusTakip.Services;
using MinibusTakip.ViewModels;

namespace MinibusTakip.Controllers
{
    [Authorize(Roles = UserRoles.Sofor)]
    public class SoforController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PushNotificationService _pushService;

        // TEK CONSTRUCTOR (Hem veritabanını hem push servisini alıyor)
        public SoforController(ApplicationDbContext db, UserManager<ApplicationUser> um, PushNotificationService pushService)
        {
            _db = db;
            _userManager = um;
            _pushService = pushService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var minibus = await _db.Vehicles
                .Include(v => v.Sofor)
                .FirstOrDefaultAsync(v => v.SoforId == user.Id);

            var vm = new SoforPanelViewModel
            {
                Minibus = minibus,
                TumDuraklar = await _db.Stops.OrderBy(s => s.Sira).ToListAsync()
            };

            if (minibus != null)
            {
                vm.Durum = await _db.VehicleStatuses
                    .Include(s => s.SonDurak)
                    .Include(s => s.HedefDurak)
                    .FirstOrDefaultAsync(s => s.VehicleId == minibus.Id);

                vm.Tarifeler = await _db.Schedules
                    .Include(s => s.Stop)
                    .Where(s => s.VehicleId == minibus.Id)
                    .OrderBy(s => s.Stop!.Sira)
                    .ToListAsync();

                vm.SonMesajlar = await _db.DriverMessages
                    .Include(m => m.Stop)
                    .Where(m => m.VehicleId == minibus.Id)
                    .OrderByDescending(m => m.Zaman)
                    .Take(5)
                    .ToListAsync();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GecikmeBildir(int dakika, int? stopId, string? aciklama)
        {
            var minibus = await KendiMinibusu();
            if (minibus == null) return Forbid();

            if (dakika < 0) dakika = 0;

            _db.DelayUpdates.Add(new DelayUpdate
            {
                VehicleId = minibus.Id,
                StopId = stopId,
                GecikmeDakika = dakika,
                Aciklama = string.IsNullOrWhiteSpace(aciklama) ? $"+{dakika} dk gecikme" : aciklama,
                Zaman = DateTime.UtcNow
            });

            await DokunDurum(minibus.Id);
            await _db.SaveChangesAsync();

            // BİLDİRİM GÖNDER
            var hedefKullanicilar = await _db.StopSubscriptions
                .Where(s => stopId == null || s.StopId == stopId)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            if (hedefKullanicilar.Any())
            {
                string baslik = $"🚐 {minibus.Plaka} Gecikme Bildirimi";
                string mesaj = string.IsNullOrWhiteSpace(aciklama)
                    ? $"Şoför {minibus.Plaka} plakalı araç için +{dakika} dk gecikme bildirdi."
                    : aciklama;

                await _pushService.SendToUsersAsync(hedefKullanicilar, baslik, mesaj);
            }

            TempData["basari"] = $"Gecikme bildirildi (+{dakika} dk).";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HedefDurakBelirle(int sonDurakId, int? hedefDurakId)
        {
            var minibus = await KendiMinibusu();
            if (minibus == null) return Forbid();

            var durum = await DurumGetirVeyaOlustur(minibus.Id);
            durum.SonDurakId = sonDurakId;
            durum.HedefDurakId = hedefDurakId;
            durum.SonGuncellemeZamani = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["basari"] = "Konum güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumGuncelle(string? durum)
        {
            var minibus = await KendiMinibusu();
            if (minibus == null) return Forbid();

            var vs = await DurumGetirVeyaOlustur(minibus.Id);
            vs.Durum = string.IsNullOrWhiteSpace(durum) ? null : durum;
            vs.SonGuncellemeZamani = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["basari"] = "Durum güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MesajGonder(int? stopId, string mesaj)
        {
            var minibus = await KendiMinibusu();
            if (minibus == null) return Forbid();

            if (string.IsNullOrWhiteSpace(mesaj))
            {
                TempData["hata"] = "Mesaj boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            _db.DriverMessages.Add(new DriverMessage
            {
                VehicleId = minibus.Id,
                StopId = stopId,
                Mesaj = mesaj.Length > 500 ? mesaj.Substring(0, 500) : mesaj,
                Zaman = DateTime.UtcNow
            });

            await DokunDurum(minibus.Id);
            await _db.SaveChangesAsync();

            TempData["basari"] = "Mesaj gönderildi.";
            return RedirectToAction(nameof(Index));
        }

        // ---- Yardımcılar ----

        private async Task<Vehicle?> KendiMinibusu()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Vehicles.FirstOrDefaultAsync(v => v.SoforId == user.Id);
        }

        private async Task<VehicleStatus> DurumGetirVeyaOlustur(int vehicleId)
        {
            var vs = await _db.VehicleStatuses.FirstOrDefaultAsync(s => s.VehicleId == vehicleId);
            if (vs == null)
            {
                vs = new VehicleStatus { VehicleId = vehicleId, SonGuncellemeZamani = DateTime.UtcNow };
                _db.VehicleStatuses.Add(vs);
            }
            return vs;
        }

        private async Task DokunDurum(int vehicleId)
        {
            var vs = await DurumGetirVeyaOlustur(vehicleId);
            vs.SonGuncellemeZamani = DateTime.UtcNow;
        }
        [HttpPost]
        public async Task<IActionResult> KonumGuncelleApi([FromBody] KonumDto model)
        {
            var minibus = await KendiMinibusu();
            if (minibus == null) return Json(new { success = false });

            // Veritabanına GPS konumunu kaydet
            _db.VehicleLocations.Add(new VehicleLocation
            {
                VehicleId = minibus.Id,
                Enlem = model.Enlem,
                Boylam = model.Boylam,
                Zaman = DateTime.UtcNow
            });

            // Anlık durumu (VehicleStatus) da güncelle ki harita en son nerede olduğunu bilsin
            var vs = await DurumGetirVeyaOlustur(minibus.Id);
            vs.SonGuncellemeZamani = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class KonumDto
        {
            public double Enlem { get; set; }
            public double Boylam { get; set; }
        }

    }
}