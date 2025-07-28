using Invi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Invi.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class OrganizationValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public OrganizationValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.Trim('/');
            var segments = path?.Split('/');

            if (segments?.Length >= 1 && Guid.TryParse(segments[0], out Guid orgKey))
            {
                var sessionStr = context.Session.GetString("TenantSession");

                if (string.IsNullOrEmpty(sessionStr))
                {
                    context.Response.Redirect("/home/login");
                    return;
                }

                var session = JsonConvert.DeserializeObject<TenantSessionModel>(sessionStr);

                if (!session.OrgExists || session.OrganizationKey != orgKey)
                {
                    context.Response.Redirect("/home/login");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class OrganizationValidationMiddlewareExtensions
    {
        public static IApplicationBuilder OrganizationValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OrganizationValidationMiddleware>();
        }
    }
}
