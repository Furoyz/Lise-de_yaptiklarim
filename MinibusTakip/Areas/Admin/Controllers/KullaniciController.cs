using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinibusTakip.Models;

namespace MinibusTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class KullaniciController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public KullaniciController(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            _userManager = um;
            _roleManager = rm;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var rolls = new Dictionary<string, string>();
            foreach (var u in users)
            {
                var r = await _userManager.GetRolesAsync(u);
                rolls[u.Id] = r.FirstOrDefault() ?? "Yolcu";
            }
            ViewBag.Roller = rolls;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolDegistir(string id, string yeniRol)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await _roleManager.RoleExistsAsync(yeniRol))
            {
                TempData["hata"] = $"'{yeniRol}' rolü yok.";
                return RedirectToAction(nameof(Index));
            }

            var mevcut = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, mevcut);
            await _userManager.AddToRoleAsync(user, yeniRol);

            TempData["basari"] = $"{user.Email} artık {yeniRol} rolünde.";
            return RedirectToAction(nameof(Index));
        }
    }
}