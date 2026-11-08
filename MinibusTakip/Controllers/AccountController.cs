using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MinibusTakip.Models;
using MinibusTakip.ViewModels;

namespace MinibusTakip.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // --- KAYIT OLMA ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!await DomainGecerliMiAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Girdiğiniz e-posta adresi geçerli bir alan adına (domain) sahip değil.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad,
                Soyad = model.Soyad,
                PhoneNumber = model.Telefon,
                EmailConfirmed = false // ONAY BEKLİYOR
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Yolcu);

            // ONAY LİNKİ OLUŞTUR VE GÖNDER
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

            string mailIcerik = $"<h3>Minibüs Takip'e Hoş Geldiniz!</h3>" +
                                $"<p>Hesabınızı onaylamak için lütfen <a href='{callbackUrl}'>buraya tıklayın</a>.</p>";

            await _emailSender.SendEmailAsync(model.Email, "Hesap Onayı - Minibüs Takip", mailIcerik);

            TempData["basari"] = "Kayıt başarılı! Lütfen e-postanıza gelen linke tıklayarak hesabınızı onaylayın.";
            return RedirectToAction(nameof(Login));
        }

        // --- E-POSTA ONAYI ---
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null) return BadRequest("Geçersiz onay linki.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                TempData["basari"] = "Hesabınız başarıyla onaylandı. Artık giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            TempData["hata"] = "Hesap onaylanamadı, link geçersiz olabilir.";
            return RedirectToAction(nameof(Login));
        }

        // --- GİRİŞ YAPMA ---
        // --- GİRİŞ YAPMA ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Identity'nin kendi giriş kontrolünü kullanıyoruz. 
            // E-posta yoksa, şifre yanlışsa veya e-posta onaylı değilse bu metot başarısız (false) döner.
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Güvenlik gereği (Account Enumeration'ı önlemek için) her durumda aynı hatayı veriyoruz.
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        // --- ÇIKIŞ ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // --- ŞİFREMİ UNUTTUM ---
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            // 1. Domain Kontrolü (Önceki adımda eklediğimiz metot)
            if (!await DomainGecerliMiAsync(email))
            {
                TempData["hata"] = "Lütfen geçerli bir e-posta adresi girin.";
                return View();
            }

            // 2. Veritabanı Kontrolü
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Kullanıcı YOKSA hata mesajı göster ve sayfada kal
                TempData["hata"] = "Bu e-posta adresi sistemimizde kayıtlı değil. Lütfen kontrol edin veya yeni bir hesap oluşturun.";
                return View();
            }

            // Kullanıcı VARSA şifre sıfırlama linkini gönder
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

            string mailIcerik = $"<h3>Şifre Sıfırlama</h3>" +
                                $"<p>Şifrenizi sıfırlamak için lütfen <a href='{callbackUrl}'>buraya tıklayın</a>.</p>";

            await _emailSender.SendEmailAsync(email, "Şifre Sıfırlama - Minibus Takip", mailIcerik);

            TempData["basari"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction(nameof(Login));
        }

        // --- ŞİFRE SIFIRLAMA EKRANI ---
        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            if (userId == null || code == null) return BadRequest("Geçersiz şifre sıfırlama isteği.");
            var model = new ResetPasswordViewModel { UserId = userId, Code = code };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                TempData["basari"] = "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }

        // E-posta domaininin gerçekten var olup olmadığını DNS üzerinden kontrol eder
        private async Task<bool> DomainGecerliMiAsync(string email)
        {
            try
            {
                var parts = email.Split('@');
                if (parts.Length != 2) return false;

                var domain = parts[1].Trim();
                if (string.IsNullOrWhiteSpace(domain) || !domain.Contains(".")) return false;

                // DNS sunucularına sor: "Bu domainin bir IP adresi var mı?"
                var addresses = await System.Net.Dns.GetHostAddressesAsync(domain);
                return addresses.Length > 0;
            }
            catch
            {
                // Eğer domain yoksa veya DNS'e ulaşılamazsa hata fırlatır, false döneriz.
                return false;
            }
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}