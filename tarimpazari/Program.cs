using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarimPazari.Business.Abstract;
using TarimPazari.Business.Concrete;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Repositories;
using TarimPazari.DataAccess.Context;
using TarimPazari.DataAccess.Repositories;
using tarimpazari.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ========== VERİ TABANI BAĞLANTISI ==========
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== IDENTITY YAPILANDIRMASI ==========
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre kuralları
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Kullanıcı ayarları
    options.User.RequireUniqueEmail = true;

    // Kilit ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ========== COOKIE YAPILANDIRMASI ==========
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// ========== DEPENDENCY INJECTION ==========
// Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// MVC + SignalR
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 512 * 1024; // 512 KB - sesli mesajlar için
});

var app = builder.Build();

// ========== SEED: ROL VE ADMİN KULLANICI OLUŞTURMA ==========
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Ciftci", "Alici" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Admin kullanıcısı oluştur
    var adminEmail = "admin@tarimpazari.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Sistem Yöneticisi",
            RoleType = TarimPazari.Core.Enums.UserRoleType.Admin,
            IsApproved = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            Console.WriteLine("✅ Admin kullanıcısı başarıyla oluşturuldu.");
        }
        else
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"❌ Admin oluşturma hatası: {error.Description}");
        }
    }

    // Şifre sıfırlama (test hesapları)
    var testAccounts = new[] { "mehmet@test.com", "ahmet@test.com" };
    foreach (var email in testAccounts)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, "Test1234");
            if (resetResult.Succeeded)
                Console.WriteLine($"✅ {email} şifresi 'Test1234' olarak sıfırlandı.");
            else
                Console.WriteLine($"❌ {email} şifre sıfırlama hatası.");
        }
    }
}

// ========== MIDDLEWARE PIPELINE ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); // uploads/voice/ gibi dinamik dosyalar için
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// SignalR Hub endpoint
app.MapHub<ChatHub>("/chatHub");

app.Run();
