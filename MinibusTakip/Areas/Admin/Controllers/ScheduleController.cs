using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinibusTakip.Data;
using MinibusTakip.Models;

namespace MinibusTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ScheduleController(ApplicationDbContext db) => _db = db;

        public IActionResult Index(int? vehicleId)
        {
            ViewBag.Vehicles = _db.Vehicles.OrderBy(v => v.Plaka).ToList();
            ViewBag.SeciliVehicle = vehicleId;
            var query = _db.Schedules.Include(s => s.Vehicle).Include(s => s.Stop).AsQueryable();
            if (vehicleId.HasValue) query = query.Where(s => s.VehicleId == vehicleId.Value);
            return View(query.OrderBy(s => s.VehicleId).ThenBy(s => s.Stop!.Sira).ToList());
        }

        [HttpGet]
        public IActionResult Ekle(int? vehicleId)
        {
            DoldurListeler(vehicleId);
            return View(new Schedule { VehicleId = vehicleId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ekle(Schedule model)
        {
            if (!ModelState.IsValid) { DoldurListeler(model.VehicleId); return View(model); }
            _db.Schedules.Add(model);
            _db.SaveChanges();
            TempData["basari"] = "Tarife kaydedildi.";
            return RedirectToAction(nameof(Index), new { vehicleId = model.VehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sil(int id)
        {
            var s = _db.Schedules.Find(id);
            if (s != null) { _db.Schedules.Remove(s); _db.SaveChanges(); TempData["basari"] = "Tarife silindi."; }
            return RedirectToAction(nameof(Index));
        }

        private void DoldurListeler(int? vehicleId)
        {
            ViewBag.VehicleList = _db.Vehicles.OrderBy(v => v.Plaka)
                .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Plaka })
                .ToList();
            ViewBag.StopList = _db.Stops.OrderBy(s => s.Sira)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Sira}. {s.Ad}" })
                .ToList();
        }
    }
}