using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using tiempo_libre.DTOs;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Services
{
    public class FestivoTrabajadoService
    {
        private readonly FreeTimeDbContext _db;
        private readonly ILogger<FestivoTrabajadoService> _logger;
        private readonly NotificacionesService _notificacionesService;
        private readonly ValidadorPorcentajeService _validadorPorcentaje;

        public FestivoTrabajadoService(
            FreeTimeDbContext db,
            ILogger<FestivoTrabajadoService> logger,
            NotificacionesService notificacionesService,
            ValidadorPorcentajeService validadorPorcentaje)
        {
            _db = db;
            _logger = logger;
            _notificacionesService = notificacionesService;
            _validadorPorcentaje = validadorPorcentaje;
        }

        /// <summary>
        /// Solicitar el intercambio de un festivo trabajado por un día de vacaciones
        /// Similar a reprogramación: auto-aprueba si no excede porcentaje, sino crea solicitud pendiente
        /// </summary>
        public async Task<ApiResponse<SolicitudFestivoTrabajadoResponse>> SolicitarIntercambioFestivoAsync(
            SolicitudFestivoTrabajadoRequest request, int usuarioSolicitanteId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Verificar que el festivo trabajado existe y pertenece al empleado
                var festivoTrabajado = await _db.DiasFestivosTrabajadosOriginalTable
                    .FirstOrDefaultAsync(f => f.Id == request.FestivoTrabajadoId);

                if (festivoTrabajado == null)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        "El festivo trabajado no existe");
                }

                // 2. Obtener el empleado y verificar que coincide con el festivo
                var empleado = await _db.Users
                    .Include(u => u.Area)
                    .Include(u => u.Grupo)
                    .FirstOrDefaultAsync(u => u.Id == request.EmpleadoId && u.Status == UserStatus.Activo);

                if (empleado == null)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        "Empleado no encontrado o inactivo");
                }

                // Verificar que el empleado coincida con el festivo trabajado (por nómina)
                if (empleado.Nomina != festivoTrabajado.Nomina)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        $"El festivo trabajado no pertenece al empleado especificado");
                }

                // 3. Verificar que el festivo no haya sido ya intercambiado o tenga solicitud pendiente
                var yaIntercambiado = await _db.VacacionesProgramadas
                    .AnyAsync(v => v.EmpleadoId == request.EmpleadoId &&
                                  v.TipoVacacion == "FestivoTrabajado" &&
                                  v.Observaciones != null &&
                                  v.Observaciones.Contains($"FestivoId:{request.FestivoTrabajadoId}"));

                if (yaIntercambiado)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        "Este festivo trabajado ya fue intercambiado anteriormente");
                }

                // Verificar si ya hay una solicitud pendiente para este festivo
                var solicitudExistente = await _db.SolicitudesFestivosTrabajados
                    .AnyAsync(s => s.FestivoTrabajadoOriginalId == request.FestivoTrabajadoId &&
                                  s.EstadoSolicitud == "Pendiente");

                if (solicitudExistente)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        "Ya existe una solicitud pendiente para este festivo trabajado");
                }

                // 4. Validar que la fecha nueva no sea un día inhábil
                var esDiaInhabil = await _db.DiasInhabiles
                    .AnyAsync(d => d.Fecha == request.FechaNueva);

                if (esDiaInhabil)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        "No se puede programar una vacación en un día inhábil o festivo");
                }

                // 5. Validar que no haya conflicto con otras vacaciones del empleado
                var conflictoVacaciones = await _db.VacacionesProgramadas
                    .AnyAsync(v => v.EmpleadoId == request.EmpleadoId &&
                                  v.FechaVacacion == request.FechaNueva &&
                                  v.EstadoVacacion == "Activa");

                if (conflictoVacaciones)
                {
                    return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                        $"Ya existe una vacación programada para el {request.FechaNueva:dd/MM/yyyy}");
                }

                // 6. Calcular porcentaje de ausencia para determinar si requiere aprobación
                decimal porcentaje = 0;
                var requiereAprobacion = false;
                var configuracion = await _db.ConfiguracionVacaciones
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                var porcentajeMaximo = configuracion?.PorcentajeAusenciaMaximo ?? 4.5m;

                if (empleado.GrupoId.HasValue)
                {
                    var grupo = await _db.Grupos
                        .Include(g => g.Area)
                        .FirstOrDefaultAsync(g => g.GrupoId == empleado.GrupoId.Value);

                    if (grupo != null && grupo.Area != null)
                    {
                        var totalEmpleados = await _db.Users
                            .CountAsync(u => u.GrupoId == empleado.GrupoId.Value &&
                                           u.Status == UserStatus.Activo);

                        var vacacionesEnFecha = await _db.VacacionesProgramadas
                            .CountAsync(v => v.Empleado.GrupoId == empleado.GrupoId.Value &&
                                           v.FechaVacacion == request.FechaNueva &&
                                           v.EstadoVacacion == "Activa");

                        var empleadosAusentes = vacacionesEnFecha + 1; // +1 por la nueva solicitud

                        if (totalEmpleados > 0)
                        {
                            porcentaje = ((decimal)empleadosAusentes / totalEmpleados) * 100;
                        }
                    }

                    requiereAprobacion = porcentaje > porcentajeMaximo;
                }

                // 7. Obtener el jefe de área
                User? jefeArea = null;
                if (empleado.AreaId.HasValue)
                {
                    jefeArea = await _db.Users
                        .FirstOrDefaultAsync(u => u.AreaId == empleado.AreaId &&
                                                 u.Roles.Any(r => r.Name == "JefeArea" ||
                                                                r.Name == "Jefe De Area"));
                }

                // 8. Crear la solicitud
                var solicitud = new SolicitudesFestivosTrabajados
                {
                    EmpleadoId = request.EmpleadoId,
                    FestivoTrabajadoOriginalId = request.FestivoTrabajadoId,
                    FechaNuevaSolicitada = request.FechaNueva,
                    Motivo = request.Motivo,
                    EstadoSolicitud = requiereAprobacion ? "Pendiente" : "Aprobada",
                    PorcentajeCalculado = porcentaje,
                    FechaSolicitud = DateTime.Now,
                    SolicitadoPorId = usuarioSolicitanteId,
                    JefeAreaId = jefeArea?.Id,
                    FestivoOriginal = festivoTrabajado.FestivoTrabajado,
                    Nomina = festivoTrabajado.Nomina
                };

                // Si no requiere aprobación, se aprueba automáticamente
                if (!requiereAprobacion)
                {
                    solicitud.FechaRespuesta = DateTime.Now;
                    solicitud.AprobadoPorId = usuarioSolicitanteId; // Auto-aprobado
                }

                _db.SolicitudesFestivosTrabajados.Add(solicitud);
                await _db.SaveChangesAsync();

                // 9. Si se auto-aprobó, crear la vacación
                int? vacacionId = null;
                if (!requiereAprobacion)
                {
                    var nuevaVacacion = new VacacionesProgramadas
                    {
                        EmpleadoId = request.EmpleadoId,
                        FechaVacacion = request.FechaNueva,
                        TipoVacacion = "FestivoTrabajado",
                        OrigenAsignacion = "Manual",
                        EstadoVacacion = "Activa",
                        PeriodoProgramacion = "IntercambioFestivo",
                        FechaProgramacion = DateTime.Now,
                        PuedeSerIntercambiada = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = usuarioSolicitanteId,
                        Observaciones = $"Intercambio de festivo trabajado del {festivoTrabajado.FestivoTrabajado:dd/MM/yyyy}. " +
                                       $"FestivoId:{request.FestivoTrabajadoId}. SolicitudId:{solicitud.Id}. Motivo: {request.Motivo}"
                    };

                    _db.VacacionesProgramadas.Add(nuevaVacacion);
                    await _db.SaveChangesAsync();

                    // Actualizar la solicitud con el ID de la vacación creada
                    solicitud.VacacionCreadaId = nuevaVacacion.Id;
                    vacacionId = nuevaVacacion.Id;
                    await _db.SaveChangesAsync();

                    // Notificar al empleado sobre la aprobación automática
                    await _notificacionesService.NotificarIntercambioFestivoAsync(
                        empleado.Id,
                        festivoTrabajado.FestivoTrabajado,
                        request.FechaNueva,
                        usuarioSolicitanteId);
                }
                else
                {
                    // Notificar al jefe de área sobre la solicitud pendiente
                    if (jefeArea != null)
                    {
                        await _notificacionesService.NotificarSolicitudFestivoTrabajadoAsync(
                            empleado.Id,
                            empleado.FullName,
                            empleado.FullName,
                            request.FechaNueva,
                            request.Motivo,
                            empleado.AreaId,
                            empleado.GrupoId);
                    }
                }

                // 10. Confirmar la transacción
                await transaction.CommitAsync();

                var response = new SolicitudFestivoTrabajadoResponse
                {
                    SolicitudId = solicitud.Id,
                    EmpleadoId = empleado.Id,
                    NombreEmpleado = empleado.FullName,
                    NominaEmpleado = empleado.Nomina?.ToString() ?? empleado.Username,
                    FestivoOriginal = festivoTrabajado.FestivoTrabajado,
                    FechaNueva = request.FechaNueva,
                    Motivo = request.Motivo,
                    EstadoSolicitud = solicitud.EstadoSolicitud,
                    RequiereAprobacion = requiereAprobacion,
                    PorcentajeCalculado = porcentaje,
                    MensajeValidacion = requiereAprobacion
                        ? $"Solicitud creada. Requiere aprobación del jefe de área (porcentaje: {porcentaje:F2}%)"
                        : "Intercambio aprobado automáticamente",
                    FechaSolicitud = DateTime.Now,
                    SolicitadoPor = empleado.FullName,
                    JefeAreaId = jefeArea?.Id,
                    NombreJefeArea = jefeArea?.FullName,
                    VacacionId = vacacionId
                };

                _logger.LogInformation(
                    "Solicitud de festivo trabajado creada para empleado {EmpleadoId}: {FestivoOriginal} -> {FechaNueva}. " +
                    "Estado: {Estado}, Porcentaje: {Porcentaje:F2}%",
                    empleado.Id, festivoTrabajado.FestivoTrabajado, request.FechaNueva,
                    solicitud.EstadoSolicitud, porcentaje);

                return new ApiResponse<SolicitudFestivoTrabajadoResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al solicitar intercambio de festivo trabajado");
                return new ApiResponse<SolicitudFestivoTrabajadoResponse>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Aprobar o rechazar una solicitud de intercambio de festivo (solo jefes de área)
        /// </summary>
        public async Task<ApiResponse<AprobarFestivoTrabajadoResponse>> AprobarRechazarSolicitudAsync(
            AprobarFestivoTrabajadoRequest request, int usuarioAprobadorId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Obtener la solicitud
                var solicitud = await _db.SolicitudesFestivosTrabajados
                    .Include(s => s.Empleado)
                    .Include(s => s.FestivoTrabajadoOriginal)
                    .FirstOrDefaultAsync(s => s.Id == request.SolicitudId);

                if (solicitud == null)
                {
                    return new ApiResponse<AprobarFestivoTrabajadoResponse>(false, null,
                        "Solicitud no encontrada");
                }

                if (solicitud.EstadoSolicitud != "Pendiente")
                {
                    return new ApiResponse<AprobarFestivoTrabajadoResponse>(false, null,
                        $"La solicitud ya fue {solicitud.EstadoSolicitud.ToLower()}");
                }

                // 2. Actualizar el estado de la solicitud
                solicitud.EstadoSolicitud = request.Aprobada ? "Aprobada" : "Rechazada";
                solicitud.FechaRespuesta = DateTime.Now;
                solicitud.AprobadoPorId = usuarioAprobadorId;
                solicitud.MotivoRechazo = request.Aprobada ? null : request.MotivoRechazo;

                // 3. Si se aprueba, crear la vacación
                bool vacacionCreada = false;
                if (request.Aprobada)
                {
                    // Verificar nuevamente que no haya conflictos
                    var conflicto = await _db.VacacionesProgramadas
                        .AnyAsync(v => v.EmpleadoId == solicitud.EmpleadoId &&
                                      v.FechaVacacion == solicitud.FechaNuevaSolicitada &&
                                      v.EstadoVacacion == "Activa");

                    if (!conflicto)
                    {
                        var nuevaVacacion = new VacacionesProgramadas
                        {
                            EmpleadoId = solicitud.EmpleadoId,
                            FechaVacacion = solicitud.FechaNuevaSolicitada,
                            TipoVacacion = "FestivoTrabajado",
                            OrigenAsignacion = "Manual",
                            EstadoVacacion = "Activa",
                            PeriodoProgramacion = "IntercambioFestivo",
                            FechaProgramacion = DateTime.Now,
                            PuedeSerIntercambiada = false,
                            CreatedAt = DateTime.Now,
                            CreatedBy = usuarioAprobadorId,
                            Observaciones = $"Intercambio de festivo trabajado del {solicitud.FestivoOriginal:dd/MM/yyyy}. " +
                                           $"FestivoId:{solicitud.FestivoTrabajadoOriginalId}. SolicitudId:{solicitud.Id}. " +
                                           $"Aprobado por jefe de área. Motivo: {solicitud.Motivo}"
                        };

                        _db.VacacionesProgramadas.Add(nuevaVacacion);
                        await _db.SaveChangesAsync();

                        solicitud.VacacionCreadaId = nuevaVacacion.Id;
                        vacacionCreada = true;
                    }
                    else
                    {
                        return new ApiResponse<AprobarFestivoTrabajadoResponse>(false, null,
                            "No se pudo aprobar: ya existe una vacación en esa fecha");
                    }
                }

                await _db.SaveChangesAsync();

                // 4. Notificar al empleado sobre la decisión
                var aprobador = await _db.Users.FindAsync(usuarioAprobadorId);

                if (request.Aprobada)
                {
                    await _notificacionesService.NotificarIntercambioFestivoAsync(
                        solicitud.EmpleadoId,
                        solicitud.FestivoOriginal,
                        solicitud.FechaNuevaSolicitada,
                        usuarioAprobadorId);
                }
                else
                {
                    // Notificar rechazo (reutilizamos el método de reprogramación)
                    await _notificacionesService.NotificarRespuestaReprogramacionAsync(
                        false,
                        solicitud.EmpleadoId,
                        aprobador?.FullName ?? "Sistema",
                        solicitud.FestivoOriginal,
                        solicitud.FechaNuevaSolicitada,
                        request.MotivoRechazo ?? "No especificado");
                }

                // 5. Confirmar transacción
                await transaction.CommitAsync();

                var response = new AprobarFestivoTrabajadoResponse
                {
                    SolicitudId = solicitud.Id,
                    Aprobada = request.Aprobada,
                    EstadoFinal = solicitud.EstadoSolicitud,
                    EmpleadoId = solicitud.EmpleadoId,
                    NombreEmpleado = solicitud.Empleado.FullName,
                    FestivoOriginal = solicitud.FestivoOriginal,
                    FechaNueva = solicitud.FechaNuevaSolicitada,
                    MotivoRechazo = request.MotivoRechazo,
                    FechaAprobacion = DateTime.Now,
                    AprobadoPor = aprobador?.FullName ?? "Sistema",
                    VacacionCreada = vacacionCreada
                };

                _logger.LogInformation(
                    "Solicitud de festivo {SolicitudId} {Estado} por usuario {UsuarioId}",
                    solicitud.Id, solicitud.EstadoSolicitud, usuarioAprobadorId);

                return new ApiResponse<AprobarFestivoTrabajadoResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al aprobar/rechazar solicitud de festivo trabajado");
                return new ApiResponse<AprobarFestivoTrabajadoResponse>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Consultar solicitudes de festivos trabajados con filtros
        /// </summary>
        public async Task<ApiResponse<ListaSolicitudesFestivoResponse>> ConsultarSolicitudesAsync(
            ConsultaSolicitudesFestivoRequest request, int usuarioConsultaId)
        {
            try
            {
                var usuarioConsulta = await _db.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == usuarioConsultaId);

                if (usuarioConsulta == null)
                {
                    return new ApiResponse<ListaSolicitudesFestivoResponse>(false, null,
                        "Usuario no encontrado");
                }

                var esJefeArea = usuarioConsulta.Roles.Any(r => r.Name == "JefeArea" ||
                                                               r.Name == "Jefe De Area" ||
                                                               r.Name == "SuperUsuario");

                // Construir query base
                var query = _db.SolicitudesFestivosTrabajados
                    .Include(s => s.Empleado)
                        .ThenInclude(e => e.Area)
                    .Include(s => s.Empleado)
                        .ThenInclude(e => e.Grupo)
                    .Include(s => s.FestivoTrabajadoOriginal)
                    .Include(s => s.JefeArea)
                    .Include(s => s.AprobadoPor)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(request.Estado))
                {
                    query = query.Where(s => s.EstadoSolicitud == request.Estado);
                }

                if (request.EmpleadoId.HasValue)
                {
                    query = query.Where(s => s.EmpleadoId == request.EmpleadoId.Value);
                }

                if (request.JefeAreaId.HasValue)
                {
                    query = query.Where(s => s.JefeAreaId == request.JefeAreaId.Value);
                }

                if (request.FechaDesde.HasValue)
                {
                    query = query.Where(s => s.FechaSolicitud >= request.FechaDesde.Value);
                }

                if (request.FechaHasta.HasValue)
                {
                    var fechaHastaFinal = request.FechaHasta.Value.Date.AddDays(1);
                    query = query.Where(s => s.FechaSolicitud < fechaHastaFinal);
                }

                if (request.AreaId.HasValue)
                {
                    query = query.Where(s => s.Empleado.Grupo.Area.AreaId == request.AreaId.Value);
                }

                var solicitudes = await query
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToListAsync();

                // Mapear a DTOs
                var solicitudesDto = solicitudes.Select(s => new SolicitudFestivoDto
                {
                    Id = s.Id,
                    EmpleadoId = s.EmpleadoId,
                    NombreEmpleado = s.Empleado.FullName,
                    NominaEmpleado = s.Nomina.ToString(),
                    AreaEmpleado = s.Empleado.Area?.NombreGeneral ?? "",
                    GrupoEmpleado = s.Empleado.Grupo?.Rol ?? "",
                    FestivoTrabajadoOriginalId = s.FestivoTrabajadoOriginalId,
                    FestivoOriginal = s.FestivoOriginal,
                    FechaNueva = s.FechaNuevaSolicitada,
                    Motivo = s.Motivo,
                    EstadoSolicitud = s.EstadoSolicitud,
                    RequiereAprobacion = s.EstadoSolicitud == "Pendiente",
                    PorcentajeCalculado = s.PorcentajeCalculado,
                    FechaSolicitud = s.FechaSolicitud,
                    SolicitadoPor = s.SolicitadoPor?.FullName ?? "",
                    FechaAprobacion = s.FechaRespuesta,
                    AprobadoPor = s.AprobadoPor?.FullName,
                    MotivoRechazo = s.MotivoRechazo,
                    PuedeAprobar = esJefeArea &&
                                  s.EstadoSolicitud == "Pendiente" &&
                                  s.Empleado.Grupo?.Area?.AreaId == usuarioConsulta.AreaId
                }).ToList();

                var response = new ListaSolicitudesFestivoResponse
                {
                    TotalSolicitudes = solicitudesDto.Count,
                    Pendientes = solicitudesDto.Count(s => s.EstadoSolicitud == "Pendiente"),
                    Aprobadas = solicitudesDto.Count(s => s.EstadoSolicitud == "Aprobada"),
                    Rechazadas = solicitudesDto.Count(s => s.EstadoSolicitud == "Rechazada"),
                    Solicitudes = solicitudesDto
                };

                return new ApiResponse<ListaSolicitudesFestivoResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar solicitudes de festivos trabajados");
                return new ApiResponse<ListaSolicitudesFestivoResponse>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Consultar festivos trabajados disponibles para un empleado
        /// </summary>
        public async Task<ApiResponse<ListaFestivosTrabajadosResponse>> ConsultarFestivosTrabajadosAsync(
            ConsultaFestivosTrabajadosRequest request)
        {
            try
            {
                var query = _db.DiasFestivosTrabajadosOriginalTable.AsQueryable();

                // Filtrar por nómina si se especifica
                if (request.Nomina.HasValue)
                {
                    query = query.Where(f => f.Nomina == request.Nomina.Value);
                }

                // Si se especifica empleadoId, buscar su nómina
                if (request.EmpleadoId.HasValue)
                {
                    var empleado = await _db.Users
                        .FirstOrDefaultAsync(u => u.Id == request.EmpleadoId.Value);

                    if (empleado?.Nomina != null)
                    {
                        query = query.Where(f => f.Nomina == empleado.Nomina.Value);
                    }
                    else
                    {
                        return new ApiResponse<ListaFestivosTrabajadosResponse>(false, null,
                            "Empleado no encontrado o sin nómina asignada");
                    }
                }

                // Filtrar por año si se especifica
                if (request.Anio.HasValue)
                {
                    query = query.Where(f => f.FestivoTrabajado.Year == request.Anio.Value);
                }

                var festivos = await query.OrderBy(f => f.FestivoTrabajado).ToListAsync();

                // Buscar cuáles ya fueron intercambiados o tienen solicitudes
                var festivosDto = new List<FestivoTrabajadoDto>();
                var culture = new CultureInfo("es-ES");

                foreach (var festivo in festivos)
                {
                    // Buscar si este festivo ya fue intercambiado
                    var vacacionIntercambio = await _db.VacacionesProgramadas
                        .FirstOrDefaultAsync(v => v.TipoVacacion == "FestivoTrabajado" &&
                                                 v.Observaciones != null &&
                                                 v.Observaciones.Contains($"FestivoId:{festivo.Id}"));

                    // Buscar si tiene solicitud pendiente
                    var solicitudPendiente = await _db.SolicitudesFestivosTrabajados
                        .AnyAsync(s => s.FestivoTrabajadoOriginalId == festivo.Id &&
                                      s.EstadoSolicitud == "Pendiente");

                    var yaIntercambiado = vacacionIntercambio != null || solicitudPendiente;

                    var dto = new FestivoTrabajadoDto
                    {
                        Id = festivo.Id,
                        Nomina = festivo.Nomina,
                        NombreEmpleado = festivo.Nombre,
                        FestivoTrabajado = festivo.FestivoTrabajado,
                        DiaSemana = festivo.FestivoTrabajado.ToString("dddd", culture),
                        YaIntercambiado = yaIntercambiado,
                        VacacionAsignadaId = vacacionIntercambio?.Id,
                        FechaIntercambio = vacacionIntercambio?.FechaVacacion
                    };

                    // Si solo queremos disponibles, filtrar los ya intercambiados
                    if (!request.SoloDisponibles || !dto.YaIntercambiado)
                    {
                        festivosDto.Add(dto);
                    }
                }

                var response = new ListaFestivosTrabajadosResponse
                {
                    TotalFestivos = festivos.Count,
                    FestivosDisponibles = festivosDto.Count(f => !f.YaIntercambiado),
                    FestivosIntercambiados = festivosDto.Count(f => f.YaIntercambiado),
                    Festivos = festivosDto
                };

                return new ApiResponse<ListaFestivosTrabajadosResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar festivos trabajados");
                return new ApiResponse<ListaFestivosTrabajadosResponse>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Validar si un intercambio de festivo es posible antes de solicitarlo
        /// </summary>
        public async Task<ApiResponse<ValidarFestivoTrabajadoResponse>> ValidarIntercambioFestivoAsync(
            ValidarFestivoTrabajadoRequest request)
        {
            try
            {
                var response = new ValidarFestivoTrabajadoResponse
                {
                    FechaNueva = request.FechaNueva
                };

                // 1. Verificar que el festivo existe
                var festivo = await _db.DiasFestivosTrabajadosOriginalTable
                    .FirstOrDefaultAsync(f => f.Id == request.FestivoTrabajadoId);

                if (festivo == null)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "El festivo trabajado no existe";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                response.FestivoOriginal = festivo.FestivoTrabajado;

                // 2. Verificar que el empleado existe y coincide con el festivo
                var empleado = await _db.Users
                    .FirstOrDefaultAsync(u => u.Id == request.EmpleadoId);

                if (empleado == null)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "Empleado no encontrado";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                response.EmpleadoCoincide = empleado.Nomina == festivo.Nomina;
                if (!response.EmpleadoCoincide)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "El festivo trabajado no pertenece al empleado especificado";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                // 3. Verificar si ya fue intercambiado o tiene solicitud pendiente
                var yaIntercambiado = await _db.VacacionesProgramadas
                    .AnyAsync(v => v.TipoVacacion == "FestivoTrabajado" &&
                                  v.Observaciones != null &&
                                  v.Observaciones.Contains($"FestivoId:{request.FestivoTrabajadoId}"));

                var tieneSolicitudPendiente = await _db.SolicitudesFestivosTrabajados
                    .AnyAsync(s => s.FestivoTrabajadoOriginalId == request.FestivoTrabajadoId &&
                                  s.EstadoSolicitud == "Pendiente");

                response.FestivoDisponible = !yaIntercambiado && !tieneSolicitudPendiente;

                if (yaIntercambiado)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "Este festivo trabajado ya fue intercambiado anteriormente";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                if (tieneSolicitudPendiente)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "Este festivo tiene una solicitud pendiente de aprobación";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                // 4. Validar fecha nueva
                var esDiaInhabil = await _db.DiasInhabiles
                    .AnyAsync(d => d.Fecha == request.FechaNueva);

                if (esDiaInhabil)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "La fecha solicitada es un día inhábil o festivo";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                // 5. Verificar conflictos con otras vacaciones
                var conflicto = await _db.VacacionesProgramadas
                    .AnyAsync(v => v.EmpleadoId == request.EmpleadoId &&
                                  v.FechaVacacion == request.FechaNueva &&
                                  v.EstadoVacacion == "Activa");

                if (conflicto)
                {
                    response.EsValido = false;
                    response.MotivoInvalidez = "Ya existe una vacación programada para esa fecha";
                    return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
                }

                // 6. Advertir sobre porcentaje de ausencia (no bloquear, solo advertir)
                if (empleado.GrupoId.HasValue)
                {
                    var grupo = await _db.Grupos
                        .Include(g => g.Area)
                        .FirstOrDefaultAsync(g => g.GrupoId == empleado.GrupoId.Value);

                    decimal porcentaje = 0;
                    if (grupo != null && grupo.Area != null)
                    {
                        var totalEmpleados = await _db.Users
                            .CountAsync(u => u.GrupoId == empleado.GrupoId.Value &&
                                           u.Status == UserStatus.Activo);

                        var vacacionesEnFecha = await _db.VacacionesProgramadas
                            .CountAsync(v => v.Empleado.GrupoId == empleado.GrupoId.Value &&
                                           v.FechaVacacion == request.FechaNueva &&
                                           v.EstadoVacacion == "Activa");

                        var empleadosAusentes = vacacionesEnFecha + 1;

                        if (totalEmpleados > 0)
                        {
                            porcentaje = ((decimal)empleadosAusentes / totalEmpleados) * 100;
                        }
                    }

                    var configuracion = await _db.ConfiguracionVacaciones
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();

                    var porcentajeMaximo = configuracion?.PorcentajeAusenciaMaximo ?? 4.5m;

                    if (porcentaje > porcentajeMaximo)
                    {
                        response.Advertencias.Add($"El porcentaje de ausencia ({porcentaje:F2}%) excede el máximo permitido ({porcentajeMaximo:F2}%). Requerirá aprobación del jefe de área.");
                    }
                    else if (porcentaje > porcentajeMaximo * 0.8m)
                    {
                        response.Advertencias.Add($"El porcentaje de ausencia está cerca del límite: {porcentaje:F2}%");
                    }
                }

                response.EsValido = true;
                return new ApiResponse<ValidarFestivoTrabajadoResponse>(true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar intercambio de festivo trabajado");
                return new ApiResponse<ValidarFestivoTrabajadoResponse>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtener el historial de festivos intercambiados de un empleado
        /// </summary>
        public async Task<ApiResponse<List<VacacionDetalle>>> ObtenerHistorialFestivosIntercambiadosAsync(
            int empleadoId, int? anio = null)
        {
            try
            {
                var query = _db.VacacionesProgramadas
                    .Where(v => v.EmpleadoId == empleadoId &&
                               v.TipoVacacion == "FestivoTrabajado" &&
                               v.EstadoVacacion == "Activa");

                if (anio.HasValue)
                {
                    query = query.Where(v => v.FechaVacacion.Year == anio.Value);
                }

                var vacaciones = await query
                    .OrderBy(v => v.FechaVacacion)
                    .ToListAsync();

                var culture = new CultureInfo("es-ES");
                var detalles = vacaciones.Select(v => new VacacionDetalle
                {
                    Id = v.Id,
                    FechaVacacion = v.FechaVacacion,
                    TipoVacacion = v.TipoVacacion,
                    OrigenAsignacion = v.OrigenAsignacion,
                    EstadoVacacion = v.EstadoVacacion,
                    PeriodoProgramacion = v.PeriodoProgramacion,
                    FechaProgramacion = v.FechaProgramacion,
                    PuedeSerIntercambiada = v.PuedeSerIntercambiada,
                    Observaciones = v.Observaciones,
                    NumeroSemana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        v.FechaVacacion.ToDateTime(TimeOnly.MinValue),
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday),
                    DiaSemana = v.FechaVacacion.ToString("dddd", culture)
                }).ToList();

                return new ApiResponse<List<VacacionDetalle>>(true, detalles, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de festivos intercambiados");
                return new ApiResponse<List<VacacionDetalle>>(false, null,
                    $"Error inesperado: {ex.Message}");
            }
        }
    }
}