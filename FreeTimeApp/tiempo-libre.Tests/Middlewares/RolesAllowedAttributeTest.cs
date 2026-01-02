using tiempo_libre.Middlewares;
using Xunit;

public class RolesAllowedAttributeTest
{
    [Fact]
    public void Stores_Roles_Correctly()
    {
        var attr = new RolesAllowedAttribute("Admin", "User");
        Assert.Contains("Admin", attr.Roles);
        Assert.Contains("User", attr.Roles);
        Assert.Equal(2, attr.Roles.Length);
    }

    [Fact]
    public void Empty_Roles_Allowed()
    {
        var attr = new RolesAllowedAttribute();
        Assert.Empty(attr.Roles);
    }
}
