using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;

public class GenericUsersGeneratorTest
{
    private FreeTimeDbContext GetFreeTimeDb(DbContextOptions<FreeTimeDbContext> options)
    {
        return new FreeTimeDbContext(options);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_SkipsIfAreaNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTestAreaNotFound")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        // No se agrega el área requerida
        freeTimeDb.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "ROL1", IdentificadorSAP = "G1" });
        freeTimeDb.Empleados.Add(new Empleado { Nomina = 123, Nombre = "Juan Perez", UnidadOrganizativa = "UO1", Rol = "ROL1" });
        freeTimeDb.Roles.Add(new Rol { Id = 2, Name = "Empleado", Description = "Rol empleado", Abreviation = "EMP" });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        var created = await generator.GenerateUsersFromEmpleadosAsync();

        Assert.Equal(0, created);
        Assert.Empty(freeTimeDb.Users);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_SkipsIfGrupoNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTestGrupoNotFound")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        freeTimeDb.Areas.Add(new Area { AreaId = 1, UnidadOrganizativaSap = "UO1", NombreGeneral = "Area 1" });
        // No se agrega el grupo requerido
        freeTimeDb.Empleados.Add(new Empleado { Nomina = 123, Nombre = "Juan Perez", UnidadOrganizativa = "UO1", Rol = "ROL1" });
        freeTimeDb.Roles.Add(new Rol { Id = 2, Name = "Empleado", Description = "Rol empleado", Abreviation = "EMP" });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        var created = await generator.GenerateUsersFromEmpleadosAsync();

        Assert.Equal(0, created);
        Assert.Empty(freeTimeDb.Users);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_SkipsIfUserAlreadyExists()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTestUserExists")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        freeTimeDb.Areas.Add(new Area { AreaId = 1, UnidadOrganizativaSap = "UO1", NombreGeneral = "Area 1" });
        freeTimeDb.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "ROL1", IdentificadorSAP = "G1" });
        freeTimeDb.Empleados.Add(new Empleado { Nomina = 123, Nombre = "Juan Perez", UnidadOrganizativa = "UO1", Rol = "ROL1" });
        var rol = new Rol { Id = 2, Name = "Empleado", Description = "Rol empleado", Abreviation = "EMP" };
        freeTimeDb.Roles.Add(rol);
        freeTimeDb.SaveChanges();
        freeTimeDb.Users.Add(new User {
            Username = "123",
            FullName = "Juan Perez",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rol }
        });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        var created = await generator.GenerateUsersFromEmpleadosAsync();

        Assert.Equal(0, created);
        Assert.Single(freeTimeDb.Users);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_UsernameAndNominaAreEqual()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTestUsernameNomina")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        freeTimeDb.Areas.Add(new Area { AreaId = 1, UnidadOrganizativaSap = "UO1", NombreGeneral = "Area 1" });
        freeTimeDb.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "ROL1", IdentificadorSAP = "G1" });
        freeTimeDb.Empleados.Add(new Empleado { Nomina = 123, Nombre = "Juan Perez", UnidadOrganizativa = "UO1", Rol = "ROL1" });
        freeTimeDb.Roles.Add(new Rol { Id = 2, Name = "Empleado", Description = "Rol empleado", Abreviation = "EMP" });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        var created = await generator.GenerateUsersFromEmpleadosAsync();

        var user = freeTimeDb.Users.FirstOrDefault(u => u.Username == "123");
        Assert.NotNull(user);
        Assert.Equal(user.Nomina.ToString(), user.Username);
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_DoesNotDuplicateUsers()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTest2")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        freeTimeDb.Empleados.Add(new Empleado { Nomina = 789, Nombre = "Luis Gomez" });
        var rol = new Rol { Id = 2, Name = "Empleado", Description = "Rol empleado", Abreviation = "EMP" };
        freeTimeDb.Roles.Add(rol);
        freeTimeDb.SaveChanges();

        freeTimeDb.Users.Add(new User
        {
            Username = "789",
            PasswordSalt = "salt",
            PasswordHash = "hash",
            FullName = "Luis Gomez",
            Roles = new List<Rol> { rol }
        });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        var created = await generator.GenerateUsersFromEmpleadosAsync();

        Assert.Equal(0, created);
        Assert.Equal(1, freeTimeDb.Users.Count());
    }

    [Fact]
    public async Task GenerateUsersFromEmpleados_ThrowsIfRolNotFound()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "FreeTimeDbTest3")
            .Options;

        var freeTimeDb = GetFreeTimeDb(options);

        freeTimeDb.Empleados.Add(new Empleado { Nomina = 111, Nombre = "Carlos Ruiz" });
        freeTimeDb.SaveChanges();

    var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator>>();
    var generator = new GenericUsersGenerator(freeTimeDb, loggerMock.Object);
        await Assert.ThrowsAsync<System.Exception>(async () => await generator.GenerateUsersFromEmpleadosAsync());
    }
}
