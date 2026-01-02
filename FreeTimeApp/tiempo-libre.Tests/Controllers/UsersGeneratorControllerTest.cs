using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using tiempo_libre;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

public class UsersGeneratorControllerTest
{
    [Fact]
    public async Task GenerateUsersFromEmpleados_ReturnsOkWithCreatedCount()
    {
        // Arrange: contexto en memoria y datos mínimos
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UsersGeneratorController")
            .Options;
        var dbContext = new FreeTimeDbContext(options);

        // Agrega datos mínimos para que el método cree usuarios
        dbContext.Areas.Add(new Area
        {
            AreaId = 1,
            UnidadOrganizativaSap = "UO1",
            NombreGeneral = "Area 1"
        });
        dbContext.Grupos.Add(new Grupo
        {
            GrupoId = 1,
            AreaId = 1,
            Rol = "ROL1",
            IdentificadorSAP = "G1"
        });
        dbContext.Empleados.Add(new Empleado
        {
            Nomina = 123,
            Nombre = "Juan Perez",
            UnidadOrganizativa = "UO1",
            Rol = "ROL1"
        });
        dbContext.Roles.Add(new Rol
        {
            Id = 2,
            Name = "Empleado",
            Description = "Rol empleado",
            Abreviation = "EMP"
        });
        dbContext.SaveChanges();

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.UsersGeneratorController>>();
        var generatorLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
        var controller = new tiempo_libre.Controllers.UsersGeneratorController(dbContext, loggerMock.Object, generatorLoggerMock.Object);

        // Act
        var result = await controller.GenerateUsersFromEmpleados();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1, (int)response.Data.GetType().GetProperty("created").GetValue(response.Data));
        Assert.Contains("Usuarios generados correctamente", response.ErrorMsg);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_OnlySuperUsuarioAllowed()
    {
        var dbContextMock = new Mock<FreeTimeDbContext>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.UsersGeneratorController>>();
        var generatorLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
        var controller = new tiempo_libre.Controllers.UsersGeneratorController(dbContextMock.Object, loggerMock.Object, generatorLoggerMock.Object);

        var authorizeAttr = (AuthorizeAttribute)typeof(UsersGeneratorController)
            .GetMethod("GenerateUsersFromEmpleados")
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)[0];
        Assert.Equal("SuperUsuario", authorizeAttr.Roles);
    }
}
