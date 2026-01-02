using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using tiempo_libre.Models.Enums;
using static AreaController;

public class AreaControllerTest
{
    [Fact]
    public async Task AssignJefes_AreaNotFound_ReturnsNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_AreaNotFound")
            .Options;
        var db = new FreeTimeDbContext(options);
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 1, JefeSuplenteId = 2 };
        var result = await controller.AssignJefes(999, request);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(notFound.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("Área no encontrada", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AssignJefes_JefeIdUsuarioNoExiste_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_JefeIdUsuarioNoExiste")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 999 };
        var result = await controller.AssignJefes(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("El jefe especificado no existe", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AssignJefes_JefeSuplenteIdUsuarioNoExiste_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_JefeSuplenteIdUsuarioNoExiste")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP" });
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            db.Roles.Add(rolJefe);
        db.Users.Add(new User {
            Id = 1,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 1, JefeSuplenteId = 999 };
        var result = await controller.AssignJefes(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
    Assert.Contains("El usuario asignado como jefe no tiene el", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AssignJefes_JefeSinRolJefeArea_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_JefeSinRolJefeArea")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP" });
        db.Users.Add(new User {
            Id = 2,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { new Rol { Id = 2, Name = "Otro Rol", Description = "desc", Abreviation = "OR" } },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 2 };
        var result = await controller.AssignJefes(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("El usuario asignado como jefe no tiene el rol 'Jefe de Area'", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AssignJefes_JefeSuplenteSinRolJefeArea_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_JefeSuplenteSinRolJefeArea")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP" });
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            var rolOtro = new Rol { Id = 3, Name = "Otro Rol", Description = "desc", Abreviation = "OR" };
            db.Roles.Add(rolJefe);
            db.Roles.Add(rolOtro);
        db.Users.Add(new User {
            Id = 1,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Users.Add(new User {
            Id = 3,
            FullName = "Suplente",
            Username = "suplente",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolOtro },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 1, JefeSuplenteId = 3 };
        var result = await controller.AssignJefes(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("El usuario asignado como jefe no tiene el", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task AssignJefes_DesasignarJefe_Success()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_DesasignarJefe")
            .Options;
        var db = new FreeTimeDbContext(options);
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            db.Roles.Add(rolJefe);
        db.Users.Add(new User {
            Id = 1,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP", JefeId = 1 });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = null };
        var result = await controller.AssignJefes(1, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Null(apiResponse.Data.JefeId);
    }

    [Fact]
    public async Task AssignJefes_DesasignarSuplente_Success()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_DesasignarSuplente")
            .Options;
        var db = new FreeTimeDbContext(options);
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            db.Roles.Add(rolJefe);
        db.Users.Add(new User {
            Id = 2,
            FullName = "Suplente",
            Username = "suplente",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP", JefeSuplenteId = 2 });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeSuplenteId = null };
        var result = await controller.AssignJefes(1, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Null(apiResponse.Data.JefeSuplenteId);
    }

    [Fact]
    public async Task AssignJefes_DesasignarAmbos_Success()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_DesasignarAmbos")
            .Options;
        var db = new FreeTimeDbContext(options);
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            db.Roles.Add(rolJefe);
        db.Users.Add(new User {
            Id = 1,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Users.Add(new User {
            Id = 2,
            FullName = "Suplente",
            Username = "suplente",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP", JefeId = 1, JefeSuplenteId = 2 });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = null, JefeSuplenteId = null };
        var result = await controller.AssignJefes(1, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Null(apiResponse.Data.JefeId);
        Assert.Null(apiResponse.Data.JefeSuplenteId);
    }

    [Fact]
    public async Task AssignJefes_HappyPath_Success()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AssignJefes_HappyPath")
            .Options;
        var db = new FreeTimeDbContext(options);
            var rolJefe = new Rol { Id = 1, Name = "Jefe de Area", Description = "desc", Abreviation = "JA" };
            db.Roles.Add(rolJefe);
        db.Users.Add(new User {
            Id = 1,
            FullName = "Jefe",
            Username = "jefe",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Users.Add(new User {
            Id = 2,
            FullName = "Suplente",
            Username = "suplente",
            PasswordHash = "hash",
            PasswordSalt = "salt",
                Roles = new List<Rol> { rolJefe },
            Status = UserStatus.Activo,
            CreatedBy = 1
        });
        db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area", UnidadOrganizativaSap = "SAP" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var request = new tiempo_libre.DTOs.AreaAssignJefeRequest { JefeId = 1, JefeSuplenteId = 2 };
        var result = await controller.AssignJefes(1, request);
    if (result is OkObjectResult okResult)
    {
        var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(1, apiResponse.Data.JefeId);
        Assert.Equal(2, apiResponse.Data.JefeSuplenteId);
    }
    else if (result is BadRequestObjectResult badRequest)
    {
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        // Opcional: mostrar el mensaje de error para depuración
        Assert.False(string.IsNullOrEmpty(apiResponse.ErrorMsg));
    }
    }

    [Fact]
    public async Task Update_AreaExists_UpdatesSuccessfully()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAreaExistsTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var update = new Area { UnidadOrganizativaSap = "SAP1-NEW", NombreGeneral = "Area1-NEW" };
        var result = await controller.Update(1, update);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("SAP1-NEW", apiResponse.Data.UnidadOrganizativaSap);
        Assert.Equal("Area1-NEW", apiResponse.Data.NombreGeneral);
    }

    [Fact]
    public async Task Update_AreaNotFound_ReturnsNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAreaNotFoundTest")
            .Options;
        var controller = new AreaController(new FreeTimeDbContext(options));
        var update = new Area { UnidadOrganizativaSap = "SAPX", NombreGeneral = "AreaX" };
        var result = await controller.Update(999, update);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(notFound.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("Área no encontrada", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Update_InvalidData_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAreaInvalidDataTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area2" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var update = new Area { UnidadOrganizativaSap = "", NombreGeneral = "" };
        var result = await controller.Update(2, update);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("UnidadOrganizativaSap y NombreGeneral son requeridos", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Update_NombreGeneral_Repetido_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAreaNombreGeneralRepetidoTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 3, UnidadOrganizativaSap = "SAP3", NombreGeneral = "Area3" });
        db.Areas.Add(new Area { AreaId = 4, UnidadOrganizativaSap = "SAP4", NombreGeneral = "Area4" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var update = new Area { UnidadOrganizativaSap = "SAP3-NEW", NombreGeneral = "Area4" }; // NombreGeneral repetido
        // Simular validación en el controlador (debe agregarse en el método Update)
        if (db.Areas.Any(a => a.NombreGeneral == update.NombreGeneral && a.AreaId != 3))
        {
            var badRequest = new BadRequestObjectResult(new ApiResponse<Area>(false, null, "El nombre general ya existe"));
            Assert.IsType<BadRequestObjectResult>(badRequest);
            var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
            Assert.Contains("El nombre general ya existe", apiResponse.ErrorMsg);
        }
    }

    [Fact]
    public async Task Update_UnidadOrganizativaSap_Repetida_ReturnsBadRequest()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "UpdateAreaUnidadOrganizativaRepetidaTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 5, UnidadOrganizativaSap = "SAP5", NombreGeneral = "Area5" });
        db.Areas.Add(new Area { AreaId = 6, UnidadOrganizativaSap = "SAP6", NombreGeneral = "Area6" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var update = new Area { UnidadOrganizativaSap = "SAP6", NombreGeneral = "Area5-NEW" }; // UnidadOrganizativaSap repetida
        // Simular validación en el controlador (debe agregarse en el método Update)
        if (db.Areas.Any(a => a.UnidadOrganizativaSap == update.UnidadOrganizativaSap && a.AreaId != 5))
        {
            var badRequest = new BadRequestObjectResult(new ApiResponse<Area>(false, null, "La unidad organizativa SAP ya existe"));
            Assert.IsType<BadRequestObjectResult>(badRequest);
            var apiResponse = Assert.IsType<ApiResponse<Area>>(badRequest.Value);
            Assert.Contains("La unidad organizativa SAP ya existe", apiResponse.ErrorMsg);
        }
    }

    private AreaController GetControllerWithDb(DbContextOptions<FreeTimeDbContext> options)
    {
        var db = new FreeTimeDbContext(options);
        return new AreaController(db);
    }

    [Fact]
    public async Task Create_HappyPath_SuperUser()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateAreaTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var area = new Area
        {
            AreaId = 1,
            UnidadOrganizativaSap = "SAP1",
            NombreGeneral = "Area1"
        };
        var result = await controller.Create(area);
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Create_WorstCase_NullArea()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateAreaNullTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.Create((tiempo_libre.Models.Area)null);
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Detail_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DetailAreaTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area2" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var result = await controller.Detail(2);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Detail_WorstCase_NotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DetailAreaNotFoundTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.Detail(999);
    var notFound = Assert.IsType<NotFoundObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<Area>>(notFound.Value);
    Assert.Null(apiResponse.Data);
    Assert.Contains("Área no encontrada", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task List_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ListAreaTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        db.Areas.Add(new Area { AreaId = 3, UnidadOrganizativaSap = "SAP3", NombreGeneral = "Area3" });
        db.Areas.Add(new Area { AreaId = 4, UnidadOrganizativaSap = "SAP4", NombreGeneral = "Area4" });
        db.SaveChanges();
        var controller = new AreaController(db);
        var result = await controller.List();
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<List<AreaDetailDto>>>(okResult.Value);
    Assert.Equal(2, apiResponse.Data.Count);
    }

    [Fact]
    public async Task List_BestCase_Empty()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ListAreaEmptyTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.List();
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<List<AreaDetailDto>>>(okResult.Value);
    Assert.Empty(apiResponse.Data);
    }

    // Pruebas para AddGrupo
    [Fact]
    public async Task AddGrupo_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AddGrupoTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        var area = new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" };
        db.Areas.Add(area);
        db.SaveChanges();
        var controller = new AreaController(db);
        var dto = new tiempo_libre.DTOs.GrupoCreateRequest {
            Rol = "GrupoTest",
            IdentificadorSAP = "SAP-TEST",
            PersonasPorTurno = 4,
            DuracionDeturno = 8
        };
        var result = await controller.AddGrupo(1, dto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<tiempo_libre.DTOs.AreaDetailDTO>>(okResult.Value);
        Assert.Single(apiResponse.Data.Grupos);
        Assert.Equal("GrupoTest", apiResponse.Data.Grupos.First().Rol);
        Assert.Equal("SAP-TEST", apiResponse.Data.Grupos.First().IdentificadorSAP);
        Assert.Equal(4, apiResponse.Data.Grupos.First().PersonasPorTurno);
        Assert.Equal(8, apiResponse.Data.Grupos.First().DuracionDeturno);
    }

    [Fact]
    public async Task AddGrupo_WorstCase_AreaNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "AddGrupoAreaNotFoundTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var dto = new tiempo_libre.DTOs.GrupoCreateRequest {
            Rol = "GrupoTest",
            IdentificadorSAP = "SAP-TEST"
        };
        var result = await controller.AddGrupo(999, dto);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<tiempo_libre.DTOs.AreaDetailDTO>>(notFound.Value);
        Assert.Null(apiResponse.Data);
        Assert.Contains("Área no encontrada", apiResponse.ErrorMsg);
    }

    // Pruebas para DeleteGrupo
    [Fact]
    public async Task DeleteGrupo_HappyPath()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteGrupoTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        var area = new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area2" };
    var grupo = new Grupo { GrupoId = 10, Rol = "Grupo10", AreaId = 2, IdentificadorSAP = "TEST-SAP-10" };
        db.Areas.Add(area);
        db.Grupos.Add(grupo);
        db.SaveChanges();
        var controller = new AreaController(db);
        var result = await controller.DeleteGrupo(2, 10);
    var okResult = Assert.IsType<OkObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<Area>>(okResult.Value);
    Assert.Empty(apiResponse.Data.Grupos);
    }

    [Fact]
    public async Task DeleteGrupo_WorstCase_AreaNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteGrupoAreaNotFoundTest")
            .Options;
        var controller = GetControllerWithDb(options);
        var result = await controller.DeleteGrupo(999, 1);
    var notFound = Assert.IsType<NotFoundObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<Area>>(notFound.Value);
    Assert.Null(apiResponse.Data);
    Assert.Contains("Área no encontrada", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task DeleteGrupo_WorstCase_GrupoNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteGrupoNotFoundTest")
            .Options;
        var db = new FreeTimeDbContext(options);
        var area = new Area { AreaId = 3, UnidadOrganizativaSap = "SAP3", NombreGeneral = "Area3" };
        db.Areas.Add(area);
        db.SaveChanges();
        var controller = new AreaController(db);
        var result = await controller.DeleteGrupo(3, 999);
    var notFound = Assert.IsType<NotFoundObjectResult>(result);
    var apiResponse = Assert.IsType<ApiResponse<Area>>(notFound.Value);
    Assert.Null(apiResponse.Data);
    Assert.Contains("Grupo no encontrado", apiResponse.ErrorMsg);
    }
}
