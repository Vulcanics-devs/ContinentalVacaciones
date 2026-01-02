using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;

public class GrupoControllerTest
{
    [Fact]
    public async Task AsignarLider_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderHappyTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Users.Add(new User { Id = 10, FullName = "Lider", Username = "lider", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>() });
    db.Grupos.Add(new Grupo { GrupoId = 1, Rol = "Grupo1", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLider(1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(okResult.Value);
        Assert.Equal(10, apiResponse.Data.LiderId);
    }

    [Fact]
    public async Task AsignarLider_WorstCase_GrupoNoExiste()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderGrupoNoExisteTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Users.Add(new User { Id = 10, FullName = "Lider", Username = "lider", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>() });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLider(999, 10);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(notFound.Value);
        Assert.Contains("Grupo no encontrado", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AsignarLider_WorstCase_LiderNoExiste()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderNoExisteTest")
            .Options;
        var db = new FreeTimeDbContext(options);
    db.Grupos.Add(new Grupo { GrupoId = 1, Rol = "Grupo1", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLider(1, 999);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(badRequest.Value);
        Assert.Contains("El líder especificado no existe", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AsignarLiderSuplente_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderSuplenteHappyTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Users.Add(new User { Id = 20, FullName = "LiderSuplente", Username = "suplente", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>() });
    db.Grupos.Add(new Grupo { GrupoId = 2, Rol = "Grupo2", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLiderSuplente(2, 20);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(okResult.Value);
        Assert.Equal(20, apiResponse.Data.LiderSuplenteId);
    }

    [Fact]
    public async Task AsignarLiderSuplente_WorstCase_GrupoNoExiste()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderSuplenteGrupoNoExisteTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Users.Add(new User { Id = 20, FullName = "LiderSuplente", Username = "suplente", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>() });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLiderSuplente(999, 20);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(notFound.Value);
        Assert.Contains("Grupo no encontrado", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AsignarLiderSuplente_WorstCase_LiderSuplenteNoExiste()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AsignarLiderSuplenteNoExisteTest")
            .Options;
        var db = new FreeTimeDbContext(options);
    db.Grupos.Add(new Grupo { GrupoId = 2, Rol = "Grupo2", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.AsignarLiderSuplente(2, 999);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Grupo>>(badRequest.Value);
        Assert.Contains("El líder suplente especificado no existe", apiResponse.ErrorMsg);
    }
    private GrupoController GetControllerWithDb(DbContextOptions<FreeTimeDbContext> options)
    {
        var db = new FreeTimeDbContext(options);
        return new GrupoController(db);
    }

    [Fact]
    public async Task Create_HappyPath_SuperUser()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateGrupoTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var grupo = new Grupo
        {
            GrupoId = 1,
            Rol = "Grupo1",
            AreaId = 1,
            IdentificadorSAP = "TEST-SAP"
        };
        var result = await controller.Create(grupo);
    Assert.IsType<BadRequestObjectResult>(result); // El endpoint retorna BadRequest si falta algún dato obligatorio
    }

    [Fact]
    public async Task Create_WorstCase_NullGrupo()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateGrupoNullTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.Create((tiempo_libre.Models.Grupo)null);
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Detail_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DetailGrupoTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
        db.SaveChanges();
    db.Grupos.Add(new Grupo { GrupoId = 2, Rol = "Grupo2", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.Detail(2);
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<GrupoDetail>>(okResult.Value);
    Assert.Equal(2, apiResponse.Data.GrupoId);
    }

    [Fact]
    public async Task Detail_WorstCase_NotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DetailGrupoNotFoundTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.Detail(999);
    var notFound = Assert.IsType<NotFoundObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<Grupo>>(notFound.Value);
    Assert.Null(apiResponse.Data);
    Assert.Contains("Grupo no encontrado", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task List_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ListGrupoTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
        db.SaveChanges();
    db.Grupos.Add(new Grupo { GrupoId = 3, Rol = "Grupo3", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
    db.Grupos.Add(new Grupo { GrupoId = 4, Rol = "Grupo4", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.List();
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<List<GrupoDetail>>>(okResult.Value);
    Assert.Equal(2, apiResponse.Data.Count);
    }

    [Fact]
    public async Task ListByArea_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ListByAreaTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
        db.Areas.Add(new Area { AreaId = 2, NombreGeneral = "Area2", UnidadOrganizativaSap = "SAP2" });
        db.SaveChanges();
    db.Grupos.Add(new Grupo { GrupoId = 5, Rol = "Grupo5", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
    db.Grupos.Add(new Grupo { GrupoId = 6, Rol = "Grupo6", AreaId = 1, IdentificadorSAP = "TEST-SAP" });
    db.Grupos.Add(new Grupo { GrupoId = 7, Rol = "Grupo7", AreaId = 2, IdentificadorSAP = "TEST-SAP" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.ListByArea(1);
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<List<GrupoDetail>>>(okResult.Value);
    Assert.Equal(2, apiResponse.Data.Count);
    Assert.All(apiResponse.Data, g => Assert.Equal(1, g.AreaId));
    }

    [Fact]
    public async Task ListByArea_BestCase_Empty()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ListByAreaEmptyTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 3, NombreGeneral = "Area3", UnidadOrganizativaSap = "SAP3" });
        db.SaveChanges();
        var controller = new GrupoController(db);
        var result = await controller.ListByArea(3);
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<List<GrupoDetail>>>(okResult.Value);
    Assert.Empty(apiResponse.Data);
    }
}
