using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using Xunit;

namespace tiempo_libre.Tests.Controllers
{
    public class IncidenciaOPermisosControllerRetrieveTest
    {
        private FreeTimeDbContext GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new FreeTimeDbContext(options);
            // Seed users with all required properties
            var roles = new List<Rol>();
            var user1 = new User
            {
                Id = 1,
                FullName = "Empleado Uno",
                Username = "empleado1",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = roles,
                Status = UserStatus.Activo,
                AreaId = 1,
                GrupoId = 1,
                Nomina = 100
            };
            var user2 = new User
            {
                Id = 2,
                FullName = "Sindicato Dos",
                Username = "sindicato2",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = roles,
                Status = UserStatus.Activo,
                AreaId = 1,
                GrupoId = 1,
                Nomina = 200
            };
            db.Users.Add(user1);
            db.Users.Add(user2);

            // Dummy Grupo y Regla para evitar null!
            var grupo = new Grupo
            {
                GrupoId = 1,
                AreaId = 1,
                Rol = "Rol",
                IdentificadorSAP = "SAP",
                Area = null
            };
            var regla = new Regla
            {
                Id = 1,
                Nombre = "Regla",
                Descripcion = "Desc",
                NumDeGrupos = 1,
                Prioridad = 1
            };

            // Seed incidencias con todos los requeridos
            db.IncidenciasOPermisos.AddRange(new List<IncidenciaOPermiso>
            {
                new IncidenciaOPermiso {
                    Id = 1,
                    Fecha = new DateOnly(2025, 9, 1),
                    FechaInicial = new DateOnly(2025, 9, 1),
                    FechaFinal = new DateOnly(2025, 9, 1),
                    DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
                    FechaRegistro = DateTime.Now,
                    NominaEmpleado = 100,
                    IdUsuarioEmpleado = 1,
                    TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce,
                    UsuarioEmpleado = user1,
                    UsuarioAutoriza = user1,
                    Grupo = grupo,
                    Regla = regla
                },
                new IncidenciaOPermiso {
                    Id = 2,
                    Fecha = new DateOnly(2025, 9, 2),
                    FechaInicial = new DateOnly(2025, 9, 2),
                    FechaFinal = new DateOnly(2025, 9, 2),
                    DiaDeLaSemana = DiasDeLaSemanaEnum.Martes,
                    FechaRegistro = DateTime.Now,
                    NominaEmpleado = 200,
                    IdUsuarioEmpleado = 2,
                    TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce,
                    UsuarioEmpleado = user2,
                    UsuarioAutoriza = user2,
                    Grupo = grupo,
                    Regla = regla
                }
            });
            db.SaveChanges();
            return db;
        }

        private IncidenciaOPermisosController GetController(FreeTimeDbContext db)
        {
            var loggerMock = new Mock<ILogger<IncidenciaOPermisosController>>();
            return new IncidenciaOPermisosController(db, loggerMock.Object);
        }

        [Fact]
        public void RetrieveIncidencias_NoParams_ReturnsAllIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Equal(2, (int)response.Data.Count);
            Assert.Equal(2, ((IList<IncidenciaOPermisoRetrieveDTO>)response.Data.Items).Count);
        }

        [Fact]
        public void RetrieveIncidencias_WithValidIdUsuarioEmpleado_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioEmpleado = 1
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Single(response.Data.Items);
            Assert.IsType<int>(response.Data.Items[0].Id);
            Assert.Equal(1, (int)response.Data.Items[0].Id);
        }

        [Fact]
        public void RetrieveIncidencias_WithValidDateRange_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                FechaInicial = new DateTime(2025, 9, 1),
                FechaFinal = new DateTime(2025, 9, 1)
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Single(response.Data.Items);
            Assert.IsType<int>(response.Data.Items[0].Id);
            Assert.Equal(1, (int)response.Data.Items[0].Id);
        }

        [Fact]
        public void RetrieveIncidencias_WithIdUsuarioEmpleadoAndDateRange_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioEmpleado = 1,
                FechaInicial = new DateTime(2025, 9, 1),
                FechaFinal = new DateTime(2025, 9, 1)
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Single(response.Data.Items);
            Assert.IsType<int>(response.Data.Items[0].Id);
            Assert.Equal(1, (int)response.Data.Items[0].Id);
        }

        [Fact]
        public void RetrieveIncidencias_DateRangeGreaterThanOneYear_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                FechaInicial = new DateTime(2024, 1, 1),
                FechaFinal = new DateTime(2025, 9, 1)
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("El rango de fechas no puede ser mayor a un año", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_FechaInicialGreaterThanFechaFinal_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                FechaInicial = new DateTime(2025, 9, 2),
                FechaFinal = new DateTime(2025, 9, 1)
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("La fecha inicial no puede ser mayor que la fecha final", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_WithValidIdUsuarioSindicato_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            // Agregar usuario sindicato a la incidencia
            var sindicato = db.Users.First(u => u.Id == 2);
            db.IncidenciasOPermisos.First(i => i.Id == 1).UsuarioSindicato = sindicato;
            db.SaveChanges();
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioSindicato = 2
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Single(response.Data.Items);
            Assert.Equal(1, (int)response.Data.Items[0].Id);
        }

        [Fact]
        public void RetrieveIncidencias_WithValidGrupoId_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                GrupoId = 1
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Equal(2, ((IList<IncidenciaOPermisoRetrieveDTO>)response.Data.Items).Count);
        }

        [Fact]
        public void RetrieveIncidencias_WithValidReglaId_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                ReglaId = 1
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Equal(2, ((IList<IncidenciaOPermisoRetrieveDTO>)response.Data.Items).Count);
        }

        [Fact]
        public void RetrieveIncidencias_WithMultipleValidFilters_ReturnsFilteredIncidencias()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            // Agregar usuario sindicato a la incidencia
            var sindicato = db.Users.First(u => u.Id == 2);
            db.IncidenciasOPermisos.First(i => i.Id == 1).UsuarioSindicato = sindicato;
            db.SaveChanges();
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioEmpleado = 1,
                FechaInicial = new DateTime(2025, 9, 1),
                FechaFinal = new DateTime(2025, 9, 1),
                IdUsuarioSindicato = 2,
                GrupoId = 1,
                ReglaId = 1
            };
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.True(response.Success);
            Assert.Single(response.Data.Items);
            Assert.Equal(1, (int)response.Data.Items[0].Id);
        }

        [Fact]
        public void RetrieveIncidencias_WithNonExistentIdUsuarioEmpleado_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioEmpleado = 999
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("El IdUsuarioEmpleado 999 no existe", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_WithNonExistentIdUsuarioSindicato_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                IdUsuarioSindicato = 999
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("El IdUsuarioSindicato 999 no existe", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_WithNonExistentGrupoId_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                GrupoId = 999
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("El GrupoId 999 no existe", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_WithNonExistentReglaId_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest
            {
                ReglaId = 999
            };
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("El ReglaId 999 no existe", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_WhenDbContextDisposed_ReturnsInternalServerError()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            db.Dispose(); // Provoca ObjectDisposedException

            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            dynamic response = result.Value;
            Assert.False(response.Success);
            Assert.Equal("Error interno al consultar las incidencias", (string)response.ErrorMsg);
        }

        [Fact]
        public void RetrieveIncidencias_InvalidModel_ReturnsBadRequest()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            // Simular modelo inválido
            controller.ModelState.AddModelError("IdUsuarioEmpleado", "Requerido");
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.False(response.Success);
        }

        [Fact]
        public void RetrieveIncidencias_ResponseHasCorrectCount()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            Assert.Equal(2, (int)response.Data.Count);
        }

        [Fact]
        public void RetrieveIncidencias_ResponseHasCorrectDTOData()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            var items = (IList<IncidenciaOPermisoRetrieveDTO>)response.Data.Items;
            Assert.Equal(2, items.Count);
            Assert.Equal(1, items[0].Id);
            Assert.Equal("Empleado Uno", items[0].UsuarioEmpleado.FullName);
            Assert.Equal(2, items[1].Id);
            Assert.Equal("Sindicato Dos", items[1].UsuarioEmpleado.FullName);
        }

        [Fact]
        public void RetrieveIncidencias_ResponseIsApiResponse()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            var response = result.Value;
            Assert.Equal("ApiResponse`1", response.GetType().Name);
        }

        [Fact]
        public void RetrieveIncidencias_ResponseUsesCorrectDTOType()
        {
            var db = GetDbContextWithData();
            var controller = GetController(db);
            var request = new IncidenciaOPermisosController.IncidenciaOPermisoRetrieveRequest();
            var result = controller.RetrieveIncidencias(request) as OkObjectResult;
            Assert.NotNull(result);
            dynamic response = result.Value;
            var items = response.Data.Items;
            foreach (var item in items)
            {
                Assert.IsType<IncidenciaOPermisoRetrieveDTO>(item);
            }
        }

    }
}
