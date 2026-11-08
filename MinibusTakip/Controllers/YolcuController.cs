using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinibusTakip.Data;
using MinibusTakip.Models;
using MinibusTakip.ViewModels;
using System.Security.Claims;

namespace MinibusTakip.Controllers
{
    [Authorize]
    public class YolcuController : Controller
    {
        private readonly ApplicationDbContext _db;
        public YolcuController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Panelim()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var abonelik = await _db.StopSubscriptions
                .Include(s => s.Stop)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var vm = new YolcuPanelViewModel
            {
                TumDuraklar = await _db.Stops.OrderBy(s => s.Sira).ToListAsync(),
                SeciliStopId = abonelik?.StopId,
                SeciliStopAd = abonelik?.Stop?.Ad,
                SeciliStop = abonelik?.Stop // Kapak resmi için
            };

            if (abonelik != null)
                vm.Minibusler = await MinibusleriGetir(abonelik.StopId);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurakSec(int stopId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Kullanıcının eski aboneliklerini sil, yenisini ekle
            var eski = _db.StopSubscriptions.Where(s => s.UserId == userId);
            _db.StopSubscriptions.RemoveRange(eski);

            _db.StopSubscriptions.Add(new StopSubscription
            {
                UserId = userId,
                StopId = stopId,
                OlusturmaZamani = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["basari"] = "Durağınız güncellendi.";
            return RedirectToAction(nameof(Panelim));
        }

        [HttpGet]
        public async Task<IActionResult> ApiMinibusler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var abonelik = await _db.StopSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (abonelik == null) return Json(Array.Empty<object>());

            var liste = await MinibusleriGetir(abonelik.StopId);
            return Json(liste);
        }

        private async Task<List<MinibusItemViewModel>> MinibusleriGetir(int stopId)
        {
            var tarifeler = await _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v!.Sofor)
                .Where(s => s.StopId == stopId)
                .ToListAsync();

            var simdi = DateTime.Now;
            var sonuc = new List<MinibusItemViewModel>();

            foreach (var t in tarifeler)
            {
                if (t.Vehicle == null) continue;

                // Bu minibüs için bu durağa (veya genel) en son gecikme
                var sonGecikme = await _db.DelayUpdates
                    .Where(d => d.VehicleId == t.VehicleId && (d.StopId == null || d.StopId == stopId))
                    .OrderByDescending(d => d.Zaman)
                    .FirstOrDefaultAsync();

                // Anlık durum
                var durum = await _db.VehicleStatuses
                    .Include(s => s.SonDurak)
                    .Include(s => s.HedefDurak)
                    .FirstOrDefaultAsync(s => s.VehicleId == t.VehicleId);

                // --- YENİ EKLENEN: GPS KONUMUNU ÇEK ---
                var sonKonum = await _db.VehicleLocations
                    .Where(v => v.VehicleId == t.VehicleId)
                    .OrderByDescending(v => v.Zaman)
                    .FirstOrDefaultAsync();
                // -------------------------------------

                string konum = "Konum bilinmiyor";
                if (durum?.SonDurak != null && durum.HedefDurak != null)
                    konum = $"{durum.SonDurak.Ad} → {durum.HedefDurak.Ad} arası";
                else if (durum?.SonDurak != null)
                    konum = $"Son: {durum.SonDurak.Ad}";
                else if (durum?.HedefDurak != null)
                    konum = $"Hedef: {durum.HedefDurak.Ad}";

                var planlananStr = t.Saat.ToString(@"hh\:mm");
                var tahminiStr = planlananStr;

                if (sonGecikme?.GecikmeDakika != null)
                {
                    var tahmini = simdi.Date + t.Saat + TimeSpan.FromMinutes(sonGecikme.GecikmeDakika.Value);
                    tahminiStr = tahmini.ToString("HH:mm");
                }

                sonuc.Add(new MinibusItemViewModel
                {
                    VehicleId = t.VehicleId,
                    Plaka = t.Vehicle.Plaka,
                    SoforAd = t.Vehicle.Sofor != null
                        ? $"{t.Vehicle.Sofor.Ad} {t.Vehicle.Sofor.Soyad}"
                        : "-",
                    PlanlananSaat = planlananStr,
                    TahminiSaat = tahminiStr,
                    GecikmeAciklama = sonGecikme?.Aciklama,
                    KonumMetni = konum,
                    Durum = durum?.Durum,
                    SonGuncellemeZamani = durum?.SonGuncellemeZamani,

                    // YENİ EKLENEN: Haritada kullanmak için koordinatları gönder
                    Enlem = sonKonum?.Enlem,
                    Boylam = sonKonum?.Boylam
                });
            }

            // AKILLI SIRALAMA MANTIĞI
            var suAnkiTimeSpan = simdi.TimeOfDay;

            var siraliListe = sonuc.OrderBy(m =>
            {
                var tahminiTimeSpan = TimeSpan.Parse(m.TahminiSaat);

                if (tahminiTimeSpan < suAnkiTimeSpan)
                    return tahminiTimeSpan.Add(TimeSpan.FromDays(1));

                return tahminiTimeSpan;
            }).ToList();

            return siraliListe;
        }
    }
}