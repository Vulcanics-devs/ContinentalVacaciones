using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace tiempo_libre.Middlewares;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RoleAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtén el endpoint actual
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var rolesAllowedAttr = endpoint.Metadata.GetMetadata<RolesAllowedAttribute>();
            if (rolesAllowedAttr != null)
            {
                // Obtén los roles del usuario autenticado (puedes ajustar esto según tu modelo de claims)
                var userRoles = context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                // Si el usuario no tiene ningún rol permitido, retorna 403
                if (!userRoles.Any(r => rolesAllowedAttr.Roles.Contains(r)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: You do not have the required role.");
                    return;
                }
            }
        }
        await _next(context);
    }
}
