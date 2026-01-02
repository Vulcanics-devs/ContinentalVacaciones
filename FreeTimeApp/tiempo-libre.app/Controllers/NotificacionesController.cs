using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using tiempo_libre.Services;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using tiempo_libre.DTOs;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/notificaciones")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionesService _notificacionesService;
        private readonly FreeTimeDbContext _context;
        private readonly ILogger<NotificacionesController> _logger;

        public NotificacionesController(
            NotificacionesService notificacionesService,
            FreeTimeDbContext context,
            ILogger<NotificacionesController> logger)
        {
            _notificacionesService = notificacionesService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtener notificaciones según el rol del usuario autenticado
        /// </summary>
        /// <param name="request">Filtros y paginación</param>
        /// <returns>Lista de notificaciones según el rol</returns>
        [HttpPost("obtener")]
        public async Task<IActionResult> ObtenerNotificacionesPorRol([FromBody] ObtenerNotificacionesRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage));
                    return BadRequest(new ApiResponse<object>(false, null, $"Datos de entrada inválidos: {errors}"));
                }

                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUsername))
                {
                    _logger.LogWarning("Usuario no autenticado intentando obtener notificaciones");
                    return Unauthorized(new ApiResponse<object>(false, null, "Usuario no autenticado"));
                }

                _logger.LogInformation("Usuario {Username} obteniendo notificaciones con filtros: {Filters}",
                    currentUsername, System.Text.Json.JsonSerializer.Serialize(request));

                var response = await _notificacionesService.ObtenerNotificacionesPorRolAsync(currentUsername, request);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para usuario {Username}", User.Identity?.Name);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }

        /// <summary>
        /// Marcar una notificación como leída
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("marcar-leida/{notificacionId}")]
        public async Task<IActionResult> MarcarComoLeida(int notificacionId)
        {
            try
            {
                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUsername))
                {
                    return Unauthorized(new ApiResponse<object>(false, null, "Usuario no autenticado"));
                }

                var success = await _notificacionesService.MarcarComoLeidaPorUsernameAsync(notificacionId, currentUsername);

                if (!success)
                    return NotFound(new ApiResponse<object>(false, null, "Notificación no encontrada o sin permisos"));

                _logger.LogInformation("Notificación {NotificacionId} marcada como leída por {Username}",
                    notificacionId, currentUsername);

                return Ok(new ApiResponse<object>(true, null, "Notificación marcada como leída"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación {NotificacionId} como leída", notificacionId);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }

        /// <summary>
        /// Archivar una notificación
        /// </summary>
        /// <param name="notificacionId">ID de la notificación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("archivar/{notificacionId}")]
        public async Task<IActionResult> ArchivarNotificacion(int notificacionId)
        {
            try
            {
                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUsername))
                {
                    return Unauthorized(new ApiResponse<object>(false, null, "Usuario no autenticado"));
                }

                var success = await _notificacionesService.ArchivarNotificacionPorUsernameAsync(notificacionId, currentUsername);

                if (!success)
                    return NotFound(new ApiResponse<object>(false, null, "Notificación no encontrada o sin permisos"));

                _logger.LogInformation("Notificación {NotificacionId} archivada por {Username}",
                    notificacionId, currentUsername);

                return Ok(new ApiResponse<object>(true, null, "Notificación archivada"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al archivar notificación {NotificacionId}", notificacionId);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtener estadísticas de notificaciones del usuario actual
        /// </summary>
        /// <returns>Estadísticas de notificaciones</returns>
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticasNotificaciones()
        {
            try
            {
                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUsername))
                {
                    return Unauthorized(new ApiResponse<object>(false, null, "Usuario no autenticado"));
                }

                var response = await _notificacionesService.ObtenerEstadisticasNotificacionesAsync(currentUsername);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de notificaciones para {Username}", User.Identity?.Name);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }
    }
}