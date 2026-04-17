using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using tiempo_libre.Helpers;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly FreeTimeDbContext _db;

        public DashboardController(FreeTimeDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Retorna ausencias agrupadas por mes para un año dado
        /// </summary>
        [HttpGet("ausencias-anuales")]
        public async Task<IActionResult> GetAusenciasAnuales([FromQuery] int anio = 0, [FromQuery] int? areaId = null)
        {
            if (anio == 0) anio = DateTime.Today.Year;

            var inicio = new DateOnly(anio, 1, 1);
            var fin = new DateOnly(anio, 12, 31);

            // Obtener días inhabiles en el rango completo
            var diasInhabiles = await _db.DiasInhabiles
                .Where(d => d.FechaInicial >= inicio && d.FechaFinal <= fin)
                .Select(d => d.Fecha)
                .ToHashSetAsync();

            var empleadosQuery = _db.Users
                .Where(u => u.Status == tiempo_libre.Models.Enums.UserStatus.Activo && u.GrupoId != null);
            if (areaId.HasValue)
                empleadosQuery = empleadosQuery.Where(u => u.AreaId == areaId.Value);

            var empleados = await empleadosQuery
                .Select(u => new { u.Id, u.Nomina, u.GrupoId })
                .ToListAsync();

            var empleadoIds = empleados.Select(e => e.Id).ToList();
            var nominasActivas = empleados.Where(e => e.Nomina.HasValue)
                .Select(e => e.Nomina!.Value)
                .ToHashSet();

            // Obtener grupos para manning
            var grupos = await _db.Grupos
                .Where(g => (areaId == null || g.AreaId == areaId.Value) && g.Area != null)
                .Select(g => new { g.GrupoId, g.AreaId, Manning = g.Area.Manning })
                .ToListAsync();

            var manningPorArea = grupos
                .GroupBy(g => g.AreaId)
                .ToDictionary(g => g.Key, g => g.First().Manning);

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => empleadoIds.Contains(v.EmpleadoId) &&
                            v.FechaVacacion >= inicio && v.FechaVacacion <= fin &&
                            v.EstadoVacacion == "Activa")
                .Select(v => new { v.EmpleadoId, v.FechaVacacion, v.TipoVacacion })
                .ToListAsync();

            var permisosRaw = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde >= inicio && p.Desde <= fin &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .Select(p => new { p.Desde, p.ClaseAbsentismo, p.Nomina, p.Hasta })
                .ToListAsync();

            var permisos = areaId.HasValue
                ? permisosRaw.Where(p => nominasActivas.Contains(p.Nomina)).ToList()
                : permisosRaw;

            var resultado = Enumerable.Range(1, 12).Select(mes => {
                var mesInicio = new DateOnly(anio, mes, 1);
                var mesFin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

                // Contar TURNOS disponibles en el mes (no empleados)
                int turnosDisponiblesMes = 0;
                for (var fecha = mesInicio; fecha <= mesFin; fecha = fecha.AddDays(1))
                {
                    if (!diasInhabiles.Contains(fecha))
                    {
                        int manningArea = areaId.HasValue && manningPorArea.ContainsKey(areaId.Value)
                            ? (int)manningPorArea[areaId.Value]
                            : manningPorArea.Values.Sum(v => (int)v);
                        turnosDisponiblesMes += manningArea;
                    }
                }

                var vacacionesMes = vacaciones
                    .Where(v => v.FechaVacacion.Month == mes && v.TipoVacacion == "Anual")
                    .Count();

                var reprogramacionMes = vacaciones
                    .Where(v => v.FechaVacacion.Month == mes && v.TipoVacacion == "Reprogramacion")
                    .Count();

                var festivoMes = vacaciones
                    .Where(v => v.FechaVacacion.Month == mes && v.TipoVacacion == "FestivoTrabajado")
                    .Count();

                var permisosMes = 0;
                var incapacidadesMes = 0;

                for (var fecha = mesInicio; fecha <= mesFin; fecha = fecha.AddDays(1))
                {
                    var permisosDelDia = permisos
                        .Where(p => p.Desde <= fecha && p.Hasta >= fecha &&
                                   (p.ClaseAbsentismo?.Contains("Permiso") == true ||
                                    p.ClaseAbsentismo?.Contains("Enfermedad") == true))
                        .Count();

                    var incapacidadesDelDia = permisos
                        .Where(p => p.Desde <= fecha && p.Hasta >= fecha &&
                                   (p.ClaseAbsentismo?.Contains("Incapacidad") == true ||
                                    p.ClaseAbsentismo?.Contains("Accidente") == true ||
                                    p.ClaseAbsentismo?.Contains("Maternidad") == true))
                        .Count();

                    permisosMes += permisosDelDia;
                    incapacidadesMes += incapacidadesDelDia;
                }

                return new
                {
                    mes,
                    totalEmpleados = empleados.Count,
                    turnosDisponibles = turnosDisponiblesMes,
                    vacacion = vacacionesMes,
                    reprogramacion = reprogramacionMes,
                    festivoTrabajado = festivoMes,
                    permiso = permisosMes,
                    incapacidad = incapacidadesMes,
                };
            }).ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }

        /// <summary>
        /// Retorna ausencias por semana del mes actual
        /// </summary>
        [HttpGet("ausencias-semanales")]
        public async Task<IActionResult> GetAusenciasSemanales([FromQuery] int anio = 0, [FromQuery] int mes = 0, [FromQuery] int? areaId = null)
        {
            if (anio == 0) anio = DateTime.Today.Year;
            if (mes == 0) mes = DateTime.Today.Month;

            var inicio = new DateOnly(anio, mes, 1);
            var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

            var diasInhabilesRaw = await _db.DiasInhabiles
                .Where(d => d.FechaFinal >= inicio && d.FechaInicial <= fin)
                .ToListAsync();

            var diasInhabiles = new HashSet<DateOnly>();
            foreach (var d in diasInhabilesRaw)
            {
                for (var f = d.FechaInicial; f <= d.FechaFinal && f <= fin; f = f.AddDays(1))
                {
                    if (f >= inicio) diasInhabiles.Add(f);
                }
            }

            var empleadosQuery = _db.Users
                .Where(u => u.Status == tiempo_libre.Models.Enums.UserStatus.Activo && u.GrupoId != null);
            if (areaId.HasValue)
                empleadosQuery = empleadosQuery.Where(u => u.AreaId == areaId.Value);

            var empleados = await empleadosQuery.Select(u => new { u.Id, u.Nomina }).ToListAsync();
            var empleadoIds = empleados.Select(e => e.Id).ToList();
            var nominasSet = empleados.Where(e => e.Nomina.HasValue)
                .Select(e => e.Nomina!.Value).ToHashSet();

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => empleadoIds.Contains(v.EmpleadoId) &&
                            v.FechaVacacion >= inicio && v.FechaVacacion <= fin &&
                            v.EstadoVacacion == "Activa")
                .Select(v => new { v.FechaVacacion, v.TipoVacacion })
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde <= fin && p.Hasta >= inicio &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .Select(p => new { p.Desde, p.Hasta, Tipo = p.ClaseAbsentismo ?? "Permiso", p.Nomina })
                .ToListAsync();

            var permisosFiltered = areaId.HasValue
                ? permisos.Where(p => nominasSet.Contains(p.Nomina)).ToList()
                : permisos;

            int GetWeek(DateOnly d) => (d.Day - 1) / 7 + 1;

            var resultado = Enumerable.Range(1, 5).Select(semana => {
                int turnosDisponiblesSemana = 0;

                for (int day = 1; day <= DateTime.DaysInMonth(anio, mes); day++)
                {
                    var fecha = new DateOnly(anio, mes, day);
                    if (GetWeek(fecha) == semana && !diasInhabiles.Contains(fecha))
                    {
                        turnosDisponiblesSemana += empleados.Count;
                    }
                }

                return new
                {
                    semana,
                    turnosDisponibles = turnosDisponiblesSemana,
                    vacacion = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "Anual"),
                    reprogramacion = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "Reprogramacion"),
                    festivoTrabajado = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "FestivoTrabajado"),
                    permiso = permisosFiltered.Count(p => GetWeek(p.Desde) >= semana && GetWeek(p.Hasta) <= semana && p.Tipo.Contains("Permiso")),
                    incapacidad = permisosFiltered.Count(p => GetWeek(p.Desde) >= semana && GetWeek(p.Hasta) <= semana && p.Tipo.Contains("Incapacidad")),
                };
            }).ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }

        /// <summary>
        /// Retorna el desglose de motivos de ausencia del día actual (o fecha indicada)
        /// </summary>
        [HttpGet("ausencias-motivos")]
        public async Task<IActionResult> GetAusenciasMotivos([FromQuery] string? fecha = null, [FromQuery] string? fechaFin = null)
        {
            var dia = fecha != null && DateOnly.TryParse(fecha, out var d) ? d : DateOnly.FromDateTime(DateTime.Today);
            var diaFin = fechaFin != null && DateOnly.TryParse(fechaFin, out var df) ? df : dia;

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => v.FechaVacacion >= dia && v.FechaVacacion <= diaFin && v.EstadoVacacion == "Activa")
                .GroupBy(v => v.TipoVacacion)
                .Select(g => new { Motivo = g.Key, Total = g.Count() })
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde <= diaFin && p.Hasta >= dia &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .GroupBy(p => p.ClaseAbsentismo)
                .Select(g => new { Motivo = g.Key ?? "Permiso", Total = g.Count() })
                .ToListAsync();

            var resultado = vacaciones.Select(v => new { motivo = v.Motivo, total = v.Total })
                .Concat(permisos.Select(p => new { motivo = p.Motivo, total = p.Total }))
                .ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }

        /// <summary>
        /// Retorna resumen de horas normales vs tiempo extra por semana del mes
        /// </summary>
        [HttpGet("resumen-tiempo-extra")]
        public async Task<IActionResult> GetResumenTiempoExtra(
            [FromQuery] int anio = 0,
            [FromQuery] int mes = 0,
            [FromQuery] int? areaId = null)
        {
            if (anio == 0) anio = DateTime.Today.Year;
            if (mes == 0) mes = DateTime.Today.Month;

            var resultado = await CalcularResumenTiempoExtraMes(anio, mes, areaId);
            var response = resultado.Select(r => new
            {
                r.Semana,
                horasExtra = Math.Round(r.HorasExtra, 1),
                horasNormales = Math.Round(r.HorasNormales, 1),
                pctExtra = r.HorasNormales > 0
                    ? Math.Round(r.HorasExtra / r.HorasNormales * 100, 1)
                    : 0.0
            }).ToList();

            return Ok(new ApiResponse<object>(true, response));
        }

        [HttpGet("resumen-tiempo-extra-anual")]
        public async Task<IActionResult> GetResumenTiempoExtraAnual(
            [FromQuery] int anio = 0,
            [FromQuery] int? areaId = null)
        {
            if (anio == 0) anio = DateTime.Today.Year;
            var resultado = new List<object>();

            for (int mes = 1; mes <= 12; mes++)
            {
                var resumenMes = await CalcularResumenTiempoExtraMes(anio, mes, areaId);
                resultado.Add(new
                {
                    mes,
                    horasExtra = Math.Round(resumenMes.Sum(r => r.HorasExtra), 1),
                    horasNormales = Math.Round(resumenMes.Sum(r => r.HorasNormales), 1),
                    pctExtra = resumenMes.Sum(r => r.HorasNormales) > 0
                        ? Math.Round(resumenMes.Sum(r => r.HorasExtra) / resumenMes.Sum(r => r.HorasNormales) * 100, 1)
                        : 0.0
                });
            }

            return Ok(new ApiResponse<object>(true, resultado));
        }

        private record ResumenSemanaDto(int Semana, double HorasExtra, double HorasNormales);

        private async Task<List<ResumenSemanaDto>> CalcularResumenTiempoExtraMes(
    int anio, int mes, int? areaId)
        {
            var inicio = new DateOnly(anio, mes, 1);
            var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

            var gruposQuery = _db.Grupos.Include(g => g.Area).AsQueryable();
            if (areaId.HasValue)
                gruposQuery = gruposQuery.Where(g => g.AreaId == areaId.Value);

            var grupos = await gruposQuery.ToListAsync();

            if (!grupos.Any()) return new List<ResumenSemanaDto>();

            var grupoIds = grupos.Select(g => g.GrupoId).ToList();
            var empleadosPorGrupo = await _db.Users
                .Where(u => grupoIds.Contains(u.GrupoId ?? 0) &&
                            u.Status == tiempo_libre.Models.Enums.UserStatus.Activo)
                .Select(u => new { u.Id, u.GrupoId, u.Nomina })
                .ToListAsync();

            var empleadosIds = empleadosPorGrupo.Select(e => e.Id).ToList();
            var nominasActivas = empleadosPorGrupo
                .Where(e => e.Nomina.HasValue)
                .Select(e => e.Nomina!.Value).Distinct().ToList();

            var diasInhabilesRaw = await _db.DiasInhabiles
                .Where(d => d.FechaFinal >= inicio && d.FechaInicial <= fin)
                .ToListAsync();

            var diasInhabiles = new HashSet<DateOnly>();
            foreach (var d in diasInhabilesRaw)
            {
                for (var f = d.FechaInicial; f <= d.FechaFinal && f <= fin; f = f.AddDays(1))
                {
                    if (f >= inicio) diasInhabiles.Add(f);
                }
            }

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => empleadosIds.Contains(v.EmpleadoId) &&
                            v.FechaVacacion >= inicio && v.FechaVacacion <= fin &&
                            v.EstadoVacacion == "Activa")
                .Select(v => new { v.EmpleadoId, v.FechaVacacion }).ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => nominasActivas.Contains(p.Nomina) &&
                            p.Desde <= fin && p.Hasta >= inicio &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .Select(p => new { p.Nomina, p.Desde, p.Hasta }).ToListAsync();

            var excepcionesManning = await _db.ExcepcionesManning
                .Where(e => grupos.Select(g => g.AreaId).Contains(e.AreaId) &&
                            e.Anio == anio && e.Mes == mes && e.Activa)
                .ToListAsync();

            static int GetWeek(DateOnly d) => (d.Day - 1) / 7 + 1;

            var resultado = new List<ResumenSemanaDto>();

            // Calcular primer lunes que cae dentro o antes del inicio del mes
            var primerLunes = inicio;
            while (primerLunes.DayOfWeek != DayOfWeek.Monday)
                primerLunes = primerLunes.AddDays(-1);

            var semanas = new List<(DateOnly desde, DateOnly hasta)>();
            var cursor = primerLunes;
            while (cursor <= fin)
            {
                var desde = cursor < inicio ? inicio : cursor;
                var hasta = cursor.AddDays(6) > fin ? fin : cursor.AddDays(6);
                if (desde <= fin)
                    semanas.Add((desde, hasta));
                cursor = cursor.AddDays(7);
            }

            int numSemana = 1;

            foreach (var (desde, hasta) in semanas)
            {
                var diasSemana = Enumerable.Range(0, hasta.DayNumber - desde.DayNumber + 1)
                    .Select(i => desde.AddDays(i))
                    .ToList();

                double totalHorasExtra = 0;
                double totalHorasNormales = 0;

                foreach (var dia in diasSemana)
                {
                    if (diasInhabiles.Contains(dia)) continue;

                    double disponiblesDia = 0;
                    bool hayActividadArea = false;

                    foreach (var grupo in grupos)
                    {
                        var rolGrupo = grupo.Rol ?? string.Empty;
                        var turnoDelDia = TurnosHelper.ObtenerTurnoParaFecha(rolGrupo, dia);

                        if (turnoDelDia == "1" || turnoDelDia == "2" || turnoDelDia == "3")
                        {
                            hayActividadArea = true;

                            var empGrupo = empleadosPorGrupo.Where(e => e.GrupoId == grupo.GrupoId).ToList();
                            var empIds = empGrupo.Select(e => e.Id).ToHashSet();
                            var nominasGrupo = empGrupo
                                .Where(e => e.Nomina.HasValue)
                                .Select(e => e.Nomina!.Value)
                                .ToHashSet();

                            var ausentesVac = vacaciones.Count(v =>
                                empIds.Contains(v.EmpleadoId) && v.FechaVacacion == dia);

                            var ausentesPerm = permisos.Count(p =>
                                nominasGrupo.Contains(p.Nomina) && p.Desde <= dia && p.Hasta >= dia);

                            disponiblesDia += empGrupo.Count - Math.Min(ausentesVac + ausentesPerm, empGrupo.Count);
                        }
                    }

                    if (!hayActividadArea) continue;

                    var primerGrupo = grupos.First();
                    var excManning = excepcionesManning.FirstOrDefault(e => e.AreaId == primerGrupo.AreaId);
                    var manning = (double)(excManning?.ManningRequeridoExcepcion ?? primerGrupo.Area?.Manning ?? 0);

                        if (manning > 0)
                        {
                            totalHorasNormales += Math.Min(disponiblesDia, manning) * 8;

                            var deficit = manning - disponiblesDia;
                            if (deficit > 0)
                                totalHorasExtra += deficit * 8;
                        }

                }

                resultado.Add(new ResumenSemanaDto(numSemana++, totalHorasExtra, totalHorasNormales));
            }

            return resultado;

        }

        [HttpGet("resumen-tiempo-extra-semanal-v2")]
        public async Task<IActionResult> GetResumenTiempoExtraSemanalV2(
    [FromQuery] int anio = 0,
    [FromQuery] int mes = 0,
    [FromQuery] int? areaId = null)
        {
            if (anio == 0) anio = DateTime.Today.Year;
            if (mes == 0) mes = DateTime.Today.Month;

            var inicio = new DateOnly(anio, mes, 1);
            var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

            // Obtener días inhabiles
            var diasInhabiles = await _db.DiasInhabiles
                .Where(d => d.Fecha >= inicio && d.Fecha <= fin)
                .Select(d => d.Fecha)
                .ToHashSetAsync();

            var gruposQuery = _db.Grupos.Include(g => g.Area).AsQueryable();
            if (areaId.HasValue)
                gruposQuery = gruposQuery.Where(g => g.AreaId == areaId.Value);

            var grupos = await gruposQuery.ToListAsync();
            if (!grupos.Any()) return Ok(new ApiResponse<object>(true, new List<object>()));

            var grupoIds = grupos.Select(g => g.GrupoId).ToList();

            var empleadosPorGrupo = await _db.Users
                .Where(u => grupoIds.Contains(u.GrupoId ?? 0) &&
                            u.Status == tiempo_libre.Models.Enums.UserStatus.Activo)
                .Select(u => new { u.Id, u.GrupoId })
                .ToListAsync();

            var empleadosIds = empleadosPorGrupo.Select(e => e.Id).ToList();

            // ← AQUÍ: Obtener datos reales de WeeklyRoles
            // Calcular basándose en disponibilidad real por día
            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => empleadosIds.Contains(v.EmpleadoId) &&
                            v.FechaVacacion >= inicio && v.FechaVacacion <= fin &&
                            v.EstadoVacacion == "Activa")
                .Select(v => new { v.EmpleadoId, v.FechaVacacion })
                .ToListAsync();

            var nominasActivas = await _db.Users
                .Where(u => grupoIds.Contains(u.GrupoId ?? 0) &&
                            u.Status == tiempo_libre.Models.Enums.UserStatus.Activo &&
                            u.Nomina.HasValue)
                .Select(u => u.Nomina!.Value)
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => nominasActivas.Contains(p.Nomina) &&
                            p.Desde <= fin && p.Hasta >= inicio &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .Select(p => new { p.Nomina, p.Desde, p.Hasta })
                .ToListAsync();

            static int GetWeekOfMonth(DateOnly d) => (d.Day - 1) / 7 + 1;
            static int GetWeekOfYear(int anio, int mes, int semana)
            {
                var primerDia = new DateOnly(anio, mes, Math.Min((semana - 1) * 7 + 1, DateTime.DaysInMonth(anio, mes)));
                var jan1 = new DateTime(primerDia.Year, 1, 1);
                var fecha = primerDia.ToDateTime(TimeOnly.MinValue);
                return (int)Math.Ceiling((fecha - jan1).TotalDays / 7) + 1;
            }

            var resultado = new List<object>();

            for (int semana = 1; semana <= 5; semana++)
            {
                var diasSemana = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
                    .Select(d => new DateOnly(anio, mes, d))
                    .Where(d => GetWeekOfMonth(d) == semana)
                    .ToList();

                if (!diasSemana.Any()) continue;

                double totalHorasNormales = 0;
                double totalHorasExtra = 0;

                foreach (var grupo in grupos)
                {
                    var empGrupo = empleadosPorGrupo.Where(e => e.GrupoId == grupo.GrupoId).ToList();
                    var totalEmp = empGrupo.Count;
                    if (totalEmp == 0) continue;

                    var empIds = empGrupo.Select(e => e.Id).ToHashSet();
                    var manning = (double)(grupo.Area?.Manning ?? 0);
                    if (manning <= 0) continue;

                    var rolGrupo = grupo.Rol ?? string.Empty;

                    foreach (var dia in diasSemana)
                    {
                        // Excluir días inhabiles
                        if (diasInhabiles.Contains(dia)) continue;

                        // Verificar si el grupo trabaja ese día
                        var turnoDelDia = TurnosHelper.ObtenerTurnoParaFecha(rolGrupo, dia);
                        if (turnoDelDia != "1" && turnoDelDia != "2" && turnoDelDia != "3") continue;

                        // Contar disponibles reales
                        var ausentesVac = vacaciones.Count(v =>
                            empIds.Contains(v.EmpleadoId) && v.FechaVacacion == dia);

                        var ausentesPerm = 0;
                        foreach (var perm in permisos)
                        {
                            if (perm.Desde <= dia && perm.Hasta >= dia)
                            {
                                var emp = await _db.Users.FirstOrDefaultAsync(u => u.Nomina == perm.Nomina);
                                if (emp != null && empIds.Contains(emp.Id))
                                    ausentesPerm++;
                            }
                        }

                        var totalAusentes = Math.Min(ausentesVac + ausentesPerm, totalEmp);
                        var disponibles = totalEmp - totalAusentes;

                        // Horas normales = disponibles × 8
                        totalHorasNormales += Math.Min((double)disponibles, manning) * 8;

                        // Horas extra = déficit × 8 (solo si hay déficit)
                        var deficit = manning - (double)disponibles;
                        if (deficit > 0)
                            totalHorasExtra += deficit * 8;
                    }
                }

                var semanaAnual = GetWeekOfYear(anio, mes, semana);

                resultado.Add(new
                {
                    semana,
                    semanaAnual,
                    horasNormales = Math.Round(totalHorasNormales, 1),
                    horasExtra = Math.Round(totalHorasExtra, 1),
                    pctExtra = totalHorasNormales > 0
                        ? Math.Round(totalHorasExtra / totalHorasNormales * 100, 1)
                        : 0.0
                });
            }

            return Ok(new ApiResponse<object>(true, resultado));
        }

    }
}