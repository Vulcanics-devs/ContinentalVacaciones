using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using tiempo_libre.Middlewares;
using System.IO;

public class LogoutTokenMiddlewareTest
{
    private static DefaultHttpContext CreateHttpContextWithClaims(params Claim[] claims)
    {
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context.User = new ClaimsPrincipal(identity);
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task Allows_Request_With_Valid_Session_Token()
    {
        var context = CreateHttpContextWithClaims(
            new Claim(ClaimTypes.Name, "user1")
        );
        var called = false;
        var middleware = new LogoutTokenMiddleware(_ => {
            called = true;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context);
        Assert.True(called); // Debe continuar la cadena
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode); // No debe modificar el status
    }

    [Fact]
    public async Task Allows_Request_With_Expired_Session_Token()
    {
        // Simula usuario autenticado pero sin claim de logout
        var context = CreateHttpContextWithClaims(
            new Claim(ClaimTypes.Name, "user2")
        );
        // Simula que el token está vencido (esto normalmente lo maneja el middleware de autenticación)
        // Aquí solo verificamos que el middleware no rechaza por claim logout
        var called = false;
        var middleware = new LogoutTokenMiddleware(_ => {
            called = true;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context);
        Assert.True(called);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Rejects_Request_With_Logout_Claim()
    {
        var context = CreateHttpContextWithClaims(
            new Claim(ClaimTypes.Name, "user3"),
            new Claim("logout", "true")
        );
        var called = false;
        var middleware = new LogoutTokenMiddleware(_ => {
            called = true;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context);
        Assert.False(called); // No debe continuar la cadena
        Assert.Equal(401, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("Sesión cerrada", body);
    }
}
