using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using tiempo_libre.app.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using tiempo_libre.DTOs;
using Moq;
using Microsoft.Extensions.Logging;

namespace tiempo_libre.Tests.Controllers
{
    public class UserControllerTest
    {
        private FreeTimeDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsOnlyActiveUsers()
        {
            var db = GetDb("GetUsuariosPorRol_OnlyActive");
            var rol = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            db.Roles.Add(rol);
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Users.Add(new User { Id = 1, Username = "activo", FullName = "Activo", AreaId = 1, GrupoId = 0, Roles = new List<Rol> { rol }, PasswordHash = "hash", PasswordSalt = "salt", Status = UserStatus.Activo });
            db.Users.Add(new User { Id = 2, Username = "inactivo", FullName = "Inactivo", AreaId = 1, GrupoId = 0, Roles = new List<Rol> { rol }, PasswordHash = "hash", PasswordSalt = "salt", Status = UserStatus.Desactivado });
            db.Users.Add(new User { Id = 3, Username = "suspendido", FullName = "Suspendido", AreaId = 1, GrupoId = 0, Roles = new List<Rol> { rol }, PasswordHash = "hash", PasswordSalt = "salt", Status = UserStatus.Suspendido });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest { RolInt = (int)RolEnum.Super_Usuario };
            var result = await controller.GetUsuariosPorRol(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Single(apiResponse.Data);
            Assert.Equal("activo", apiResponse.Data[0].Username);
        }

        [Fact]
        public async Task UpdateUserMaquina_ReturnsOk_IfSuperUsuarioUpdatesSindicalizado()
        {
            var db = GetDb("UpdateUserMaquinaOk");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var superRole = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            var sindicalizadoRole = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Empleado_Sindicalizado", Description = "Sindicalizado", Abreviation = "EMP" };
            db.Roles.Add(superRole);
            db.Roles.Add(sindicalizadoRole);
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { superRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.Users.Add(new User { Id = 2, Username = "sindi", FullName = "Sindicalizado", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { sindicalizadoRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "super") }));
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(2, request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("PC-99", apiResponse.Data.GetType().GetProperty("Maquina")?.GetValue(apiResponse.Data, null));
        }

        [Fact]
        public async Task UpdateUserMaquina_ReturnsNotFound_IfUserDoesNotExist()
        {
            var db = GetDb("UpdateUserMaquinaNotFound");
            var superRole = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            db.Roles.Add(superRole);
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { superRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "super") }));
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(999, request);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserMaquina_ReturnsForbid_IfUserHasNoPermission()
        {
            var db = GetDb("UpdateUserMaquinaNoPerm");
            var otherRole = new Rol { Id = 99, Name = "Otro", Description = "Otro", Abreviation = "OTR" };
            var sindicalizadoRole = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Empleado_Sindicalizado", Description = "Sindicalizado", Abreviation = "EMP" };
            db.Roles.Add(otherRole);
            db.Roles.Add(sindicalizadoRole);
            db.Users.Add(new User { Id = 1, Username = "noauth", FullName = "Sin Permiso", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { otherRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.Users.Add(new User { Id = 2, Username = "sindi", FullName = "Sindicalizado", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { sindicalizadoRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "noauth") }));
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(2, request);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUserMaquina_ReturnsBadRequest_IfTargetUserIsNotSindicalizado()
        {
            var db = GetDb("UpdateUserMaquinaBadTarget");
            var superRole = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            var otherRole = new Rol { Id = 99, Name = "Otro", Description = "Otro", Abreviation = "OTR" };
            db.Roles.Add(superRole);
            db.Roles.Add(otherRole);
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { superRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.Users.Add(new User { Id = 2, Username = "noSindi", FullName = "No Sindicalizado", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { otherRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "super") }));
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(2, request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Solo se puede asignar máquina a usuarios con rol Empleado_Sindicalizado", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task UpdateUserMaquina_ReturnsUnauthorized_IfCurrentUserNotFound()
        {
            var db = GetDb("UpdateUserMaquinaNoAuth");
            var sindicalizadoRole = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Empleado_Sindicalizado", Description = "Sindicalizado", Abreviation = "EMP" };
            db.Roles.Add(sindicalizadoRole);
            db.Users.Add(new User { Id = 2, Username = "sindi", FullName = "Sindicalizado", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { sindicalizadoRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "noexiste") }));
            var request = new UpdateMaquinaRequest { Maquina = "PC-99" };
            var result = await controller.UpdateUserMaquina(2, request);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(unauthorized.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("No hay sesión iniciada", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task UpdateUser_ReturnsForbid_IfNotSuperUserAndNotSelf()
        {
            var db = GetDb("UpdateUserForbid");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var role = new Rol { Id = 1, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(role);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.Users.Add(new User { Id = 2, Username = "user2", FullName = "User Two", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            var request = new UpdateUserRequest { Username = "user2", FullName = "User Two", AreaId = 1, GrupoId = 1, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(2, request);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_IfSelfUpdate()
        {
            var db = GetDb("UpdateUserSelf");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var role = new Rol { Id = 1, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(role);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            var request = new UpdateUserRequest { Username = "user1", FullName = "User One Updated", AreaId = 1, GrupoId = 1, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(1, request);
            Assert.IsType<OkObjectResult>(result);
            var apiResponse = (result as OkObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            var user = apiResponse.Data.GetType().GetProperty("FullName")?.GetValue(apiResponse.Data, null) as string;
            Assert.Equal("User One Updated", user);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_IfSuperUserUpdatesOther()
        {
            var db = GetDb("UpdateUserSuperUser");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var superRole = new Rol { Id = 1, Name = "SuperUsuario", Description = "Super user", Abreviation = "SUP" };
            var userRole = new Rol { Id = 2, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(superRole);
            db.Roles.Add(userRole);
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { superRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.Users.Add(new User { Id = 2, Username = "user2", FullName = "User Two", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { userRole }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "super") }));
            var request = new UpdateUserRequest { Username = "user2", FullName = "User Two Updated", AreaId = 1, GrupoId = 1, Roles = new List<int> { 2 } };
            var result = await controller.UpdateUser(2, request);
            Assert.IsType<OkObjectResult>(result);
            var apiResponse = (result as OkObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            var user = apiResponse.Data.GetType().GetProperty("FullName")?.GetValue(apiResponse.Data, null) as string;
            Assert.Equal("User Two Updated", user);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_IfUserDoesNotExist()
        {
            var db = GetDb("UpdateUserNotFound");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "super") }));
            var request = new UpdateUserRequest { Username = "nouser", FullName = "No User", AreaId = 1, GrupoId = 1, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(999, request);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_IfAreaDoesNotExist()
        {
            var db = GetDb("UpdateUserBadArea");
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var role = new Rol { Id = 1, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(role);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            var request = new UpdateUserRequest { Username = "user1", FullName = "User One", AreaId = 999, GrupoId = 1, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(1, request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_IfGrupoDoesNotExist()
        {
            var db = GetDb("UpdateUserBadGrupo");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            var role = new Rol { Id = 1, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(role);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            var request = new UpdateUserRequest { Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 999, Roles = new List<int> { 1 } };
            var result = await controller.UpdateUser(1, request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_IfRolesDoNotExist()
        {
            var db = GetDb("UpdateUserBadRoles");
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Grupos.Add(new Grupo { GrupoId = 1, AreaId = 1, Rol = "Grupo1", IdentificadorSAP = "GRP001" });
            var role = new Rol { Id = 1, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(role);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol> { role }, PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            var request = new UpdateUserRequest { Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<int> { 999 } };
            var result = await controller.UpdateUser(1, request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsBadRequest_IfNoRolProvided()
        {
            var db = GetDb("GetUsuariosPorRol_NoRol");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest();
            var result = await controller.GetUsuariosPorRol(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Se debe proporcionar un rol válido (string o int)", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsBadRequest_IfRolIntDoesNotExist()
        {
            var db = GetDb("GetUsuariosPorRol_RolIntNoExiste");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest { RolInt = 999 };
            var result = await controller.GetUsuariosPorRol(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("El rol proporcionado no es válido", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsBadRequest_IfRolStringDoesNotExist()
        {
            var db = GetDb("GetUsuariosPorRol_RolStringNoExiste");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest { RolString = "NoExiste" };
            var result = await controller.GetUsuariosPorRol(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("El rol proporcionado no es válido", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsOk_IfValidRolInt()
        {
            var db = GetDb("GetUsuariosPorRol_ValidInt");
            var rol = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            db.Roles.Add(rol);
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 0, Roles = new List<Rol> { rol }, PasswordHash = "hash", PasswordSalt = "salt", Status = UserStatus.Activo });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest { RolInt = (int)RolEnum.Super_Usuario };
            var result = await controller.GetUsuariosPorRol(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Single(apiResponse.Data);
            Assert.Equal("super", apiResponse.Data[0].Username);
            Assert.Equal("Super_Usuario", apiResponse.Data[0].Rol);
        }

        [Fact]
        public async Task GetUsuariosPorRol_ReturnsOk_IfValidRolString()
        {
            var db = GetDb("GetUsuariosPorRol_ValidString");
            var rol = new Rol { Id = (int)RolEnum.Super_Usuario, Name = "Super_Usuario", Description = "Super user", Abreviation = "SUP" };
            db.Roles.Add(rol);
            db.Areas.Add(new Area { AreaId = 1, NombreGeneral = "Area1", UnidadOrganizativaSap = "SAP001" });
            db.Users.Add(new User { Id = 1, Username = "super", FullName = "Super User", AreaId = 1, GrupoId = 0, Roles = new List<Rol> { rol }, PasswordHash = "hash", PasswordSalt = "salt", Status = UserStatus.Activo });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new UsuariosPorRolRequest { RolString = "Super_Usuario" };
            var result = await controller.GetUsuariosPorRol(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Single(apiResponse.Data);
            Assert.Equal("super", apiResponse.Data[0].Username);
            Assert.Equal("Super_Usuario", apiResponse.Data[0].Rol);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Unauthorized_When_User_Not_Authenticated()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoAuth").Options);
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var result = controller.GetCurrentUserProfile();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_NotFound_When_User_Does_Not_Exist()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileUserNotFound").Options);
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "nouser") })
            );
            var result = controller.GetCurrentUserProfile();
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Profile_Without_Area()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoArea").Options);
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 0, GrupoId = 0, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user1") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data.Area);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Profile_Without_Grupo_But_With_Area()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoGrupo").Options);
            db.Areas.Add(new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" });
            db.Users.Add(new User { Id = 2, Username = "user2", FullName = "User Two", AreaId = 1, GrupoId = 0, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user2") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data.Area);
            Assert.Null(apiResponse.Data.Grupo);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Profile_Without_FechaIngreso()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoFechaIngreso").Options);
            db.Areas.Add(new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area2" });
            db.Users.Add(new User { Id = 3, Username = "user3", FullName = "User Three", AreaId = 2, GrupoId = 0, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt", FechaIngreso = null });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user3") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data.FechaIngreso);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Profile_Without_Maquina()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoMaquina").Options);
            db.Areas.Add(new Area { AreaId = 3, UnidadOrganizativaSap = "SAP3", NombreGeneral = "Area3" });
            db.Users.Add(new User { Id = 4, Username = "user4", FullName = "User Four", AreaId = 3, GrupoId = 0, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt", Maquina = null });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user4") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data.Maquina);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Profile_With_All_Data()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileAllData").Options);
            db.Areas.Add(new Area { AreaId = 4, UnidadOrganizativaSap = "SAP4", NombreGeneral = "Area4" });
            db.Grupos.Add(new Grupo { GrupoId = 5, Rol = "Grupo5", AreaId = 4, IdentificadorSAP = "SAP-G5" });
            db.Users.Add(new User
            {
                Id = 5,
                Username = "user5",
                FullName = "User Five",
                AreaId = 4,
                GrupoId = 5,
                Roles = new List<Rol>(),
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Maquina = "PC-01",
                FechaIngreso = new System.DateOnly(2025, 8, 27)
            });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user5") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data.Area);
            Assert.NotNull(apiResponse.Data.Grupo);
            Assert.Equal("PC-01", apiResponse.Data.Maquina);
            Assert.Equal(new System.DateOnly(2025, 8, 27), apiResponse.Data.FechaIngreso);
        }

        private List<User> GetFakeUsers() => new List<User>
        {
            new User {
                Id = 1,
                FullName = "Juan Perez",
                Username = "jperez",
                AreaId = 1,
                GrupoId = 1,
                PasswordHash = "hash1",
                PasswordSalt = "salt1",
                Roles = new List<Rol> { new Rol { Id = 1, Name = "Admin", Description = "Admin role", Abreviation = "ADM" } }
            },
            new User {
                Id = 2,
                FullName = "Ana Lopez",
                Username = "alopez",
                AreaId = 2,
                GrupoId = 2,
                PasswordHash = "hash2",
                PasswordSalt = "salt2",
                Roles = new List<Rol> { new Rol { Id = 2, Name = "User", Description = "User role", Abreviation = "USR" } }
            }
        };

        private List<Area> GetFakeAreas() => new List<Area>
        {
            new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area 1" },
            new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area 2" }
        };

        private List<Grupo> GetFakeGrupos() => new List<Grupo>
        {
            new Grupo { GrupoId = 1, AreaId = 1, Rol = "Admin", IdentificadorSAP = "TEST-SAP-1" },
            new Grupo { GrupoId = 2, AreaId = 2, Rol = "User", IdentificadorSAP = "TEST-SAP-2" }
        };

        // Eliminado: método de Moq, ahora usamos contexto en memoria
        [Fact]
        public async Task ChangeUserStatus_ReturnsNotFound_IfUserDoesNotExist()
        {
            var db = GetDb("ChangeStatusNotFound");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new ChangeUserStatusRequest { NewStatus = (int)tiempo_libre.Models.Enums.UserStatus.Activo };
            var result = await controller.ChangeUserStatus(999, request);
            Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = (result as NotFoundObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Equal("Usuario no encontrado", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task ChangeUserStatus_ReturnsBadRequest_IfStatusIsInvalid()
        {
            var db = GetDb("ChangeStatusInvalidStatus");
            db.Users.Add(new User { Id = 1, Username = "user1", FullName = "User One", AreaId = 1, GrupoId = 1, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new ChangeUserStatusRequest { NewStatus = 999 }; // Valor inválido
            var result = await controller.ChangeUserStatus(1, request);
            Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Equal("El estatus proporcionado no es válido", apiResponse.ErrorMsg);
        }

        [Fact]
        public async Task ChangeUserStatus_UpdatesStatus_WhenValid()
        {
            var db = GetDb("ChangeStatusValid");
            db.Users.Add(new User { Id = 2, Username = "user2", FullName = "User Two", AreaId = 1, GrupoId = 1, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt", Status = tiempo_libre.Models.Enums.UserStatus.Activo });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new ChangeUserStatusRequest { NewStatus = (int)tiempo_libre.Models.Enums.UserStatus.Suspendido };
            var result = await controller.ChangeUserStatus(2, request);
            Assert.IsType<OkObjectResult>(result);
            var apiResponse = (result as OkObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            var msg = apiResponse.Data.GetType().GetProperty("msg")?.GetValue(apiResponse.Data, null) as string;
            Assert.Equal("Estatus actualizado correctamente.", msg);
            var user = db.Users.Find(2);
            Assert.Equal(tiempo_libre.Models.Enums.UserStatus.Suspendido, user.Status);
        }

        [Fact]
        public async Task ChangeUserStatus_ReturnsOk_IfStatusIsAlreadyTheSame()
        {
            var db = GetDb("ChangeStatusAlreadySame");
            db.Users.Add(new User { Id = 3, Username = "user3", FullName = "User Three", AreaId = 1, GrupoId = 1, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt", Status = tiempo_libre.Models.Enums.UserStatus.Activo });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new ChangeUserStatusRequest { NewStatus = (int)tiempo_libre.Models.Enums.UserStatus.Activo };
            var result = await controller.ChangeUserStatus(3, request);
            Assert.IsType<OkObjectResult>(result);
            var apiResponse = (result as OkObjectResult)?.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            var msg = apiResponse.Data.GetType().GetProperty("msg")?.GetValue(apiResponse.Data, null) as string;
            Assert.Equal("El estatus ya es el mismo, no se realizaron cambios.", msg);
        }

        private UserController GetController()
        {
            var optionsApp = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var optionsFree = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var appDbContext = new FreeTimeDbContext(optionsApp);

            // Poblar datos
            appDbContext.Users.AddRange(GetFakeUsers());

            var dbContext = new FreeTimeDbContext(optionsFree);
            dbContext.Users.AddRange(GetFakeUsers());
            dbContext.Areas.AddRange(GetFakeAreas());
            dbContext.Grupos.AddRange(GetFakeGrupos());
            dbContext.SaveChanges();

            var loggerMock = new Mock<ILogger<UserController>>();
            return new UserController(dbContext, loggerMock.Object);
        }

        [Fact]
        public void GetUserDetail_ReturnsNotFound_IfUserDoesNotExist()
        {
            var controller = GetController();
            var result = controller.GetUserDetail(999);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(notFound.Value);
            Assert.Contains("Usuario no encontrado", apiResponse.ErrorMsg);
        }

        [Fact]
        public void GetUserDetail_ReturnsOk_IfUserExists()
        {
            var controller = GetController();
            var result = controller.GetUserDetail(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserDetailDto>>(okResult.Value);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public void GetUsuariosPorArea_ReturnsEmpty_IfAreaDoesNotExist()
        {
            var controller = GetController();
            var result = controller.GetUsuariosPorArea(999) as OkObjectResult;
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(result.Value);
            Assert.Empty(apiResponse.Data);
        }

        [Fact]
        public void GetUsuariosPorArea_ReturnsUsuarios_IfAreaExists()
        {
            var controller = GetController();
            var result = controller.GetUsuariosPorArea(1) as OkObjectResult;
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(result.Value);
            Assert.True(apiResponse.Data != null && apiResponse.Data.Any());
        }

        [Fact]
        public void GetUsuariosPorGrupo_ReturnsEmpty_IfGrupoDoesNotExist()
        {
            var controller = GetController();
            var result = controller.GetUsuariosPorGrupo(999) as OkObjectResult;
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(result.Value);
            Assert.Empty(apiResponse.Data);
        }

        [Fact]
        public void GetUsuariosPorGrupo_ReturnsUsuarios_IfGrupoExists()
        {
            var controller = GetController();
            var result = controller.GetUsuariosPorGrupo(1) as OkObjectResult;
            var apiResponse = Assert.IsType<ApiResponse<List<UsuarioInfoDto>>>(result.Value);
            Assert.True(apiResponse.Data != null && apiResponse.Data.Any());
        }

        [Fact]
        public async Task EmpleadosSindicalizados_AreaNoExiste()
        {
            var db = GetDb("TestAreaNoExiste");
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new EmpleadosSindicalizadosGetListRequest { AreaId = 999 };
            var result = await controller.GetEmpleadosSindicalizadosPorArea(request);
            if (result.Result is BadRequestObjectResult badRequest)
            {
                var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
                Assert.Contains("área", apiResponse.ErrorMsg.ToLower());
            }
            else if (result.Result is OkObjectResult okResult)
            {
                var apiResponse = Assert.IsType<ApiResponse<PaginatedEmpleadosResponse>>(okResult.Value);
                Assert.True(apiResponse.Success);
            }
        }

        [Fact]
        public async Task EmpleadosSindicalizados_GrupoNoExiste()
        {
            var db = GetDb("TestGrupoNoExiste");
            db.Areas.Add(new Area { AreaId = 101, UnidadOrganizativaSap = "SAP101", NombreGeneral = "Area101" });
            db.SaveChanges();
            // No se agrega grupo con GrupoId = 999 para evitar duplicidad
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new EmpleadosSindicalizadosGetListRequest { AreaId = 101, GrupoId = 999 };
            var result = await controller.GetEmpleadosSindicalizadosPorArea(request);
            if (result.Result is BadRequestObjectResult badRequest)
            {
                var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
                Assert.Contains("grupo no encontrado", apiResponse.ErrorMsg.ToLower());
            }
            else if (result.Result is OkObjectResult okResult)
            {
                var apiResponse = Assert.IsType<ApiResponse<PaginatedEmpleadosResponse>>(okResult.Value);
                Assert.True(apiResponse.Success);
            }
        }

        [Fact]
        public async Task EmpleadosSindicalizados_GrupoNoPerteneceArea()
        {
            var db = GetDb("TestGrupoNoPerteneceArea");
            db.Areas.Add(new Area { AreaId = 102, UnidadOrganizativaSap = "SAP102", NombreGeneral = "Area102" });
            db.Areas.Add(new Area { AreaId = 202, UnidadOrganizativaSap = "SAP202", NombreGeneral = "Area202" });
            db.Grupos.Add(new Grupo { GrupoId = 110, AreaId = 202, Rol = "GrupoX", IdentificadorSAP = "SAP-GX" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new EmpleadosSindicalizadosGetListRequest { AreaId = 102, GrupoId = 110 };
            var result = await controller.GetEmpleadosSindicalizadosPorArea(request);
            if (result.Result is BadRequestObjectResult badRequest)
            {
                var apiResponse = Assert.IsType<ApiResponse<object>>(badRequest.Value);
                Assert.Contains("grupo no pertenece", apiResponse.ErrorMsg.ToLower());
            }
            else if (result.Result is OkObjectResult okResult)
            {
                var apiResponse = Assert.IsType<ApiResponse<PaginatedEmpleadosResponse>>(okResult.Value);
                Assert.True(apiResponse.Success);
            }
        }

        [Fact]
        public async Task EmpleadosSindicalizados_SinUsuariosSindicalizados()
        {
            var db = GetDb("TestSinSindicalizados");
            db.Areas.Add(new Area { AreaId = 103, UnidadOrganizativaSap = "SAP103", NombreGeneral = "Area103" });
            db.Grupos.Add(new Grupo { GrupoId = 120, AreaId = 103, Rol = "GrupoX", IdentificadorSAP = "SAP-GX" });
            db.Users.Add(new User
            {
                Id = 1001,
                FullName = "User1",
                Username = "user1",
                AreaId = 103,
                GrupoId = 120,
                Roles = new List<Rol>(),
                PasswordHash = "hash",
                PasswordSalt = "salt"
            });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            var request = new EmpleadosSindicalizadosGetListRequest { AreaId = 103, GrupoId = 120 };
            var result = await controller.GetEmpleadosSindicalizadosPorArea(request);
            if (result.Result is OkObjectResult okResult)
            {
                var apiResponse = Assert.IsType<ApiResponse<PaginatedEmpleadosResponse>>(okResult.Value);
                Assert.Empty(apiResponse.Data.Usuarios);
            }
            else if (result.Result is BadRequestObjectResult badRequest)
            {
                var apiResponse = Assert.IsType<ApiResponse<PaginatedEmpleadosResponse>>(badRequest.Value);
                Assert.False(apiResponse.Success);
            }
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Empty_Rols_When_User_Has_No_Roles()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileNoRoles").Options);
            db.Users.Add(new User { Id = 10, Username = "noroles", FullName = "Sin Roles", AreaId = 0, GrupoId = 0, Roles = new List<Rol>(), PasswordHash = "hash", PasswordSalt = "salt" });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "noroles") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Empty(apiResponse.Data.rols);
        }

        [Fact]
        public void GetCurrentUserProfile_Returns_Rols_When_User_Has_One_Or_More_Roles()
        {
            var db = new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("ProfileWithRoles").Options);
            var rol1 = new Rol { Id = 1, Name = "Admin", Description = "Admin role", Abreviation = "ADM" };
            var rol2 = new Rol { Id = 2, Name = "User", Description = "User role", Abreviation = "USR" };
            db.Roles.Add(rol1);
            db.Roles.Add(rol2);
            db.Users.Add(new User
            {
                Id = 11,
                Username = "conroles",
                FullName = "Con Roles",
                AreaId = 0,
                GrupoId = 0,
                Roles = new List<Rol> { rol1, rol2 },
                PasswordHash = "hash",
                PasswordSalt = "salt"
            });
            db.SaveChanges();
            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(db, loggerMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "conroles") })
            );
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            Assert.NotNull(result);
            var apiResponse = result.Value as ApiResponse<UserProfileDto>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotEmpty(apiResponse.Data.rols);
            Assert.Equal(2, apiResponse.Data.rols.Count);
            Assert.Contains(apiResponse.Data.rols, r => r.Name == "Admin");
            Assert.Contains(apiResponse.Data.rols, r => r.Name == "User");
        }

        [Fact]
        public void GetCurrentUserProfile_ReturnsRoles_WhenUserHasRoles()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(databaseName: "UserProfileRolesTest")
                .Options;
            using var context = new FreeTimeDbContext(options);

            var roles = new List<Rol>
            {
                new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" },
                new Rol { Id = 2, Name = "Empleado_Sindicalizado", Description = "desc2", Abreviation = "ES" }
            };
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                FullName = "Test User",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = roles,
                AreaId = 1,
                GrupoId = 1
            };
            context.Users.Add(user);
            context.Roles.AddRange(roles);
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<UserController>>();
            var controller = new UserController(context, loggerMock.Object);
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = controller.GetCurrentUserProfile() as OkObjectResult;
            var apiResponse = result?.Value as ApiResponse<UserProfileDto>;

            // Assert
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.NotNull(apiResponse.Data.rols);
            Assert.Equal(2, apiResponse.Data.rols.Count);
            Assert.Contains(apiResponse.Data.rols, r => r.Name == "SuperUsuario");
            Assert.Contains(apiResponse.Data.rols, r => r.Name == "Empleado_Sindicalizado");
        }
    }
}
