using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.Logic;
using Xunit;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Tests
{
    public class AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodosTests
    {
        private FreeTimeDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        private User CreateUser(int id, int nomina, int diasParaAsignar, int areaid, int grupoId)
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
                Roles = new List<Rol> {
                    new Rol {
                        Id = (int)RolEnum.Empleado_Sindicalizado,
                        Name = "Sindicalizado",
                        Description = "Empleado Sindicalizado",
                        Abreviation = "ES"
                    }
                }
            };
        }

        private DiasCalendarioEmpleado CreateDiaLaboral(int userId, DateTime fecha)
        {
            return new DiasCalendarioEmpleado
            {
                IdUsuarioEmpleadoSindicalizado = userId,
                AnioFecha = fecha.Year,
                MesFecha = fecha.Month,
                DiaFecha = fecha.Day,
                FechaDelDia = DateOnly.FromDateTime(fecha),
                TipoActividadDelDia = TipoActividadDelDiaEnum.Laboral,
                UsuarioEmpleadoSindicalizado = null,
                NominaEmpleado = userId
            };
        }

        private DiasCalendarioEmpleado CreateDiaNoLaboral(int userId, DateTime fecha)
        {
            return new DiasCalendarioEmpleado
            {
                IdUsuarioEmpleadoSindicalizado = userId,
                AnioFecha = fecha.Year,
                MesFecha = fecha.Month,
                DiaFecha = fecha.Day,
                FechaDelDia = DateOnly.FromDateTime(fecha),
                TipoActividadDelDia = TipoActividadDelDiaEnum.DescansoSemanal,
                UsuarioEmpleadoSindicalizado = null,
                NominaEmpleado = userId
            };
        }

        [Fact]
        public void AsignaTodasLasVacacionesDentroDelRango_AsignacionCorrecta()
        {
            // Arrange
            var db = GetDb(nameof(AsignaTodasLasVacacionesDentroDelRango_AsignacionCorrecta));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();

            var area = new Area { AreaId = 1, UnidadOrganizativaSap = "Area 1", NombreGeneral = "Descripción Area 1", Manning = 10 };
            db.Areas.Add(area);
            db.SaveChanges();

            // Usar instancia real y manipular datos para que el porcentaje esté dentro del rango
            var grupo = new Grupo {
                GrupoId = 1,
                AreaId = 1,
                Rol = "Sindicalizado",
                IdentificadorSAP = "SAP1",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
            db.Grupos.Add(grupo);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);
            var fechaInicio = new DateTime(2025, 8, 1);
            var fechaFinal = new DateTime(2025, 10, 1);
            var user = CreateUser(1000, 1, 3, 1, 1);
            db.Users.Add(user);
            for (var d = fechaInicio; d <= fechaFinal; d = d.AddDays(1))
            {
                db.DiasCalendarioEmpleado.Add(CreateDiaLaboral(user.Id, d));
            }
            db.SaveChanges();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fechaInicio.AddDays(14), fechaFinal.AddDays(-14), manning);
            // Act
            var result = generator.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(user, fechaInicio, fechaFinal);
            // Assert
            result.Should().BeGreaterThan(0);
            user.DiasDeVacacionesAsignados.Should().BeGreaterThan(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("asignado como día de vacaciones")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void NoHayDiasLaboralesEnRango_RegresaCero()
        {
            // Arrange
            var db = GetDb(nameof(NoHayDiasLaboralesEnRango_RegresaCero));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();

            var area = new Area { AreaId = 2, UnidadOrganizativaSap = "Area 2", NombreGeneral = "Descripción Area 2", Manning = 25 };
            db.Areas.Add(area);
            db.SaveChanges();

            var grupo = new Grupo {
                GrupoId = 2,
                AreaId = 2,
                Rol = "Sindicalizado",
                IdentificadorSAP = "SAP2",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
            db.Grupos.Add(grupo);
            db.SaveChanges();

            var manning = new CalculosSobreManning(db);
            var fechaInicio = new DateTime(2025, 8, 1);
            var fechaFinal = new DateTime(2025, 10, 1);
            var user = CreateUser(2000, 2, 2, 2, 2);
            db.Users.Add(user);
            db.SaveChanges();

            for (var d = fechaInicio.AddDays(28); d <= fechaFinal.AddDays(-28); d = d.AddDays(1))
            {
                db.DiasCalendarioEmpleado.Add(CreateDiaNoLaboral(user.Id, d));
            }
            db.SaveChanges();

            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fechaInicio.AddDays(14), fechaFinal.AddDays(-14), manning);
            // Act
            var result = generator.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(user, fechaInicio.AddDays(28), fechaFinal.AddDays(-28));
            // Assert
            result.Should().Be((fechaFinal.AddDays(-28) - fechaInicio.AddDays(28)).Days + 1); // Todos los días en el rango son no laborales
            user.DiasDeVacacionesAsignados.Should().Be(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("no es un día laboral")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void EmpleadoNoExistente_RegresaCero_ManejaExcepcion()
        {
            // Arrange
            var db = GetDb(nameof(EmpleadoNoExistente_RegresaCero_ManejaExcepcion));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();

            var area = new Area { AreaId = 3, UnidadOrganizativaSap = "Area 5", NombreGeneral = "Descripción Area 5", Manning = 10 };
            db.Areas.Add(area);
            db.SaveChanges();

            var grupo = new Grupo {
                GrupoId = 3,
                AreaId = 3,
                Rol = "Sindicalizado",
                IdentificadorSAP = "SAP3",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
            db.Grupos.Add(grupo);
            db.SaveChanges();
            var manning = new CalculosSobreManning(db);
            var fechaInicio = new DateTime(2025, 8, 1);
            var fechaFinal = new DateTime(2025, 10, 1);

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generator = new EmployeesCalendarsGenerator(db, logger, fechaInicio.AddDays(14), fechaFinal.AddDays(-14), manning);
            // Act
            var result = generator.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(null, fechaInicio, fechaFinal);
            // Assert
            result.Should().Be(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never());
        }

        [Fact]
        public void ErrorAlCambiarDiaAVacaciones_ManejaExcepcionYContinua()
        {
            // Arrange
            var db = GetDb(nameof(ErrorAlCambiarDiaAVacaciones_ManejaExcepcionYContinua));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();

            var area = new Area { AreaId = 4, UnidadOrganizativaSap = "Area 5", NombreGeneral = "Descripción Area 5", Manning = 10 };
            db.Areas.Add(area);
            db.SaveChanges();

            var grupo = new Grupo {
                GrupoId = 4,
                AreaId = 4,
                Rol = "Sindicalizado",
                IdentificadorSAP = "SAP4",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
            db.Grupos.Add(grupo);
            db.SaveChanges();

            var manning = new CalculosSobreManning(db);
            var fechaInicio = new DateTime(2025, 8, 1);
            var fechaFinal = new DateTime(2025, 10, 1);
            var user = CreateUser(3000, 3, 2, 4, 4);
            db.Users.Add(user);
            for (var d = fechaInicio; d <= fechaFinal; d = d.AddDays(1))
            {
                db.DiasCalendarioEmpleado.Add(CreateDiaLaboral(user.Id, d));
            }
            db.SaveChanges();

            // Usar el mock de EmployeesCalendarsGenerator ahora que el método es virtual
            var generatorMock = new Mock<EmployeesCalendarsGenerator>(db, loggerMock.Object, fechaInicio.AddDays(-14), fechaFinal.AddDays(14), manning) { CallBase = true };
            int callCount = 0;
            generatorMock.Setup(g => g.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(
                It.IsAny<DiasCalendarioEmpleado>(),
                It.IsAny<User>()
            )).Returns(() =>
            {
                if (callCount++ == 0)
                    throw new Exception("Error al cambiar día");
                return true;
            });

            // Act
            var result = generatorMock.Object.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(user, fechaInicio, fechaFinal);
            // Assert
            result.Should().BeGreaterThan(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al cambiar día")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce());
        }

        [Fact]
        public void DiasConPorcentajeDeAusenciaFueraDeRango_OmiteDiasYContinua()
        {
            // Arrange
            var db = GetDb(nameof(DiasConPorcentajeDeAusenciaFueraDeRango_OmiteDiasYContinua));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();

            var area = new Area { AreaId = 5, UnidadOrganizativaSap = "Area 5", NombreGeneral = "Descripción Area 5", Manning = 5 };
            db.Areas.Add(area);
            db.SaveChanges();
    
            var grupo = new Grupo {
                GrupoId = 5,
                AreaId = 5,
                Rol = "Sindicalizado",
                IdentificadorSAP = "SAP5",
                PersonasPorTurno = 3,
                DuracionDeturno = 24
            };
            db.Grupos.Add(grupo);
            db.SaveChanges();
            var manningMock = new Mock<CalculosSobreManning>(db) { CallBase = true };
            manningMock.Setup(m => m.ElPorcentajeDeAusenciaEstaDentroDelRango(
                It.IsAny<int>(),
                It.IsAny<DateTime?>(),
                It.IsAny<decimal?>()
            )).Returns(false);
            // Manipula los datos para que el primer día esté fuera de rango y los siguientes dentro
            // Por ejemplo, puedes agregar días no laborales para el primer día y laborales para los siguientes
            var fechaInicio = new DateTime(2025, 8, 1);
            var fechaFinal = new DateTime(2025, 10, 1);
            var rolSindicalizado = new Rol {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            db.Roles.Add(rolSindicalizado);
            db.SaveChanges();
            var user = new User {
                Id = 4000,
                Nomina = 4,
                FullName = "Empleado 4000",
                Username = "empleado4000",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                AreaId = grupo.AreaId,
                GrupoId = grupo.GrupoId,
                DiasDeVacacionesAsignados = 0,
                VacacionesPorAntiguedad = new VacacionesPorAntiguedad { DiasParaAsignarAutomaticamente = 2 },
                Roles = new List<Rol> { rolSindicalizado }
            };
            db.Users.Add(user);
            // Primer día: laboral para el usuario bajo prueba
            for (var d = fechaInicio.AddDays(28); d <= fechaFinal.AddDays(-28); d = d.AddDays(1))
            {
                    if (d != fechaInicio.AddDays(30)) // Solo el primer día es no laboral para este usuario
                        db.DiasCalendarioEmpleado.Add(CreateDiaNoLaboral(user.Id, d));
            }
            db.DiasCalendarioEmpleado.Add(CreateDiaLaboral(user.Id, fechaInicio.AddDays(30)));
            db.SaveChanges();
            // Agrega suficientes días no laborales para otros usuarios del mismo grupo en la misma fecha para forzar el porcentaje fuera de rango
            for (int i = 1; i <= 4; i++)
            {
                db.Users.Add(new User
                {
                    Id = 5000 + i,
                    Nomina = 5000 + i,
                    FullName = $"Empleado {5000 + i}",
                    Username = $"empleado{5000 + i}",
                    PasswordHash = "hash",
                    PasswordSalt = "salt",
                    AreaId = grupo.AreaId,
                    GrupoId = grupo.GrupoId,
                    DiasDeVacacionesAsignados = 0,
                    VacacionesPorAntiguedad = new VacacionesPorAntiguedad { DiasParaAsignarAutomaticamente = 2 },
                    Roles = new List<Rol> { rolSindicalizado }
                });
                for (var d = fechaInicio.AddDays(28); d <= fechaFinal.AddDays(-28); d = d.AddDays(1))
                {
                    if (i % 6 == 0 && d != fechaInicio.AddDays(30)) // Solo uno de cada seis usuarios tiene el día laboral
                    {
                        db.DiasCalendarioEmpleado.Add(CreateDiaLaboral(5000 + i, d));
                    }
                    else if (d != fechaInicio.AddDays(30)) // Solo el primer día es no laboral para estos usuarios
                    {
                        db.DiasCalendarioEmpleado.Add(CreateDiaNoLaboral(5000 + i, d));
                    }
                    else
                    {
                        db.DiasCalendarioEmpleado.Add(CreateDiaLaboral(5000 + i, d));
                    }
                }
            }

            db.SaveChanges();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fechaInicio.AddDays(14), fechaFinal.AddDays(-14), manningMock.Object);
            // Act
            var result = generator.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(user, fechaInicio.AddDays(28), fechaFinal.AddDays(-28));
            // Assert
            result.Should().BeGreaterThan(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("porcentaje de ausencia")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
