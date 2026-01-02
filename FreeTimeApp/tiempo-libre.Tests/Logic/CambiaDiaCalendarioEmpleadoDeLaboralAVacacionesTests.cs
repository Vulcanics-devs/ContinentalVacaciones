using System;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.Logic;
using Xunit;
using Microsoft.Extensions.Logging;

namespace tiempo_libre.Tests
{
    public class CambiaDiaCalendarioEmpleadoDeLaboralAVacacionesTests
    {
        private FreeTimeDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        private User CreateUser(int id, int nomina)
        {
            return new User
            {
                Id = id,
                Nomina = nomina,
                FullName = $"Empleado {id}",
                Username = $"empleado{id}",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                DiasDeVacacionesAsignados = 0,
                Roles = new System.Collections.Generic.List<Rol> {
                    new Rol {
                        Id = 1,
                        Name = "Test",
                        Description = "Test Role",
                        Abreviation = "TS"
                    }
                }
            };
        }

        private DiasCalendarioEmpleado CreateDiaLaboral(int userId, DateTime fecha, User usuario)
        {
            return new DiasCalendarioEmpleado
            {
                IdUsuarioEmpleadoSindicalizado = userId,
                AnioFecha = fecha.Year,
                MesFecha = fecha.Month,
                DiaFecha = fecha.Day,
                FechaDelDia = DateOnly.FromDateTime(fecha),
                TipoActividadDelDia = tiempo_libre.Models.Enums.TipoActividadDelDiaEnum.Laboral,
                UsuarioEmpleadoSindicalizado = usuario,
                NominaEmpleado = userId,
                Grupo = null,
                TurnoXRolSemanalXRegla = null
            };
        }

        private DiasCalendarioEmpleado CreateDiaNoLaboral(int userId, DateTime fecha, User usuario)
        {
            return new DiasCalendarioEmpleado
            {
                IdUsuarioEmpleadoSindicalizado = userId,
                AnioFecha = fecha.Year,
                MesFecha = fecha.Month,
                DiaFecha = fecha.Day,
                FechaDelDia = DateOnly.FromDateTime(fecha),
                TipoActividadDelDia = tiempo_libre.Models.Enums.TipoActividadDelDiaEnum.DescansoSemanal,
                UsuarioEmpleadoSindicalizado = usuario,
                NominaEmpleado = userId,
                Grupo = null,
                TurnoXRolSemanalXRegla = null
            };
        }

        [Fact]
        public void DiaLaboral_SeCambiaAVacaciones_ActualizaDiaYContador()
        {
            // Esta prueba verifica que un día laboral se pueda cambiar a vacaciones y que se actualicen correctamente el día y el contador de días de vacaciones del usuario.
            // Arrange
            var db = GetDb(nameof(DiaLaboral_SeCambiaAVacaciones_ActualizaDiaYContador));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var fecha = new DateTime(2025, 9, 20);
            var user = CreateUser(1, 1);
            db.Users.Add(user);
            var dia = CreateDiaLaboral(user.Id, fecha, user);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fecha, fecha);
            // Act
            var result = generator.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(dia, user);
            // Assert
            result.Should().BeTrue();
            dia.TipoActividadDelDia.Should().Be(tiempo_libre.Models.Enums.TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp);
            dia.EsDiaDeVacaciones.Should().BeTrue();
            dia.EsDiaLaboral.Should().BeFalse();
            user.DiasDeVacacionesAsignados.Should().Be(1);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se cambió el día")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se actualizó el contador")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void DiaNoLaboral_RegresaFalse_NoHaceCambios()
        {
            // Esta prueba verifica que un día no laboral no se pueda cambiar a vacaciones y que no se realicen cambios en el día ni en el contador de días de vacaciones del usuario.
            // Arrange
            var db = GetDb(nameof(DiaNoLaboral_RegresaFalse_NoHaceCambios));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var fecha = new DateTime(2025, 9, 21);
            var user = CreateUser(2, 2);
            db.Users.Add(user);
            var dia = CreateDiaNoLaboral(user.Id, fecha, user);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fecha, fecha);
            // Act
            var result = generator.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(dia, user);
            // Assert
            result.Should().BeFalse();
            dia.TipoActividadDelDia.Should().Be(tiempo_libre.Models.Enums.TipoActividadDelDiaEnum.DescansoSemanal);
            user.DiasDeVacacionesAsignados.Should().Be(0);
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("no es un día laboral")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void EmpleadoNoExistente_RegresaFalse_ManejaExcepcion()
        {
            // Esta prueba verifica que un día laboral no se pueda cambiar a vacaciones si el empleado no existe. Se espera que el método maneje la excepción y registre un error.
            // Arrange
            var db = GetDb(nameof(EmpleadoNoExistente_RegresaFalse_ManejaExcepcion));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var fecha = new DateTime(2025, 9, 22);
            var dia = CreateDiaLaboral(999, fecha, null);
            // No se agrega el usuario al contexto
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fecha, fecha);
            // Act
            var result = generator.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(dia, null);
            // Assert
            result.Should().BeFalse();
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al cambiar el día")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ErrorAlGuardarCambios_RegresaFalse_ManejaExcepcion()
        {
            // Esta prueba verifica que si ocurre un error al guardar los cambios en la base de datos, el método maneje la excepción y registre un error. 
            // Arrange
            var db = GetDb(nameof(ErrorAlGuardarCambios_RegresaFalse_ManejaExcepcion));
            var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
            var fecha = new DateTime(2025, 9, 23);
            var user = CreateUser(3, 3);
            db.Users.Add(user);
            var dia = CreateDiaLaboral(user.Id, fecha, user);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fecha, fecha);
            // Simular error al guardar cambios
            db.Dispose();
            // Act
            var result = generator.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(dia, user);
            // Assert
            result.Should().BeFalse();
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al cambiar el día")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
