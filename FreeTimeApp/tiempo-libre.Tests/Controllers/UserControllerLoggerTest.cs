using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using tiempo_libre.app.Controllers;
using tiempo_libre.Models;
using Xunit;
using Microsoft.Extensions.Logging;
using tiempo_libre.DTOs;
using System.Threading.Tasks;

namespace tiempo_libre.Tests.Controllers
{
    public class UserControllerLoggerTest
    {
        private UserController GetControllerWithLogger(FreeTimeDbContext context, out Mock<ILogger<UserController>> loggerMock, ClaimsPrincipal? user = null)
        {
            loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(context, loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") })) }
            };
            return controller;
        }

        [Fact]
        public async Task UpdateUserMaquina_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_UpdateUserMaquina").Options;
            using var context = new FreeTimeDbContext(options);
            var roleSuper = new Rol { Id = 1, Name = "Super_Usuario", Description = "desc", Abreviation = "SU" };
            var roleSind = new Rol { Id = 2, Name = "Empleado_Sindicalizado", Description = "desc2", Abreviation = "ES" };
            context.Roles.AddRange(roleSuper, roleSind);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSuper }, AreaId = 1, GrupoId = 1 });
            context.Users.Add(new User { Id = 2, Username = "sindi", FullName = "Sindicalizado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSind }, AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(2, request);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Máquina modificada para usuario")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_UpdateUser").Options;
            using var context = new FreeTimeDbContext(options);
            // Crear área válida para que la validación pase
            context.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
            // Crear grupo válido para que la validación pase
            context.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "G1" });
            var roleSuper = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
            context.Roles.Add(roleSuper);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSuper }, AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var request = new UpdateUserRequest { Username = "testuser", FullName = "Test User Updated", AreaId = 1, GrupoId = 1, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(1, request);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Datos de usuario")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetUsuariosPorRol_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_UsuariosPorRol").Options;
            using var context = new FreeTimeDbContext(options);
            var roleSuper = new Rol { Id = 1, Name = "Super_Usuario", Description = "desc", Abreviation = "SU" };
            context.Roles.Add(roleSuper);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSuper }, AreaId = 1, GrupoId = 1, Status = tiempo_libre.Models.Enums.UserStatus.Activo });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var request = new UsuariosPorRolRequest { RolInt = 1 };
            var result = await controller.GetUsuariosPorRol(request);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuarios por rol consultados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ChangeUserStatus_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_ChangeUserStatus").Options;
            using var context = new FreeTimeDbContext(options);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), AreaId = 1, GrupoId = 1, Status = tiempo_libre.Models.Enums.UserStatus.Activo });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var request = new ChangeUserStatusRequest { NewStatus = (int)tiempo_libre.Models.Enums.UserStatus.Suspendido };
            var result = await controller.ChangeUserStatus(1, request);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Estatus de usuario")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void GetUserDetail_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_GetUserDetail").Options;
            using var context = new FreeTimeDbContext(options);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var result = controller.GetUserDetail(1);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Detalle de usuario consultado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void GetUsuariosPorArea_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_UsuariosPorArea").Options;
            using var context = new FreeTimeDbContext(options);
            context.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
            context.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "G1" });
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var result = controller.GetUsuariosPorArea(1);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuarios por área consultados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void GetUsuariosPorGrupo_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_UsuariosPorGrupo").Options;
            using var context = new FreeTimeDbContext(options);
            context.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
            context.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "G1" });
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var result = controller.GetUsuariosPorGrupo(1);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuarios por grupo consultados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void GetEmpleadosSindicalizadosPorArea_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_EmpleadosSindicalizados").Options;
            using var context = new FreeTimeDbContext(options);
            context.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP1" });
            context.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "G1" });
            var roleSind = new Rol { Id = 2, Name = "Empleado_Sindicalizado", Description = "desc2", Abreviation = "ES" };
            context.Roles.Add(roleSind);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSind }, AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = GetControllerWithLogger(context, out loggerMock);
            var request = new EmpleadosSindicalizadosGetListRequest { AreaId = 1, GrupoId = 1 };
            var result = controller.GetEmpleadosSindicalizadosPorArea(request);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Empleados sindicalizados consultados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void GetCurrentUserProfile_LogsInformation_OnSuccess()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("Logger_GetCurrentUserProfile").Options;
            using var context = new FreeTimeDbContext(options);
            var roleSuper = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
            context.Roles.Add(roleSuper);
            context.Users.Add(new User { Id = 1, Username = "testuser", FullName = "Test User", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { roleSuper }, AreaId = 1, GrupoId = 1 });
            context.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));
            var controller = GetControllerWithLogger(context, out loggerMock, userClaims);
            var result = controller.GetCurrentUserProfile();
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Perfil consultado para usuario")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
