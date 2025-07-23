using Invi.DataAccess;
using Invi.HelperClass;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Register services
builder.Services.AddScoped<AdoDataAccess>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<EncryptDecryptService>();
builder.Services.AddScoped<GeneralSevice>();

// 🔧 Add MVC with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 🔐 Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Middleware to enforce Organization step (Zoho-style)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    // Allow static files, login, signup, logout to be accessible
    var allowedPaths = new[]
    {
        "/user/signup", "/user/login", "/user/logout", "/home/error"
    };

    // Skip check for static files and allowed paths
    if (!context.Request.Path.StartsWithSegments("/css") &&
        !context.Request.Path.StartsWithSegments("/js") &&
        !context.Request.Path.StartsWithSegments("/lib") &&
        !allowedPaths.Contains(path) &&
        context.Request.Cookies.TryGetValue("NeedOrganization", out string needOrg) &&
        needOrg == "true")
    {
        context.Response.Redirect("/User/Organization");
        return;
    }

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // ✅ Required to serve CSS/JS
app.UseRouting();
app.UseAuthorization();

// ✅ Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=SignUp}/{id?}");

app.Run();
