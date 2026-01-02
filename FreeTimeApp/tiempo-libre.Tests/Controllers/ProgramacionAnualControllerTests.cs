using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using Xunit;
using FluentAssertions;

namespace tiempo_libre.Tests.Controllers
{
    public class ProgramacionAnualControllerTests
    {
        private FreeTimeDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        [Fact]
        public async Task GetActual_Returns_Actual_If_Exists()
        {
            var db = GetDb("GetActual1");
            db.ProgramacionesAnuales.Add(new ProgramacionesAnuales { Id = 1, Anio = 2025, Estatus = EstatusProgramacionAnualEnum.Pendiente, BorradoLogico = false, FechaInicia = DateTime.UtcNow, FechaTermina = DateTime.UtcNow });
            db.SaveChanges();
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.GetActual();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            var response = ok.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActual_Returns_NotFound_If_None()
        {
            var db = GetDb("GetActual2");
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.GetActual();
            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            var response = notFound.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Activar_Actualiza_Estatus()
        {
            var db = GetDb("Activar1");
            db.ProgramacionesAnuales.Add(new ProgramacionesAnuales { Id = 2, Anio = 2025, Estatus = EstatusProgramacionAnualEnum.Pendiente, BorradoLogico = false, FechaInicia = DateTime.UtcNow, FechaTermina = DateTime.UtcNow });
            db.SaveChanges();
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.Activar(2);
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            db.ProgramacionesAnuales.Find(2).Estatus.Should().Be(EstatusProgramacionAnualEnum.EnProceso);
            // Validar LoggerAcciones
            var log = await db.LoggerAcciones.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.Modelo.Should().Be("ProgramacionesAnuales");
            log.Accion.Should().Be(TiposDeAccionesEnum.Actualizacion);
        }

        [Fact]
        public async Task Activar_Returns_NotFound_If_Not_Exists()
        {
            var db = GetDb("Activar2");
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.Activar(99);
            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
        }

        [Fact]
        public async Task Reprogramar_Actualiza_Estatus()
        {
            var db = GetDb("Reprog1");
            db.ProgramacionesAnuales.Add(new ProgramacionesAnuales { Id = 3, Anio = 2025, Estatus = EstatusProgramacionAnualEnum.Pendiente, BorradoLogico = false, FechaInicia = DateTime.UtcNow, FechaTermina = DateTime.UtcNow });
            db.SaveChanges();
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.Reprogramar(3);
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            db.ProgramacionesAnuales.Find(3).Estatus.Should().Be(EstatusProgramacionAnualEnum.Reprogramacion);
            // Validar LoggerAcciones
            var log = await db.LoggerAcciones.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.Modelo.Should().Be("ProgramacionesAnuales");
            log.Accion.Should().Be(TiposDeAccionesEnum.Actualizacion);
        }

        [Fact]
        public async Task Cerrar_Actualiza_Estatus()
        {
            var db = GetDb("Cerrar1");
            db.ProgramacionesAnuales.Add(new ProgramacionesAnuales { Id = 4, Anio = 2025, Estatus = EstatusProgramacionAnualEnum.Pendiente, BorradoLogico = false, FechaInicia = DateTime.UtcNow, FechaTermina = DateTime.UtcNow });
            db.SaveChanges();
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.Cerrar(4);
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            db.ProgramacionesAnuales.Find(4).Estatus.Should().Be(EstatusProgramacionAnualEnum.Cerrada);
            // Validar LoggerAcciones
            var log = await db.LoggerAcciones.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.Modelo.Should().Be("ProgramacionesAnuales");
            log.Accion.Should().Be(TiposDeAccionesEnum.Actualizacion);
        }

        [Fact]
        public async Task BorrarActual_Cambia_BorradoLogico()
        {
            var db = GetDb("Borrar1");
            db.ProgramacionesAnuales.Add(new ProgramacionesAnuales { Id = 5, Anio = 2025, Estatus = EstatusProgramacionAnualEnum.EnProceso, BorradoLogico = false, FechaInicia = DateTime.UtcNow, FechaTermina = DateTime.UtcNow });
            db.SaveChanges();
            var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<tiempo_libre.Controllers.ProgramacionAnualController>>();
            var controller = new ProgramacionAnualController(db, loggerMock.Object);
            var result = await controller.BorrarActual();
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            db.ProgramacionesAnuales.Find(5).BorradoLogico.Should().BeTrue();
            // Validar LoggerAcciones
            var log = await db.LoggerAcciones.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.Modelo.Should().Be("ProgramacionesAnuales");
            log.Accion.Should().Be(TiposDeAccionesEnum.Eliminacion);
        }
    }
}
