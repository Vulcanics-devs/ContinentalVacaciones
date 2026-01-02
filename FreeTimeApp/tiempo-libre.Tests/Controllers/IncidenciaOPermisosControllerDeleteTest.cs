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
using FluentAssertions;

namespace tiempo_libre.Tests.Controllers
{
    public class IncidenciaOPermisosControllerDeleteTest
    {
        private readonly Mock<FreeTimeDbContext> _dbMock;
        private readonly Mock<ILogger<IncidenciaOPermisosController>> _loggerMock;
        public IncidenciaOPermisosControllerDeleteTest()
        {
            _dbMock = new Mock<FreeTimeDbContext>();
            _loggerMock = new Mock<ILogger<IncidenciaOPermisosController>>();
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

        [Fact]
        public void DeleteIncidencia_SoloEliminaUnaSiNoEsGrupo()
        {
            // Arrange
            var incidencia = new IncidenciaOPermiso { Id = 1, Detalles = "A", TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, IdUsuarioEmpleado = 10, FechaInicial = new DateOnly(2025,9,1), FechaFinal = new DateOnly(2025,9,1) };
            var dbSet = CreateMockDbSet(new List<IncidenciaOPermiso> { incidencia });
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(dbSet.Object);
            _dbMock.Setup(db => db.SaveChanges()).Returns(1);
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            // Act
            var result = controller.DeleteIncidencia(1) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(new { eliminados = 1 });
        }

        [Fact]
        public void DeleteIncidencia_EliminaGrupoCompleto()
        {
            // Arrange
            var incidencia1 = new IncidenciaOPermiso { Id = 1, Detalles = "A", TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, IdUsuarioEmpleado = 10, FechaInicial = new DateOnly(2025,9,1), FechaFinal = new DateOnly(2025,9,3) };
            var incidencia2 = new IncidenciaOPermiso { Id = 2, Detalles = "A", TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, IdUsuarioEmpleado = 10, FechaInicial = new DateOnly(2025,9,1), FechaFinal = new DateOnly(2025,9,3) };
            var incidencia3 = new IncidenciaOPermiso { Id = 3, Detalles = "A", TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, IdUsuarioEmpleado = 10, FechaInicial = new DateOnly(2025,9,1), FechaFinal = new DateOnly(2025,9,3) };
            var dbSet = CreateMockDbSet(new List<IncidenciaOPermiso> { incidencia1, incidencia2, incidencia3 });
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(dbSet.Object);
            _dbMock.Setup(db => db.SaveChanges()).Returns(3);
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            // Act
            var result = controller.DeleteIncidencia(2) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(new { eliminados = 3 });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void DeleteIncidencia_IdInvalido_ReturnsBadRequest(int id)
        {
            // Arrange
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            // Act
            var result = controller.DeleteIncidencia(id) as BadRequestObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("entero positivo");
        }

        [Fact]
        public void DeleteIncidencia_NoExiste_ReturnsNotFound()
        {
            // Arrange
            var dbSet = CreateMockDbSet(new List<IncidenciaOPermiso>());
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(dbSet.Object);
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            // Act
            var result = controller.DeleteIncidencia(99) as NotFoundObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("No se encontró");
        }

        [Fact]
        public void DeleteIncidencia_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var incidencia = new IncidenciaOPermiso { Id = 1, Detalles = "A", TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, IdUsuarioEmpleado = 10, FechaInicial = new DateOnly(2025,9,1), FechaFinal = new DateOnly(2025,9,1) };
            var dbSet = CreateMockDbSet(new List<IncidenciaOPermiso> { incidencia });
            _dbMock.Setup(db => db.IncidenciasOPermisos).Returns(dbSet.Object);
            _dbMock.Setup(db => db.SaveChanges()).Throws(new Exception("DB error"));
            var controller = new IncidenciaOPermisosController(_dbMock.Object, _loggerMock.Object);
            // Act
            var result = controller.DeleteIncidencia(1) as ObjectResult;
            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(500);
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("Error interno");
        }
    }
}
