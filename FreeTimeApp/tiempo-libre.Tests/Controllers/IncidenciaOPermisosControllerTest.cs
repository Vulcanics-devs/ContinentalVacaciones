using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using Xunit;
using System.Security.Claims;

namespace tiempo_libre.Tests.Controllers
{
    public class IncidenciaOPermisosControllerTest
    {
        [Fact]
        public void CreateIncidencias_ValidSinNominaEmpleado_ReturnsCreated()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public void CreateIncidencias_ValidConIdUsuarioSindicato_ReturnsCreated()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1,
                IdUsuarioSindicato = 2
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var sindicato = new User { Id = 2, FullName = "Sindicato", Username = "sindicato1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            var autoriza = new User { Id = 3, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, sindicato, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(3));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public void CreateIncidencias_RangoDeFechas_CreaVariasIncidencias()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 3),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            Assert.Equal(3, response.Data.Count);
        }

        [Fact]
        public void CreateIncidencias_IdUsuarioSindicatoOpcional()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            Assert.Null(response.Data[0].UsuarioSindicato);
        }

        [Fact]
        public void CreateIncidencias_Exception_ReturnsInternalServerError()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges()).Throws(new Exception("DB error"));
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return mockSet;
        }

        private Mock<FreeTimeDbContext> _dbMock;
        private Mock<ILogger<IncidenciaOPermisosController>> _loggerMock;

        public IncidenciaOPermisosControllerTest()
        {
            _dbMock = new Mock<FreeTimeDbContext>();
            _loggerMock = new Mock<ILogger<IncidenciaOPermisosController>>();
        }

        private ClaimsPrincipal GetUserWithId(int id)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        private IncidenciaOPermisosController CreateController(ClaimsPrincipal user = null)
        {
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            if (user != null)
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            return controller;
        }

        [Fact]
        public void CreateIncidencias_ValidData_ReturnsCreated()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                Detalles = "Test",
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1,
                NominaEmpleado = 123
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var autoriza = new User
            {
                Id = 2,
                FullName = "Autoriza",
                Username = "autoriza1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 0,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user, autoriza });
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var dbSetIncidencias = new Mock<DbSet<IncidenciaOPermiso>>();
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(dbSetIncidencias.Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            // Mockear los DbSet adicionales requeridos por el controlador
            var dbSetCalendario = CreateMockDbSet(new List<DiasCalendarioEmpleado>());
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(dbSetCalendario.Object);
            var dbSetGrupos = CreateMockDbSet(new List<Grupo>());
            _dbMock.Setup(db => db.Grupos).Returns(dbSetGrupos.Object);
            var dbSetReglas = CreateMockDbSet(new List<Regla>());
            _dbMock.Setup(db => db.Reglas).Returns(dbSetReglas.Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public void CreateIncidencias_RangoMayorUnAnio_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 1, 1),
                FechaFinal = new DateOnly(2026, 2, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user });
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_FechaInicialMayorFechaFinal_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 2),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user });
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_TipoIncidenciaInvalido_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = 999, // inválido
                IdUsuarioEmpleado = 1
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user });
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_IdUsuarioEmpleadoInvalido_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 0 // inválido
            };
            var dbSetUser = CreateMockDbSet(new List<User>()); // vacío
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_IdUsuarioEmpleadoNoExiste_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 99 // no existe
            };
            var dbSetUser = CreateMockDbSet(new List<User>()); // vacío
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_IdUsuarioSindicatoNoExiste_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1,
                IdUsuarioSindicato = 99 // no existe
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user }); // no existe 99
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_NominaNoCoincide_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1,
                NominaEmpleado = 999 // no coincide
            };
            var user = new User
            {
                Id = 1,
                FullName = "Empleado",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol>(),
                Nomina = 123,
                Status = UserStatus.Activo
            };
            var dbSetUser = CreateMockDbSet(new List<User> { user });
            _dbMock.Setup(db => db.Users).Returns(dbSetUser.Object);
            var controller = CreateController(GetUserWithId(1));
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_ModeloInvalido_ReturnsBadRequest()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest(); // Sin datos obligatorios
            var controller = CreateController(GetUserWithId(1));
            controller.ModelState.AddModelError("FechaInicial", "Required");
            controller.ModelState.AddModelError("FechaFinal", "Required");
            controller.ModelState.AddModelError("TiposDeIncedencia", "Required");
            controller.ModelState.AddModelError("IdUsuarioEmpleado", "Required");
            var result = controller.CreateIncidencias(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateIncidencias_IdUsuarioAutoiza_TokenCorrecto()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            Assert.True(response.Data.All(i => i.UsuarioAutoriza != null && i.UsuarioAutoriza.Id == 2));
        }

        [Fact]
        public void CreateIncidencias_CamposCalculados_AsignadosCorrectamente()
        {
            var fechaInicial = new DateOnly(2025, 9, 1);
            var fechaFinal = new DateOnly(2025, 9, 1);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = fechaInicial,
                FechaFinal = fechaFinal,
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            var grupo = new Grupo { GrupoId = 10, Rol = "TestRol" };
            var regla = new Regla { Id = 20, Nombre = "TestRegla" };
            var calendario = new DiasCalendarioEmpleado { IdUsuarioEmpleadoSindicalizado = 1, FechaDelDia = fechaInicial, IdGrupo = grupo.GrupoId, IdRegla = regla.Id };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado> { calendario }).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo> { grupo }).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla> { regla }).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            var dto = response.Data.First();
            Assert.Equal((DiasDeLaSemanaEnum)fechaInicial.DayOfWeek, dto.DiaDeLaSemana);
            Assert.True((DateTime.Now - dto.FechaRegistro).TotalSeconds < 5); // FechaRegistro reciente
            Assert.Equal(grupo.GrupoId, dto.IdGrupo);
            Assert.Equal(regla.Id, dto.IdRegla);
            Assert.NotNull(dto.Grupo);
            Assert.NotNull(dto.Regla);
        }

        [Fact]
        public void CreateIncidencias_ContadorIncidenciasCorrecto()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 3),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            Assert.Equal(3, response.Data.Count);
            Assert.Contains("3 incidencia", response.ErrorMsg);
        }

        [Fact]
        public void CreateIncidencias_RespuestaApiResponseYDTOCorrecto()
        {
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 1),
                FechaFinal = new DateOnly(2025, 9, 1),
                TiposDeIncedencia = (int)TiposDeIncidenciasEnum.PermisoConGoce,
                IdUsuarioEmpleado = 1
            };
            var user = new User { Id = 1, FullName = "Empleado", Username = "empleado1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 123, Status = UserStatus.Activo };
            var autoriza = new User { Id = 2, FullName = "Autoriza", Username = "autoriza1", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>(), Nomina = 0, Status = UserStatus.Activo };
            _dbMock.Setup(db => db.Users).Returns(CreateMockDbSet(new List<User> { user, autoriza }).Object);
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(new Mock<DbSet<IncidenciaOPermiso>>().Object);
            _dbMock.Setup(db => db.SaveChanges(default)).Returns(1);
            _dbMock.Setup(db => db.DiasCalendarioEmpleado).Returns(CreateMockDbSet(new List<DiasCalendarioEmpleado>()).Object);
            _dbMock.Setup(db => db.Grupos).Returns(CreateMockDbSet(new List<Grupo>()).Object);
            _dbMock.Setup(db => db.Reglas).Returns(CreateMockDbSet(new List<Regla>()).Object);
            var controller = CreateController(GetUserWithId(2));
            var result = controller.CreateIncidencias(request);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            var response = objectResult.Value as ApiResponse<List<IncidenciaOPermisoCreatedDTO>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.IsType<List<IncidenciaOPermisoCreatedDTO>>(response.Data);
        }
    }
}
