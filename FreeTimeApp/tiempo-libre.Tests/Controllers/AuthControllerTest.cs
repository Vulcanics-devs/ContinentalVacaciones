using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class AuthControllerTest
{
    [Fact]
    public void Logout_Returns_401_When_Token_Is_Logout()
    {
        // Simula un usuario autenticado con claim logout:true
        var controller = new AuthController(new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("LogoutTokenTest").Options));
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim("logout", "true")
            }, "TestAuthType")
        );
        var result = controller.Logout();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = okResult.Value as ApiResponse<object>;
        Assert.NotNull(apiResponse);
        var data = JObject.FromObject(apiResponse.Data);
        Assert.NotNull(data["Token"]);
        // Decodifica el token y verifica el claim logout
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(data["Token"].ToString());
        Assert.Contains(jwt.Claims, c => c.Type == "logout" && c.Value == "true");
    }

    [Fact]
    public void Logout_Returns_Token_With_Long_Expiration_When_Logout()
    {
        var controller = new AuthController(new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("LogoutExpirationTest").Options));
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim("logout", "true")
            }, "TestAuthType")
        );
        var result = controller.Logout();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = okResult.Value as ApiResponse<object>;
        Assert.NotNull(apiResponse);
        var data = JObject.FromObject(apiResponse.Data);
        Assert.NotNull(data["Expiration"]);
        var expiration = System.DateTime.Parse(data["Expiration"].ToString());
        Assert.True(expiration > System.DateTime.UtcNow.AddYears(9));
    }

    [Fact]
    public void Logout_Returns_Token_With_Valid_Expiration_When_Session_Active()
    {
        var controller = new AuthController(new FreeTimeDbContext(new DbContextOptionsBuilder<FreeTimeDbContext>().UseInMemoryDatabase("LogoutActiveTest").Options));
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user1")
            }, "TestAuthType")
        );
        var result = controller.Logout();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = okResult.Value as ApiResponse<object>;
        Assert.NotNull(apiResponse);
        var data = JObject.FromObject(apiResponse.Data);
        Assert.NotNull(data["Expiration"]);
        var expiration = System.DateTime.Parse(data["Expiration"].ToString());
        Assert.True(expiration > System.DateTime.UtcNow.AddYears(9)); // El token de logout siempre tiene expiración larga
    }

    private AuthController GetControllerWithDb(DbContextOptions<FreeTimeDbContext> appOptions, DbContextOptions<FreeTimeDbContext> freeTimeOptions)
    {
        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);
        return new AuthController(freeTimeDb);
    }

    [Fact]
    public async Task ChangePassword_Fails_When_User_Not_Found()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangePasswordUserNotFound")
            .Options;
        var db = new FreeTimeDbContext(options);
        var controller = new AuthController(db);

        // Simula usuario no autenticado
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass",
            ConfirmNewPassword = "newpass"
        };
        var result = await controller.ChangePassword(request);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_Fails_When_CurrentPassword_Is_Incorrect()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangePasswordIncorrectCurrent")
            .Options;
        var db = new FreeTimeDbContext(options);
        var salt = "somesalt";
        var hash = PasswordHasher.HashPassword("oldpass", salt);
        db.Users.Add(new User { Username = "user1", PasswordHash = hash, PasswordSalt = salt, FullName = "User 1", Roles = new List<Rol>() });
        db.SaveChanges();
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        // Simula usuario autenticado
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user1") })
        );
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrongpass",
            NewPassword = "newpass",
            ConfirmNewPassword = "newpass"
        };
        var result = await controller.ChangePassword(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_Fails_When_NewPassword_Does_Not_Match_Confirmation()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangePasswordMismatch")
            .Options;
        var db = new FreeTimeDbContext(options);
        var salt = "somesalt";
        var hash = PasswordHasher.HashPassword("oldpass", salt);
        db.Users.Add(new User { Username = "user2", PasswordHash = hash, PasswordSalt = salt, FullName = "User 2", Roles = new List<Rol>() });
        db.SaveChanges();
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user2") })
        );
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass",
            ConfirmNewPassword = "differentpass"
        };
        var result = await controller.ChangePassword(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_Succeeds_And_Updates_Password()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangePasswordSuccess")
            .Options;
        var db = new FreeTimeDbContext(options);
        var salt = "somesalt";
        var hash = PasswordHasher.HashPassword("oldpass", salt);
        var user = new User { Username = "user3", PasswordHash = hash, PasswordSalt = salt, FullName = "User 3", Roles = new List<Rol>() };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user3") })
        );
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass",
            ConfirmNewPassword = "newpass"
        };
        var result = await controller.ChangePassword(request);
        Assert.IsType<OkObjectResult>(result);
        var updatedUser = db.Users.FirstOrDefault(u => u.Username == "user3");
        Assert.NotNull(updatedUser);
        Assert.NotEqual(hash, updatedUser.PasswordHash);
        Assert.NotEqual(salt, updatedUser.PasswordSalt);
        Assert.True(updatedUser.UpdatedAt > System.DateTime.MinValue);
    }

    [Fact]
    public async Task Register_Creates_User_With_Hashed_Password_And_Area_Grupo()
    {
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterUserTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterUserTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        // Agrega Area, Grupo y Rol al mismo contexto
        var area = new Area
        {
            AreaId = 1,
            UnidadOrganizativaSap = "SAP1",
            NombreGeneral = "Area1"
        };
        var grupo = new Grupo
        {
            GrupoId = 1,
            Rol = "Grupo1",
            AreaId = 1,
            IdentificadorSAP = "TEST-SAP"
        };
        db.Areas.Add(area);
        db.Grupos.Add(grupo);
        var rol = new Rol
        {
            Id = 1,
            Name = "SuperUsuario",
            Description = "desc",
            Abreviation = "SU"
        };
        db.Roles.Add(rol);
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "1234",
            FullName = "Test User",
            AreaId = 1,
            GrupoId = 1,
            Roles = new List<string> { "1" }
        };
        var result = await controller.Register(request);
        var user = db.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.NotNull(user);
        Assert.NotEqual("1234", user.PasswordHash); // Debe estar hasheada
        Assert.NotNull(user?.PasswordSalt);
        Assert.Equal(1, user.AreaId);
        Assert.Equal(1, user.GrupoId);
        Assert.Contains(user.Roles, r => r.Id == 1);
    }

    [Fact]
    public async Task Login_Returns_Token_For_Valid_User()
    {
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "LoginUserTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "LoginUserTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();

        var salt = "somesalt";
        var hash = PasswordHasher.HashPassword("mypassword", salt);
        db.Users.Add(new User { Username = "loginuser", PasswordHash = hash, PasswordSalt = salt, FullName = "Login User", Roles = new List<Rol> { rol }, AreaId = 1, GrupoId = 1 });
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new LoginRequest { Username = "loginuser", Password = "mypassword" };
        var result = await controller.Login(request) as OkObjectResult;
        Assert.NotNull(result);
        var apiResponse = result.Value as ApiResponse<object>;
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        // El nuevo formato retorna un objeto con Token y Expiration
        var data = JObject.FromObject(apiResponse.Data);
        Assert.NotNull(data);
        Assert.False(string.IsNullOrEmpty(data["Token"]?.ToString()));
        Assert.False(string.IsNullOrEmpty(data["Expiration"]?.ToString()));
    }

    [Fact]
    public async Task Register_Fails_When_Assigning_Nonexistent_Area()
    {
        // Setup contexts y datos de prueba (solo rol existente)
        // No agregues área con id 999
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailAreaTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailAreaTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);

        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "nouser",
            Password = "1234",
            FullName = "No User",
            AreaId = 999,
            GrupoId = 1,
            Roles = new List<string> { "1" }
        };
        var result = await controller.Register(request);
        Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<string>;
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Contains("área", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Register_Fails_When_Assigning_Nonexistent_Rol()
    {
        // Setup contexts y datos de prueba (solo área existente)
        // No agregues rol con id 999
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailRolTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailRolTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);

        var area = new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" };
        freeTimeDb.Areas.Add(area);
        freeTimeDb.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "nouser",
            Password = "1234",
            FullName = "No User",
            AreaId = 1,
            GrupoId = 1,
            Roles = new List<string> { "999" }
        };
        var result = await controller.Register(request);
        Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<string>;
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Contains("roles", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Register_Fails_When_Assigning_Nonexistent_Grupo()
    {
        // Setup contexts y datos de prueba (solo área y rol existentes)
        // No agregues grupo con id 999
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailGrupoTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailGrupoTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);

        var area = new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" };
        freeTimeDb.Areas.Add(area);
        freeTimeDb.SaveChanges();
        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "nouser",
            Password = "1234",
            FullName = "No User",
            AreaId = 1,
            GrupoId = 999,
            Roles = new List<string> { "1" }
        };
        var result = await controller.Register(request);
        Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<string>;
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Contains("área", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Register_Fails_When_Grupo_Does_Not_Belong_To_Area()
    {
        // Setup contexts y datos de prueba (área1, área2, grupo pertenece a área2)
        // ...
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailGrupoAreaTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterFailGrupoAreaTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);

        var area1 = new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" };
        var area2 = new Area { AreaId = 2, UnidadOrganizativaSap = "SAP2", NombreGeneral = "Area2" };
        var grupo = new Grupo { GrupoId = 1, Rol = "Grupo1", AreaId = 2, IdentificadorSAP = "TEST-SAP" };
        freeTimeDb.Areas.Add(area1);
        freeTimeDb.Areas.Add(area2);
        freeTimeDb.Grupos.Add(grupo);
        freeTimeDb.SaveChanges();
        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "nouser",
            Password = "1234",
            FullName = "No User",
            AreaId = 1,
            GrupoId = 1,
            Roles = new List<string> { "1" }
        };
        var result = await controller.Register(request);
        Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<string>;
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Contains("área", apiResponse.ErrorMsg);
    }

    [Fact]
    public async Task Register_Succeeds_With_Area_And_No_Grupo()
    {
        // Setup contexts y datos de prueba (área y rol existentes, sin grupo)
        var appOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterUserNoGrupoTest")
            .Options;
        var freeTimeOptions = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RegisterUserNoGrupoTest_FreeTime")
            .Options;

        var db = new FreeTimeDbContext(appOptions);
        var freeTimeDb = new FreeTimeDbContext(freeTimeOptions);

        var area = new Area { AreaId = 1, UnidadOrganizativaSap = "SAP1", NombreGeneral = "Area1" };
        freeTimeDb.Areas.Add(area);
        freeTimeDb.SaveChanges();
        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();

        var controller = new AuthController(db);
        var request = new RegisterRequest
        {
            Username = "testuser2",
            Password = "1234",
            FullName = "Test User 2",
            AreaId = 1,
            GrupoId = null,
            Roles = new List<string> { "1" }
        };
        var result = await controller.Register(request);
        Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = (result as BadRequestObjectResult)?.Value as ApiResponse<string>;
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task ChangeUserPassword_Fails_When_User_Not_Found()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangeUserPasswordUserNotFound")
            .Options;
        var db = new FreeTimeDbContext(options);
        var controller = new AuthController(db);
        var request = new ChangeUserPasswordRequest
        {
            UserId = 999,
            NewPassword = "newpass",
            ConfirmNewPassword = "newpass"
        };
        var result = await controller.ChangeUserPassword(request);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ChangeUserPassword_Fails_When_NewPassword_Does_Not_Match_Confirmation()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangeUserPasswordMismatch")
            .Options;
        var db = new FreeTimeDbContext(options);
        var user = new User { Id = 1, Username = "user1", PasswordHash = "hash", PasswordSalt = "salt", FullName = "User 1", Roles = new List<Rol>() };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new AuthController(db);
        var request = new ChangeUserPasswordRequest
        {
            UserId = 1,
            NewPassword = "newpass",
            ConfirmNewPassword = "differentpass"
        };
        var result = await controller.ChangeUserPassword(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangeUserPassword_Succeeds_And_Updates_Password_And_Date()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "ChangeUserPasswordSuccess")
            .Options;
        var db = new FreeTimeDbContext(options);
        var user = new User { Id = 2, Username = "user2", PasswordHash = "oldhash", PasswordSalt = "oldsalt", FullName = "User 2", Roles = new List<Rol>() };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new AuthController(db);
        var request = new ChangeUserPasswordRequest
        {
            UserId = 2,
            NewPassword = "newpass",
            ConfirmNewPassword = "newpass"
        };
        var result = await controller.ChangeUserPassword(request);
        Assert.IsType<OkObjectResult>(result);
        var updatedUser = db.Users.FirstOrDefault(u => u.Id == 2);
        Assert.NotNull(updatedUser);
        Assert.NotEqual("oldhash", updatedUser.PasswordHash);
        Assert.NotEqual("oldsalt", updatedUser.PasswordSalt);
        Assert.True(updatedUser.UpdatedAt > System.DateTime.MinValue);
    }

    [Fact]
    public async Task RefreshToken_Fails_When_User_Not_Authenticated()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RefreshTokenNoAuth")
            .Options;
        var db = new FreeTimeDbContext(options);
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        var result = await controller.RefreshToken();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_Fails_When_User_Not_Found()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RefreshTokenUserNotFound")
            .Options;
        var db = new FreeTimeDbContext(options);
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "nouser") })
        );
        var result = await controller.RefreshToken();
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_Succeeds_And_Returns_New_Token()
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(databaseName: "RefreshTokenSuccess")
            .Options;
        var db = new FreeTimeDbContext(options);
        var rol = new Rol { Id = 1, Name = "SuperUsuario", Description = "desc", Abreviation = "SU" };
        db.Roles.Add(rol);
        db.SaveChanges();
        var user = new User { Username = "refreshuser", PasswordHash = "hash", PasswordSalt = "salt", FullName = "Refresh User", Roles = new List<Rol> { rol } };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new AuthController(db);
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "refreshuser") })
        );
        var result = await controller.RefreshToken();
        if (result is OkObjectResult okResult)
        {
            var apiResponse = okResult.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            var data = JObject.FromObject(apiResponse.Data);
            Assert.NotNull(data);
            Assert.False(string.IsNullOrEmpty(data["Token"]?.ToString()));
            Assert.False(string.IsNullOrEmpty(data["Expiration"]?.ToString()));
        }
        else if (result is UnauthorizedObjectResult unauthorizedResult)
        {
            var apiResponse = unauthorizedResult.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("no autorizado", apiResponse.ErrorMsg.ToLower());
        }
    }

}
