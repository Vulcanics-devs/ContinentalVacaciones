using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using Xunit;
using FluentAssertions;

namespace tiempo_libre.Tests.Controllers
{
    public static class TestHelpers
    {
        public static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return dbSetMock;
        }
    }

    public class DiasInhabilesControllerTests
    {
        private Mock<ILogger<DiasInhabilesController>> _loggerMock = new();
        private Mock<FreeTimeDbContext> _dbMock = new();

        private DiasInhabilesController CreateControllerWithDb(List<DiasInhabiles> diasInhabiles, Exception? exception = null)
        {
            // Usar una lista interna para simular el DbSet y permitir LINQ
            var data = diasInhabiles;
            var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<DiasInhabiles>>();
            dbSetMock.As<IQueryable<DiasInhabiles>>().Setup(m => m.Provider).Returns(() => data.AsQueryable().Provider);
            dbSetMock.As<IQueryable<DiasInhabiles>>().Setup(m => m.Expression).Returns(() => data.AsQueryable().Expression);
            dbSetMock.As<IQueryable<DiasInhabiles>>().Setup(m => m.ElementType).Returns(() => data.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<DiasInhabiles>>().Setup(m => m.GetEnumerator()).Returns(() => data.AsQueryable().GetEnumerator());
            dbSetMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Callback<IEnumerable<DiasInhabiles>>(items => data.AddRange(items));
            dbSetMock.Setup(m => m.Add(It.IsAny<DiasInhabiles>())).Callback<DiasInhabiles>(item => data.Add(item));
            dbSetMock.Setup(m => m.Remove(It.IsAny<DiasInhabiles>())).Callback<DiasInhabiles>(item => data.Remove(item));
            dbSetMock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Callback<IEnumerable<DiasInhabiles>>(items => {
                foreach (var i in items.ToList()) data.Remove(i);
            });
            // Forzar que el mock siempre devuelva el estado actual de la lista
            dbSetMock.Setup(m => m.AsQueryable()).Returns(() => data.AsQueryable());
            _dbMock = new Mock<FreeTimeDbContext>();
            _dbMock.Setup(db => db.DiasInhabiles).Returns(dbSetMock.Object);
            if (exception != null)
            {
                _dbMock.Setup(db => db.SaveChanges()).Throws(exception);
            }
            else
            {
                _dbMock.Setup(db => db.SaveChanges()).Returns(1);
            }
            return new DiasInhabilesController(_loggerMock.Object);
        }
        private DiasInhabilesController CreateControllerWithDbSetMock(Mock<Microsoft.EntityFrameworkCore.DbSet<DiasInhabiles>> dbSetMock, Exception? exception = null)
        {
            _dbMock = new Mock<FreeTimeDbContext>();
            _dbMock.Setup(db => db.DiasInhabiles).Returns(dbSetMock.Object);
            if (exception != null)
            {
                _dbMock.Setup(db => db.SaveChanges()).Throws(exception);
            }
            else
            {
                _dbMock.Setup(db => db.SaveChanges()).Returns(1);
            }
            return new DiasInhabilesController(_loggerMock.Object);
        }

        [Fact]
        public void CreateDiasInhabiles_SingleDay_CreatesOneRecord()
        {
            // Arrange
            var dbSetMock = TestHelpers.CreateDbSetMock(new List<DiasInhabiles>());
            dbSetMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Verifiable();
            var controller = CreateControllerWithDbSetMock(dbSetMock);
            var request = new DiasInhabilesController.DiasInhabilesCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 4),
                FechaFinal = new DateOnly(2025, 9, 4),
                Detalles = "Unico",
                TipoActividadDelDia = (int)TipoActividadDelDiaEnum.IncidenciaOPermiso
            };
            // Act
            var result = controller.CreateDiasInhabiles(request, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var ids = response.Data as IEnumerable<int>;
            ids.Should().NotBeNull();
            ids.Count().Should().Be(1);
            response.ErrorMsg.Should().Contain("creados correctamente");
            dbSetMock.Verify(m => m.AddRange(It.Is<IEnumerable<DiasInhabiles>>(l => l.Count() == 1)), Times.Once);
        }

        [Fact]
        public void CreateDiasInhabiles_Range_CreatesMultipleRecords()
        {
            // Arrange
            var dbSetMock = TestHelpers.CreateDbSetMock(new List<DiasInhabiles>());
            dbSetMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Verifiable();
            var controller = CreateControllerWithDbSetMock(dbSetMock);
            var request = new DiasInhabilesController.DiasInhabilesCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 4),
                FechaFinal = new DateOnly(2025, 9, 6),
                Detalles = "Rango",
                TipoActividadDelDia = (int)TipoActividadDelDiaEnum.IncidenciaOPermiso
            };
            // Act
            var result = controller.CreateDiasInhabiles(request, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var ids = response.Data as IEnumerable<int>;
            ids.Should().NotBeNull();
            ids.Count().Should().Be(3);
            response.ErrorMsg.Should().Contain("creados correctamente");
            dbSetMock.Verify(m => m.AddRange(It.Is<IEnumerable<DiasInhabiles>>(l => l.Count() == 3)), Times.Once);
        }

        [Fact]
        public void CreateDiasInhabiles_DetallesDuplicado_ReturnsConflict()
        {
            // Arrange
            var existing = new List<DiasInhabiles> {
                new DiasInhabiles { 
                    Detalles = "Duplicado", 
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso,
                    FechaInicial = new DateOnly(2025, 9, 4),
                    FechaFinal = new DateOnly(2025, 9, 4)
                }
            };
            var dbSetMock = TestHelpers.CreateDbSetMock(existing);
            _dbMock = new Mock<FreeTimeDbContext>();
            _dbMock.Setup(db => db.DiasInhabiles).Returns(dbSetMock.Object);
            _dbMock.Setup(db => db.SaveChanges()).Returns(1);
            
            var controller = new DiasInhabilesController(_loggerMock.Object);
            var request = new DiasInhabilesController.DiasInhabilesCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 4),
                FechaFinal = new DateOnly(2025, 9, 4),
                Detalles = "Duplicado",
                TipoActividadDelDia = (int)TipoActividadDelDiaEnum.IncidenciaOPermiso
            };
            // Act
            var result = controller.CreateDiasInhabiles(request, _dbMock.Object) as ObjectResult;
            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(409);
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("Ya existe un día inhábil");
        }

        [Fact]
        public void CreateDiasInhabiles_TipoActividadInvalido_ReturnsBadRequest()
        {
            // Arrange
            var dbSetMock = TestHelpers.CreateDbSetMock(new List<DiasInhabiles>());
            var controller = CreateControllerWithDbSetMock(dbSetMock);
            var request = new DiasInhabilesController.DiasInhabilesCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 4),
                FechaFinal = new DateOnly(2025, 9, 4),
                Detalles = "TipoInvalido",
                TipoActividadDelDia = 999 // Valor inválido
            };
            // Act
            var result = controller.CreateDiasInhabiles(request, _dbMock.Object) as BadRequestObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("inválido");
        }

        [Fact]
        public void CreateDiasInhabiles_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var dbSetMock = TestHelpers.CreateDbSetMock(new List<DiasInhabiles>());
            dbSetMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Verifiable();
            var controller = CreateControllerWithDbSetMock(dbSetMock, new Exception("DB error"));
            var request = new DiasInhabilesController.DiasInhabilesCreateRequest
            {
                FechaInicial = new DateOnly(2025, 9, 4),
                FechaFinal = new DateOnly(2025, 9, 4),
                Detalles = "Unico",
                TipoActividadDelDia = (int)TipoActividadDelDiaEnum.IncidenciaOPermiso
            };
            // Act
            var result = controller.CreateDiasInhabiles(request, _dbMock.Object) as ObjectResult;
            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(500);
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("Error interno");
        }
        [Fact]
        public void DeleteDiasInhabil_SingleDay_DeletesOnlyThatDay()
        {
            // Arrange
            var dia = new DiasInhabiles { Id = 1, Detalles = "Unico", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso, FechaInicial = new DateOnly(2025, 9, 3), FechaFinal = new DateOnly(2025, 9, 3) };
            var controller = CreateControllerWithDb(new List<DiasInhabiles> { dia });
            _dbMock.Setup(db => db.DiasInhabiles.Remove(dia)).Verifiable();

            // Act
            var result = controller.DeleteDiasInhabil(1, _dbMock.Object) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().Be(1);
            response.ErrorMsg.Should().Contain("eliminaron 1 día(s)");
            _dbMock.Verify(db => db.DiasInhabiles.Remove(dia), Times.Once);
        }

        [Fact]
        public void DeleteDiasInhabil_Group_DeletesAllGroupDays()
        {
            // Arrange
            var grupo = new List<DiasInhabiles>
            {
                new DiasInhabiles { Id = 1, Detalles = "Grupo", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso, FechaInicial = new DateOnly(2025,9,3), FechaFinal = new DateOnly(2025,9,5) },
                new DiasInhabiles { Id = 2, Detalles = "Grupo", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso, FechaInicial = new DateOnly(2025,9,3), FechaFinal = new DateOnly(2025,9,5) },
                new DiasInhabiles { Id = 3, Detalles = "Grupo", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso, FechaInicial = new DateOnly(2025,9,3), FechaFinal = new DateOnly(2025,9,5) }
            };
            var controller = CreateControllerWithDb(grupo);
            _dbMock.Setup(db => db.DiasInhabiles.RemoveRange(It.IsAny<IEnumerable<DiasInhabiles>>())).Verifiable();

            // Act
            var result = controller.DeleteDiasInhabil(2, _dbMock.Object) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().Be(3);
            response.ErrorMsg.Should().Contain("eliminaron 3 día(s)");
            _dbMock.Verify(db => db.DiasInhabiles.RemoveRange(It.IsAny<IEnumerable<DiasInhabiles>>()), Times.Once);
        }

        [Fact]
        public void DeleteDiasInhabil_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateControllerWithDb(new List<DiasInhabiles>());

            // Act
            var result = controller.DeleteDiasInhabil(-1, _dbMock.Object) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("entero positivo");
        }

        [Fact]
        public void DeleteDiasInhabil_NotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateControllerWithDb(new List<DiasInhabiles>());

            // Act
            var result = controller.DeleteDiasInhabil(99, _dbMock.Object) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("No existe un día inhábil");
        }

        [Fact]
        public void DeleteDiasInhabil_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var dia = new DiasInhabiles { Id = 1, Detalles = "Unico", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso, FechaInicial = new DateOnly(2025, 9, 3), FechaFinal = new DateOnly(2025, 9, 3) };
            var controller = CreateControllerWithDb(new List<DiasInhabiles> { dia }, new Exception("DB error"));

            // Act
            var result = controller.DeleteDiasInhabil(1, _dbMock.Object) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(500);
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("Error interno");
        }

        [Fact]
        public void GetDiasInhabiles_SinParametros_RegresaTodosLosDias()
        {
            // Arrange
            int anioActual = DateTime.Now.Year;
            var dias = new List<DiasInhabiles>
            {
                new DiasInhabiles {
                    Id = 1,
                    Fecha = new DateOnly(anioActual, 1, 1),
                    FechaInicial = new DateOnly(anioActual, 1, 1),
                    FechaFinal = new DateOnly(anioActual, 1, 1),
                    Detalles = "A",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 2,
                    Fecha = new DateOnly(anioActual, 2, 2),
                    FechaInicial = new DateOnly(anioActual, 2, 2),
                    FechaFinal = new DateOnly(anioActual, 2, 2),
                    Detalles = "B",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 3,
                    Fecha = new DateOnly(anioActual-1, 3, 3),
                    FechaInicial = new DateOnly(anioActual-1, 3, 3),
                    FechaFinal = new DateOnly(anioActual-1, 3, 3),
                    Detalles = "C",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                }
            };
            var controller = CreateControllerWithDb(dias);
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(null, null, null, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var lista = response.Data as IEnumerable<object>;
            lista.Should().NotBeNull();
            lista.Count().Should().Be(3); // Todos los registros sin filtrar por año
        }

        [Fact]
        public void GetDiasInhabiles_RangoFechasValido_RegresaDiasEnRango()
        {
            // Arrange
            int anioActual = DateTime.Now.Year;
            var dias = new List<DiasInhabiles>
            {
                new DiasInhabiles {
                    Id = 1,
                    Fecha = new DateOnly(anioActual, 9, 1),
                    FechaInicial = new DateOnly(anioActual, 9, 1),
                    FechaFinal = new DateOnly(anioActual, 9, 1),
                    Detalles = "A",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 2,
                    Fecha = new DateOnly(anioActual, 9, 2),
                    FechaInicial = new DateOnly(anioActual, 9, 2),
                    FechaFinal = new DateOnly(anioActual, 9, 2),
                    Detalles = "B",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 3,
                    Fecha = new DateOnly(anioActual, 9, 3),
                    FechaInicial = new DateOnly(anioActual, 9, 3),
                    FechaFinal = new DateOnly(anioActual, 9, 3),
                    Detalles = "C",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                }
            };
            var controller = CreateControllerWithDb(dias);
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(new DateOnly(anioActual, 9, 2), new DateOnly(anioActual, 9, 3), null, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var lista = response.Data as IEnumerable<object>;
            lista.Should().NotBeNull();
            lista.Count().Should().Be(2);
        }

        [Fact]
        public void GetDiasInhabiles_RangoMayorAUnAnio_RegresaOk()
        {
            // Arrange
            var controller = CreateControllerWithDb(new List<DiasInhabiles>());
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(new DateOnly(2025, 1, 1), new DateOnly(2026, 2, 1), null, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
        }

        [Fact]
        public void GetDiasInhabiles_FechaInicioMayorQueFin_RegresaBadRequest()
        {
            // Arrange
            var controller = CreateControllerWithDb(new List<DiasInhabiles>());
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(new DateOnly(2025, 9, 5), new DateOnly(2025, 9, 4), null, _dbMock.Object) as BadRequestObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("fecha de inicio no puede ser mayor que la fecha de fin");
        }

        [Fact]
        public void GetDiasInhabiles_TipoDiaInhabilValido_RegresaSoloEseTipo()
        {
            // Arrange
            int anioActual = DateTime.Now.Year;
            var dias = new List<DiasInhabiles>
            {
                new DiasInhabiles {
                    Id = 1,
                    Fecha = new DateOnly(anioActual, 9, 1),
                    FechaInicial = new DateOnly(anioActual, 9, 1),
                    FechaFinal = new DateOnly(anioActual, 9, 1),
                    Detalles = "A",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 2,
                    Fecha = new DateOnly(anioActual, 9, 2),
                    FechaInicial = new DateOnly(anioActual, 9, 2),
                    FechaFinal = new DateOnly(anioActual, 9, 2),
                    Detalles = "B",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental
                },
            };
            var controller = CreateControllerWithDb(dias);
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(null, null, (int)TipoActividadDelDiaEnum.IncidenciaOPermiso, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var lista = response.Data as IEnumerable<object>;
            lista.Should().NotBeNull();
            lista.Count().Should().Be(1);
        }

        [Fact]
        public void GetDiasInhabiles_RangoYTipoDiaInhabilValido_RegresaSoloCoincidentes()
        {
            // Arrange
            int anioActual = DateTime.Now.Year;
            var dias = new List<DiasInhabiles>
            {
                new DiasInhabiles {
                    Id = 1,
                    Fecha = new DateOnly(anioActual, 9, 1),
                    FechaInicial = new DateOnly(anioActual, 9, 1),
                    FechaFinal = new DateOnly(anioActual, 9, 1),
                    Detalles = "A",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                },
                new DiasInhabiles {
                    Id = 2,
                    Fecha = new DateOnly(anioActual, 9, 2),
                    FechaInicial = new DateOnly(anioActual, 9, 2),
                    FechaFinal = new DateOnly(anioActual, 9, 2),
                    Detalles = "B",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental
                },
                new DiasInhabiles {
                    Id = 3,
                    Fecha = new DateOnly(anioActual, 9, 2),
                    FechaInicial = new DateOnly(anioActual, 9, 2),
                    FechaFinal = new DateOnly(anioActual, 9, 2),
                    Detalles = "C",
                    TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso
                }
            };
            var controller = CreateControllerWithDb(dias);
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(new DateOnly(anioActual, 9, 1), new DateOnly(anioActual, 9, 2), (int)TipoActividadDelDiaEnum.IncidenciaOPermiso, _dbMock.Object) as OkObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var lista = response.Data as IEnumerable<object>;
            lista.Should().NotBeNull();
            lista.Count().Should().Be(2);
        }

        [Fact]
        public void GetDiasInhabiles_TipoDiaInhabilInvalido_RegresaBadRequest()
        {
            // Arrange
            var controller = CreateControllerWithDb(new List<DiasInhabiles>());
            // Act
            var result = controller.GetDiasInhabilesDelAnioActual(null, null, 999, _dbMock.Object) as BadRequestObjectResult;
            // Assert
            result.Should().NotBeNull();
            var response = result.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("inválido");
        }

        [Fact]
        public void GetDiasInhabiles_Exception_RegresaInternalServerError()
        {
            // Arrange
            var dias = new List<DiasInhabiles>
            {
                new DiasInhabiles { Id = 1, Fecha = new DateOnly(2025, 9, 1), Detalles = "A", TipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso }
            };
            var dbSetMock = TestHelpers.CreateDbSetMock(dias);
            _dbMock = new Mock<FreeTimeDbContext>();
            _dbMock.Setup(db => db.DiasInhabiles).Throws(new Exception("DB error"));
            var controller = new DiasInhabilesController(_loggerMock.Object);
            // Act
            Action act = () => controller.GetDiasInhabilesDelAnioActual(null, null, null, _dbMock.Object);
            // Assert
            act.Should().Throw<Exception>().WithMessage("DB error*");
        }
    }
}
