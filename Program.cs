using Invi.DataAccess;
using Invi.HelperClass;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<AdoDataAccess>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<EncryptDecryptService>();
builder.Services.AddScoped<GeneralSevice>();
// Register MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
   pattern: "{controller=User}/{action=SignUp}/{id?}");

app.Run();
