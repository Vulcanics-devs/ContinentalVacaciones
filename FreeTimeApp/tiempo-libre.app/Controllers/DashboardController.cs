using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.DTOs;

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
        public async Task<IActionResult> GetAusenciasAnuales([FromQuery] int anio = 0)
        {
            if (anio == 0) anio = DateTime.Today.Year;

            var inicio = new DateOnly(anio, 1, 1);
            var fin = new DateOnly(anio, 12, 31);

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => v.FechaVacacion >= inicio && v.FechaVacacion <= fin && v.EstadoVacacion == "Activa")
                .GroupBy(v => new { v.FechaVacacion.Month, v.TipoVacacion })
                .Select(g => new { g.Key.Month, g.Key.TipoVacacion, Total = g.Count() })
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde >= inicio && p.Desde <= fin &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .GroupBy(p => new { p.Desde.Month, p.ClaseAbsentismo })
                .Select(g => new { g.Key.Month, Tipo = g.Key.ClaseAbsentismo ?? "Permiso", Total = g.Count() })
                .ToListAsync();

            var resultado = Enumerable.Range(1, 12).Select(mes => new
            {
                mes,
                vacacion = vacaciones.Where(v => v.Month == mes && v.TipoVacacion == "Anual").Sum(v => v.Total),
                reprogramacion = vacaciones.Where(v => v.Month == mes && v.TipoVacacion == "Reprogramacion").Sum(v => v.Total),
                festivoTrabajado = vacaciones.Where(v => v.Month == mes && v.TipoVacacion == "FestivoTrabajado").Sum(v => v.Total),
                permiso = permisos.Where(p => p.Month == mes && p.Tipo.Contains("Permiso")).Sum(p => p.Total),
                incapacidad = permisos.Where(p => p.Month == mes && p.Tipo.Contains("Incapacidad")).Sum(p => p.Total),
            }).ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }

        /// <summary>
        /// Retorna ausencias por semana del mes actual
        /// </summary>
        [HttpGet("ausencias-semanales")]
        public async Task<IActionResult> GetAusenciasSemanales([FromQuery] int anio = 0, [FromQuery] int mes = 0)
        {
            if (anio == 0) anio = DateTime.Today.Year;
            if (mes == 0) mes = DateTime.Today.Month;

            var inicio = new DateOnly(anio, mes, 1);
            var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => v.FechaVacacion >= inicio && v.FechaVacacion <= fin && v.EstadoVacacion == "Activa")
                .Select(v => new { v.FechaVacacion, v.TipoVacacion })
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde >= inicio && p.Desde <= fin &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .Select(p => new { Fecha = p.Desde, Tipo = p.ClaseAbsentismo ?? "Permiso" })
                .ToListAsync();

            // Agrupar por semana del mes (1-5)
            static int GetWeek(DateOnly d) => (d.Day - 1) / 7 + 1;

            var resultado = Enumerable.Range(1, 5).Select(semana => new
            {
                semana,
                vacacion = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "Anual"),
                reprogramacion = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "Reprogramacion"),
                festivoTrabajado = vacaciones.Count(v => GetWeek(v.FechaVacacion) == semana && v.TipoVacacion == "FestivoTrabajado"),
                permiso = permisos.Count(p => GetWeek(p.Fecha) == semana && p.Tipo.Contains("Permiso")),
                incapacidad = permisos.Count(p => GetWeek(p.Fecha) == semana && p.Tipo.Contains("Incapacidad")),
            }).ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }

        /// <summary>
        /// Retorna el desglose de motivos de ausencia del día actual (o fecha indicada)
        /// </summary>
        [HttpGet("ausencias-motivos")]
        public async Task<IActionResult> GetAusenciasMotivos([FromQuery] string? fecha = null)
        {
            var dia = fecha != null && DateOnly.TryParse(fecha, out var d) ? d : DateOnly.FromDateTime(DateTime.Today);

            var vacaciones = await _db.VacacionesProgramadas
                .Where(v => v.FechaVacacion == dia && v.EstadoVacacion == "Activa")
                .GroupBy(v => v.TipoVacacion)
                .Select(g => new { Motivo = g.Key, Total = g.Count() })
                .ToListAsync();

            var permisos = await _db.PermisosEIncapacidadesSAP
                .Where(p => p.Desde <= dia && p.Hasta >= dia &&
                            (p.FechaSolicitud == null || p.EstadoSolicitud == "Aprobada"))
                .GroupBy(p => p.ClaseAbsentismo)
                .Select(g => new { Motivo = g.Key ?? "Permiso", Total = g.Count() })
                .ToListAsync();

            var resultado = vacaciones.Select(v => new { motivo = v.Motivo, total = v.Total })
                .Concat(permisos.Select(p => new { motivo = p.Motivo, total = p.Total }))
                .ToList();

            return Ok(new ApiResponse<object>(true, resultado));
        }
    }
}