using Invi.HelperClass;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Invi.Middleware
{
    public class InitialRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public InitialRedirectMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
                path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".gif") ||
                path.EndsWith(".svg") || path.EndsWith(".woff") || path.EndsWith(".ttf") ||
                path.EndsWith(".map") ||
                path.Contains("/user/login") || path.Contains("/user/logout") ||
                path.Contains("/user/signup") || path.Contains("/error"))
            {
                await _next(context);
                return;
            }

            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                // 🔁 Fallback: check for your custom cookies
                var tenantKeyExists = context.Request.Cookies.ContainsKey("TenantKey");
                var needOrg = context.Request.Cookies["NeedOrganization"];
                if (tenantKeyExists)
                {
                    // 🧠 Optional: rehydrate identity from cookie and sign in
                    var claims = new List<Claim>
                    {
                        new Claim("TenantKey", context.Request.Cookies["TenantKey"]),
                        new Claim("NeedOrganization", needOrg ?? "false")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // ⚠️ Now skip login redirect
                }
                else
                {
                    context.Response.Redirect("/user/Login");
                    return;
                }
            }

            var tenantKey = context.Request.Cookies["TenantKey"];

            if (string.IsNullOrEmpty(tenantKey))
            {
                context.Response.Redirect("/user/Logout");
                return;
            }

            // ✅ Create scope to resolve DataService
            using (var scope = _scopeFactory.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<DataService>();

                var parameters = new Dictionary<string, object>
                {
                    { "@tenantKey", tenantKey }
                };

                var orgTable = await dataService.GetDataAsync("SP_Check_OrganizationExists", parameters);
                bool organizationExists = orgTable != null && orgTable.Rows.Count > 0;

                if (!organizationExists && !path.Contains("/user/organization"))
                {
                    context.Response.Redirect("/User/Organization");
                    return;
                }

                if (organizationExists && (path == "/" || path == "/home/index" || path == "/user/organization"))
                {
                    context.Response.Redirect("/Dashboard/Index");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class InitialRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseInitialRedirectMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InitialRedirectMiddleware>();
        }
    }
}
