using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using tiempo_libre.Middlewares;
using Xunit;

public class RoleAuthorizationMiddlewareTest
{
    [Fact]
    public async Task Allows_Request_When_User_Has_Allowed_Role()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));
        var endpoint = new Endpoint((ctx) => Task.CompletedTask, new EndpointMetadataCollection(new RolesAllowedAttribute("Admin")), "test-endpoint");
        context.SetEndpoint(endpoint);
        var nextCalled = false;
        var middleware = new RoleAuthorizationMiddleware(ctx => { nextCalled = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);
        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Denies_Request_When_User_Does_Not_Have_Allowed_Role()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "User")
        }, "mock"));
        var endpoint = new Endpoint((ctx) => Task.CompletedTask, new EndpointMetadataCollection(new RolesAllowedAttribute("Admin")), "test-endpoint");
        context.SetEndpoint(endpoint);
        var nextCalled = false;
        var middleware = new RoleAuthorizationMiddleware(ctx => { nextCalled = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Allows_Request_When_No_RolesAllowedAttribute()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "User")
        }, "mock"));
        var endpoint = new Endpoint((ctx) => Task.CompletedTask, new EndpointMetadataCollection(), "test-endpoint");
        context.SetEndpoint(endpoint);
        var nextCalled = false;
        var middleware = new RoleAuthorizationMiddleware(ctx => { nextCalled = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);
        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }
}
