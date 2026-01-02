using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using tiempo_libre.Logic;
using Xunit;

namespace tiempo_libre.Tests
{
    public class AsignacionDeVacacionesTests
    {
        private ILogger<EmployeesCalendarsGenerator> _logger;

        private FreeTimeDbContext GetDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        private User CreateSindicalizado(int id, int grupoId, int? antiguedadDias, int nomina, int diasParaAsignar, Rol rolSindicalizado)
        {
            return new User
            {
                Id = id,
                GrupoId = grupoId,
                Roles = new List<Rol> {
                    rolSindicalizado
                },
                AntiguedadEnDias = antiguedadDias,
                Nomina = nomina,
                VacacionesPorAntiguedad = new VacacionesPorAntiguedad { DiasParaAsignarAutomaticamente = diasParaAsignar },
                FullName = $"Empleado {id}",
                Username = $"empleado{id}",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
        }

        private Rol CreateRol(FreeTimeDbContext db, int id, string name)
        {
            var rol = new Rol
            {
                Id = id,
                Name = name,
                Description = $"{name} Description",
                Abreviation = name.Substring(0, 2).ToUpper()
            };
            var existsRol = db.Roles.FirstOrDefault(r => r.Id == rol.Id);
            if (existsRol == null)
            {
                db.Roles.Add(rol);
                db.SaveChanges();
            }
            return rol;
        }

        [Fact]
        public void GrupoConEmpleadosSindicalizadosConVacacionesPorAsignar_RegresaListaOrdenadaCorrectamente()
        {
            // Arrange
            var db = GetDb(nameof(GrupoConEmpleadosSindicalizadosConVacacionesPorAsignar_RegresaListaOrdenadaCorrectamente));

            var rolSindicalizado = CreateRol(db, (int)RolEnum.Empleado_Sindicalizado, "Sindicalizado");
            db.Users.AddRange(
                CreateSindicalizado(1, 10, 1000, 200, 5, rolSindicalizado), // Más antigüedad
                CreateSindicalizado(2, 10, 500, 100, 5, rolSindicalizado),  // Menos antigüedad
                CreateSindicalizado(3, 10, 1000, 150, 5, rolSindicalizado)  // Igual antigüedad, menor nómina
            );
            db.SaveChanges();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
            var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now, DateTime.Now);

            // Act
            var result = generator.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(10);
            // Assert
            result.Should().HaveCount(3);
            result[0].Id.Should().Be(3); // 1000 días, nómina 150
            result[1].Id.Should().Be(1); // 1000 días, nómina 200
            result[2].Id.Should().Be(2); // 500 días, nómina 100
        }

        [Fact]
        public void GrupoSinEmpleadosSindicalizados_RegresaListaVacia()
        {
            // Arrange
            var db = GetDb(nameof(GrupoSinEmpleadosSindicalizados_RegresaListaVacia));

            var rolSindicalizado = new Rol
            {
                Id = (int)RolEnum.Empleado_Sindicalizado,
                Name = "Sindicalizado",
                Description = "Empleado Sindicalizado",
                Abreviation = "ES"
            };
            var existsRol = db.Roles.FirstOrDefault(r => r.Id == rolSindicalizado.Id);
            if (existsRol == null)
            {
                db.Roles.Add(rolSindicalizado);
                db.SaveChanges();
            }

            db.Users.Add(new User {
                Id = 4,
                GrupoId = 20,
                Roles = new List<Rol> {
                    rolSindicalizado
                },
                Nomina = 300,
                FullName = "Supervisor 4",
                Username = "supervisor4",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            });
            db.SaveChanges();


            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
            var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now, DateTime.Now);
            // Act
            var result = generator.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(10);
            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void EmpleadosSindicalizadosSinVacacionesPorAsignar_RegresaListaVacia()
        {
            // Arrange
            var db = GetDb(nameof(EmpleadosSindicalizadosSinVacacionesPorAsignar_RegresaListaVacia));
            var rolSindicalizado = CreateRol(db, (int)RolEnum.Empleado_Sindicalizado, "Sindicalizado");
            db.Users.Add(CreateSindicalizado(5, 10, 800, 400, 0, rolSindicalizado)); // DiasParaAsignarAutomaticamente = 0
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<ILogger<EmployeesCalendarsGenerator>>().Object, DateTime.Now, DateTime.Now);
            // Act
            var result = generator.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(10);
            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void EmpleadosConMismaAntiguedad_OrdenaPorNomina()
        {
            // Arrange
            var db = GetDb(nameof(EmpleadosConMismaAntiguedad_OrdenaPorNomina));
            var rolSindicalizado = CreateRol(db, (int)RolEnum.Empleado_Sindicalizado, "Sindicalizado");
            db.Users.AddRange(
                CreateSindicalizado(6, 10, 1000, 500, 5, rolSindicalizado),
                CreateSindicalizado(7, 10, 1000, 300, 5, rolSindicalizado),
                CreateSindicalizado(8, 10, 1000, 400, 5, rolSindicalizado)
            );
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<ILogger<EmployeesCalendarsGenerator>>().Object, DateTime.Now, DateTime.Now);
            // Act
            var result = generator.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(10);
            // Assert
            result.Select(u => u.Nomina).Should().ContainInOrder(300, 400, 500);
        }

        [Fact]
        public void EmpleadosSinAntiguedad_SeColocanAlFinalDeLaLista()
        {
            // Arrange
            var db = GetDb(nameof(EmpleadosSinAntiguedad_SeColocanAlFinalDeLaLista));
            var rolSindicalizado = CreateRol(db, (int)RolEnum.Empleado_Sindicalizado, "Sindicalizado");
            db.Users.AddRange(
                CreateSindicalizado(9, 10, null, 600, 5, rolSindicalizado), // Sin antigüedad
                CreateSindicalizado(10, 10, 1000, 700, 5, rolSindicalizado) // Con antigüedad
            );
            db.SaveChanges();
            var generator = new EmployeesCalendarsGenerator(db, new Mock<ILogger<EmployeesCalendarsGenerator>>().Object, DateTime.Now, DateTime.Now);
            // Act
            var result = generator.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(10);
            // Assert
            result.Last().Id.Should().Be(9); // El sin antigüedad al final
        }
    }
}
