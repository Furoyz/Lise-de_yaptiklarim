using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinibusTakip.Data;
using MinibusTakip.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MinibusTakip.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        // Kullanýlmayan _logger deðiþkeni kaldýrýldý, constructor temizlendi.
        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                // YOLCU VEYA ADMÝN ÝSE DURAKLARI GÖNDER
                if (User.IsInRole(UserRoles.Yolcu) || User.IsInRole(UserRoles.Admin))
                {
                    var duraklar = await _db.Stops.OrderBy(s => s.Sira).ToListAsync();
                    ViewBag.Duraklar = duraklar;
                }
                // ÞOFÖR ÝSE MÝNÝBÜS BÝLGÝLERÝNÝ GÖNDER
                else if (User.IsInRole(UserRoles.Sofor))
                {
                    var user = await _userManager.GetUserAsync(User);
                    var minibus = await _db.Vehicles
                        .FirstOrDefaultAsync(v => v.SoforId == user!.Id);

                    if (minibus != null)
                    {
                        ViewBag.Minibus = minibus;

                        ViewBag.Durum = await _db.VehicleStatuses
                            .Include(s => s.SonDurak)
                            .Include(s => s.HedefDurak)
                            .FirstOrDefaultAsync(s => s.VehicleId == minibus.Id);

                        // Bir sonraki seferi bul (þu andan sonraki en yakýn saat)
                        var simdi = DateTime.Now.TimeOfDay;
                        var yaklasanTarife = await _db.Schedules
                            .Include(s => s.Stop)
                            .Where(s => s.VehicleId == minibus.Id && s.Saat >= simdi)
                            .OrderBy(s => s.Saat)
                            .FirstOrDefaultAsync();

                        // Eðer günün tüm seferleri bittiyse, yarýnýn ilk seferini göster
                        if (yaklasanTarife == null)
                        {
                            yaklasanTarife = await _db.Schedules
                                .Include(s => s.Stop)
                                .Where(s => s.VehicleId == minibus.Id)
                                .OrderBy(s => s.Saat)
                                .FirstOrDefaultAsync();
                        }

                        ViewBag.SonrakiSefer = yaklasanTarife;
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}