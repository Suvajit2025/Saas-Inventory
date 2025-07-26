using Invi.DataAccess;
using Invi.HelperClass;
using Invi.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Register services
builder.Services.AddScoped<AdoDataAccess>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<EncryptDecryptService>();
builder.Services.AddScoped<GeneralSevice>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ✅ 🔐 Register Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.Name = "Invi.AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // or adjust as needed
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 🌐 Error handling and HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ 🧠 Custom Zoho-style middleware
app.UseInitialRedirectMiddleware();

// ✅ MVC route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
