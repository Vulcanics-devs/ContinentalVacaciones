using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models;
using tiempo_libre.Logic;
using tiempo_libre.Models.Enums;
using Xunit;

namespace tiempo_libre.Tests.Logic
{
    public class AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActualTests
    {
        private FreeTimeDbContext GetDb(string dbName = null)
        {
            var name = dbName ?? Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new FreeTimeDbContext(options);
        }

        private User CreateUser(int id, int nomina, int diasParaAsignar, int areaid, int grupoId, Rol rolSindicalizado)
        {
            return new User
            {
                Id = id,
                Nomina = nomina,
                FullName = $"Empleado {id}",
                Username = $"empleado{id}",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                AreaId = areaid,
                GrupoId = grupoId,
                DiasDeVacacionesAsignados = 0,
                VacacionesPorAntiguedad = new VacacionesPorAntiguedad { DiasParaAsignarAutomaticamente = diasParaAsignar },
                Roles = new List<Rol> { rolSindicalizado }
            };
        }

        private Grupo CreateGrupo(int id, int areaId)
        {
            return new Grupo {
                GrupoId = id,
                AreaId = areaId,
                Rol = "Sindicalizado",
                IdentificadorSAP = $"SAP{id}",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
        }

        private ProgramacionesAnuales CreateProgramacionAnual(DateTime inicio, DateTime fin)
        {
            return new ProgramacionesAnuales {
                Id = 1,
                Anio = inicio.Year,
                FechaInicia = inicio,
                FechaTermina = fin,
                Estatus = EstatusProgramacionAnualEnum.EnProceso,
                BorradoLogico = false
            };
        }

        [Fact]
        public void AsignaVacaciones_EmpleadosEnRango_AsignacionCorrecta()
        {
            // Arrange
            var db = GetDb();
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var grupo = CreateGrupo(1, 1);
            db.Grupos.Add(grupo);
            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();
            var user = CreateUser(1, 100, 2, 1, 1, rolSindicalizado);
            db.Users.Add(user);
            db.SaveChanges();
            var programacion = CreateProgramacionAnual(DateTime.Today.AddDays(-28), DateTime.Today.AddDays(28));
            db.ProgramacionesAnuales.Add(programacion);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);
            var generatorMock = new Mock<EmployeesCalendarsGenerator>(db, loggerMock.Object, programacion.FechaInicia, programacion.FechaTermina, manning) { CallBase = true };
            generatorMock.Setup(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(2);
            // Act
            generatorMock.Object.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(new List<User> { user });
            // Assert
            generatorMock.Verify(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.Is<User>(u => u.Id == user.Id), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se asignaron")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void NoHayEmpleadosSindicalizados_RegresaSinHacerNada()
        {
            // Arrange
            var db = GetDb();
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Today, DateTime.Today.AddDays(30), manning);
            // Act
            generator.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(new List<User>());
            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No hay empleados sindicalizados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void NoHayProgramacionAnualActiva_RegresaSinHacerNada()
        {
            // Arrange
            var db = GetDb();
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var grupo = CreateGrupo(1, 1);
            db.Grupos.Add(grupo);

            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();

            var user = CreateUser(1, 100, 2, 1, 1, rolSindicalizado);
            db.Users.Add(user);
            db.SaveChanges();

            var manning = new CalculosSobreManning(db);

            // var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            // var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Today, DateTime.Today.AddDays(30), manning);
            // Act
            generator.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(new List<User> { user });
            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No hay una Programación Anual activa")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void EmpleadoNoExistente_ManejaExcepcionYContinua()
        {
            // Arrange
            var db = GetDb();
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var grupo = CreateGrupo(1, 1);
            db.Grupos.Add(grupo);
            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();
            var user = CreateUser(1, 100, 2, 1, 1, rolSindicalizado);
            db.Users.Add(user);
            db.SaveChanges();
            var programacion = CreateProgramacionAnual(DateTime.Today.AddDays(-28), DateTime.Today.AddDays(28));
            db.ProgramacionesAnuales.Add(programacion);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generatorMock = new Mock<EmployeesCalendarsGenerator>(db, loggerMock.Object, programacion.FechaInicia, programacion.FechaTermina, manning) { CallBase = true };
            generatorMock.Setup(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.Is<User>(u => u != null), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(2);
            // Act
            generatorMock.Object.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(new List<User> { null, user });
            // Assert
            generatorMock.Verify(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                null, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            generatorMock.Verify(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.Is<User>(u => u != null), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("El empleado sindicalizado es nulo")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ErrorAlAsignarVacaciones_ManejaExcepcionYContinua()
        {
            // Arrange
            var db = GetDb();
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var grupo = CreateGrupo(1, 1);
            db.Grupos.Add(grupo);
            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();
            var user1 = CreateUser(1, 100, 2, 1, 1, rolSindicalizado);
            var user2 = CreateUser(2, 101, 2, 1, 1, rolSindicalizado);
            db.Users.Add(user1);
            db.Users.Add(user2);
            db.SaveChanges();
            var programacion = CreateProgramacionAnual(DateTime.Today.AddDays(-28), DateTime.Today.AddDays(28));
            db.ProgramacionesAnuales.Add(programacion);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generatorMock = new Mock<EmployeesCalendarsGenerator>(db, logger, programacion.FechaInicia, programacion.FechaTermina, manning) { CallBase = true };
            generatorMock.SetupSequence(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.IsAny<User>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Error al asignar vacaciones"))
                .Returns(2);
            // Act
            Action act = () => generatorMock.Object.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(new List<User> { user1, user2 });
            // Assert
            act.Should().NotThrow();
            generatorMock.Verify(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.Is<User>(u => u.Id == user1.Id), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            generatorMock.Verify(g => g.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(
                It.Is<User>(u => u.Id == user2.Id), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No hay empleados sindicalizados")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
        }
    }
}
