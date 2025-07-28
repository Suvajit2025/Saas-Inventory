using Invi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

public class OrganizationRouteTransformer : DynamicRouteValueTransformer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationRouteTransformer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<RouteValueDictionary?> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        var orgKeyStr = values["organizationKey"]?.ToString();
        var action = values["action"]?.ToString();

        if (string.IsNullOrEmpty(orgKeyStr) || !Guid.TryParse(orgKeyStr, out var orgKey))
            return new ValueTask<RouteValueDictionary?>((RouteValueDictionary?)null);

        if (string.IsNullOrEmpty(action))
            return new ValueTask<RouteValueDictionary?>((RouteValueDictionary?)null);

        var session = httpContext.Session;
        var tenantSession = session.GetObject<TenantSessionModel>("TenantSession");

        if (tenantSession == null || tenantSession.OrganizationKey != orgKey)
        {
            httpContext.Response.Redirect("/user/login");
            return new ValueTask<RouteValueDictionary?>((RouteValueDictionary?)null);
        }

        var controller = ResolveControllerFromAction(action) ?? "Dashboard";

        var newRouteValues = new RouteValueDictionary
        {
            ["controller"] = controller,
            ["action"] = action,
            ["organizationKey"] = orgKeyStr
        };

        return new ValueTask<RouteValueDictionary?>(newRouteValues);
    }

    private string? ResolveControllerFromAction(string? action)
    {
        if (string.IsNullOrEmpty(action)) return null;

        var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Enumerable.Empty<Type>(); }
            })
            .Where(t => typeof(Controller).IsAssignableFrom(t) && t.Name.EndsWith("Controller"));

        foreach (var controllerType in controllerTypes)
        {
            var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m =>
                    string.Equals(m.Name, action, StringComparison.OrdinalIgnoreCase) &&
                    typeof(IActionResult).IsAssignableFrom(m.ReturnType) ||
                    typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType));

            if (methods.Any(m => m.Name.Equals(action, StringComparison.OrdinalIgnoreCase)))
            {
                return controllerType.Name.Replace("Controller", "");
            }
        }

        return null;
    }

}
