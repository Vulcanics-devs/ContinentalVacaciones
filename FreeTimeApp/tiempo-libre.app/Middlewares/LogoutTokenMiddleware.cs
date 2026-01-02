using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace tiempo_libre.Middlewares;

public class LogoutTokenMiddleware
{
    private readonly RequestDelegate _next;

    public LogoutTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var logoutClaim = user.Claims.FirstOrDefault(c => c.Type == "logout" && c.Value == "true");
            if (logoutClaim != null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Sesión cerrada. El token corresponde a una sesión cerrada.");
                return;
            }
        }
        await _next(context);
    }
}
