using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinibusTakip.Data;
using MinibusTakip.Models;

namespace MinibusTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class StopController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHost; // Resim kaydetmek için

        public StopController(ApplicationDbContext db, IWebHostEnvironment webHost)
        {
            _db = db;
            _webHost = webHost;
        }

        public IActionResult Index()
            => View(_db.Stops.OrderBy(s => s.Sira).ToList());

        [HttpGet]
        public IActionResult EkleGuncelle(int? id)
        {
            if (id == null || id == 0) return View(new Stop());
            var s = _db.Stops.Find(id);
            return s == null ? NotFound() : View(s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EkleGuncelle(Stop model, IFormFile? resim)
        {
            if (!ModelState.IsValid) return View(model);

            // Resim yükleme işlemi
            if (resim != null && resim.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(resim.FileName);
                string path = Path.Combine(_webHost.WebRootPath, "img/duraklar");

                // Klasör yoksa oluştur
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                resim.CopyTo(stream);
                model.ResimUrl = $"/img/duraklar/{fileName}";
            }

            if (model.Id == 0) _db.Stops.Add(model);
            else _db.Stops.Update(model);

            _db.SaveChanges();
            TempData["basari"] = "Durak kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sil(int id)
        {
            var s = _db.Stops.Find(id);
            if (s != null) { _db.Stops.Remove(s); _db.SaveChanges(); TempData["basari"] = "Durak silindi."; }
            return RedirectToAction(nameof(Index));
        }
    }
}