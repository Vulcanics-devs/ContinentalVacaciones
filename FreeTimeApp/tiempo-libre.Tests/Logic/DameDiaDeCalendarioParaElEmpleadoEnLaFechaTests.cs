using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.Logic;
using Xunit;

namespace tiempo_libre.Tests
{
    public class DameDiaDeCalendarioParaElEmpleadoEnLaFechaTests
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
                Roles = new List<Rol> {
                    new Rol {
                        Id = 1,
                        Name = "Test",
                        Description = "Test Role",
                        Abreviation = "TS"
                    }
                }
            };
        }

        private DiasCalendarioEmpleado CreateDia(int userId, DateTime fecha, Vacaciones vacaciones = null, IncidenciaOPermiso permiso = null, DiasInhabiles inhabil = null)
        {
            return new DiasCalendarioEmpleado
            {
                IdUsuarioEmpleadoSindicalizado = userId,
                AnioFecha = fecha.Year,
                MesFecha = fecha.Month,
                DiaFecha = fecha.Day,
                Vacaciones = vacaciones,
                IncidenciaOPermiso = permiso,
                DiaInhabil = inhabil
            };
        }

        [Fact]
        public void DiaExistenteParaEmpleadoEnFecha_RegresaDiaConRelacionesCargadas()
        {
            // Arrange
            var db = GetDb(nameof(DiaExistenteParaEmpleadoEnFecha_RegresaDiaConRelacionesCargadas));
            var user = CreateUser(1, 100);
            db.Users.Add(user);
            var vacaciones = new Vacaciones { Id = 1, NominaEmpleado = 100 };
            var permiso = new IncidenciaOPermiso { Id = 2, NominaEmpleado = 100 };
            var inhabil = new DiasInhabiles { Id = 3 };
            var fecha = new DateTime(2025, 5, 15);
            var dia = CreateDia(user.Id, fecha, vacaciones, permiso, inhabil);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>().Object, fecha, fecha);
            // Act
            var result = generator.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(user.Id, fecha);
            // Assert
            result.Should().NotBeNull();
            result.Vacaciones.Should().NotBeNull();
            result.IncidenciaOPermiso.Should().NotBeNull();
            result.DiaInhabil.Should().NotBeNull();
        }

        [Fact]
        public void DiaNoExistenteParaEmpleadoEnFecha_RegresaNull()
        {
            // Arrange
            var db = GetDb(nameof(DiaNoExistenteParaEmpleadoEnFecha_RegresaNull));
            var user = CreateUser(2, 101);
            db.Users.Add(user);
            var fecha = new DateTime(2025, 6, 10);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>().Object, fecha, fecha);
            // Act
            var result = generator.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(user.Id, fecha);
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void EmpleadoNoExistente_RegresaNull()
        {
            // Arrange
            var db = GetDb(nameof(EmpleadoNoExistente_RegresaNull));
            var fecha = new DateTime(2025, 7, 20);
            var generator = new EmployeesCalendarsGenerator(db, new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>().Object, fecha, fecha);
            // Act
            var result = generator.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(999, fecha);
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FechaEnAnioBisiesto_RegresaDiaCorrecto()
        {
            // Arrange
            var db = GetDb(nameof(FechaEnAnioBisiesto_RegresaDiaCorrecto));
            var user = CreateUser(3, 102);
            db.Users.Add(user);
            var fecha = new DateTime(2024, 2, 29); // Año bisiesto
            var dia = CreateDia(user.Id, fecha);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>().Object, fecha, fecha);
            // Act
            var result = generator.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(user.Id, fecha);
            // Assert
            result.Should().NotBeNull();
            result.AnioFecha.Should().Be(2024);
            result.MesFecha.Should().Be(2);
            result.DiaFecha.Should().Be(29);
        }

        [Fact]
        public void FechaEnMesCon30Dias_RegresaDiaCorrecto()
        {
            // Arrange
            var db = GetDb(nameof(FechaEnMesCon30Dias_RegresaDiaCorrecto));
            var user = CreateUser(4, 103);
            db.Users.Add(user);
            var fecha = new DateTime(2025, 4, 30); // Abril tiene 30 días
            var dia = CreateDia(user.Id, fecha);
            db.DiasCalendarioEmpleado.Add(dia);
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>().Object, fecha, fecha);
            // Act
            var result = generator.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(user.Id, fecha);
            // Assert
            result.Should().NotBeNull();
            result.AnioFecha.Should().Be(2025);
            result.MesFecha.Should().Be(4);
            result.DiaFecha.Should().Be(30);
        }
    }
}
