using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MinibusTakip.Data;
using MinibusTakip.Models;
using MinibusTakip.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;

    // --- YENÝ ÞÝFRE KURALLARI ---
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PushNotificationService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ======================== SEED VE DEMO VERÝLERÝ ========================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // EKLENDÝ

    // 1. ROLLER
    string[] roller = { UserRoles.Admin, UserRoles.Sofor, UserRoles.Yolcu };
    foreach (var role in roller)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 2. VARSAYILAN ADMÝN
    if (await userManager.FindByEmailAsync("admin@minibus.com") == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@minibus.com",
            Email = "admin@minibus.com",
            Ad = "Admin",
            Soyad = "User",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin123.");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
    }

    // 3. SAHTE DEMO VERÝLERÝ
    if (!db.Stops.Any())
    {
        // DURAKLARI EKLE
        var duraklar = new List<Stop>
        {
            new Stop { Ad = "Cumhuriyet Meydaný", Sira = 1 },
            new Stop { Ad = "Devlet Hastanesi", Sira = 2 },
            new Stop { Ad = "Merkez Pazar Yeri", Sira = 3 },
            new Stop { Ad = "Þehirlerarasý Terminal", Sira = 4 },
            new Stop { Ad = "Sanayi Sitesi", Sira = 5 },
            new Stop { Ad = "Üniversite Kampüsü", Sira = 6 }
        };
        db.Stops.AddRange(duraklar);
        await db.SaveChangesAsync();

        // ÞOFÖRLERÝ EKLE
        var sofor1 = new ApplicationUser { UserName = "ahmet@minibus.com", Email = "ahmet@minibus.com", Ad = "Ahmet", Soyad = "Yýlmaz", EmailConfirmed = true };
        var sofor2 = new ApplicationUser { UserName = "mehmet@minibus.com", Email = "mehmet@minibus.com", Ad = "Mehmet", Soyad = "Kaya", EmailConfirmed = true };
        var sofor3 = new ApplicationUser { UserName = "ali@minibus.com", Email = "ali@minibus.com", Ad = "Ali", Soyad = "Demir", EmailConfirmed = true };

        await userManager.CreateAsync(sofor1, "Sofor123.");
        await userManager.CreateAsync(sofor2, "Sofor123.");
        await userManager.CreateAsync(sofor3, "Sofor123.");

        await userManager.AddToRoleAsync(sofor1, UserRoles.Sofor);
        await userManager.AddToRoleAsync(sofor2, UserRoles.Sofor);
        await userManager.AddToRoleAsync(sofor3, UserRoles.Sofor);

        // MÝNÝBÜSLERÝ EKLE
        var minibus1 = new Vehicle { Plaka = "34 MNS 01", SoforId = sofor1.Id };
        var minibus2 = new Vehicle { Plaka = "34 MNS 02", SoforId = sofor2.Id };
        var minibus3 = new Vehicle { Plaka = "34 MNS 03", SoforId = sofor3.Id };

        db.Vehicles.AddRange(minibus1, minibus2, minibus3);
        await db.SaveChangesAsync();

        // TARÝFELERÝ EKLE
        var tarifeler = new List<Schedule>();
        foreach (var durak in duraklar)
        {
            tarifeler.Add(new Schedule { VehicleId = minibus1.Id, StopId = durak.Id, Saat = new TimeSpan(8, 0, 0).Add(TimeSpan.FromMinutes((durak.Sira - 1) * 10)) });
            tarifeler.Add(new Schedule { VehicleId = minibus2.Id, StopId = durak.Id, Saat = new TimeSpan(8, 15, 0).Add(TimeSpan.FromMinutes((durak.Sira - 1) * 10)) });
            tarifeler.Add(new Schedule { VehicleId = minibus3.Id, StopId = durak.Id, Saat = new TimeSpan(8, 30, 0).Add(TimeSpan.FromMinutes((durak.Sira - 1) * 10)) });
        }

        db.Schedules.AddRange(tarifeler);
        await db.SaveChangesAsync();
    }
}

app.Run();