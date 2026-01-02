using System;

namespace tiempo_libre.Middlewares;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RolesAllowedAttribute : Attribute
{
    public string[] Roles { get; }

    public RolesAllowedAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
