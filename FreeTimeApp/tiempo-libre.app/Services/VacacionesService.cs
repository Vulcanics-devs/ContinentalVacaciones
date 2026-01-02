using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models;
using tiempo_libre.DTOs;

namespace tiempo_libre.Services
{
    public class VacacionesService
    {
        private readonly FreeTimeDbContext _db;
        private readonly ValidadorPorcentajeService _validadorPorcentaje;
        private readonly NotificacionesService _notificacionesService;
        private readonly ILogger<VacacionesService> _logger;

        public VacacionesService(
            FreeTimeDbContext db,
            ValidadorPorcentajeService validadorPorcentaje,
            NotificacionesService notificacionesService,
            ILogger<VacacionesService> logger)
        {
            _db = db;
            _validadorPorcentaje = validadorPorcentaje;
            _notificacionesService = notificacionesService;
            _logger = logger;
        }

        public async Task<ApiResponse<VacacionesEmpleadoResponse>> CalcularVacacionesPorEmpleadoAsync(int empleadoId, int anio)
        {
            var empleado = await _db.Users.FindAsync(empleadoId);

            if (empleado == null)
                return new ApiResponse<VacacionesEmpleadoResponse>(false, null, "El empleado especificado no existe.");

            if (empleado.FechaIngreso == null)
                return new ApiResponse<VacacionesEmpleadoResponse>(false, null, "El empleado no tiene fecha de ingreso registrada.");

            var fechaReferencia = new DateOnly(anio, 12, 31);
            var antiguedadEnAnios = CalcularAntiguedadEnAnios(empleado.FechaIngreso.Value, fechaReferencia);

            if (antiguedadEnAnios < 1)
                return new ApiResponse<VacacionesEmpleadoResponse>(false, null, "El empleado no tiene antigüedad suficiente para el año especificado.");

            var vacaciones = CalcularVacacionesPorAntiguedad(antiguedadEnAnios);

            var response = new VacacionesEmpleadoResponse
            {
                EmpleadoId = empleadoId,
                NombreCompleto = empleado.FullName,
                FechaIngreso = empleado.FechaIngreso.Value,
                AnioConsulta = anio,
                AntiguedadEnAnios = antiguedadEnAnios,
                DiasEmpresa = vacaciones.DiasEmpresa,
                DiasAsignadosAutomaticamente = vacaciones.DiasAsignadosAutomaticamente,
                DiasProgramables = vacaciones.DiasProgramables,
                TotalDias = vacaciones.TotalDias
            };

            return new ApiResponse<VacacionesEmpleadoResponse>(true, response, null);
        }

        public VacacionesCalculadas CalcularVacacionesPorAntiguedad(int antiguedadEnAnios)
        {
            const int diasEmpresa = 12;
            int diasAsignadosAutomaticamente = 0;
            int diasProgramables = 0;

            if (antiguedadEnAnios <= 5)
            {
                switch (antiguedadEnAnios)
                {
                    case 1: diasProgramables = 0; break;
                    case 2: diasProgramables = 2; break;
                    case 3: diasProgramables = 4; break;
                    case 4: diasAsignadosAutomaticamente = 3; diasProgramables = 3; break;
                    case 5: diasAsignadosAutomaticamente = 4; diasProgramables = 4; break;
                }
            }
            else
            {
                diasAsignadosAutomaticamente = 5;
                int diasProgramablesBase = 5;
                int gruposDeCincoAnios = (antiguedadEnAnios - 6) / 5;
                diasProgramables = diasProgramablesBase + (gruposDeCincoAnios * 2);
            }

            return new VacacionesCalculadas
            {
                DiasEmpresa = diasEmpresa,
                DiasAsignadosAutomaticamente = diasAsignadosAutomaticamente,
                DiasProgramables = diasProgramables,
                TotalDias = diasEmpresa + diasAsignadosAutomaticamente + diasProgramables
            };
        }

        private int CalcularAntiguedadEnAnios(DateOnly fechaIngreso, DateOnly fechaReferencia)
        {
            var antiguedad = fechaReferencia.Year - fechaIngreso.Year;

            if (fechaReferencia.Month < fechaIngreso.Month ||
                (fechaReferencia.Month == fechaIngreso.Month && fechaReferencia.Day < fechaIngreso.Day))
            {
                antiguedad--;
            }

            return Math.Max(0, antiguedad);
        }

        public async Task<ApiResponse<AsignacionManualResponse>> AsignarVacacionesManualAsync(
            AsignacionManualRequest request, int usuarioAsignaId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var empleado = await _db.Users.Include(u => u.Grupo).FirstOrDefaultAsync(u => u.Id == request.EmpleadoId);
                if (empleado == null)
                    return new ApiResponse<AsignacionManualResponse>(false, null, "Empleado no encontrado");

                var vacacionesAsignadas = new List<VacacionesProgramadas>();
                var advertencias = new List<string>();

                if (!request.IgnorarRestricciones)
                {
                    var diasYaAsignados = await _db.VacacionesProgramadas
                        .Where(v => v.EmpleadoId == request.EmpleadoId
                            && request.FechasVacaciones.Contains(v.FechaVacacion)
                            && v.EstadoVacacion == "Activa")
                        .Select(v => v.FechaVacacion)
                        .ToListAsync();

                    if (diasYaAsignados.Any())
                    {
                        advertencias.Add($"Ya existen vacaciones asignadas en las fechas: {string.Join(", ", diasYaAsignados)}");
                        request.FechasVacaciones = request.FechasVacaciones
                            .Where(f => !diasYaAsignados.Contains(f))
                            .ToList();
                    }
                }

                foreach (var fecha in request.FechasVacaciones)
                {
                    var vacacion = new VacacionesProgramadas
                    {
                        EmpleadoId = request.EmpleadoId,
                        FechaVacacion = fecha,
                        TipoVacacion = request.TipoVacacion,
                        OrigenAsignacion = request.OrigenAsignacion,
                        EstadoVacacion = request.EstadoVacacion,
                        Observaciones = request.Observaciones,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _db.VacacionesProgramadas.Add(vacacion);
                    vacacionesAsignadas.Add(vacacion);
                }

                await _db.SaveChangesAsync();

                if (request.NotificarEmpleado)
                {
                    await _notificacionesService.CrearNotificacionAsync(
                        Models.Enums.TiposDeNotificacionEnum.RegistroVacaciones,
                        "Vacaciones Asignadas",
                        $"Se te han asignado {vacacionesAsignadas.Count} días de vacaciones. " +
                        $"Tipo: {request.TipoVacacion}. " +
                        $"Motivo: {request.MotivoAsignacion ?? "Asignación administrativa"}",
                        "Sistema de Vacaciones",
                        request.EmpleadoId,
                        usuarioAsignaId,
                        empleado.Grupo?.AreaId,
                        empleado.GrupoId,
                        "AsignacionManual",
                        null,
                        new
                        {
                            TotalDias = vacacionesAsignadas.Count,
                            Fechas = request.FechasVacaciones,
                            Tipo = request.TipoVacacion
                        }
                    );
                }

                var usuarioAsigno = await _db.Users.FindAsync(usuarioAsignaId);
                await transaction.CommitAsync();

                var response = new AsignacionManualResponse
                {
                    Exitoso = true,
                    EmpleadoId = request.EmpleadoId,
                    NombreEmpleado = empleado.FullName,
                    VacacionesAsignadasIds = vacacionesAsignadas.Select(v => v.Id).ToList(),
                    FechasAsignadas = vacacionesAsignadas.Select(v => v.FechaVacacion).ToList(),
                    TotalDiasAsignados = vacacionesAsignadas.Count,
                    TipoVacacion = request.TipoVacacion,
                    Mensaje = $"Se asignaron {vacacionesAsignadas.Count} días de vacaciones exitosamente",
                    Advertencias = advertencias,
                    FechaAsignacion = DateTime.Now,
                    UsuarioAsigno = usuarioAsigno?.FullName ?? $"Usuario {usuarioAsignaId}"
                };

                return new ApiResponse<AsignacionManualResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al asignar vacaciones manualmente");
                return new ApiResponse<AsignacionManualResponse>(false, null, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> EliminarVacacionesAsync(List<int> vacacionesIds)
        {
            try
            {
                var vacaciones = await _db.VacacionesProgramadas
                    .Where(v => vacacionesIds.Contains(v.Id))
                    .ToListAsync();

                if (vacaciones == null || !vacaciones.Any())
                    return new ApiResponse<object>(false, null, "No se encontraron vacaciones con los IDs especificados.");

                _db.VacacionesProgramadas.RemoveRange(vacaciones);
                await _db.SaveChangesAsync();

                return new ApiResponse<object>(true, null, $"Se eliminaron {vacaciones.Count} vacaciones correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vacaciones");
                return new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> EliminarVacacionesPorFechaAsync(int empleadoId, List<DateOnly> fechas)
        {
            try
            {
                var vacaciones = await _db.VacacionesProgramadas
                    .Where(v => v.EmpleadoId == empleadoId && fechas.Contains(v.FechaVacacion))
                    .ToListAsync();

                if (!vacaciones.Any())
                    return new ApiResponse<object>(false, null, "No se encontraron vacaciones para las fechas especificadas.");

                _db.VacacionesProgramadas.RemoveRange(vacaciones);
                await _db.SaveChangesAsync();

                return new ApiResponse<object>(true, null, $"Se eliminaron {vacaciones.Count} vacaciones correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vacaciones por fecha");
                return new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AsignacionManualLoteResponse>> AsignarVacacionesManualLoteAsync(
            AsignacionManualLoteRequest request, int usuarioAsignaId)
        {
            var response = new AsignacionManualLoteResponse
            {
                TotalEmpleados = request.EmpleadosIds.Count,
                FechaEjecucion = DateTime.Now,
                Detalles = new List<AsignacionManualResponse>()
            };

            var usuarioAsigno = await _db.Users.FindAsync(usuarioAsignaId);
            response.UsuarioEjecuto = usuarioAsigno?.FullName ?? $"Usuario {usuarioAsignaId}";

            foreach (var empleadoId in request.EmpleadosIds)
            {
                var asignacionIndividual = new AsignacionManualRequest
                {
                    EmpleadoId = empleadoId,
                    FechasVacaciones = request.FechasVacaciones,
                    TipoVacacion = request.TipoVacacion,
                    OrigenAsignacion = request.OrigenAsignacion,
                    EstadoVacacion = request.EstadoVacacion,
                    Observaciones = request.Observaciones,
                    MotivoAsignacion = request.MotivoAsignacion,
                    IgnorarRestricciones = request.IgnorarRestricciones,
                    NotificarEmpleado = request.NotificarEmpleados,
                    BloqueId = request.BloqueId,
                    OrigenSolicitud = request.OrigenSolicitud
                };

                var resultado = await AsignarVacacionesManualAsync(asignacionIndividual, usuarioAsignaId);

                if (resultado.Success && resultado.Data != null)
                {
                    response.AsignacionesExitosas++;
                    response.Detalles.Add(resultado.Data);
                }
                else
                {
                    response.AsignacionesFallidas++;
                    response.ErroresGenerales.Add($"Empleado {empleadoId}: {resultado.ErrorMsg}");
                }
            }

            return new ApiResponse<AsignacionManualLoteResponse>(true, response,
                $"Proceso completado: {response.AsignacionesExitosas} exitosas, {response.AsignacionesFallidas} fallidas");
        }
    }

    public class VacacionesCalculadas
    {
        public int DiasEmpresa { get; set; }
        public int DiasAsignadosAutomaticamente { get; set; }
        public int DiasProgramables { get; set; }
        public int TotalDias { get; set; }
    }
}
