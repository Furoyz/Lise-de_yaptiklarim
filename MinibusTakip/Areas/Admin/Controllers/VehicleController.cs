using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MinibusTakip.Data;
using MinibusTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace MinibusTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var list = _db.Vehicles.Include(v => v.Sofor).OrderBy(v => v.Plaka).ToList();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> EkleGuncelle(int? id)
        {
            await DoldurSoforlerAsync();
            if (id == null || id == 0) return View(new Vehicle());
            var v = _db.Vehicles.Find(id);
            if (v == null) return NotFound();
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EkleGuncelle(Vehicle model)
        {
            if (!ModelState.IsValid)
            {
                await DoldurSoforlerAsync();
                return View(model);
            }

            if (model.Id == 0) _db.Vehicles.Add(model);
            else _db.Vehicles.Update(model);

            _db.SaveChanges();
            TempData["basari"] = "Minibüs kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sil(int id)
        {
            var v = _db.Vehicles.Find(id);
            if (v != null)
            {
                _db.Vehicles.Remove(v);
                _db.SaveChanges();
                TempData["basari"] = "Minibüs silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task DoldurSoforlerAsync()
        {
            var soforler = await _userManager.GetUsersInRoleAsync(UserRoles.Sofor);
            ViewBag.SoforList = soforler
                .Select(u => new SelectListItem { Value = u.Id, Text = $"{u.Ad} {u.Soyad} ({u.Email})" })
                .ToList();
        }
    }
}