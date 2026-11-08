using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinibusTakip.Models;

namespace MinibusTakip.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();
    }
}