using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using tiempo_libre.DTOs;
using System.Threading.Tasks;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgramacionAnualController : ControllerBase
    {
        private readonly FreeTimeDbContext _db;
        private readonly ILogger<ProgramacionAnualController> _logger;

        public ProgramacionAnualController(FreeTimeDbContext db, ILogger<ProgramacionAnualController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetActual()
        {
            try
            {
                var actual = await _db.ProgramacionesAnuales
                    .Where(pa => (pa.Estatus == EstatusProgramacionAnualEnum.Pendiente || pa.Estatus == EstatusProgramacionAnualEnum.EnProceso) && !pa.BorradoLogico)
                    .OrderByDescending(pa => pa.Id)
                    .FirstOrDefaultAsync();
                if (actual == null)
                    return NotFound(new ApiResponse<object>(false, null, "No existe programación anual actual."));
                return Ok(new ApiResponse<object>(true, actual));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en GetActual");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error inesperado: " + ex.Message));
            }
        }

        [HttpPut("activar")]
        public async Task<IActionResult> Activar([FromBody] int id)
        {
            try
            {
                var pa = await _db.ProgramacionesAnuales.FindAsync(id);
                if (pa == null || pa.BorradoLogico)
                    return NotFound(new ApiResponse<object>(false, null, "No existe programación anual para activar."));
                pa.Estatus = EstatusProgramacionAnualEnum.EnProceso;
                await _db.SaveChangesAsync();
                LogAccion(TiposDeAccionesEnum.Actualizacion, pa.Id, "ProgramacionesAnuales", $"Activada programación anual {pa.Id}");
                _logger.LogInformation("Programación anual {Id} activada por {User}", pa.Id, GetUserName());
                return Ok(new ApiResponse<object>(true, null, "Programación anual activada."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en Activar");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error inesperado: " + ex.Message));
            }
        }

        [HttpPut("reprogramacion")]
        public async Task<IActionResult> Reprogramar([FromBody] int id)
        {
            try
            {
                var pa = await _db.ProgramacionesAnuales.FindAsync(id);
                if (pa == null || pa.BorradoLogico)
                    return NotFound(new ApiResponse<object>(false, null, "No existe programación anual para reprogramar."));
                pa.Estatus = EstatusProgramacionAnualEnum.Reprogramacion;
                await _db.SaveChangesAsync();
                LogAccion(TiposDeAccionesEnum.Actualizacion, pa.Id, "ProgramacionesAnuales", $"Reprogramada programación anual {pa.Id}");
                _logger.LogInformation("Programación anual {Id} reprogramada por {User}", pa.Id, GetUserName());
                return Ok(new ApiResponse<object>(true, null, "Programación anual reprogramada."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en Reprogramar");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error inesperado: " + ex.Message));
            }
        }

        /// <summary>
        /// Revertir COMPLETAMENTE toda la programación anual (bloques, vacaciones, solicitudes)
        /// </summary>
        /// <param name="anio">Año a revertir</param>
        /// <param name="confirmar">Debe ser true para ejecutar (seguridad)</param>
        /// <returns>Detalle de todos los elementos eliminados</returns>
        [HttpDelete("revertir-completo")]
        [Authorize(Roles = "SuperUsuario")]
        public async Task<IActionResult> RevertirProgramacionCompleta(
            [FromQuery] int anio,
            [FromQuery] bool confirmar = false)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (anio < 2020 || anio > 2050)
                {
                    return BadRequest(new ApiResponse<object>(false, null, "El año debe estar entre 2020 y 2050"));
                }

                if (!confirmar)
                {
                    return BadRequest(new ApiResponse<object>(false, null,
                        "Debe confirmar la operación enviando confirmar=true. ADVERTENCIA: Esta operación eliminará TODOS los datos del año."));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usuario = await _db.Users.FindAsync(int.Parse(userIdClaim ?? "0"));

                _logger.LogWarning("Usuario {Usuario} está revirtiendo TODA la programación del año {Anio}",
                    usuario?.FullName ?? userIdClaim, anio);

                var resultado = new ReversionCompletaResponse
                {
                    Anio = anio,
                    FechaEjecucion = DateTime.Now,
                    UsuarioEjecuto = usuario?.FullName ?? $"Usuario {userIdClaim}"
                };

                // 1. Eliminar Solicitudes de Reprogramación
                var solicitudesReprogramacion = await _db.SolicitudesReprogramacion
                    .Include(s => s.VacacionOriginal)
                    .Where(s => s.VacacionOriginal.FechaVacacion.Year == anio)
                    .ToListAsync();

                if (solicitudesReprogramacion.Any())
                {
                    _db.SolicitudesReprogramacion.RemoveRange(solicitudesReprogramacion);
                    resultado.SolicitudesReprogramacionEliminadas = solicitudesReprogramacion.Count;
                    _logger.LogInformation("Eliminando {Count} solicitudes de reprogramación", solicitudesReprogramacion.Count);
                }

                // 2. Eliminar Solicitudes de Festivos Trabajados
                var solicitudesFestivos = await _db.SolicitudesFestivosTrabajados
                    .Where(s => s.FechaNuevaSolicitada.Year == anio || s.FestivoOriginal.Year == anio)
                    .ToListAsync();

                if (solicitudesFestivos.Any())
                {
                    _db.SolicitudesFestivosTrabajados.RemoveRange(solicitudesFestivos);
                    resultado.SolicitudesFestivosEliminadas = solicitudesFestivos.Count;
                    _logger.LogInformation("Eliminando {Count} solicitudes de festivos trabajados", solicitudesFestivos.Count);
                }

                // 3. Eliminar TODAS las Vacaciones Programadas del año
                var vacacionesProgramadas = await _db.VacacionesProgramadas
                    .Where(v => v.FechaVacacion.Year == anio)
                    .ToListAsync();

                if (vacacionesProgramadas.Any())
                {
                    _db.VacacionesProgramadas.RemoveRange(vacacionesProgramadas);
                    resultado.VacacionesEliminadas = vacacionesProgramadas.Count;
                    resultado.VacacionesAutomaticasEliminadas = vacacionesProgramadas.Count(v => v.OrigenAsignacion == "Automatica");
                    resultado.VacacionesManualesEliminadas = vacacionesProgramadas.Count(v => v.OrigenAsignacion != "Automatica");
                    _logger.LogInformation("Eliminando {Count} vacaciones programadas", vacacionesProgramadas.Count);
                }

                // 4. Eliminar Cambios de Bloque
                var cambiosBloques = await _db.CambiosBloque
                    .Include(c => c.BloqueOrigen)
                    .Include(c => c.BloqueDestino)
                    .Where(c => c.BloqueOrigen.AnioGeneracion == anio || c.BloqueDestino.AnioGeneracion == anio)
                    .ToListAsync();

                if (cambiosBloques.Any())
                {
                    _db.CambiosBloque.RemoveRange(cambiosBloques);
                    resultado.CambiosBloqueEliminados = cambiosBloques.Count;
                    _logger.LogInformation("Eliminando {Count} cambios de bloque", cambiosBloques.Count);
                }

                // 5. Eliminar Asignaciones de Bloque
                var asignacionesBloques = await _db.AsignacionesBloque
                    .Include(a => a.Bloque)
                    .Where(a => a.Bloque.AnioGeneracion == anio)
                    .ToListAsync();

                if (asignacionesBloques.Any())
                {
                    _db.AsignacionesBloque.RemoveRange(asignacionesBloques);
                    resultado.AsignacionesBloqueEliminadas = asignacionesBloques.Count;
                    _logger.LogInformation("Eliminando {Count} asignaciones de bloque", asignacionesBloques.Count);
                }

                // 6. Eliminar Bloques de Reservación
                var bloques = await _db.BloquesReservacion
                    .Where(b => b.AnioGeneracion == anio)
                    .ToListAsync();

                if (bloques.Any())
                {
                    _db.BloquesReservacion.RemoveRange(bloques);
                    resultado.BloquesEliminados = bloques.Count;
                    resultado.GruposAfectados = bloques.Select(b => b.GrupoId).Distinct().Count();
                    _logger.LogInformation("Eliminando {Count} bloques de reservación", bloques.Count);
                }

                // 7. Guardar cambios
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                resultado.OperacionExitosa = true;
                resultado.Mensaje = $"Se ha revertido completamente la programación del año {anio}";

                _logger.LogWarning("Reversión completa del año {Anio} ejecutada exitosamente por {Usuario}",
                    anio, usuario?.FullName);

                return Ok(new ApiResponse<ReversionCompletaResponse>(true, resultado,
                    "Programación anual revertida exitosamente"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al revertir programación anual del año {Anio}", anio);
                return StatusCode(500, new ApiResponse<object>(false, null,
                    $"Error al revertir la programación: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtener resumen de datos de programación anual antes de revertir
        /// </summary>
        /// <param name="anio">Año a consultar</param>
        /// <returns>Resumen de todos los elementos que serían eliminados</returns>
        /// <summary>
        /// Endpoint de prueba para verificar que el controlador está funcionando
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "ProgramacionAnualController está funcionando", timestamp = DateTime.Now });
        }

        [HttpGet("resumen-reversion")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> ObtenerResumenReversion([FromQuery] int anio)
        {
            try
            {
                if (anio < 2020 || anio > 2050)
                {
                    return BadRequest(new ApiResponse<object>(false, null, "El año debe estar entre 2020 y 2050"));
                }

                var resumen = new ResumenReversionResponse
                {
                    Anio = anio
                };

                // Contar Bloques
                resumen.TotalBloques = await _db.BloquesReservacion
                    .CountAsync(b => b.AnioGeneracion == anio);

                resumen.BloquesAprobados = await _db.BloquesReservacion
                    .CountAsync(b => b.AnioGeneracion == anio && b.Estado == "Aprobado");

                // Contar Asignaciones
                resumen.TotalAsignacionesBloque = await _db.AsignacionesBloque
                    .Include(a => a.Bloque)
                    .CountAsync(a => a.Bloque.AnioGeneracion == anio);

                // Contar Vacaciones
                var vacaciones = await _db.VacacionesProgramadas
                    .Where(v => v.FechaVacacion.Year == anio)
                    .ToListAsync();

                resumen.TotalVacaciones = vacaciones.Count;
                resumen.VacacionesAutomaticas = vacaciones.Count(v => v.OrigenAsignacion == "Automatica");
                resumen.VacacionesManuales = vacaciones.Count(v => v.OrigenAsignacion != "Automatica");
                resumen.VacacionesTomadas = vacaciones.Count(v => v.EstadoVacacion == "Tomada");

                // Contar Solicitudes
                resumen.SolicitudesReprogramacion = await _db.SolicitudesReprogramacion
                    .Include(s => s.VacacionOriginal)
                    .CountAsync(s => s.VacacionOriginal.FechaVacacion.Year == anio);

                resumen.SolicitudesFestivos = await _db.SolicitudesFestivosTrabajados
                    .CountAsync(s => s.FechaNuevaSolicitada.Year == anio || s.FestivoOriginal.Year == anio);

                resumen.CambiosBloque = await _db.CambiosBloque
                    .Include(c => c.BloqueOrigen)
                    .CountAsync(c => c.BloqueOrigen.AnioGeneracion == anio);

                // Empleados afectados
                var empleadosConVacaciones = vacaciones.Select(v => v.EmpleadoId).Distinct().Count();
                var empleadosConBloques = await _db.AsignacionesBloque
                    .Include(a => a.Bloque)
                    .Where(a => a.Bloque.AnioGeneracion == anio)
                    .Select(a => a.EmpleadoId)
                    .Distinct()
                    .CountAsync();

                resumen.EmpleadosAfectados = Math.Max(empleadosConVacaciones, empleadosConBloques);

                // Grupos afectados
                resumen.GruposAfectados = await _db.BloquesReservacion
                    .Where(b => b.AnioGeneracion == anio)
                    .Select(b => b.GrupoId)
                    .Distinct()
                    .CountAsync();

                // Advertencias
                if (resumen.VacacionesTomadas > 0)
                {
                    resumen.Advertencias.Add($"Hay {resumen.VacacionesTomadas} vacaciones ya tomadas que serán eliminadas");
                }
                if (resumen.BloquesAprobados > 0)
                {
                    resumen.Advertencias.Add($"Hay {resumen.BloquesAprobados} bloques aprobados que serán eliminados");
                }
                if (resumen.SolicitudesReprogramacion > 0)
                {
                    resumen.Advertencias.Add($"Se eliminarán {resumen.SolicitudesReprogramacion} solicitudes de reprogramación");
                }

                return Ok(new ApiResponse<ResumenReversionResponse>(true, resumen,
                    "Resumen de reversión obtenido exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de reversión para año {Anio}", anio);
                return StatusCode(500, new ApiResponse<object>(false, null,
                    $"Error al obtener resumen: {ex.Message}"));
            }
        }

        [HttpPut("cerrar")]
        public async Task<IActionResult> Cerrar([FromBody] int id)
        {
            try
            {
                var pa = await _db.ProgramacionesAnuales.FindAsync(id);
                if (pa == null || pa.BorradoLogico)
                    return NotFound(new ApiResponse<object>(false, null, "No existe programación anual para cerrar."));
                pa.Estatus = EstatusProgramacionAnualEnum.Cerrada;
                await _db.SaveChangesAsync();
                LogAccion(TiposDeAccionesEnum.Actualizacion, pa.Id, "ProgramacionesAnuales", $"Cerrada programación anual {pa.Id}");
                _logger.LogInformation("Programación anual {Id} cerrada por {User}", pa.Id, GetUserName());
                return Ok(new ApiResponse<object>(true, null, "Programación anual cerrada."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en Cerrar");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error inesperado: " + ex.Message));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> BorrarActual()
        {
            try
            {
                var pa = await _db.ProgramacionesAnuales
                    .Where(pa => (pa.Estatus == EstatusProgramacionAnualEnum.Pendiente || pa.Estatus == EstatusProgramacionAnualEnum.EnProceso || pa.Estatus == EstatusProgramacionAnualEnum.Reprogramacion || pa.Estatus == EstatusProgramacionAnualEnum.Cerrada) && !pa.BorradoLogico)
                    .OrderByDescending(pa => pa.Id)
                    .FirstOrDefaultAsync();
                if (pa == null)
                    return NotFound(new ApiResponse<object>(false, null, "No existe programación anual actual para borrar."));
                pa.BorradoLogico = true;
                await _db.SaveChangesAsync();
                LogAccion(TiposDeAccionesEnum.Eliminacion, pa.Id, "ProgramacionesAnuales", $"Borrado lógico programación anual {pa.Id}");
                _logger.LogInformation("Programación anual {Id} borrada lógicamente por {User}", pa.Id, GetUserName());
                return Ok(new ApiResponse<object>(true, null, "Programación anual borrada lógicamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en BorrarActual");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error inesperado: " + ex.Message));
            }
        }

        // Helper para registrar acciones en LoggerAcciones
        private void LogAccion(TiposDeAccionesEnum accion, int idRegistro, string modelo, string detalles)
        {
            var userName = GetUserName();
            var user = _db.Users.FirstOrDefault(u => u.Username == userName);
            var loggerAccion = new LoggerAcciones
            {
                Accion = accion,
                IdUsuario = user?.Id ?? 0,
                User = user,
                NombreCompletoUsuario = user?.FullName ?? userName,
                NominaUsuario = user?.Nomina,
                IdRegistro = idRegistro,
                Modelo = modelo,
                Fecha = DateTime.UtcNow,
                Detalles = detalles,
                IdArea = user?.AreaId,
                IdGrupo = user?.GrupoId,
                Created_At = DateTime.UtcNow
            };
            _db.LoggerAcciones.Add(loggerAccion);
            _db.SaveChanges();
        }

        // Helper para obtener el usuario autenticado
        private string GetUserName()
        {
            return User?.Identity?.Name ?? "sistema";
        }
    }

    // DTOs para las respuestas de reversión
    public class ReversionCompletaResponse
    {
        public int Anio { get; set; }
        public bool OperacionExitosa { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaEjecucion { get; set; }
        public string UsuarioEjecuto { get; set; } = string.Empty;

        // Contadores de eliminación
        public int BloquesEliminados { get; set; }
        public int AsignacionesBloqueEliminadas { get; set; }
        public int VacacionesEliminadas { get; set; }
        public int VacacionesAutomaticasEliminadas { get; set; }
        public int VacacionesManualesEliminadas { get; set; }
        public int SolicitudesReprogramacionEliminadas { get; set; }
        public int SolicitudesFestivosEliminadas { get; set; }
        public int CambiosBloqueEliminados { get; set; }
        public int GruposAfectados { get; set; }
    }

    public class ResumenReversionResponse
    {
        public int Anio { get; set; }

        // Bloques
        public int TotalBloques { get; set; }
        public int BloquesAprobados { get; set; }
        public int TotalAsignacionesBloque { get; set; }

        // Vacaciones
        public int TotalVacaciones { get; set; }
        public int VacacionesAutomaticas { get; set; }
        public int VacacionesManuales { get; set; }
        public int VacacionesTomadas { get; set; }

        // Solicitudes
        public int SolicitudesReprogramacion { get; set; }
        public int SolicitudesFestivos { get; set; }
        public int CambiosBloque { get; set; }

        // Afectados
        public int EmpleadosAfectados { get; set; }
        public int GruposAfectados { get; set; }

        public List<string> Advertencias { get; set; } = new();
    }
}
