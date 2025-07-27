using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

            // Allow static files and public routes
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

            // STEP 1: Check SESSION first
            var tenantSessionJson = context.Session.GetString("TenantSession");
            TenantSessionModel tenantSession = null;

            if (!string.IsNullOrEmpty(tenantSessionJson))
            {
                tenantSession = JsonConvert.DeserializeObject<TenantSessionModel>(tenantSessionJson);
            }
            else
            {
                // STEP 2: Check COOKIE only if session is missing
                var tenantKey = context.Request.Cookies["TenantKey"];

                if (string.IsNullOrEmpty(tenantKey))
                {
                    context.Response.Redirect("/user/login");
                    return;
                }

                // STEP 3: Fetch from DB and set session
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
                    var parameters = new Dictionary<string, object> { { "@tenantKey", tenantKey } };
                    var orgTable = await dataService.GetDataAsync("SP_Check_OrganizationExists", parameters);

                    if (orgTable == null || orgTable.Rows.Count == 0)
                    {
                        // Invalid tenant - clear cookies and redirect to login
                        context.Response.Cookies.Delete("TenantKey");
                        context.Response.Cookies.Delete("NeedOrganization");
                        context.Response.Cookies.Delete("username");

                        await context.SignOutAsync();
                        context.Response.Redirect("/user/login");
                        return;
                    }

                    var row = orgTable.Rows[0];
                    tenantSession = new TenantSessionModel
                    {
                        TenantId = Convert.ToInt32(row["TenantId"]),
                        TenantCode = Guid.Parse(row["TenantCode"].ToString()),
                        TenantName = row["BusinessName"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Phone"].ToString(),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        OrgExists = Convert.ToBoolean(row["ExistsFlag"])
                    };

                    context.Session.SetString("TenantSession", JsonConvert.SerializeObject(tenantSession));
                }

                // STEP 4: Also authenticate user if not yet authenticated
                if (!(context.User?.Identity?.IsAuthenticated ?? false))
                {
                    var claims = new List<Claim>
            {
                new Claim("TenantKey", tenantSession.TenantCode.ToString()),
                new Claim("NeedOrganization", tenantSession.OrgExists.ToString())
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
            }

            // STEP 5: Routing logic based on OrgExists
            if (!tenantSession.OrgExists && !path.Contains("/user/organization"))
            {
                context.Response.Redirect("/user/organization");
                return;
            }

            if (tenantSession.OrgExists &&
                (path == "/" || path == "/home/index" || path == "/user/organization"))
            {
                context.Response.Redirect("/tenanttransaction/dashboard");
                return;
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
