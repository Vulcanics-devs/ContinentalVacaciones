using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;

public class EmployeesCalendarsGeneratorTests
{
    private FreeTimeDbContext GetDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new FreeTimeDbContext(options);
    }

    private readonly ITestOutputHelper _output;
    public EmployeesCalendarsGeneratorTests(ITestOutputHelper output) { _output = output; }

    [Theory]
    [InlineData(1, 5)] // antigüedad 1 año, espera 5 días
    [InlineData(3, 10)] // antigüedad 3 años, espera 10 días
    [InlineData(6, 15)] // antigüedad 6 años, espera 15 días
    [InlineData(23, 20)] // antigüedad 23 años, espera 15 días
    public void BuscarVacacionesPorAntiguedad_AsignaCorrectamente(int antiguedadAnios, int diasEsperados)
    {
        var db = GetDb($"BuscarVacacionesPorAntiguedad_{antiguedadAnios}");
        db.VacacionesPorAntiguedad.AddRange(new List<VacacionesPorAntiguedad>
        {
            new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 5 },
            new VacacionesPorAntiguedad { Id = 2, AntiguedadEnAniosRangoInicial = 3, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 10 },
            new VacacionesPorAntiguedad { Id = 3, AntiguedadEnAniosRangoInicial = 6, AntiguedadEnAniosRangoFinal = 10, TotalDiasDeVacaciones = 15 },
            new VacacionesPorAntiguedad { Id = 4, AntiguedadEnAniosRangoInicial = 21, AntiguedadEnAniosRangoFinal = 25, TotalDiasDeVacaciones = 20 }
        });
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        var antiguedad = TimeSpan.FromDays(antiguedadAnios * 366);
        var vacaciones = generator.BuscarVacacionesPorAntiguedad(antiguedad);
        vacaciones.Should().NotBeNull();
        vacaciones.TotalDiasDeVacaciones.Should().Be(diasEsperados);
    }

    [Fact]
    public void CalculaAntiguedadParaTodosEmpleados_AsignaAntiguedadYVacacionesCorrectamente()
    {
        var db = GetDb("CalculaAntiguedadParaTodosEmpleados");
        // VacacionesPorAntiguedad reglas
        db.VacacionesPorAntiguedad.AddRange(new List<VacacionesPorAntiguedad>
            {
                new VacacionesPorAntiguedad { Id = 7, AntiguedadEnAniosRangoInicial = 0, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 5 },
                new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 5 },
                new VacacionesPorAntiguedad { Id = 2, AntiguedadEnAniosRangoInicial = 2, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 10 },
                new VacacionesPorAntiguedad { Id = 3, AntiguedadEnAniosRangoInicial = 3, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 10 },
                new VacacionesPorAntiguedad { Id = 4, AntiguedadEnAniosRangoInicial = 4, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 15 },
                new VacacionesPorAntiguedad { Id = 5, AntiguedadEnAniosRangoInicial = 5, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 15 },
                new VacacionesPorAntiguedad { Id = 6, AntiguedadEnAniosRangoInicial = 6, AntiguedadEnAniosRangoFinal = 10, TotalDiasDeVacaciones = 20 }
            });
        db.SaveChanges();

        // Roles
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        db.SaveChanges();

        // Crear 10 usuarios con diferentes fechas de ingreso
        var fechaBase = new DateTime(2016, 1, 1);
        var usuarios = new List<User>();
        for (int i = 0; i < 10; i++)
        {
            var fechaIngreso = fechaBase.AddYears(i); // antigüedad de 0 a 9 años
            var user = new User
            {
                Id = i + 1,
                FullName = $"Empleado {i + 1}",
                Nomina = 1000 + i,
                FechaIngreso = DateOnly.FromDateTime(fechaIngreso),
                Roles = new List<Rol> { rolSindicalizado },
                Username = $"empleado{i + 1}",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                AreaId = 1,
                GrupoId = 1
            };
            usuarios.Add(user);
        }
        db.Users.AddRange(usuarios);
        db.SaveChanges();

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        generator.CalculaAntiguedadParaTodosEmpleados();

        // Verificar que la antigüedad y vacaciones se asignaron correctamente
        var usersFromDb = db.Users.Include(u => u.VacacionesPorAntiguedad).ToList();
        foreach (var user in usersFromDb)
        {
            var antiguedadEsperada = (int)((DateTime.Now.Date - user.FechaIngreso.Value.ToDateTime(new TimeOnly(0, 0))).Days / 365.25);
            user.AntiguedadEnAnios.Should().Be(antiguedadEsperada);
            if (antiguedadEsperada <= 1)
                user.VacacionesPorAntiguedad.TotalDiasDeVacaciones.Should().Be(5);
            else if (antiguedadEsperada <= 3)
                user.VacacionesPorAntiguedad.TotalDiasDeVacaciones.Should().Be(10);
            else if (antiguedadEsperada <= 5)
                user.VacacionesPorAntiguedad.TotalDiasDeVacaciones.Should().Be(15);
            else
                user.VacacionesPorAntiguedad.TotalDiasDeVacaciones.Should().Be(20);
        }
    }

    [Fact]
    public void ObtenTurnoParaLaFecha_RetornaTurnoCorrectoPorReglaYCiclo()
    {
        var db = GetDb("ObtenTurnoParaLaFecha");
        // Crear regla y turnos con todos los campos requeridos

        var regla = new Regla
        {
            Id = 1,
            Nombre = "Regla 1",
            Descripcion = "Descripción de la Regla 1",
            TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>()
        };

        var rolSemanal = new RolSemanal
        {
            RolSemanalId = 1,
            Rol = "Rol Semanal 1",
            Regla = regla,
            // Si hay más campos requeridos, agrégalos aquí
        };

        var rolSemanal2 = new RolSemanal
        {
            RolSemanalId = 2,
            Rol = "Rol Semanal 2",
            Regla = regla,
            // Si hay más campos requeridos, agrégalos aquí
        };

        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.RolesSemanales.Add(rolSemanal2);
        db.SaveChanges();

        // Crear turnos para 7 días, inicializando todos los campos requeridos
        for (int i = 0; i < 7; i++)
        {
            var turno = new TurnoXRolSemanalXRegla
            {
                Id = i + 1,
                IdRegla = regla.Id,
                IdRolSemanal = rolSemanal.RolSemanalId,
                IndicePorRegla = i,
                DiaDeLaSemana = (DiasDeLaSemanaEnum)((i + 1) % 7),
                Turno = TurnosEnum.Matutino,
                ActividadDelDia = i % 3 == 0 ? TipoActividadDelDiaEnum.DescansoSemanal : TipoActividadDelDiaEnum.Laboral,
                Regla = regla,
                RolSemanal = rolSemanal,
                // Inicializa campos requeridos adicionales si existen
            };
            regla.TurnosXRolSemanalXRegla.Add(turno);
            db.TurnosXRolSemanalXRegla.Add(turno);
        }
        for (int j = 0; j < 7; j++)
        {
            var turno = new TurnoXRolSemanalXRegla
            {
                Id = j + 8,
                IdRegla = regla.Id,
                IdRolSemanal = rolSemanal2.RolSemanalId,
                IndicePorRegla = j + 7,
                DiaDeLaSemana = (DiasDeLaSemanaEnum)((j + 1) % 7),
                Turno = TurnosEnum.Vespertino,
                ActividadDelDia = (j + 7) % 3 == 0 ? TipoActividadDelDiaEnum.DescansoSemanal : TipoActividadDelDiaEnum.Laboral,
                Regla = regla,
                RolSemanal = rolSemanal2,
                // Inicializa campos requeridos adicionales si existen
            };
            regla.TurnosXRolSemanalXRegla.Add(turno);
            db.TurnosXRolSemanalXRegla.Add(turno);
        }
        db.SaveChanges();

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);

        var fechaInicial = new DateTime(2025, 9, 15); // lunes
        var turnoInicial = db.TurnosXRolSemanalXRegla.First(t => t.IndicePorRegla == 0);

        turnoInicial.Should().NotBeNull();
        turnoInicial.IndicePorRegla.Should().Be(0);
        turnoInicial.DiaDeLaSemana.Should().Be(DiasDeLaSemanaEnum.Lunes);
        turnoInicial.Turno.Should().Be(TurnosEnum.Matutino);

        // Prueba: ciclo de 28 días, debe regresar el turno correcto para cada día
        for (int offset = 0; offset < 28; offset++)
        {
            var fechaObjetivo = fechaInicial.AddDays(offset);
            var esperadoIndice = (turnoInicial.IndicePorRegla + offset) % regla.TurnosXRolSemanalXRegla.Count;
            var esperadoDia = (DiasDeLaSemanaEnum)fechaObjetivo.DayOfWeek;
            var turno = generator.ObtenTurnoParaLaFecha(fechaInicial, fechaObjetivo, regla, turnoInicial);

            _output.WriteLine($"TotalTurnos={regla.TurnosXRolSemanalXRegla.Count}");
            _output.WriteLine($"offset={offset}, esperado={esperadoIndice}, fechaInicial={fechaInicial}, fecha={fechaObjetivo}, turnoInicial={turnoInicial.IndicePorRegla}");

            turno.Should().NotBeNull();
            turno.IndicePorRegla.Should().Be(esperadoIndice);
            turno.DiaDeLaSemana.Should().Be(esperadoDia);
            turno.Turno.Should().Be(esperadoIndice < 7 ? TurnosEnum.Matutino : TurnosEnum.Vespertino);
            turno.ActividadDelDia.Should().Be(esperadoIndice % 3 == 0 ? TipoActividadDelDiaEnum.DescansoSemanal : TipoActividadDelDiaEnum.Laboral);
        }

        // Prueba: si no existe turno para el día, retorna null
        var regla2 = new Regla
        {
            Id = 2,
            Nombre = "Regla 2",
            Descripcion = "Descripción de la Regla 2",
            TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>()
        };
        db.Reglas.Add(regla2);
        db.SaveChanges();
        var turnoNull = generator.ObtenTurnoParaLaFecha(fechaInicial, fechaInicial.AddDays(1), regla2, turnoInicial);
        turnoNull.Should().BeNull();
    }

    [Fact]
    public void GetUsersByArea_RetornaSoloUsuariosDelAreaYEnOrdenCorrecto()
    {
        var db = GetDb("GetUsersByArea");
        var area1 = new Area { AreaId = 1, NombreGeneral = "Area 1", UnidadOrganizativaSap = "UO1" };
        var area2 = new Area { AreaId = 2, NombreGeneral = "Area 2", UnidadOrganizativaSap = "UO2" };
        db.Areas.AddRange(area1, area2);
        db.SaveChanges();

        // Roles requeridos
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        db.SaveChanges();

        // Usuarios en diferentes áreas y antigüedad, todos con campos requeridos
        var usuarios = new List<User>
        {
            new User { Id = 1, FullName = "A", AreaId = 1, GrupoId = 1, AntiguedadEnAnios = 5, Nomina = 100, Username = "a", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>{rolSindicalizado}, Status = UserStatus.Activo },
            new User { Id = 2, FullName = "B", AreaId = 1, GrupoId = 1, AntiguedadEnAnios = 3, Nomina = 101, Username = "b", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>{rolSindicalizado}, Status = UserStatus.Activo },
            new User { Id = 3, FullName = "C", AreaId = 1, GrupoId = 1, AntiguedadEnAnios = 5, Nomina = 99, Username = "c", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>{rolSindicalizado}, Status = UserStatus.Activo },
            new User { Id = 4, FullName = "D", AreaId = 2, GrupoId = 1, AntiguedadEnAnios = 2, Nomina = 102, Username = "d", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>{rolSindicalizado}, Status = UserStatus.Activo },
            new User { Id = 5, FullName = "E", AreaId = 0, GrupoId = 1, AntiguedadEnAnios = 1, Nomina = 103, Username = "e", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol>{rolSindicalizado}, Status = UserStatus.Activo }
        };
        db.Users.AddRange(usuarios);
        db.SaveChanges();

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);

        // Prueba: retorna solo usuarios del área 1, ordenados por antigüedad descendente y luego nómina ascendente
        var result = generator.GetUsersByArea(area1);
        result.Should().HaveCount(3);
        result.Select(u => u.FullName).Should().ContainInOrder("C", "A", "B"); // C y A tienen 5 años, C tiene menor nómina

        // Prueba: retorna solo usuarios del área 2
        var result2 = generator.GetUsersByArea(area2);
        result2.Should().HaveCount(1);
        result2[0].FullName.Should().Be("D");

        // Prueba: área sin usuarios
        var area3 = new Area { AreaId = 3, NombreGeneral = "Area 3", UnidadOrganizativaSap = "UO3" };
        db.Areas.Add(area3);
        db.SaveChanges();
        var result3 = generator.GetUsersByArea(area3);
        result3.Should().BeEmpty();
    }

    [Fact]
    void ObtenDatosDeInicioPara_UsuarioConDatosCorrectos_CreaCalendarioYActualizaUsuario()
    {
        var db = GetDb("ObtenDatosDeInicioPara_Correcto");
        // Crear area
        var area = new Area { AreaId = 1, NombreGeneral = "Area 1", UnidadOrganizativaSap = "UO1" };
        db.Areas.Add(area);

        // Crear grupo
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);

        // Crear regla y rol semanal
        var regla = new Regla { Id = 1, Nombre = "Regla 1", Descripcion = "Descripcion de la regla 1", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal 1", Regla = regla };
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);

        // Crear turnos
        var turno = new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            IndicePorRegla = 0,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            Turno = TurnosEnum.Matutino,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Regla = regla,
            RolSemanal = rolSemanal
        };
        var turno2 = new TurnoXRolSemanalXRegla
        {
            Id = 2,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            IndicePorRegla = 1,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Martes,
            Turno = TurnosEnum.Matutino,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Regla = regla,
            RolSemanal = rolSemanal
        };
        regla.TurnosXRolSemanalXRegla.Add(turno);
        db.TurnosXRolSemanalXRegla.Add(turno);
        regla.TurnosXRolSemanalXRegla.Add(turno2);
        db.TurnosXRolSemanalXRegla.Add(turno2);

        // Crear vacaciones por antigüedad
        var vacaciones = new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, AntiguedadEnAniosRangoFinal = null, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        db.VacacionesPorAntiguedad.Add(vacaciones);
        db.SaveChanges();

        // Crear usuario
        var user = new User
        {
            Id = 1,
            FullName = "Empleado Correcto",
            Nomina = 1001,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            Roles = new List<Rol>(),
            Username = "empleado1",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            AreaId = 1,
            GrupoId = grupo.GrupoId,
            VacacionesPorAntiguedad = vacaciones,
            VacacionesPorAntiguedadId = vacaciones.Id
        };
        db.Users.Add(user);
        db.SaveChanges();

        // Crear rol inicial por empleado
        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 1,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 8))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();

        // Crear programacion anual
        var programacion = new ProgramacionesAnuales
        {
            Id = 1,
            Anio = DateTime.Now.Year,
            FechaInicia = DateTime.Now.AddMonths(-1),
            BorradoLogico = false,
            Estatus = EstatusProgramacionAnualEnum.Pendiente
        };
        db.ProgramacionesAnuales.Add(programacion);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now, DateTime.Now);

        var datos = generator.ObtenDatosDeInicioPara(user);
        datos.Should().NotBeNull();
        var calendario = db.CalendarioEmpleados.FirstOrDefault(c => c.IdUsuarioEmpleadoSindicalizado == user.Id);
        calendario.Should().NotBeNull();
        var userActualizado = db.Users.First(u => u.Id == user.Id);
        userActualizado.Should().NotBeNull();
        userActualizado.VacacionesPorAntiguedadId.Should().Be(vacaciones.Id);
        userActualizado.VacacionesPorAntiguedad.Should().NotBeNull();
        userActualizado.VacacionesPorAntiguedad.TotalDiasDeVacaciones.Should().Be(5);

        datos.RolInicial.Should().Be(rolSemanal);
        datos.Regla.Should().Be(regla);
        datos.turnoRolSemanalInicial.Should().Be(turno);
        datos.VacacionesPorAntiguedad.Should().Be(vacaciones);
        datos.CalendarioEmpleado.Should().Be(calendario);
    }

    [Fact]
    void ObtenDatosDeInicioPara_UsuarioSinRolInicial_RetornaNull()
    {
        var db = GetDb("ObtenDatosDeInicioPara_SinRolInicial");
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);
        var regla = new Regla { Id = 1, Nombre = "Regla 1", Descripcion = "Descripcion de la regla 1", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal 1", Regla = regla };
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        var turno = new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            IndicePorRegla = 0,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            Turno = TurnosEnum.Matutino,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Regla = regla,
            RolSemanal = rolSemanal
        };
        regla.TurnosXRolSemanalXRegla.Add(turno);
        db.TurnosXRolSemanalXRegla.Add(turno);
        var vacaciones = new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        db.VacacionesPorAntiguedad.Add(vacaciones);
        db.SaveChanges();
        var user = new User
        {
            Id = 2,
            FullName = "Empleado Sin Rol Inicial",
            Nomina = 1002,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            Roles = new List<Rol>(),
            Username = "empleado2",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            AreaId = 1,
            GrupoId = grupo.GrupoId,
            VacacionesPorAntiguedad = vacaciones,
            VacacionesPorAntiguedadId = vacaciones.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        var datos = generator.ObtenDatosDeInicioPara(user);
        datos.Should().BeNull();
    }

    [Fact]
    void ObtenDatosDeInicioPara_UsuarioSinRolSemanal_RetornaNull()
    {
        var db = GetDb("ObtenDatosDeInicioPara_SinRolSemanal");
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);
        var regla = new Regla { Id = 1, Nombre = "Regla 1", Descripcion = "Descripcion de la regla 1", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        db.Reglas.Add(regla);
        // No se agrega rol semanal
        var turno = new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IdRegla = regla.Id,
            IdRolSemanal = 99, // No existe
            IndicePorRegla = 0,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            Turno = TurnosEnum.Matutino,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Regla = regla
        };
        regla.TurnosXRolSemanalXRegla.Add(turno);
        db.TurnosXRolSemanalXRegla.Add(turno);
        var vacaciones = new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        db.VacacionesPorAntiguedad.Add(vacaciones);
        db.SaveChanges();
        var user = new User
        {
            Id = 3,
            FullName = "Empleado Sin Rol Semanal",
            Nomina = 1003,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            Roles = new List<Rol>(),
            Username = "empleado3",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            AreaId = 1,
            GrupoId = grupo.GrupoId,
            VacacionesPorAntiguedad = vacaciones,
            VacacionesPorAntiguedadId = vacaciones.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 2,
            Nomina = user.Nomina ?? 0,
            RolSemanal = "Rol Semanal Inexistente",
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddYears(-2))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        var datos = generator.ObtenDatosDeInicioPara(user);
        datos.Should().BeNull();
    }

    [Fact]
    void ObtenDatosDeInicioPara_UsuarioSinTurno_RetornaNull()
    {
        var db = GetDb("ObtenDatosDeInicioPara_SinTurno");
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);
        var regla = new Regla { Id = 1, Nombre = "Regla 1", Descripcion = "Descripcion de la regla 1", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal 1", Regla = regla };
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        // No se agrega turno
        var vacaciones = new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        db.VacacionesPorAntiguedad.Add(vacaciones);
        db.SaveChanges();
        var user = new User
        {
            Id = 4,
            FullName = "Empleado Sin Turno",
            Nomina = 1004,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            Roles = new List<Rol>(),
            Username = "empleado4",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            AreaId = 1,
            GrupoId = grupo.GrupoId,
            VacacionesPorAntiguedad = vacaciones,
            VacacionesPorAntiguedadId = vacaciones.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 3,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddYears(-2))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        var datos = generator.ObtenDatosDeInicioPara(user);
        datos.Should().BeNull();
    }

    [Fact]
    void ObtenDatosDeInicioPara_UsuarioSinVacacionesPorAntiguedad_RetornaNull()
    {
        var db = GetDb("ObtenDatosDeInicioPara_SinVacaciones");
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);
        var regla = new Regla { Id = 1, Nombre = "Regla 1", Descripcion = "Descripcion de la regla 1", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal 1", Regla = regla };
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        var turno = new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            IndicePorRegla = 0,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            Turno = TurnosEnum.Matutino,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Regla = regla,
            RolSemanal = rolSemanal
        };
        regla.TurnosXRolSemanalXRegla.Add(turno);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.SaveChanges();
        var user = new User
        {
            Id = 5,
            FullName = "Empleado Sin Vacaciones",
            Nomina = 1005,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            Roles = new List<Rol>(),
            Username = "empleado5",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            AreaId = 1,
            GrupoId = grupo.GrupoId
            // No se asigna VacacionesPorAntiguedad
        };
        db.Users.Add(user);
        db.SaveChanges();
        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 4,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddYears(-2))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now, DateTime.Now);
        var datos = generator.ObtenDatosDeInicioPara(user);
        datos.Should().BeNull();
    }

    [Fact]
    void GetIncidenciaOPermisoDentroDelRangoPorEmpleado_RetornaSoloDelEmpleadoYEnRango()
    {
        var db = GetDb("GetIncidenciaOPermisoDentroDelRangoPorEmpleado_ConDatos");
        var empleadoNomina = 1234;
        var otroNomina = 5678;
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // Permisos/incapacidades para el empleado dentro y fuera del rango
        db.IncidenciasOPermisos.AddRange(new List<IncidenciaOPermiso> {
            new IncidenciaOPermiso { Id = 1, NominaEmpleado = empleadoNomina, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 2)), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce },
            new IncidenciaOPermiso { Id = 2, NominaEmpleado = empleadoNomina, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 5)), TiposDeIncedencia = TiposDeIncidenciasEnum.IncapacidadEnfermedadGeneral },
            new IncidenciaOPermiso { Id = 3, NominaEmpleado = empleadoNomina, Fecha = DateOnly.FromDateTime(new DateTime(2025, 8, 30)), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce }, // fuera de rango
            new IncidenciaOPermiso { Id = 4, NominaEmpleado = otroNomina, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 3)), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce }
        });
        db.SaveChanges();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, fechaInicio, fechaFinal);
        var result = generator.GetIncidenciaOPermisoDentroDelRangoPorEmpleado(empleadoNomina, fechaInicio, fechaFinal);
        result.Should().HaveCount(2);
        result.All(p => p.NominaEmpleado == empleadoNomina).Should().BeTrue();
        result.All(p => p.Fecha.ToDateTime(new TimeOnly(0, 0)) >= fechaInicio && p.Fecha.ToDateTime(new TimeOnly(0, 0)) <= fechaFinal).Should().BeTrue();
    }

    [Fact]
    void GetIncidenciaOPermisoDentroDelRangoPorEmpleado_EmpleadoSinPermisos_RetornaListaVacia()
    {
        var db = GetDb("GetIncidenciaOPermisoDentroDelRangoPorEmpleado_SinDatos");
        var empleadoNomina = 9999;
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // No se agregan permisos/incapacidades para el empleado
        db.SaveChanges();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, fechaInicio, fechaFinal);
        var result = generator.GetIncidenciaOPermisoDentroDelRangoPorEmpleado(empleadoNomina, fechaInicio, fechaFinal);
        result.Should().BeEmpty();
    }

    [Fact]
    void GetPermisosEincapacidadesDentroDelRango_RetornaSoloEnRango()
    {
        var db = GetDb("GetPermisosEincapacidadesDentroDelRango_ConDatos");
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // Permisos/incapacidades dentro y fuera del rango
        db.IncidenciasOPermisos.AddRange(new List<IncidenciaOPermiso> {
            new IncidenciaOPermiso { Id = 1, NominaEmpleado = 1234, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 2)), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce },
            new IncidenciaOPermiso { Id = 2, NominaEmpleado = 5678, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 5)), TiposDeIncedencia = TiposDeIncidenciasEnum.IncapacidadEnfermedadGeneral },
            new IncidenciaOPermiso { Id = 3, NominaEmpleado = 1234, Fecha = DateOnly.FromDateTime(new DateTime(2025, 8, 30)), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce } // fuera de rango
        });
        db.SaveChanges();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, fechaInicio, fechaFinal);
        var result = generator.GetPermisosEincapacidadesDentroDelRango(fechaInicio, fechaFinal);
        result.Should().HaveCount(2);
        result.All(p => p.Fecha.ToDateTime(new TimeOnly(0, 0)) >= fechaInicio && p.Fecha.ToDateTime(new TimeOnly(0, 0)) <= fechaFinal).Should().BeTrue();
    }

    [Fact]
    void GetPermisosEincapacidadesDentroDelRango_SinDatos_RetornaListaVacia()
    {
        var db = GetDb("GetPermisosEincapacidadesDentroDelRango_SinDatos");
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // No se agregan permisos/incapacidades
        db.SaveChanges();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, fechaInicio, fechaFinal);
        var result = generator.GetPermisosEincapacidadesDentroDelRango(fechaInicio, fechaFinal);
        result.Should().BeEmpty();
    }

    [Fact]
    void GetDiasInhabilesDentroDelRango_RetornaSoloEnRango()
    {
        var db = GetDb("GetDiasInhabilesDentroDelRango_ConDatos");
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // Días inhábiles dentro y fuera del rango
        db.DiasInhabiles.AddRange(new List<DiasInhabiles> {
            new DiasInhabiles { Id = 1, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 2)), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental, Detalles = "Continental" },
            new DiasInhabiles { Id = 2, Fecha = DateOnly.FromDateTime(new DateTime(2025, 9, 5)), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey, Detalles = "Ley" },
            new DiasInhabiles { Id = 3, Fecha = DateOnly.FromDateTime(new DateTime(2025, 8, 30)), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental, Detalles = "Fuera de rango" } // fuera de rango
        });
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fechaInicio, fechaFinal);
        var result = generator.GetDiasInhabilesDentroDelRango(fechaInicio, fechaFinal);
        result.Should().HaveCount(2);
        result.All(d => d.Fecha.ToDateTime(new TimeOnly(0, 0)) >= fechaInicio && d.Fecha.ToDateTime(new TimeOnly(0, 0)) <= fechaFinal).Should().BeTrue();
        result.Select(d => d.Detalles).Should().Contain(new[] { "Continental", "Ley" });
    }

    [Fact]
    void GetDiasInhabilesDentroDelRango_SinDatos_RetornaListaVacia()
    {
        var db = GetDb("GetDiasInhabilesDentroDelRango_SinDatos");
        var fechaInicio = new DateTime(2025, 9, 1);
        var fechaFinal = new DateTime(2025, 9, 10);
        // No se agregan días inhábiles
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, fechaInicio, fechaFinal);
        var result = generator.GetDiasInhabilesDentroDelRango(fechaInicio, fechaFinal);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Ley")]
    [InlineData("Continental")]
    [InlineData("Permiso")]
    [InlineData("Vacaciones")]
    [InlineData("Turno")]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_CasosSimples(string caso)
    {
        var db = GetDb($"CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_{caso}");
        // Arrange
        var area = new Area { AreaId = 1, NombreGeneral = "Area 1", UnidadOrganizativaSap = "UO1" };
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad, Status = UserStatus.Activo };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);

        db.Areas.Add(area);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.Users.Add(user);
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        db.CalendarioEmpleados.Add(calendario);
        db.SaveChanges();

        TurnoXRolSemanalXRegla? turno = null;
        DiasInhabiles? diaLey = null;
        DiasInhabiles? diaContinental = null;
        IncidenciaOPermiso? permiso = null;
        Vacaciones? vacaciones = null;
        if (caso == "Ley")
        {
            diaLey = new DiasInhabiles { Id = 10, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey, Detalles = "Ley" };
            db.DiasInhabiles.Add(diaLey);
            db.SaveChanges();
        }
        if (caso == "Continental")
        {
            diaContinental = new DiasInhabiles { Id = 11, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental, Detalles = "Continental" };
            db.DiasInhabiles.Add(diaContinental);
            db.SaveChanges();
        }
        if (caso == "Permiso")
        {
            permiso = new IncidenciaOPermiso { Id = 12, Fecha = DateOnly.FromDateTime(fecha), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, NominaEmpleado = user.Nomina ?? 0 };
            db.IncidenciasOPermisos.Add(permiso);
            db.SaveChanges();
        }
        if (caso == "Vacaciones")
        {
            vacaciones = new Vacaciones { Id = 13, Fecha = DateOnly.FromDateTime(fecha), ActividadDelDia = TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp };
            db.Vacaciones.Add(vacaciones);
            db.SaveChanges();
        }
        if (caso == "Turno")
        {
            turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
            db.TurnosXRolSemanalXRegla.Add(turno);
            db.SaveChanges();
        }

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var generator = new EmployeesCalendarsGenerator(db, loggerFactory.CreateLogger<EmployeesCalendarsGenerator>(), fecha, fecha);
        // Act
        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, diaContinental, diaLey, permiso, vacaciones);
        // Assert
        if (caso == "Ley")
        {
            dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.InhabilPorLey);
            dia.IdDiaInhabil.Should().Be(diaLey.Id);
            dia.DetallesDiaInhabil.Should().Be("Ley");
        }
        if (caso == "Continental")
        {
            dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.InhabilPorContinental);
            dia.IdDiaInhabil.Should().Be(diaContinental.Id);
            dia.DetallesDiaInhabil.Should().Be("Continental");
        }
        if (caso == "Permiso")
        {
            dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.IncidenciaOPermiso);
            dia.IdIncidenciaOPermiso.Should().Be(permiso.Id);
            dia.TipoDeIncedencia.Should().Be(TiposDeIncidenciasEnum.PermisoConGoce);
        }
        if (caso == "Vacaciones")
        {
            dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp);
            dia.IdVacaciones.Should().Be(vacaciones.Id);
        }
        if (caso == "Turno")
        {
            dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.Laboral);
            dia.Turno.Should().Be(TurnosEnum.Matutino);
            dia.IdTurnoXRolSemanalXRegla.Should().Be(turno.Id);
        }
    }

    [Fact]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadLeySobreTurno()
    {
        var db = GetDb("CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadLeySobreTurno");
        // Arrange
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);
        var turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
        var diaLey = new DiasInhabiles { Id = 10, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey, Detalles = "Ley" };

        // store in DB to simulate real scenario
        db.Roles.Add(rol);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.Users.Add(user);
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        db.CalendarioEmpleados.Add(calendario);
        db.DiasInhabiles.Add(diaLey);
        db.SaveChanges();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var generator = new EmployeesCalendarsGenerator(db, loggerFactory.CreateLogger<EmployeesCalendarsGenerator>(), fecha, fecha);

        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, null, diaLey, null, null);
        dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.InhabilPorLey);
        dia.IdDiaInhabil.Should().Be(diaLey.Id);
        dia.DetallesDiaInhabil.Should().Be("Ley");
        dia.Turno.Should().Be(TurnosEnum.Descanso);
    }

    [Fact]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadContinentalSobreTurno()
    {
        var db = GetDb("CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadContinentalSobreTurno");

        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);
        var turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
        var diaContinental = new DiasInhabiles { Id = 11, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental, Detalles = "Continental" };

        // store in DB to simulate real scenario
        db.Roles.Add(rol);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.Users.Add(user);
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        db.CalendarioEmpleados.Add(calendario);
        db.DiasInhabiles.Add(diaContinental);
        db.SaveChanges();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var generator = new EmployeesCalendarsGenerator(db, loggerFactory.CreateLogger<EmployeesCalendarsGenerator>(), fecha, fecha);
        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, diaContinental, null, null, null);
        dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.InhabilPorContinental);
        dia.IdDiaInhabil.Should().Be(diaContinental.Id);
        dia.DetallesDiaInhabil.Should().Be("Continental");
        dia.Turno.Should().Be(TurnosEnum.Descanso);
    }

    [Fact]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadPermisoSobreTurno()
    {
        var db = GetDb("CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadPermisoSobreTurno");
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);
        var turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
        var permiso = new IncidenciaOPermiso { Id = 12, Fecha = DateOnly.FromDateTime(fecha), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, NominaEmpleado = user.Nomina ?? 0 };

        // store in db
        db.Users.Add(user);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.CalendarioEmpleados.Add(calendario);
        db.IncidenciasOPermisos.Add(permiso);
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        db.SaveChanges();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var generator = new EmployeesCalendarsGenerator(db, loggerFactory.CreateLogger<EmployeesCalendarsGenerator>(), fecha, fecha);
        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, null, null, permiso, null);
        dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.IncidenciaOPermiso);
        dia.IdIncidenciaOPermiso.Should().Be(permiso.Id);
        dia.TipoDeIncedencia.Should().Be(TiposDeIncidenciasEnum.PermisoConGoce);
        dia.Turno.Should().Be(TurnosEnum.Descanso);
    }

    [Fact]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadVacacionesSobreTurno()
    {
        var db = GetDb("CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadVacacionesSobreTurno");
        
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);
        var turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
        var vacaciones = new Vacaciones { Id = 13, Fecha = DateOnly.FromDateTime(fecha), ActividadDelDia = TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp };

        db.Users.Add(user);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.CalendarioEmpleados.Add(calendario);
        db.Vacaciones.Add(vacaciones);
        db.SaveChanges();


        var loggerReal = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, loggerReal, fecha, fecha);

        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, null, null, null, vacaciones);
        dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp);
        dia.IdVacaciones.Should().Be(vacaciones.Id);
        dia.Turno.Should().Be(TurnosEnum.Descanso);
    }

    [Fact]
    void CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadCompleta()
    {
        var db = GetDb("CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos_PrioridadCompleta");
        // Arrange
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, TotalDiasDeVacaciones = 5, DiasParaAsignarAutomaticamente = 2 };
        var rol = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        var user = new User { Id = 1, FullName = "Empleado", AreaId = 1, GrupoId = 1, Nomina = 100, Username = "empleado", PasswordHash = "hash", PasswordSalt = "salt", Roles = new List<Rol> { rol }, VacacionesPorAntiguedad = vacacionesPorAntiguedad };
        var grupo = new Grupo { GrupoId = 1, IdentificadorSAP = "Grupo 1", Rol = "Grupo 1" };
        var regla = new Regla { Id = 1, Nombre = "Regla", Descripcion = "Regla", TurnosXRolSemanalXRegla = new List<TurnoXRolSemanalXRegla>() };
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "Rol Semanal", Regla = regla };
        var calendario = new CalendarioEmpleado { Id = 1, IdRegla = regla.Id, IdArea = user.AreaId, IdGrupo = user.GrupoId, IdUsuarioEmpleadoSindicalizado = user.Id, NominaEmpleado = user.Nomina ?? 0 };
        
        var datos = new DatosTempEmpleadoSindicalizado(user, calendario, grupo, regla, rolSemanal, null!, vacacionesPorAntiguedad);
        var fecha = new DateTime(2025, 9, 2);
        var turno = new TurnoXRolSemanalXRegla { Id = 14, IdRegla = regla.Id, IdRolSemanal = rolSemanal.RolSemanalId, IndicePorRegla = 0, DiaDeLaSemana = DiasDeLaSemanaEnum.Martes, Turno = TurnosEnum.Matutino, ActividadDelDia = TipoActividadDelDiaEnum.Laboral, Regla = regla, RolSemanal = rolSemanal };
        var diaLey = new DiasInhabiles { Id = 10, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey, Detalles = "Ley" };
        var diaContinental = new DiasInhabiles { Id = 11, Fecha = DateOnly.FromDateTime(fecha), TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental, Detalles = "Continental" };
        var permiso = new IncidenciaOPermiso { Id = 12, Fecha = DateOnly.FromDateTime(fecha), TiposDeIncedencia = TiposDeIncidenciasEnum.PermisoConGoce, NominaEmpleado = user.Nomina ?? 0 };
        var vacaciones = new Vacaciones { Id = 13, Fecha = DateOnly.FromDateTime(fecha), ActividadDelDia = TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp };

        db.Users.Add(user);
        db.Grupos.Add(grupo);
        db.Reglas.Add(regla);
        db.RolesSemanales.Add(rolSemanal);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.CalendarioEmpleados.Add(calendario);
        db.DiasInhabiles.Add(diaLey);
        db.DiasInhabiles.Add(diaContinental);
        db.IncidenciasOPermisos.Add(permiso);
        db.Vacaciones.Add(vacaciones);
        db.SaveChanges();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var generator = new EmployeesCalendarsGenerator(db, loggerFactory.CreateLogger<EmployeesCalendarsGenerator>(), fecha, fecha);
        var dia = generator.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datos, fecha, turno, diaContinental, diaLey, permiso, vacaciones);
        dia.TipoActividadDelDia.Should().Be(TipoActividadDelDiaEnum.InhabilPorLey);
        dia.IdDiaInhabil.Should().Be(diaLey.Id);
        dia.DetallesDiaInhabil.Should().Be("Ley");
        // dia.IdIncidenciaOPermiso.Should().BeNull();
        // dia.IdVacaciones.Should().BeNull();
        dia.Turno.Should().Be(TurnosEnum.Descanso);
    }

}
