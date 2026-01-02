using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.app.Controllers;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class RolControllerTest
{
    private DbContextOptions<FreeTimeDbContext> GetDbOptions(string dbName) =>
        new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase(dbName).Options;

    [Fact]
    public async Task GetRoles_NoRoles_ReturnsEmptyList()
    {
        var options = GetDbOptions("NoRoles");
        using var context = new FreeTimeDbContext(options);
        var controller = new RolController(context);
        var result = await controller.GetRoles();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<List<RolListDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetRoles_HappyPath_ReturnsRoles()
    {
        var options = GetDbOptions("RolesHappyPath");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.Roles.Add(new Rol { Id = 2, Name = "User", Description = "Desc2", Abreviation = "USR" });
        context.SaveChanges();
        var controller = new RolController(context);
        var result = await controller.GetRoles();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<List<RolListDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count);
        Assert.Contains(response.Data, r => r.Name == "Admin");
        Assert.Contains(response.Data, r => r.Name == "User");
    }

    [Fact]
    public async Task EditRol_RolNotFound_ReturnsNotFound()
    {
        var options = GetDbOptions("EditRolNotFound");
        using var context = new FreeTimeDbContext(options);
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "Nuevo", Description = "Desc", Abreviation = "NVO" };
        var result = await controller.EditRol(99, request);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal("Rol no encontrado", response.ErrorMsg);
    }

    [Fact]
    public async Task EditRol_RolNameAlreadyExists_ReturnsConflict()
    {
        var options = GetDbOptions("EditRolNameExists");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.Roles.Add(new Rol { Id = 2, Name = "User", Description = "Desc2", Abreviation = "USR" });
        context.SaveChanges();
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "User", Description = "NuevaDesc", Abreviation = "NVO" };
        var conflict = Assert.IsType<ConflictObjectResult>(await controller.EditRol(1, request));
        var response = Assert.IsType<ApiResponse<object>>(conflict.Value);
        Assert.False(response.Success);
        Assert.Equal("Ya existe un rol con ese nombre", response.ErrorMsg);
    }

    [Fact]
    public async Task EditRol_MissingName_ReturnsBadRequest()
    {
        var options = GetDbOptions("EditRolMissingName");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.SaveChanges();
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "", Description = "NuevaDesc", Abreviation = "NVO" };
        var result = await controller.EditRol(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("Todos los campos son requeridos", response.ErrorMsg);
    }

    [Fact]
    public async Task EditRol_MissingDescription_ReturnsBadRequest()
    {
        var options = GetDbOptions("EditRolMissingDesc");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.SaveChanges();
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "Nuevo", Description = "", Abreviation = "NVO" };
        var result = await controller.EditRol(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("Todos los campos son requeridos", response.ErrorMsg);
    }

    [Fact]
    public async Task EditRol_MissingAbreviation_ReturnsBadRequest()
    {
        var options = GetDbOptions("EditRolMissingAbrev");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.SaveChanges();
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "Nuevo", Description = "NuevaDesc", Abreviation = "" };
        var result = await controller.EditRol(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("Todos los campos son requeridos", response.ErrorMsg);
    }

    [Fact]
    public async Task EditRol_HappyPath_UpdatesRol()
    {
        var options = GetDbOptions("EditRolHappyPath");
        using var context = new FreeTimeDbContext(options);
        context.Roles.Add(new Rol { Id = 1, Name = "Admin", Description = "Desc", Abreviation = "ADM" });
        context.SaveChanges();
        var controller = new RolController(context);
        var request = new EditRolRequestDto { Name = "Nuevo", Description = "NuevaDesc", Abreviation = "NVO" };
        var result = await controller.EditRol(1, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<RolListDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Nuevo", response.Data.Name);
        Assert.Equal("NuevaDesc", response.Data.Description);
        Assert.Equal("NVO", response.Data.Abreviation);
    }
}
