using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.DTOs;
using tiempo_libre.Models;

namespace tiempo_libre.Services
{
    public class ReportesVacacionesService
    {
        private readonly FreeTimeDbContext _db;
        private readonly ILogger<ReportesVacacionesService> _logger;

        public ReportesVacacionesService(
            FreeTimeDbContext db,
            ILogger<ReportesVacacionesService> logger)
        {
            _db = db;
            _logger = logger;
            // Sin límite de tiempo para consultas de reportes pesados
            _db.Database.SetCommandTimeout(0);
        }

        public async Task<ApiResponse<EmpleadosFaltantesCapturaResponse>> ObtenerEmpleadosFaltantesCapturaVacacionesAsync(
            int anioObjetivo,
            int? areaId = null,
            int? grupoId = null)
        {
            try
            {
                // 0 = sin límite de timeout para permitir consultas largas
                _db.Database.SetCommandTimeout(0);
                if (anioObjetivo <= 0)
                {
                    return new ApiResponse<EmpleadosFaltantesCapturaResponse>(false, null, "El año objetivo es obligatorio");
                }

                _logger.LogInformation("Obteniendo empleados sin vacaciones manuales para año {Anio} (Area={AreaId}, Grupo={GrupoId})",
                    anioObjetivo, areaId?.ToString() ?? "Todos", grupoId?.ToString() ?? "Todos");

                var manualesActivas = _db.VacacionesProgramadas
                    .Where(v => v.FechaVacacion.Year == anioObjetivo
                             && v.EstadoVacacion == "Activa"
                             && v.OrigenAsignacion != null
                             && v.OrigenAsignacion.Trim().ToUpper() == "MANUAL");

                var query = from asignacion in _db.AsignacionesBloque
                            join bloque in _db.BloquesReservacion on asignacion.BloqueId equals bloque.Id
                            join grupo in _db.Grupos on bloque.GrupoId equals grupo.GrupoId
                            join area in _db.Areas on grupo.AreaId equals area.AreaId
                            join empleado in _db.Users on asignacion.EmpleadoId equals empleado.Id
                            join vacacionManual in manualesActivas on empleado.Id equals vacacionManual.EmpleadoId into vacacionManualJoin
                            from vacacionManual in vacacionManualJoin.DefaultIfEmpty()
                            where bloque.AnioGeneracion == anioObjetivo
                                  && bloque.EsBloqueCola
                            select new
                            {
                                asignacion,
                                bloque,
                                grupo,
                                area,
                                empleado,
                                vacacionManual
                            };

                if (areaId.HasValue)
                {
                    query = query.Where(x => x.area.AreaId == areaId.Value);
                }

                if (grupoId.HasValue)
                {
                    query = query.Where(x => x.grupo.GrupoId == grupoId.Value);
                }

                var empleados = await query
                    .Where(x => x.vacacionManual == null)
                    .OrderBy(x => x.area.NombreGeneral)
                    .ThenBy(x => x.grupo.Rol)
                    .ThenBy(x => x.bloque.NumeroBloque)
                    .ThenBy(x => x.empleado.FullName)
                    .Select(x => new EmpleadoFaltanteCapturaDto
                    {
                        EmpleadoId = x.empleado.Id,
                        NombreCompleto = x.empleado.FullName ?? "",
                        Nomina = x.empleado.Nomina.HasValue ? x.empleado.Nomina.Value.ToString() : "",
                        Maquina = x.empleado.Maquina,
                        GrupoId = x.grupo.GrupoId,
                        NombreGrupo = x.grupo.Rol ?? "",
                        AreaId = x.area.AreaId,
                        NombreArea = x.area.NombreGeneral ?? "",
                        BloqueId = x.bloque.Id,
                        NumeroBloque = x.bloque.NumeroBloque,
                        EsBloqueCola = x.bloque.EsBloqueCola,
                        FechaLimiteBloque = x.bloque.FechaHoraFin,
                        FechaAsignacion = x.asignacion.FechaAsignacion,
                        Observaciones = x.asignacion.Observaciones,
                        RequiereAccionUrgente = x.bloque.EsBloqueCola
                    })
                    .ToListAsync();

                var response = new EmpleadosFaltantesCapturaResponse
                {
                    Anio = anioObjetivo,
                    TotalEmpleados = empleados.Count,
                    TotalCriticos = empleados.Count(e => e.EsBloqueCola),
                    Empleados = empleados,
                    FechaReporte = DateTime.Now
                };

                return new ApiResponse<EmpleadosFaltantesCapturaResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados faltantes de captura de vacaciones");
                return new ApiResponse<EmpleadosFaltantesCapturaResponse>(false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VacacionesAsignadasEmpresaResponse>> ObtenerVacacionesAsignadasPorEmpresaAsync(
            int anioObjetivo,
            int? areaId = null,
            int? grupoId = null)
        {
            try
            {
                if (anioObjetivo <= 0)
                {
                    return new ApiResponse<VacacionesAsignadasEmpresaResponse>(false, null, "El año objetivo es obligatorio");
                }

                _logger.LogInformation("Obteniendo vacaciones asignadas por la empresa para {Anio} (Area={AreaId}, Grupo={GrupoId})",
                    anioObjetivo, areaId?.ToString() ?? "Todos", grupoId?.ToString() ?? "Todos");

                var query = _db.VacacionesProgramadas
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Area)
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Grupo)
                    .Where(v => v.FechaVacacion.Year == anioObjetivo
                             && v.EstadoVacacion == "Activa")
                    .Where(v =>
                        (v.OrigenAsignacion != null && v.OrigenAsignacion.Trim().ToUpper() == "AUTOMATICA") ||
                        (v.OrigenAsignacion != null && v.OrigenAsignacion.Trim().ToUpper() == "SISTEMA") ||
                        v.TipoVacacion == "Automatica");

                if (areaId.HasValue)
                {
                    query = query.Where(v => v.Empleado.AreaId == areaId.Value);
                }

                if (grupoId.HasValue)
                {
                    query = query.Where(v => v.Empleado.GrupoId == grupoId.Value);
                }

                var vacaciones = await query
                    .OrderBy(v => v.Empleado.Nomina)
                    .ThenBy(v => v.FechaVacacion)
                    .Select(v => new VacacionAsignadaEmpresaDto
                    {
                        EmpleadoId = v.EmpleadoId,
                        NombreCompleto = v.Empleado.FullName ?? "",
                        Nomina = v.Empleado.Nomina.HasValue ? v.Empleado.Nomina.Value.ToString() : "",
                        Maquina = v.Empleado.Maquina,
                        AreaId = v.Empleado.AreaId,
                        NombreArea = v.Empleado.Area != null ? v.Empleado.Area.NombreGeneral : null,
                        GrupoId = v.Empleado.GrupoId,
                        NombreGrupo = v.Empleado.Grupo != null ? v.Empleado.Grupo.Rol : null,
                        FechaVacacion = v.FechaVacacion,
                        TipoVacacion = v.TipoVacacion,
                        OrigenAsignacion = v.OrigenAsignacion,
                        EstadoVacacion = v.EstadoVacacion,
                        PeriodoProgramacion = v.PeriodoProgramacion,
                        FechaProgramacion = v.FechaProgramacion,
                        Observaciones = v.Observaciones
                    })
                    .ToListAsync();

                var response = new VacacionesAsignadasEmpresaResponse
                {
                    Anio = anioObjetivo,
                    TotalVacaciones = vacaciones.Count,
                    TotalEmpleados = vacaciones.Select(v => v.EmpleadoId).Distinct().Count(),
                    Vacaciones = vacaciones,
                    FechaReporte = DateTime.Now
                };

                return new ApiResponse<VacacionesAsignadasEmpresaResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vacaciones asignadas por la empresa");
                return new ApiResponse<VacacionesAsignadasEmpresaResponse>(false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<EmpleadosEnVacacionesResponse>> ObtenerEmpleadosEnVacacionesAsync(
            DateOnly? fechaConsulta = null,
            int? areaId = null,
            int? grupoId = null)
        {
            try
            {
                var fecha = fechaConsulta ?? DateOnly.FromDateTime(DateTime.Now);

                _logger.LogInformation("Obteniendo empleados en vacaciones para fecha {Fecha} (Area={AreaId}, Grupo={GrupoId})",
                    fecha, areaId?.ToString() ?? "Todos", grupoId?.ToString() ?? "Todos");

                var query = _db.VacacionesProgramadas
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Area)
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Grupo)
                    .Where(v => v.FechaVacacion == fecha
                             && v.EstadoVacacion == "Activa");

                if (areaId.HasValue)
                {
                    query = query.Where(v => v.Empleado.AreaId == areaId.Value);
                }

                if (grupoId.HasValue)
                {
                    query = query.Where(v => v.Empleado.GrupoId == grupoId.Value);
                }

                var empleados = await query
                    .OrderBy(v => v.Empleado.Area != null ? v.Empleado.Area.NombreGeneral : "")
                    .ThenBy(v => v.Empleado.Grupo != null ? v.Empleado.Grupo.Rol : "")
                    .ThenBy(v => v.Empleado.Nomina)
                    .Select(v => new EmpleadoEnVacacionesDto
                    {
                        EmpleadoId = v.EmpleadoId,
                        NombreCompleto = v.Empleado.FullName ?? "",
                        Nomina = v.Empleado.Nomina.HasValue ? v.Empleado.Nomina.Value.ToString() : "",
                        Maquina = v.Empleado.Maquina,
                        AreaId = v.Empleado.AreaId,
                        NombreArea = v.Empleado.Area != null ? v.Empleado.Area.NombreGeneral : null,
                        GrupoId = v.Empleado.GrupoId,
                        NombreGrupo = v.Empleado.Grupo != null ? v.Empleado.Grupo.Rol : null,
                        FechaVacacion = v.FechaVacacion,
                        TipoVacacion = v.TipoVacacion,
                        OrigenAsignacion = v.OrigenAsignacion,
                        EstadoVacacion = v.EstadoVacacion,
                        PeriodoProgramacion = v.PeriodoProgramacion,
                        Observaciones = v.Observaciones
                    })
                    .ToListAsync();

                var response = new EmpleadosEnVacacionesResponse
                {
                    FechaConsulta = fecha,
                    TotalRegistros = empleados.Count,
                    TotalEmpleados = empleados.Select(e => e.EmpleadoId).Distinct().Count(),
                    Empleados = empleados,
                    FechaReporte = DateTime.Now
                };

                return new ApiResponse<EmpleadosEnVacacionesResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados en vacaciones");
                return new ApiResponse<EmpleadosEnVacacionesResponse>(false, null, $"Error inesperado: {ex.Message}");
            }
        }
    }
}




