using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;

public class EmployeesCalendarsGenerator_GeneraCalendarioPara_Tests
{
    private FreeTimeDbContext GetDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new FreeTimeDbContext(options);
    }

    public List<TurnoXRolSemanalXRegla> GetTurnosForRegla(FreeTimeDbContext db, Regla regla, RolSemanal rolSemanal)
    {
        var turnosXRolSemanasXRegla = new List<TurnoXRolSemanalXRegla>(){
        new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IndicePorRegla = 0,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 2,
            IndicePorRegla = 1,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Martes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 3,
            IndicePorRegla = 2,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Miercoles,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 4,
            IndicePorRegla = 3,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Jueves,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 5,
            IndicePorRegla = 4,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Viernes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 6,
            IndicePorRegla = 5,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Sabado,
            ActividadDelDia = TipoActividadDelDiaEnum.DescansoSemanal,
            Turno = TurnosEnum.Descanso,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 7,
            IndicePorRegla = 6,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Domingo,
            ActividadDelDia = TipoActividadDelDiaEnum.DescansoSemanal,
            Turno = TurnosEnum.Descanso,
            Regla = regla,
            RolSemanal = rolSemanal
        }
        };

        db.TurnosXRolSemanalXRegla.AddRange(turnosXRolSemanasXRegla);
        db.SaveChanges();

        return turnosXRolSemanasXRegla;
    }

    [Fact]
    public void UsuarioValido_ConDiasInhabilesLeyYContinental_GeneraDiasCorrectos()
    {
        var db = GetDb("UsuarioValido_ConDiasInhabilesLeyYContinental");
        var area = new Area { AreaId = 1, NombreGeneral = "Area 1", UnidadOrganizativaSap = "UO1" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 1, AreaId = area.AreaId, IdentificadorSAP = "G1", Rol = "Grupo 1" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 1, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 2, DiasPorEscogerPorEmpleado = 3 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 1,
            FullName = "Empleado Correcto",
            Username = "empleado1",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1001,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var regla = new Regla { Id = 1, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 1", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 1, Rol = "R0144", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);
        var turnos = GetTurnosForRegla(db, regla, rolSemanal);
        regla.TurnosXRolSemanalXRegla = turnos;
        db.SaveChanges();

        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 1,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-7))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();
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
        var diasInhabilLey = new DiasInhabiles
        {
            Id = 1,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(14).Date),
            Detalles = "Día inhábil por ley",
            TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey
        };
        var diasInhabilContinental = new DiasInhabiles
        {
            Id = 2,
            Fecha = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(18).Date),
            Detalles = "Día inhábil por continental",
            TipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental
        };
        db.DiasInhabiles.AddRange(diasInhabilLey, diasInhabilContinental);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now.AddDays(7), DateTime.Now.Date.AddDays(35));
        generator.GeneraCalendarioPara(user, DateTime.Now.AddDays(7), DateTime.Now.Date.AddDays(35));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().NotBeEmpty();
        diasCalendario.Any(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorLey).Should().BeTrue();
        diasCalendario.Any(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorContinental).Should().BeTrue();
        var userActualizado = db.Users.First(u => u.Id == user.Id);
        userActualizado.VacacionesPorAntiguedadId.Should().Be(vacacionesPorAntiguedad.Id);
    }

    // Agregar aquí las demás pruebas para los casos solicitados, siguiendo el mismo patrón

    [Fact]
    public void UsuarioValido_SinDiasInhabiles_GeneraSoloDiasLaborales()
    {
        var db = GetDb("UsuarioValido_SinDiasInhabiles");
        var area = new Area { AreaId = 2, NombreGeneral = "Area 2", UnidadOrganizativaSap = "UO2" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 2, AreaId = area.AreaId, IdentificadorSAP = "G2", Rol = "Grupo 2" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 2, AntiguedadEnAniosRangoInicial = 2, TotalDiasDeVacaciones = 10, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 5, DiasPorEscogerPorEmpleado = 5 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 2,
            FullName = "Empleado Sin Inhabiles",
            Username = "empleado2",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1002,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-3)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var regla = new Regla { Id = 2, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 2", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 2, Rol = "Rol Semanal 2", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);
        var turnos = GetTurnosForRegla(db, regla, rolSemanal);
        regla.TurnosXRolSemanalXRegla = turnos;

        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 1,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();

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
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(user, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().NotBeEmpty();
        diasCalendario.All(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.Laboral || d.TipoActividadDelDia == TipoActividadDelDiaEnum.DescansoSemanal).Should().BeTrue();
    }

    [Fact]
    public void UsuarioValido_SinTurnosAsignados_NoGeneraDiasCalendario()
    {
        var db = GetDb("UsuarioValido_SinTurnosAsignados");
        var area = new Area { AreaId = 3, NombreGeneral = "Area 3", UnidadOrganizativaSap = "UO3" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 3, AreaId = area.AreaId, IdentificadorSAP = "G3", Rol = "Grupo 3" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 3, AntiguedadEnAniosRangoInicial = 3, TotalDiasDeVacaciones = 15, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 7, DiasPorEscogerPorEmpleado = 8 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 3,
            FullName = "Empleado Sin Turnos",
            Username = "empleado3",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1003,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-4)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var regla = new Regla { Id = 3, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 3", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        // No se agregan turnos
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(user, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().BeEmpty();
    }

    [Fact]
    public void UsuarioValido_SinVacacionesAsignadas_GeneraDiasSinVacaciones()
    {
        var db = GetDb("UsuarioValido_SinVacacionesAsignadas");
        var area = new Area { AreaId = 4, NombreGeneral = "Area 4", UnidadOrganizativaSap = "UO4" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 4, AreaId = area.AreaId, IdentificadorSAP = "G4", Rol = "Grupo 4" };
        db.Grupos.Add(grupo);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 4,
            FullName = "Empleado Sin Vacaciones",
            Username = "empleado4",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1004,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-5)),
            VacacionesPorAntiguedad = null,
            VacacionesPorAntiguedadId = null
        };
        db.Users.Add(user);
        db.SaveChanges();

        var regla = new Regla { Id = 4, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 4", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 4, Rol = "Rol Semanal 4", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);
        var turnos = GetTurnosForRegla(db, regla, rolSemanal).First();
        regla.TurnosXRolSemanalXRegla.Add(turnos);

        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 1,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();

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

        db.SaveChanges();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now.AddDays(7), DateTime.Now.Date.AddDays(35));
        generator.GeneraCalendarioPara(user, DateTime.Now.AddDays(7).Date, DateTime.Now.Date.AddDays(35));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().NotBeEmpty();
        diasCalendario.All(d => d.TipoActividadDelDia != TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp && d.TipoActividadDelDia != TipoActividadDelDiaEnum.VacacionesSeleccionadasPorEmp && d.TipoActividadDelDia != TipoActividadDelDiaEnum.InhabilPorContinental).Should().BeTrue();
    }

    [Fact]
    public void UsuarioValido_SinDatosIniciales_NoGeneraDiasCalendario()
    {
        var db = GetDb("UsuarioValido_SinDatosIniciales");
        var area = new Area { AreaId = 5, NombreGeneral = "Area 5", UnidadOrganizativaSap = "UO5", Manning = 100 };
        var grupo = new Grupo { GrupoId = 5, AreaId = area.AreaId, IdentificadorSAP = "G5", Rol = "Grupo 5" };
        var user = new User
        {
            Id = 5,
            FullName = "Empleado Sin Datos Iniciales",
            Username = "empleado5",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol>(),
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1005,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-6)),
            VacacionesPorAntiguedad = null,
            VacacionesPorAntiguedadId = null
        };
        db.Users.Add(user);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(user, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().BeEmpty();
    }

    [Fact]
    public void UsuarioValido_SinRolSindicalizado_NoGeneraDiasCalendario()
    {
        var db = GetDb("UsuarioValido_SinRolSindicalizado");
        var area = new Area { AreaId = 6, NombreGeneral = "Area 6", UnidadOrganizativaSap = "UO6" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 6, AreaId = area.AreaId, IdentificadorSAP = "G6", Rol = "Grupo 6" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 6, AntiguedadEnAniosRangoInicial = 6, TotalDiasDeVacaciones = 20, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 10, DiasPorEscogerPorEmpleado = 10 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolNoSindicalizado = new Rol { Id = (int)RolEnum.Delegado_Sindical, Name = "Administrativo", Description = "Empleado Administrativo", Abreviation = "ADM" };
        db.Roles.Add(rolNoSindicalizado);
        var user = new User
        {
            Id = 6,
            FullName = "Empleado Sin Rol Sindicalizado",
            Username = "empleado6",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolNoSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1006,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-7)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var regla = new Regla { Id = 6, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 6", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 6, Rol = "Rol Semanal 6", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);
        var turno = new TurnoXRolSemanalXRegla
        {
            Id = 6,
            IndicePorRegla = 0,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Jueves,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        };
        regla.TurnosXRolSemanalXRegla.Add(turno);
        db.TurnosXRolSemanalXRegla.Add(turno);
        db.SaveChanges();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(user, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().BeEmpty();
    }

    [Fact]
    public void VerificaDiasAgregadosYUsuarioActualizado()
    {
        var db = GetDb("VerificaDiasAgregadosYUsuarioActualizado");
        var area = new Area { AreaId = 7, NombreGeneral = "Area 7", UnidadOrganizativaSap = "UO7" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 7, AreaId = area.AreaId, IdentificadorSAP = "G7", Rol = "Grupo 7" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 7, AntiguedadEnAniosRangoInicial = 7, TotalDiasDeVacaciones = 25, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 12, DiasPorEscogerPorEmpleado = 13 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 7,
            FullName = "Empleado Verifica Dias",
            Username = "empleado7",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 1007,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-8)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();

        var regla = new Regla { Id = 7, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 7", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 7, Rol = "Rol Semanal 7", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);

        var turnos = GetTurnosForRegla(db, regla, rolSemanal);
        regla.TurnosXRolSemanalXRegla = turnos;
        db.SaveChanges();

        var rolInicial = new RolInicialPorEmpleado
        {
            Id = 1,
            Nomina = user.Nomina ?? 0,
            RolSemanal = rolSemanal.Rol,
            Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
        };
        db.RolesInicialesPorEmpleado.Add(rolInicial);
        db.SaveChanges();

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

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();

        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now.AddDays(10), DateTime.Now.AddDays(38));
        generator.GeneraCalendarioPara(user, DateTime.Now.AddDays(10), DateTime.Now.AddDays(38));

        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
        diasCalendario.Should().NotBeEmpty();
        var userActualizado = db.Users.First(u => u.Id == user.Id);
        userActualizado.VacacionesPorAntiguedadId.Should().Be(vacacionesPorAntiguedad.Id);
    }

    
}

public class EmployeesCalendarsGenerator_GeneraCalendarioPara_List_Tests
{
    public List<TurnoXRolSemanalXRegla> GetTurnosForRegla(FreeTimeDbContext db, Regla regla, RolSemanal rolSemanal)
    {
        var turnosXRolSemanasXRegla = new List<TurnoXRolSemanalXRegla>(){
        new TurnoXRolSemanalXRegla
        {
            Id = 1,
            IndicePorRegla = 0,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Lunes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 2,
            IndicePorRegla = 1,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Martes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 3,
            IndicePorRegla = 2,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Miercoles,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 4,
            IndicePorRegla = 3,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Jueves,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 5,
            IndicePorRegla = 4,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Viernes,
            ActividadDelDia = TipoActividadDelDiaEnum.Laboral,
            Turno = TurnosEnum.Matutino,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 6,
            IndicePorRegla = 5,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Sabado,
            ActividadDelDia = TipoActividadDelDiaEnum.DescansoSemanal,
            Turno = TurnosEnum.Descanso,
            Regla = regla,
            RolSemanal = rolSemanal
        },
        new TurnoXRolSemanalXRegla
        {
            Id = 7,
            IndicePorRegla = 6,
            IdRegla = regla.Id,
            IdRolSemanal = rolSemanal.RolSemanalId,
            DiaDeLaSemana = DiasDeLaSemanaEnum.Domingo,
            ActividadDelDia = TipoActividadDelDiaEnum.DescansoSemanal,
            Turno = TurnosEnum.Descanso,
            Regla = regla,
            RolSemanal = rolSemanal
        }
        };

        db.TurnosXRolSemanalXRegla.AddRange(turnosXRolSemanasXRegla);
        db.SaveChanges();

        return turnosXRolSemanasXRegla;
    }

    private FreeTimeDbContext GetDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new FreeTimeDbContext(options);
    }

    [Fact]
    public void GeneraCalendarioPara_LlamaPorCadaUsuario()
    {
        var db = GetDb("GeneraCalendarioPara_LlamaPorCadaUsuario");
        var area = new Area { AreaId = 100, NombreGeneral = "Area 100", UnidadOrganizativaSap = "UO100" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 100, AreaId = area.AreaId, IdentificadorSAP = "G100", Rol = "Grupo 100" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 100, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 2, DiasPorEscogerPorEmpleado = 3 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var users = new List<User>();
        for (int i = 0; i < 3; i++)
        {
            var user = new User
            {
                Id = 1000 + i,
                FullName = $"Empleado {i}",
                Username = $"empleado{i}",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Roles = new List<Rol> { rolSindicalizado },
                Status = UserStatus.Activo,
                AreaId = area.AreaId,
                GrupoId = grupo.GrupoId,
                Nomina = 2000 + i,
                FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
                VacacionesPorAntiguedad = vacacionesPorAntiguedad,
                VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
            };
            db.Users.Add(user);
            users.Add(user);
        }
        db.SaveChanges();
        var regla = new Regla { Id = 100, ReglaEnumId = ReglaEnum.R0144, Nombre = "Regla 100", Descripcion = "Regla de prueba", NumDeGrupos = 1, Prioridad = 1 };
        db.Reglas.Add(regla);
        var rolSemanal = new RolSemanal { RolSemanalId = 100, Rol = "R0144", IndiceSemana = 0, IdRegla = regla.Id, Regla = regla };
        db.RolesSemanales.Add(rolSemanal);

        var turnos = GetTurnosForRegla(db, regla, rolSemanal);
        regla.TurnosXRolSemanalXRegla = turnos;
        db.SaveChanges();

        foreach (var user in users)
        {
            db.RolesInicialesPorEmpleado.Add(new RolInicialPorEmpleado
            {
                Id = user.Id,
                Nomina = user.Nomina ?? 0,
                RolSemanal = rolSemanal.Rol,
                Fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-7))
            });
        }
        db.SaveChanges();

        db.ProgramacionesAnuales.Add(new ProgramacionesAnuales
        {
            Id = 100,
            Anio = DateTime.Now.Year,
            FechaInicia = DateTime.Now.AddMonths(-1),
            BorradoLogico = false,
            Estatus = EstatusProgramacionAnualEnum.Pendiente
        });
        db.SaveChanges();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
        var generator = new EmployeesCalendarsGenerator(db, logger, DateTime.Now.AddDays(14), DateTime.Now.Date.AddDays(42));
        generator.GeneraCalendarioPara(users, DateTime.Now.AddDays(14), DateTime.Now.Date.AddDays(42));
        foreach (var user in users)
        {
            var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == user.Id).ToList();
            diasCalendario.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void GeneraCalendarioPara_ListaVacia_NoGeneraNada()
    {
        var db = GetDb("GeneraCalendarioPara_ListaVacia");
        var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(new List<User>(), DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        db.DiasCalendarioEmpleado.Should().BeEmpty();
    }

    [Fact]
    public void GeneraCalendarioPara_UsuariosNoValidos_NoGeneraDiasCalendario()
    {
        var db = GetDb("GeneraCalendarioPara_UsuariosNoValidos");
        var area = new Area { AreaId = 101, NombreGeneral = "Area 101", UnidadOrganizativaSap = "UO101" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 101, AreaId = area.AreaId, IdentificadorSAP = "G101", Rol = "Grupo 101" };
        db.Grupos.Add(grupo);
        var userSinRol = new User
        {
            Id = 2000,
            FullName = "Empleado Sin Rol",
            Username = "empleadoSinRol",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol>(),
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 3000,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            VacacionesPorAntiguedad = null,
            VacacionesPorAntiguedadId = null
        };
        db.Users.Add(userSinRol);
        db.SaveChanges();
        var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(new List<User> { userSinRol }, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        var diasCalendario = db.DiasCalendarioEmpleado.Where(d => d.IdUsuarioEmpleadoSindicalizado == userSinRol.Id).ToList();
        diasCalendario.Should().BeEmpty();
    }

    [Fact]
    public void GeneraCalendarioPara_ManejoDeExcepciones_SeRegistraEnLog()
    {
        var db = GetDb("GeneraCalendarioPara_ManejoDeExcepciones");
        var area = new Area { AreaId = 102, NombreGeneral = "Area 102", UnidadOrganizativaSap = "UO102" };
        db.Areas.Add(area);
        var grupo = new Grupo { GrupoId = 102, AreaId = area.AreaId, IdentificadorSAP = "G102", Rol = "Grupo 102" };
        db.Grupos.Add(grupo);
        var vacacionesPorAntiguedad = new VacacionesPorAntiguedad { Id = 102, AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 5, DiasAsignadosPorContinental = 0, DiasParaAsignarAutomaticamente = 2, DiasPorEscogerPorEmpleado = 3 };
        db.VacacionesPorAntiguedad.Add(vacacionesPorAntiguedad);
        var rolSindicalizado = new Rol { Id = (int)RolEnum.Empleado_Sindicalizado, Name = "Sindicalizado", Description = "Empleado Sindicalizado", Abreviation = "SIND" };
        db.Roles.Add(rolSindicalizado);
        var user = new User
        {
            Id = 3000,
            FullName = "Empleado Excepcion",
            Username = "empleadoExcepcion",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Roles = new List<Rol> { rolSindicalizado },
            Status = UserStatus.Activo,
            AreaId = area.AreaId,
            GrupoId = grupo.GrupoId,
            Nomina = 4000,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
            VacacionesPorAntiguedad = vacacionesPorAntiguedad,
            VacacionesPorAntiguedadId = vacacionesPorAntiguedad.Id
        };
        db.Users.Add(user);
        db.SaveChanges();
        var loggerMock = new Mock<ILogger<EmployeesCalendarsGenerator>>();
        loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
        ));
        var generator = new EmployeesCalendarsGenerator(db, loggerMock.Object, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        generator.GeneraCalendarioPara(new List<User> { user }, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
        ), Times.AtLeastOnce());
    }
}
